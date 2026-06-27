CREATE TABLE IF NOT EXISTS patients (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    clinical_context TEXT
);

CREATE TABLE IF NOT EXISTS journal_entries (
    id SERIAL PRIMARY KEY,
    patient_id INTEGER REFERENCES patients(id),
    status VARCHAR(50) DEFAULT 'processing',
    audio_s3_key VARCHAR(255) NOT NULL,
    transcription TEXT,
    ai_response_text TEXT,
    response_audio_s3_key VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indice critico para que el Frontend haga Polling muy rápido
CREATE INDEX idx_journal_status ON journal_entries (patient_id, status);

-- Datos iniciales de prueba para el MVP
INSERT INTO patients (name, clinical_context) VALUES ('Juan Perez', 'El paciente presenta estrés laboral y ansiedad leve. Tu rol como psicólogo es ser empático, recomendar calma e invitarlo a hacer ejercicios de respiración de 4 tiempos si su día fue agitado.');
