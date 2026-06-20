-- =====================================================================
-- Enterprise Monitoring Platform — PostgreSQL Schema
-- Tables: users, roles, user_roles, alerts, audit_logs
-- =====================================================================

-- Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Dedicated schema
CREATE SCHEMA IF NOT EXISTS app;
SET search_path TO app;

-- ---------------------------------------------------------------------
-- ENUM TYPES
-- ---------------------------------------------------------------------
CREATE TYPE alert_severity AS ENUM ('info', 'warning', 'error', 'critical');
CREATE TYPE alert_status   AS ENUM ('firing', 'acknowledged', 'resolved', 'silenced');
CREATE TYPE audit_action   AS ENUM ('create', 'read', 'update', 'delete', 'login', 'logout');

-- ---------------------------------------------------------------------
-- ROLES
-- ---------------------------------------------------------------------
CREATE TABLE roles (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name          VARCHAR(50)  NOT NULL UNIQUE,
    description   VARCHAR(255),
    permissions   JSONB        NOT NULL DEFAULT '[]'::jsonb,
    is_system     BOOLEAN      NOT NULL DEFAULT FALSE,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- USERS
-- ---------------------------------------------------------------------
CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email           VARCHAR(255) NOT NULL UNIQUE,
    username        VARCHAR(100) NOT NULL UNIQUE,
    password_hash   TEXT         NOT NULL,
    full_name       VARCHAR(150),
    is_active       BOOLEAN      NOT NULL DEFAULT TRUE,
    is_email_verified BOOLEAN    NOT NULL DEFAULT FALSE,
    last_login_at   TIMESTAMPTZ,
    failed_attempts SMALLINT     NOT NULL DEFAULT 0,
    locked_until    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE INDEX idx_users_email      ON users (email);
CREATE INDEX idx_users_username  ON users (username);
CREATE INDEX idx_users_is_active ON users (is_active);

-- ---------------------------------------------------------------------
-- USER ↔ ROLE (many-to-many)
-- ---------------------------------------------------------------------
CREATE TABLE user_roles (
    user_id     UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    role_id     UUID NOT NULL REFERENCES roles (id) ON DELETE CASCADE,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    assigned_by UUID REFERENCES users (id) ON DELETE SET NULL,
    PRIMARY KEY (user_id, role_id)
);

CREATE INDEX idx_user_roles_role ON user_roles (role_id);

-- ---------------------------------------------------------------------
-- ALERTS
-- ---------------------------------------------------------------------
CREATE TABLE alerts (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title           VARCHAR(255)   NOT NULL,
    description     TEXT,
    severity        alert_severity NOT NULL DEFAULT 'info',
    status          alert_status   NOT NULL DEFAULT 'firing',
    source          VARCHAR(150),                 -- e.g. service/host name
    labels          JSONB          NOT NULL DEFAULT '{}'::jsonb,
    fingerprint     VARCHAR(128),                 -- dedup key from Alertmanager
    fired_at        TIMESTAMPTZ    NOT NULL DEFAULT now(),
    acknowledged_at TIMESTAMPTZ,
    acknowledged_by UUID REFERENCES users (id) ON DELETE SET NULL,
    resolved_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ    NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ    NOT NULL DEFAULT now()
);

CREATE INDEX idx_alerts_status      ON alerts (status);
CREATE INDEX idx_alerts_severity    ON alerts (severity);
CREATE INDEX idx_alerts_fired_at    ON alerts (fired_at DESC);
CREATE INDEX idx_alerts_fingerprint ON alerts (fingerprint);
CREATE INDEX idx_alerts_labels_gin  ON alerts USING GIN (labels);

-- ---------------------------------------------------------------------
-- AUDIT LOGS
-- ---------------------------------------------------------------------
CREATE TABLE audit_logs (
    id             BIGSERIAL PRIMARY KEY,
    user_id        UUID REFERENCES users (id) ON DELETE SET NULL,
    action         audit_action NOT NULL,
    entity_type    VARCHAR(100),                  -- e.g. 'alert', 'user'
    entity_id      VARCHAR(100),
    old_values     JSONB,
    new_values     JSONB,
    ip_address     INET,
    user_agent     TEXT,
    correlation_id UUID,                          -- trace correlation w/ OTel
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_audit_user_id    ON audit_logs (user_id);
CREATE INDEX idx_audit_action     ON audit_logs (action);
CREATE INDEX idx_audit_entity     ON audit_logs (entity_type, entity_id);
CREATE INDEX idx_audit_created_at ON audit_logs (created_at DESC);
CREATE INDEX idx_audit_correlation ON audit_logs (correlation_id);

-- ---------------------------------------------------------------------
-- TRIGGER: auto-update updated_at
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated   BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_roles_updated   BEFORE UPDATE ON roles
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_alerts_updated  BEFORE UPDATE ON alerts
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ---------------------------------------------------------------------
-- SEED DATA (system roles)
-- ---------------------------------------------------------------------
INSERT INTO roles (name, description, permissions, is_system) VALUES
    ('SuperAdmin', 'Full system access',
        '["*"]'::jsonb, TRUE),
    ('Admin', 'Manage users, alerts, and config',
        '["users:*","alerts:*","audit:read"]'::jsonb, TRUE),
    ('Operator', 'Acknowledge and resolve alerts',
        '["alerts:read","alerts:update"]'::jsonb, TRUE),
    ('Viewer', 'Read-only dashboard access',
        '["dashboards:read","alerts:read"]'::jsonb, TRUE);
