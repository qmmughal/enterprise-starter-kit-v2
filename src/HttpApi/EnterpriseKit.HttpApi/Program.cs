using EnterpriseKit.Application.Common.Behaviours;
using EnterpriseKit.HttpApi.Middleware;
using EnterpriseKit.Infrastructure.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;
using EnterpriseKit.Infrastructure.Persistence;

// ── Bootstrap Logger (before DI is built) ─────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting EnterpriseKit API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithMachineName()
           .Enrich.WithThreadId());

    // ── Application Layer ──────────────────────────────────────────────────
    var appAssembly = Assembly.Load("EnterpriseKit.Application");

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(appAssembly);

        // Pipeline order: Logging → Validation → Transaction → Handler
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
    });

    builder.Services.AddValidatorsFromAssembly(appAssembly);
    builder.Services.AddAutoMapper(appAssembly);

    // ── Infrastructure ─────────────────────────────────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Redis Distributed Cache ────────────────────────────────────────────
    var redisConn = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConn))
    {
        builder.Services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = redisConn;
            opts.InstanceName = "ek:";
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache(); // Fallback for local dev
    }

    // ── Controllers ────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EnterpriseKit API",
            Version = "v1",
            Description = "Production-grade Enterprise Starter Kit — Clean Architecture + CQRS + ABP"
        });

        // Include XML comments from all projects
        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (var xmlFile in xmlFiles)
            c.IncludeXmlComments(xmlFile);

        // Bearer token auth
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            }
        });
    });

    // ── Auth ───────────────────────────────────────────────────────────────
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer(opts =>
        {
            opts.Authority = builder.Configuration["Auth:Authority"];
            opts.Audience = builder.Configuration["Auth:Audience"];
        });
    builder.Services.AddAuthorization();

    // ── Health Checks ──────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();
    // TODO: Add .AddNpgSql(connectionString) after adding AspNetCore.HealthChecks.NpgSql package

    // ────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Auto-apply migrations on startup (dev/staging only) ───────────────
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied.");
    }

    // ── Middleware Pipeline ────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();   // ← must be first
    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "");
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserId", httpContext.User?.Identity?.Name ?? "anonymous");
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseKit API v1");
            c.RoutePrefix = string.Empty; // Serve Swagger at root
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
