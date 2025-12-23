-- 04_privileges_keycloak.sql
-- on se place dans la base « keycloak »
\connect keycloak
-- Le propriétaire keycloak dispose déjà de tous les droits sur la base,
-- mais on s’assure qu’il possède également le schéma public.
ALTER SCHEMA public OWNER TO keycloak;
