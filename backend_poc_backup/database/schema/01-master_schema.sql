-- master_db.sql
CREATE TABLE users (
    id VARCHAR(100) PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_names VARCHAR(30) NOT NULL,
    email VARCHAR(60) NOT NULL UNIQUE,
    role VARCHAR(20) NOT NULL, 
    password VARCHAR(200) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE psychologist_applications (
    id VARCHAR(100) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(60) NOT NULL UNIQUE,
    license_number VARCHAR(50),
    status VARCHAR(20) DEFAULT 'PENDING',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE psychologists_profile (
    id VARCHAR(100) PRIMARY KEY,
    user_id VARCHAR(100) NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    specialty VARCHAR(45),
    profile_photo VARCHAR(255),
    years_experience INT,
    biography VARCHAR(255)
);

CREATE TABLE tenants (
    id VARCHAR(100) PRIMARY KEY,
    psychologist_id VARCHAR(100) NOT NULL REFERENCES psychologists_profile(id) ON DELETE CASCADE,
    domain VARCHAR(45),
    database_name VARCHAR(45) NOT NULL UNIQUE,
    state VARCHAR(20) DEFAULT 'ACTIVE'
);

CREATE TABLE treatments_registry (
    id VARCHAR(100) PRIMARY KEY,
    psychologist_id VARCHAR(100) NOT NULL REFERENCES psychologists_profile(id),
    patient_id VARCHAR(100) NOT NULL REFERENCES users(id),
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    finished_at TIMESTAMP,
    state VARCHAR(20) DEFAULT 'ACTIVE'
);
