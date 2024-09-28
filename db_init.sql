-- TO CREATE THIS DATABASE RUN: psql -h localhost -U josephlarrivy -d postgres -f db_init.sql
-- TO CONNECT TO PSQL IN THE TERMINAL AND VIEW THIS DATABASE RUN: psql -h localhost -U josephlarrivy -d gatewayprojectdotnetdatabase 

-- Connect to the default "postgres" database (or another database that is not the one you want to drop)
\c postgres

-- Drop the existing database
DROP DATABASE IF EXISTS gatewayprojectdotnetdatabase;

-- Recreate the database
CREATE DATABASE gatewayprojectdotnetdatabase;

-- Switch to the newly created database
\c gatewayprojectdotnetdatabase

-- Drop the Users table if it exists
DROP TABLE IF EXISTS users;

-- Create the Users table
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    HashedPassword VARCHAR(255),
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE LoginCodes (
    Id SERIAL PRIMARY KEY,
    Email VARCHAR(255) NOT NULL REFERENCES Users(Email) ON DELETE CASCADE,
    Code VARCHAR(8) NOT NULL,
    CodeType VARCHAR(8) NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);



-- Insert some initial data into the Users table
INSERT INTO users
    (Email, HashedPassword, FirstName, LastName)
    VALUES
    ('test@test.com','xxx', 'Test', 'User');