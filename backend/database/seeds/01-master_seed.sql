-- 1. Insert Users (Psicólogos)
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_psy_01', 'Dr.', 'Carlos Ruiz', 'carlos.ruiz@clinic.com', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_psy_02', 'Dra.', 'Marta Gómez', 'marta.gomez@clinic.com', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_psy_03', 'Dr.', 'Luis Fernandez', 'luis.fernandez@clinic.com', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_psy_04', 'Dra.', 'Elena Silva', 'elena.silva@clinic.com', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_psy_05', 'Dr.', 'Jorge Medina', 'jorge.medina@clinic.com', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;

-- 2. Insert Perfiles de Psicólogos
INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('psy_01', 'u_psy_01', 'Terapia Cognitivo-Conductual', 15) ON CONFLICT DO NOTHING;
INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('psy_02', 'u_psy_02', 'Terapia de Pareja y Familia', 8) ON CONFLICT DO NOTHING;
INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('psy_03', 'u_psy_03', 'Psicología Clínica y Trauma', 20) ON CONFLICT DO NOTHING;
INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('psy_04', 'u_psy_04', 'Manejo de Adicciones', 12) ON CONFLICT DO NOTHING;
INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('psy_05', 'u_psy_05', 'Psicología Infantil y Adolescente', 5) ON CONFLICT DO NOTHING;

-- 3. Insert Tenants (Las bases de datos)
INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('tenant_a_db_id', 'psy_01', 'app.com/tenant_a_db', 'tenant_a_db', 'ACTIVE') ON CONFLICT DO NOTHING;
INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('tenant_b_db_id', 'psy_02', 'app.com/tenant_b_db', 'tenant_b_db', 'ACTIVE') ON CONFLICT DO NOTHING;
INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('tenant_c_db_id', 'psy_03', 'app.com/tenant_c_db', 'tenant_c_db', 'ACTIVE') ON CONFLICT DO NOTHING;
INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('tenant_d_db_id', 'psy_04', 'app.com/tenant_d_db', 'tenant_d_db', 'ACTIVE') ON CONFLICT DO NOTHING;
INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('tenant_e_db_id', 'psy_05', 'app.com/tenant_e_db', 'tenant_e_db', 'ACTIVE') ON CONFLICT DO NOTHING;

-- 4. Insert Users (Pacientes)
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat1', 'Juan', 'Pérez', 'juan.perez@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat2', 'Ana', 'Lopez', 'ana.lopez@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat3', 'Luis', 'Martinez', 'luis.m@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat4', 'Sofia', 'Castro', 'sofia.castro@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat5', 'Roberto', 'Diaz', 'roberto.diaz@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat6', 'Diana', 'Ruiz', 'diana.ruiz@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat7', 'Carlos', 'Vargas', 'carlos.vargas@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat8', 'Maria', 'Gomez', 'maria.gomez@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat9', 'Pedro', 'Alonso', 'pedro.alonso@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat10', 'Lucas', 'Torres', 'lucas.torres@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat11', 'Valeria', 'Rios', 'valeria.rios@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;
INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_pat12', 'Mateo', 'Blanco', 'mateo.blanco@email.com', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;

-- 5. Insert Treatments Registry (Master)
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat1', 'psy_01', 'u_pat1') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat2', 'psy_01', 'u_pat2') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat3', 'psy_02', 'u_pat3') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat4', 'psy_02', 'u_pat4') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat5', 'psy_03', 'u_pat5') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat6', 'psy_03', 'u_pat6') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat7', 'psy_03', 'u_pat7') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat8', 'psy_04', 'u_pat8') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat9', 'psy_04', 'u_pat9') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat10', 'psy_05', 'u_pat10') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat11', 'psy_05', 'u_pat11') ON CONFLICT DO NOTHING;
INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_u_pat12', 'psy_05', 'u_pat12') ON CONFLICT DO NOTHING;
