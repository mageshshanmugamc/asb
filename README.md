# ASB — Admin Security & Banking Platform

A full-stack RBAC (Role-Based Access Control) platform built with **.NET 10**, **React 19**, **Keycloak**, and **SQL Server**, fully containerized with Docker Compose.

## Architecture

```
┌────────────┐       ┌───────────────────────────────────────────────────┐
│  Browser   │──────▶│  Nginx (port 5000)                               │
└────────────┘       │    /           → React UI (asb-ui:80)            │
                     │    /api/       → .NET API  (asb-api:8080)        │
                     │    /api/swagger/→ Swagger UI                     │
                     │    /keycloak/  → Keycloak  (keycloak:8080)       │
                     └───────────────────────────────────────────────────┘
                                          │
                     ┌────────────────────┼────────────────────┐
                     ▼                    ▼                    ▼
              ┌─────────────┐    ┌──────────────┐    ┌──────────────┐
              │  React UI   │    │  .NET 10 API │    │  Keycloak    │
              │  Port 3001  │    │  Port 4000   │    │  Port 8080   │
              └─────────────┘    └──────┬───────┘    └──────┬───────┘
                                        │                   │
                                        ▼                   ▼
                                 ┌──────────────────────────────┐
                                 │  SQL Server 2022 (port 1433) │
                                 │   ├─ asbdb                   │
                                 │   └─ keycloakdb              │
                                 └──────────────────────────────┘
```

## Tech Stack

| Layer        | Technology                                     |
| ------------ | ---------------------------------------------- |
| Frontend     | React 19, TypeScript 4.9, RxJS 7, react-router-dom 7 |
| Backend      | .NET 10 Web API, Entity Framework Core, Serilog |
| Auth         | Keycloak 26.1 (OIDC) + App JWT (HMAC-SHA256)  |
| Database     | SQL Server 2022                                |
| Proxy        | Nginx                                          |
| Containers   | Docker Compose                                 |

## Project Structure

```
ASB/
├── ASB.Admin/              # Web API host (controllers, migrations, Dockerfile)
│   └── v1/
│       ├── Controllers/    # AuthController, UserController, UserGroupController
│       ├── Infrastructure/ # Keycloak admin service
│       ├── Models/         # API models
│       ├── Requests/       # Request DTOs
│       └── Response/       # Response mappers
├── ASB.Services/           # Business logic layer
│   └── v1/
│       ├── Dtos/           # Data transfer objects
│       ├── Implementations/
│       └── Interfaces/
├── ASB.Repositories/       # Data access layer (EF Core)
│   └── v1/
│       ├── Contexts/       # AsbContext (DbContext)
│       ├── Entities/       # User, Role, Menu, Policy, UserGroup, etc.
│       ├── Implementations/
│       └── Interfaces/
├── ASB.ErrorHandler/       # Global error handling middleware
├── ASB.Authorization/      # Authorization helpers
├── sample-react-app/       # React SPA
│   └── src/
│       ├── auth/           # OIDC config, AppAuthContext (token exchange)
│       ├── components/     # Header, Sidebar, DataGrid, pages
│       └── services/       # api.ts, user.service.ts, auth.service.ts
├── nginx/                  # Nginx reverse proxy config
├── docker-compose.yaml
└── sql_init.sql
```

## RBAC Model

```
User ──▶ UserGroup ──▶ Role ──▶ Policy
                         │
                         └──▶ Menu (with permissions)
```

- **Users** are assigned to **User Groups**
- User Groups have **Roles**
- Roles grant access to **Menus** (with read/write/delete permissions) and **Policies**
- Menus are hierarchical (parent → children) and drive the sidebar + route guarding

## Authentication Flow

