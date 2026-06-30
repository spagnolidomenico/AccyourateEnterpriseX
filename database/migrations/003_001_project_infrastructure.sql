-- Accyourate Enterprise X 3.1
-- Project Infrastructure baseline

CREATE TABLE IF NOT EXISTS database_versions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    version TEXT NOT NULL UNIQUE,
    description TEXT NOT NULL,
    applied_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS app_settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    group_name TEXT NOT NULL
);
