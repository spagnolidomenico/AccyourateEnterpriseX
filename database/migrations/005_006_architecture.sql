-- Accyourate Enterprise X 5.6
-- Enterprise Architecture Foundation
-- Questa migrazione è documentale: la release introduce foundation tecnica.
-- Le tabelle operative restano quelle validate nelle versioni precedenti.

INSERT OR IGNORE INTO database_versions (version, description, applied_at)
VALUES ('5.6.0', 'Enterprise Architecture Foundation', datetime('now'));
