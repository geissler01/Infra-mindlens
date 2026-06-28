CREATE TABLE IF NOT EXISTS "patients" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "ClinicalContext" TEXT
);

CREATE TABLE IF NOT EXISTS "JournalEntries" (
    "Id" SERIAL PRIMARY KEY,
    "PacienteId" INTEGER,
    "Texto" TEXT,
    "Status" VARCHAR(50) DEFAULT 'processing',
    "Respuesta" TEXT
);

-- Indice critico para que el Frontend haga Polling muy rápido
CREATE INDEX idx_journal_status ON "JournalEntries" ("PacienteId", "Status");

-- Datos iniciales de prueba para el MVP
INSERT INTO "patients" ("Name", "ClinicalContext") VALUES ('Juan Perez', 'El paciente presenta estrés laboral y ansiedad leve. Tu rol como psicólogo es ser empático, recomendar calma e invitarlo a hacer ejercicios de respiración de 4 tiempos si su día fue agitado.');