1. User authenticates via **Keycloak** (OIDC Authorization Code flow)
2. React app receives a Keycloak access token
3. React calls `POST /api/v1/Auth/token` (token exchange) with the Keycloak token
4. Backend looks up the user, resolves roles/menus, and issues an **app JWT** (HMAC-SHA256)
5. App JWT is stored in `sessionStorage` and used for all subsequent API calls
6. Sidebar and routes are dynamically rendered from the menus embedded in the JWT

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for local development)
- [Node.js 18+](https://nodejs.org/) (for local frontend development)

## Quick Start

### 1. Build Docker Images

```bash
# From the repo root
docker build -t asb-api:latest -f ASB.Admin/Dockerfile .

# From the React app folder
cd sample-react-app
docker build -t asb-ui:latest .
cd ..
```

### 2. Start All Services

```bash
docker-compose up -d
```

This starts:

| Service        | Container      | Port  |
| -------------- | -------------- | ----- |
| SQL Server     | asb-sqlserver  | 1433  |
| SQL Init       | sql-init       | —     |
| Keycloak       | keycloak       | 8080  |
| .NET API       | asb-api        | 4000  |
| React UI       | asb-ui         | 3001  |
| Nginx Proxy    | nginx          | 5000  |

The `sql-init` container waits for SQL Server health, creates the `asbdb` and `keycloakdb` databases, and provisions database logins/users before Keycloak and the API start.

### 3. Access the App

| URL                              | Description          |
| -------------------------------- | -------------------- |
| http://localhost:5000            | App (via Nginx)      |
| http://localhost:5000/api/swagger/ | Swagger UI         |
| http://localhost:5000/keycloak/  | Keycloak Admin       |

### 4. Keycloak Setup

1. Log in to Keycloak admin at `http://localhost:5000/keycloak/` with `admin` / `admin`
2. Create realm: `asb-keycloak`
3. Create client: `asb-oidc-provider` (public, authorization code flow)
4. Create users as needed

## Local Development

### Backend

```bash
cd ASB.Admin
dotnet restore
dotnet build
dotnet run
```

The API runs on `http://localhost:5113` by default (see `Properties/launchSettings.json`).

### Frontend

```bash
cd sample-react-app
npm install --legacy-peer-deps
npm start
```

The React dev server runs on `http://localhost:3000`.

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project ASB.Admin

# Apply migrations
dotnet ef database update --project ASB.Admin
```

## API Endpoints

### Auth

| Method | Endpoint              | Auth | Description                         |
| ------ | --------------------- | ---- | ----------------------------------- |
| POST   | `/api/v1/Auth/token`  | No   | Token exchange (Keycloak → app JWT) |

### Users

| Method | Endpoint                             | Auth | Description              |
| ------ | ------------------------------------ | ---- | ------------------------ |
| GET    | `/api/v1/User`                       | Yes  | List users               |
| POST   | `/api/v1/User`                       | Yes  | Create user              |
| POST   | `/api/v1/User/{id}/groups/{groupId}` | Yes  | Assign user to group     |
| GET    | `/api/v1/User/keycloak/lookup?email=`| Yes  | Lookup user in Keycloak  |

### User Groups

| Method | Endpoint                                    | Auth | Description              |
| ------ | ------------------------------------------- | ---- | ------------------------ |
| GET    | `/api/v1/UserGroup`                         | Yes  | List user groups         |
| POST   | `/api/v1/UserGroup`                         | Yes  | Create user group        |
| POST   | `/api/v1/UserGroup/{id}/roles`              | Yes  | Assign role to group     |

## Environment Variables

Key variables configured in `docker-compose.yaml`:

| Variable                          | Description                       |
| --------------------------------- | --------------------------------- |
| `ConnectionStrings__AsbDatabase`  | SQL Server connection string      |
| `Jwt__Secret`                     | Symmetric key for app JWT signing |
| `Keycloak__Authority`             | Keycloak realm URL                |
| `Keycloak__ClientId`              | OIDC client ID                    |
| `Keycloak__AdminBaseUrl`          | Keycloak admin API base URL       |
| `Keycloak__AdminRealm`            | Keycloak realm for admin queries  |
| `Keycloak__AdminUsername`         | Keycloak admin username           |
| `Keycloak__AdminPassword`         | Keycloak admin password           |

## Entity Model

| Entity             | Description                              |
| ------------------ | ---------------------------------------- |
| `User`             | System user (username, email)            |
| `UserGroup`        | Logical grouping of users                |
| `UserGroupMapping` | Many-to-many: User ↔ UserGroup           |
| `Role`             | Permission role                          |
| `UserGroupRole`    | Many-to-many: UserGroup ↔ Role           |
| `Menu`             | Hierarchical navigation item             |
| `RoleMenuPermission` | Maps roles to menus with permissions   |
| `Policy`           | Fine-grained policy                      |
| `RolePolicy`       | Many-to-many: Role ↔ Policy              |
