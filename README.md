# LoanApp Backend (Loan Eligibility + Chat Assistant)

-----------------------------------------------------
**Author:** Atul Kharecha 
**Last updated:**31-Dec-2025
-----------------------------------------------------

A clean-architecture **.NET9** backend for a Loan Eligibility application. It exposes REST APIs to:

- Fetch **Loan Types** from the database.
- Evaluate **Loan Eligibility** based on configurable rules.
- Provide an optional **AI-powered Loan Assistant** (chat) via **Azure AI Foundry / Azure OpenAI**.

> Backend-only repository. A separate frontend (if any) is expected to call these APIs.

---

## Key Features

- **Loan Types**: query active loan types from SQL Server.
- **Eligibility rules in DB**: change rules without redeploying.
- **Consistent error responses**: RFC7807 `ProblemDetails`.
- **AI chat assistant (optional)**: config-driven integration with Azure OpenAI.

---

## Architecture

The solution follows a layered (Clean Architecture) layout:

- `LoanApp.Domain`
 - Core domain model / domain rules.
- `LoanApp.Application`
 - Use-cases, DTOs, application interfaces.
- `LoanApp.Infrastructure`
 - EF Core data access, query/service implementations, integrations.
- `LoanApp.Api`
 - ASP.NET Core Web API host, middleware, controllers, DI wiring.

---

## Tech Stack

- .NET9 / C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger / OpenAPI
- Microsoft Foundry (Azure AI Foundry) / Azure OpenAI for chat with own knowledge based with RAG

---

## Solution Structure

Projects:

- `LoanApp.Api` – Web API host
- `LoanApp.Infrastructure` – persistence & integrations
- `LoanApp.Application` – application layer
- `LoanApp.Domain` – domain layer

---

## Getting Started

### Prerequisites

- .NET SDK9.x
- SQL Server (LocalDB / SQL Express / full SQL Server)
- Azure OpenAI / Azure AI Foundry resource for chat

### Configuration

Configuration files:

- `LoanApp.Api/appsettings.json` – template defaults
- `LoanApp.Api/appsettings.Development.json` – local development overrides

Key settings:

- Database
 - `ConnectionStrings:LoanApp`
- CORS
 - `Cors:AllowedOrigins` (example: `http://localhost:5173`)
- Chat (Azure OpenAI)
 - `FoundryChat:Endpoint`
 - `FoundryChat:Deployment`
 - `FoundryChat:ApiKey`
 - `FoundryChat:SystemPrompt`
 - `FoundryChat:MaxHistoryMessages`
 - `FoundryChat:Temperature`

> Do not commit secrets. Prefer environment variables, user-secrets, or Key Vault.

### Database Setup

SQL Server schema script:

- `LoanApp.Infrastructure/Scripts/LoanApp.Database.sql`

The script is **re-runnable/idempotent** and creates:

- Database:
 - `LoanApp`
- Tables:
 - `dbo.LoanType`
 - `dbo.LoanEligibilityRule` (FK to `LoanType`)

Run it against your SQL Server instance and ensure `ConnectionStrings:LoanApp` points to the created database.

### Run the API

- Visual Studio: set `LoanApp.Api` as startup project and run
- CLI: run the `LoanApp.Api` project using `dotnet run`

In Development, Swagger is enabled.

### Swagger

When running in Development, open:

- `/swagger`

---

## API Behavior

### Error Handling (ProblemDetails)

Centralized exception handling is implemented in:

- `LoanApp.Api/Middleware/ApiExceptionHandlingMiddleware.cs`

Behavior:

- Returns `application/problem+json` (RFC7807)
- Includes `traceId` extension for correlation
- Includes exception details (`ProblemDetails.Detail`) only in Development
- Handles client-aborted requests as HTTP **499** (Client Closed Request)

### CORS

CORS uses a default policy with a single allowed origin:

- `Cors:AllowedOrigins`

---

## AI Chat Assistant (Azure OpenAI)

Chat is wired in `LoanApp.Api/Program.cs` via:

- `IChatSessionStore` ? `InMemoryChatSessionStore`
- `IChatService` ? `FoundryChatClient`

Notes:

- Session storage is **in-memory** by default (sessions reset on app restart). Require to maintain chat context (Configurable).
- Configuration is validated on startup (endpoint, deployment, and API key are required).

---

## Azure Setup (RAG / Knowledge Base)

This project can be used with Azure AI Foundry + Azure AI Search for a Retrieval-Augmented Generation (RAG) setup where the agent answers from your uploaded documents.

###1) Create resource group

- Create an Azure **Resource Group** (e.g. `rg-loanapp-dev`).

###2) Create Azure AI Foundry resources

- Create a **Microsoft Foundry / Azure AI Foundry** resource.
 - This typically creates a **Foundry resource** and a **Foundry project**.

###3) Create an agent and choose the model

- From the Foundry portal:
 - Create **Agent**
 - Select the **LLM model** deployment you want to use.

###4) Configure agent instructions (system prompt)

- Create a **System Prompt** under agent **Instructions** so the agent answers using the uploaded documents.

###5) Create a Storage Account (documents)

- Create an **Azure Storage Account**.
- Create a **Blob container** (example: `documents`).
- Upload your knowledge-base files (PDF/DOCX/TXT/etc.) to this container.

###6) Create Azure AI Search and import data

- Create an **Azure AI Search** service.
- Use **Import data**:
 - Data source: **Azure Blob Storage**
 - Scenario: **RAG**
 - Select your Storage Account + container

###7) Vectorize text using Foundry

- Under **Vectorize your text**:
 - Select **Kind** = `Azure AI Foundry (Microsoft Foundry)`

This will create:

- an **index**
- an **indexer**

Wait until indexing + vectorization completes.

###8) Reduce search payload (optional)

- In the created **index**, remove the field `text_vector` if you want to reduce embeddings returned in search results.

###9) Connect AI Search with Foundry knowledge

- In **Microsoft Foundry**:
 - Agent ? Knowledge ? Add ? set up a **Data source via tool**
 - Select **Azure AI Search**
 - Create a new connection using the Azure AI Search resource created above

---

## Data Model (SQL)

From `LoanApp.Infrastructure/Scripts/LoanApp.Database.sql`:

### `dbo.LoanType`

- `LoanTypeId` (PK, identity)
- `LoanTypeName` (nvarchar)
- `InterestRatePct` (decimal, nullable)
- `IsActive` (bit)
- `CreatedAt` (datetime2 UTC)

### `dbo.LoanEligibilityRule`

- `RuleId` (PK, identity)
- `LoanTypeId` (FK ? `LoanType`)
- `MinAge`, `MaxAge`
- `MinMonthlyIncome`
- `MinCreditScore` (nullable)
- `MaxEmiToIncomePct` (nullable)
- `IsActive` (bit)
- `CreatedAt` (datetime2 UTC)

---

## Development Notes

- EF Core SQL Server is configured in `LoanApp.Api/Program.cs`.
- Example query implementation:
 - `LoanApp.Infrastructure/Queries/LoanTypeQuery.cs`
 - Uses `AsNoTracking()` and projects to DTOs.

---

## Security Notes

Before publishing to a public repo:

- Remove real secrets from `appsettings.Development.json`.
- Use:
 - environment variables
 - `.NET user-secrets`
 - managed identity / Azure Key Vault (production)

---

## License

Appropriate for your intended usage.
