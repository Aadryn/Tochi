-- ============================================================================
-- Migration: Création du schéma routing pour YARP Dynamic Configuration
-- Date: 2025-01-XX
-- Description: Crée les tables pour stocker les routes et clusters YARP
--              dans PostgreSQL au lieu d'appsettings.json.
-- ============================================================================

-- Créer le schéma routing s'il n'existe pas
CREATE SCHEMA IF NOT EXISTS routing;

-- ============================================================================
-- Table: routing.proxy_clusters
-- Description: Stocke les clusters YARP (groupes de destinations backend)
-- ============================================================================
CREATE TABLE IF NOT EXISTS routing.proxy_clusters (
    id UUID PRIMARY KEY,
    cluster_id VARCHAR(100) NOT NULL,
    tenant_id UUID NOT NULL,
    load_balancing_policy VARCHAR(50) NOT NULL DEFAULT 'RoundRobin',
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Health Check Configuration
    health_check_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    health_check_interval DOUBLE PRECISION NOT NULL DEFAULT 30,
    health_check_timeout DOUBLE PRECISION NOT NULL DEFAULT 10,
    health_check_path VARCHAR(200) DEFAULT '/health',
    
    -- HTTP Client Configuration
    http_request_timeout DOUBLE PRECISION NOT NULL DEFAULT 100,
    http_max_connections_per_server INTEGER NOT NULL DEFAULT 100,
    http_enable_multiple_http2 BOOLEAN NOT NULL DEFAULT TRUE,
    http_ssl_protocols VARCHAR(50),
    
    -- Session Affinity Configuration
    session_affinity_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    session_affinity_policy VARCHAR(50) DEFAULT 'Cookie',
    session_affinity_failure_policy VARCHAR(50) DEFAULT 'Redistribute',
    session_affinity_key_name VARCHAR(100) DEFAULT '.Yarp.Affinity',
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT uq_proxy_clusters_cluster_id UNIQUE (cluster_id)
);

-- Index pour performance
CREATE INDEX IF NOT EXISTS ix_proxy_clusters_tenant_id 
    ON routing.proxy_clusters (tenant_id);

CREATE INDEX IF NOT EXISTS ix_proxy_clusters_is_enabled 
    ON routing.proxy_clusters (is_enabled) 
    WHERE is_enabled = TRUE;

-- ============================================================================
-- Table: routing.cluster_destinations
-- Description: Stocke les destinations (backends) des clusters
-- ============================================================================
CREATE TABLE IF NOT EXISTS routing.cluster_destinations (
    id UUID PRIMARY KEY,
    cluster_id UUID NOT NULL REFERENCES routing.proxy_clusters(id) ON DELETE CASCADE,
    destination_id VARCHAR(100) NOT NULL,
    address VARCHAR(500) NOT NULL,
    health VARCHAR(20) DEFAULT 'Unknown',
    weight INTEGER NOT NULL DEFAULT 1,
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    metadata JSONB,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT uq_cluster_destinations_cluster_destination 
        UNIQUE (cluster_id, destination_id)
);

-- Index pour performance
CREATE INDEX IF NOT EXISTS ix_cluster_destinations_cluster_id 
    ON routing.cluster_destinations (cluster_id);

CREATE INDEX IF NOT EXISTS ix_cluster_destinations_is_enabled 
    ON routing.cluster_destinations (is_enabled) 
    WHERE is_enabled = TRUE;

-- ============================================================================
-- Table: routing.proxy_routes
-- Description: Stocke les routes YARP (règles de correspondance et routage)
-- ============================================================================
CREATE TABLE IF NOT EXISTS routing.proxy_routes (
    id UUID PRIMARY KEY,
    route_id VARCHAR(100) NOT NULL,
    cluster_id VARCHAR(100) NOT NULL,
    match_path VARCHAR(500) NOT NULL,
    match_methods JSONB,
    match_headers JSONB,
    transforms JSONB,
    tenant_id UUID NOT NULL,
    "order" INTEGER NOT NULL DEFAULT 0,
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    authorization_policy VARCHAR(100),
    rate_limiter_policy VARCHAR(100),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT uq_proxy_routes_route_id UNIQUE (route_id)
);

-- Index pour performance
CREATE INDEX IF NOT EXISTS ix_proxy_routes_route_id 
    ON routing.proxy_routes (route_id);

CREATE INDEX IF NOT EXISTS ix_proxy_routes_cluster_id 
    ON routing.proxy_routes (cluster_id);

CREATE INDEX IF NOT EXISTS ix_proxy_routes_tenant_id 
    ON routing.proxy_routes (tenant_id);

CREATE INDEX IF NOT EXISTS ix_proxy_routes_tenant_enabled 
    ON routing.proxy_routes (tenant_id, is_enabled);

