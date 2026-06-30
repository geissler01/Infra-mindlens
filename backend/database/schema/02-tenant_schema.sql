-- init_v2.sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE patients (
    id SERIAL PRIMARY KEY,
    global_user_id VARCHAR(100) NOT NULL UNIQUE, 
    phone VARCHAR(50),
    emergency_phone VARCHAR(50),
    address TEXT,
    -- Datos demográficos para mejor contexto de la IA y del psicólogo
    age_range VARCHAR(20),
    relationship_status VARCHAR(50),
    occupation_type VARCHAR(50),
    living_situation VARCHAR(50),
    primary_goal VARCHAR(100),
    has_previous_therapy BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE treatments (
    id SERIAL PRIMARY KEY,
    patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
    started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    finished_at TIMESTAMP,
    session_day VARCHAR(50),
    state VARCHAR(50) NOT NULL DEFAULT 'ACTIVE'
);

CREATE TABLE journalings (
    id SERIAL PRIMARY KEY,
    treatment_id INTEGER NOT NULL REFERENCES treatments(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    entry_type VARCHAR(20) DEFAULT 'audio',      -- Multimodal ('text' o 'audio')
    idempotency_key VARCHAR(255) UNIQUE,         -- Idempotencia SQS
    voice_record_url VARCHAR(2083),              
    transcription TEXT,                          
    ai_reply_url VARCHAR(2083),                  
    ai_reply_key VARCHAR(2083),
    ai_reply_text TEXT,                  
    is_emergency BOOLEAN DEFAULT FALSE,
    status VARCHAR(50) DEFAULT 'processing',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE questions (
    id SERIAL PRIMARY KEY,
    question TEXT NOT NULL,
    type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE treatments_questions (
    id SERIAL PRIMARY KEY,
    treatment_id INTEGER NOT NULL REFERENCES treatments(id) ON DELETE CASCADE,
    question_id INTEGER NOT NULL REFERENCES questions(id) ON DELETE CASCADE
);

CREATE TABLE journaling_answers (
    id SERIAL PRIMARY KEY,
    journaling_id INTEGER NOT NULL REFERENCES journalings(id) ON DELETE CASCADE,
    question_id INTEGER NOT NULL REFERENCES questions(id) ON DELETE RESTRICT,
    entry_type VARCHAR(20) DEFAULT 'audio',      -- Multimodalidad
    idempotency_key VARCHAR(255) UNIQUE,         -- Idempotencia SQS
    voice_record_url VARCHAR(2083),
    transcription TEXT,
    status VARCHAR(50) DEFAULT 'processing'
);

CREATE TABLE journaling_register (
    id SERIAL PRIMARY KEY,
    journaling_id INTEGER NOT NULL REFERENCES journalings(id) ON DELETE CASCADE,
    pillar_type VARCHAR(50) NOT NULL, 
    analyzed_content TEXT NOT NULL,
    severity_score INTEGER,
    confidence_score NUMERIC(3,2),
    embedding vector(1536) NOT NULL 
);

CREATE TABLE weekly_reports (
    id SERIAL PRIMARY KEY,
    treatment_id INTEGER NOT NULL REFERENCES treatments(id) ON DELETE CASCADE,
    summary TEXT NOT NULL,
    started_at DATE NOT NULL,
    finished_at DATE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE weekly_cluster_reports (
    id SERIAL PRIMARY KEY,
    weekly_report_id INTEGER NOT NULL REFERENCES weekly_reports(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    type VARCHAR(50) NOT NULL,
    repetitions INTEGER NOT NULL DEFAULT 1,
    cluster_embedding vector(1536) NOT NULL
);

CREATE TABLE pre_session_reports (
    id SERIAL PRIMARY KEY,
    treatment_id INTEGER NOT NULL REFERENCES treatments(id) ON DELETE CASCADE,
    flash_briefing TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_register_embedding_hnsw ON journaling_register USING hnsw (embedding vector_cosine_ops);
CREATE INDEX idx_cluster_embedding_hnsw ON weekly_cluster_reports USING hnsw (cluster_embedding vector_cosine_ops);
