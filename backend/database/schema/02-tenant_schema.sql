CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE "Patients" (
    "Id" uuid NOT NULL,
    "GlobalUserId" uuid NOT NULL,
    "Phone" text NOT NULL,
    "EmergencyPhone" text NOT NULL,
    "Address" text NOT NULL,
    "AgeRange" text NOT NULL,
    "RelationshipStatus" text NOT NULL,
    "Occupation" text NOT NULL,
    "LivingSituation" text NOT NULL,
    "PrimaryGoal" text NOT NULL,
    "HasPreviousTherapy" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Patients" PRIMARY KEY ("Id")
);

CREATE TABLE "Questions" (
    "Id" uuid NOT NULL,
    "Message" text NOT NULL,
    pillar_type text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Questions" PRIMARY KEY ("Id")
);

CREATE TABLE "Treatments" (
    "Id" uuid NOT NULL,
    "StartedAt" date NOT NULL DEFAULT (CURRENT_DATE),
    "FinishedAt" date,
    "SessionDay" text NOT NULL,
    "State" text NOT NULL DEFAULT 'InProcess',
    "PatientId" uuid NOT NULL,
    CONSTRAINT "PK_Treatments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Treatments_Patients_PatientId" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Journalings" (
    "Id" uuid NOT NULL,
    "Date" date NOT NULL,
    "EntryType" text NOT NULL DEFAULT 'Text',
    "IdempotencyKey" text,
    "Transcription" text,
    "VoiceRecordKey" text,
    "AiReplyKey" text,
    "AiReplyText" text,
    "State" text NOT NULL DEFAULT 'Pending',
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "TreatmentId" uuid NOT NULL,
    CONSTRAINT "PK_Journalings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Journalings_Treatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "Treatments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Notes" (
    "Id" uuid NOT NULL,
    "Message" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "TreatmentId" uuid NOT NULL,
    CONSTRAINT "PK_Notes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notes_Treatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "Treatments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PresessionReports" (
    "Id" uuid NOT NULL,
    "FlashBriefing" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "TreatmentId" uuid NOT NULL,
    CONSTRAINT "PK_PresessionReports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PresessionReports_Treatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "Treatments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "TreatmentQuestions" (
    "Id" uuid NOT NULL,
    "TreatmentId" uuid NOT NULL,
    "QuestionId" uuid NOT NULL,
    CONSTRAINT "PK_TreatmentQuestions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TreatmentQuestions_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TreatmentQuestions_Treatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "Treatments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "WeeklyReports" (
    "Id" uuid NOT NULL,
    "Summary" text NOT NULL,
    "StartedAt" date NOT NULL,
    "FinishedAt" date NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "TreatmentId" uuid NOT NULL,
    CONSTRAINT "PK_WeeklyReports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WeeklyReports_Treatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "Treatments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "JournalingAnswers" (
    "Id" uuid NOT NULL,
    "EntryType" text NOT NULL DEFAULT 'Text',
    "IdempotencyKey" text,
    "VoiceRecordUrl" text,
    "Transcription" text,
    "State" text NOT NULL DEFAULT 'Pending',
    "JournalingId" uuid NOT NULL,
    "QuestionId" uuid NOT NULL,
    CONSTRAINT "PK_JournalingAnswers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_JournalingAnswers_Journalings_JournalingId" FOREIGN KEY ("JournalingId") REFERENCES "Journalings" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_JournalingAnswers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "JournalingRegisters" (
    "Id" uuid NOT NULL,
    pillar_type text NOT NULL,
    "AnalyzedContent" text NOT NULL,
    "Embedding" vector(1536) NOT NULL,
    "JournalingId" uuid NOT NULL,
    CONSTRAINT "PK_JournalingRegisters" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_JournalingRegisters_Journalings_JournalingId" FOREIGN KEY ("JournalingId") REFERENCES "Journalings" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "WeeklyClusterReports" (
    "Id" uuid NOT NULL,
    "Title" text NOT NULL,
    pillar_type text NOT NULL,
    "Repetitions" integer NOT NULL,
    "ClusterEmbedding" vector(1536) NOT NULL,
    "WeeklyReportId" uuid NOT NULL,
    CONSTRAINT "PK_WeeklyClusterReports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WeeklyClusterReports_WeeklyReports_WeeklyReportId" FOREIGN KEY ("WeeklyReportId") REFERENCES "WeeklyReports" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_JournalingAnswers_JournalingId" ON "JournalingAnswers" ("JournalingId");

CREATE INDEX "IX_JournalingAnswers_QuestionId" ON "JournalingAnswers" ("QuestionId");

CREATE INDEX "IX_JournalingRegisters_Embedding" ON "JournalingRegisters" USING hnsw ("Embedding" vector_cosine_ops);

CREATE INDEX "IX_JournalingRegisters_JournalingId" ON "JournalingRegisters" ("JournalingId");

CREATE INDEX "IX_Journalings_TreatmentId" ON "Journalings" ("TreatmentId");

CREATE INDEX "IX_Notes_TreatmentId" ON "Notes" ("TreatmentId");

CREATE UNIQUE INDEX "IX_PresessionReports_TreatmentId" ON "PresessionReports" ("TreatmentId");

CREATE INDEX "IX_TreatmentQuestions_QuestionId" ON "TreatmentQuestions" ("QuestionId");

CREATE INDEX "IX_TreatmentQuestions_TreatmentId" ON "TreatmentQuestions" ("TreatmentId");

CREATE INDEX "IX_Treatments_PatientId" ON "Treatments" ("PatientId");

CREATE INDEX "IX_WeeklyClusterReports_ClusterEmbedding" ON "WeeklyClusterReports" USING hnsw ("ClusterEmbedding" vector_cosine_ops);

CREATE INDEX "IX_WeeklyClusterReports_WeeklyReportId" ON "WeeklyClusterReports" ("WeeklyReportId");

CREATE INDEX "IX_WeeklyReports_TreatmentId" ON "WeeklyReports" ("TreatmentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260630105929_InitialTenant', '10.0.9');

COMMIT;

