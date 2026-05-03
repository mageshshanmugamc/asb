USE master;
GO

IF DB_ID(N'asbdb') IS NULL
    CREATE DATABASE asbdb;
GO

IF DB_ID(N'keycloakdb') IS NULL
    CREATE DATABASE keycloakdb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'banking_user')
BEGIN
    CREATE LOGIN banking_user WITH PASSWORD = N'BankingUser@123', CHECK_POLICY = OFF;
END
ELSE
BEGIN
    ALTER LOGIN banking_user WITH PASSWORD = N'BankingUser@123';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'keycloak_user')
BEGIN
    CREATE LOGIN keycloak_user WITH PASSWORD = N'KeycloakUser@123', CHECK_POLICY = OFF;
END
ELSE
BEGIN
    ALTER LOGIN keycloak_user WITH PASSWORD = N'KeycloakUser@123';
END
GO

USE asbdb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'banking_user')
    CREATE USER banking_user FOR LOGIN banking_user;

IF IS_ROLEMEMBER(N'db_owner', N'banking_user') <> 1
    ALTER ROLE db_owner ADD MEMBER banking_user;
GO

USE keycloakdb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'keycloak_user')
    CREATE USER keycloak_user FOR LOGIN keycloak_user;

IF IS_ROLEMEMBER(N'db_owner', N'keycloak_user') <> 1
    ALTER ROLE db_owner ADD MEMBER keycloak_user;
GO