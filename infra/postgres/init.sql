-- PostgreSQL init script — runs once on first container startup
-- Creates additional schemas if needed

-- Application schema
CREATE SCHEMA IF NOT EXISTS app;

-- Grant schema permissions
GRANT ALL ON SCHEMA app TO enterprise;
GRANT ALL ON SCHEMA public TO enterprise;

-- Optional: create a read-only reporting user
-- CREATE USER reporting WITH PASSWORD 'ReportPass!';
-- GRANT CONNECT ON DATABASE enterprise_kit TO reporting;
-- GRANT USAGE ON SCHEMA app TO reporting;
-- GRANT SELECT ON ALL TABLES IN SCHEMA app TO reporting;