CREATE INDEX IF NOT EXISTS ix_proxy_routes_is_enabled 
    ON routing.proxy_routes (is_enabled) 
    WHERE is_enabled = TRUE;

-- ============================================================================
-- Données initiales: Migration des routes existantes (appsettings.json)
-- ============================================================================

-- Insérer le tenant par défaut (si nécessaire, à adapter selon votre tenant existant)
-- Cette section utilise un tenant_id fictif, à remplacer par le vrai ID

-- Cluster OpenAI
INSERT INTO routing.proxy_clusters (
    id, cluster_id, tenant_id, load_balancing_policy, 
    health_check_enabled, health_check_path
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    'openai-cluster',
    '00000000-0000-0000-0000-000000000001', -- Remplacer par vrai tenant_id
    'RoundRobin',
    TRUE,
    '/v1/models'
) ON CONFLICT (cluster_id) DO NOTHING;

INSERT INTO routing.cluster_destinations (
    id, cluster_id, destination_id, address
) VALUES (
    '21111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',
    'openai-primary',
    'https://api.openai.com'
) ON CONFLICT (cluster_id, destination_id) DO NOTHING;

-- Cluster Ollama
INSERT INTO routing.proxy_clusters (
    id, cluster_id, tenant_id, load_balancing_policy, 
    health_check_enabled, health_check_path
) VALUES (
    '22222222-2222-2222-2222-222222222222',
    'ollama-cluster',
    '00000000-0000-0000-0000-000000000001', -- Remplacer par vrai tenant_id
    'RoundRobin',
    TRUE,
    '/api/tags'
) ON CONFLICT (cluster_id) DO NOTHING;

INSERT INTO routing.cluster_destinations (
    id, cluster_id, destination_id, address
) VALUES (
    '32222222-2222-2222-2222-222222222222',
    '22222222-2222-2222-2222-222222222222',
    'ollama-local',
    'http://localhost:11434'
) ON CONFLICT (cluster_id, destination_id) DO NOTHING;

-- Cluster Anthropic
INSERT INTO routing.proxy_clusters (
    id, cluster_id, tenant_id, load_balancing_policy, 
    health_check_enabled, health_check_path
) VALUES (
    '33333333-3333-3333-3333-333333333333',
    'anthropic-cluster',
    '00000000-0000-0000-0000-000000000001', -- Remplacer par vrai tenant_id
    'RoundRobin',
    TRUE,
    '/v1/messages'
) ON CONFLICT (cluster_id) DO NOTHING;

INSERT INTO routing.cluster_destinations (
    id, cluster_id, destination_id, address
) VALUES (
    '43333333-3333-3333-3333-333333333333',
    '33333333-3333-3333-3333-333333333333',
    'anthropic-primary',
    'https://api.anthropic.com'
) ON CONFLICT (cluster_id, destination_id) DO NOTHING;

-- Routes
INSERT INTO routing.proxy_routes (
    id, route_id, cluster_id, match_path, tenant_id, "order"
) VALUES 
(
    '51111111-1111-1111-1111-111111111111',
    'openai-route',
    'openai-cluster',
    '/api/openai/{**catch-all}',
    '00000000-0000-0000-0000-000000000001',
    1
),
(
    '52222222-2222-2222-2222-222222222222',
    'ollama-route',
    'ollama-cluster',
    '/api/ollama/{**catch-all}',
    '00000000-0000-0000-0000-000000000001',
    2
),
(
    '53333333-3333-3333-3333-333333333333',
    'anthropic-route',
    'anthropic-cluster',
    '/api/anthropic/{**catch-all}',
    '00000000-0000-0000-0000-000000000001',
    3
)
ON CONFLICT (route_id) DO NOTHING;

-- ============================================================================
-- Commentaires sur les tables pour documentation
-- ============================================================================
COMMENT ON SCHEMA routing IS 'Schéma pour la configuration dynamique YARP';
COMMENT ON TABLE routing.proxy_clusters IS 'Clusters YARP - groupes de destinations backend';
COMMENT ON TABLE routing.cluster_destinations IS 'Destinations (backends) des clusters YARP';
COMMENT ON TABLE routing.proxy_routes IS 'Routes YARP - règles de correspondance et routage';

COMMENT ON COLUMN routing.proxy_clusters.cluster_id IS 'Identifiant métier YARP du cluster';
COMMENT ON COLUMN routing.proxy_clusters.load_balancing_policy IS 'Politique LB: RoundRobin, Random, LeastRequests, PowerOfTwoChoices';
COMMENT ON COLUMN routing.proxy_routes.route_id IS 'Identifiant métier YARP de la route';
COMMENT ON COLUMN routing.proxy_routes.match_path IS 'Pattern de correspondance du chemin (ex: /api/{**catch-all})';
