-- 03_privileges_development.sql
-- On se place dans la base « development »
\connect development

CREATE EXTENSION IF NOT EXISTS pg_trgm;
-- 0) (Optionnel PG14-) Si besoin de reprendre la main sur public (PG ≤ 14)
--    En PG15+, ceci est généralement inutile.
-- ALTER SCHEMA public OWNER TO supervisor;

-- 1) Autoriser l’utilisateur application à se connecter
GRANT CONNECT ON DATABASE development TO application;

-- 2) Sécuriser le schéma public :
--    Par défaut, tout le monde peut CREATE dans public : on le retire.
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
--    On garde l'accès (USAGE) pour tous si tu veux que la résolution de noms fonctionne,
--    sinon révoque aussi USAGE et attribue-le explicitement.
GRANT USAGE ON SCHEMA public TO PUBLIC;

-- 3) Donner à supervisor les droits attendus (il est owner de la DB)
GRANT ALL PRIVILEGES ON DATABASE development TO supervisor;
-- (Souvent superflu si supervisor est owner, mais inoffensif)

-- 4) Droits sur les schémas EXISTANTS (hors systèmes)

GRANT USAGE ON SCHEMA public TO application;
GRANT USAGE ON SCHEMA public TO supervisor;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO application;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO application;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO supervisor;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO supervisor;
ALTER DEFAULT PRIVILEGES FOR ROLE supervisor IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO application;
ALTER DEFAULT PRIVILEGES FOR ROLE supervisor IN SCHEMA public GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO application;
ALTER DEFAULT PRIVILEGES FOR ROLE application IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO application;
ALTER DEFAULT PRIVILEGES FOR ROLE application IN SCHEMA public GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO application;
ALTER DEFAULT PRIVILEGES FOR ROLE supervisor IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO supervisor;
ALTER DEFAULT PRIVILEGES FOR ROLE supervisor IN SCHEMA public GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO supervisor;
ALTER DEFAULT PRIVILEGES FOR ROLE application IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO supervisor;
ALTER DEFAULT PRIVILEGES FOR ROLE application IN SCHEMA public GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO supervisor;
-- Si fonctions créées à l’avenir :
ALTER DEFAULT PRIVILEGES FOR ROLE supervisor IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO application;
ALTER DEFAULT PRIVILEGES FOR ROLE application IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO application;
