# .NET Coding Challenge
This is a coding challenge for .NET developers under the assumption that they are familiar with .NET Core, EF Core, MVC and REST.

## Overview

The objective of this challenge is to create an API that manages customers and their respective users.

## Requirements

1. You must use .NET Core (Version 8.0 or newer) and C#
2. The API must consume and return JSON data
3. The GET API should utilize the following OData parameters to query data 
    - $select
    - $filter
    - $top
    - $skip
    - $orderby
4. The data must be stored in a persistent database using EF Core and SQLite 
5. You do not need to implement Authorization/Authentication or consider any other security implications 
6. You are free to use any NuGet package if you can justify its usage

## Specification

The application must expose two RESTful API endpoints providing standard CRUD functionality for the **Customer** and **User** entities.

### Customer
#### URL Path
**GET**
```
/api/customer
/api/customer/{id}
```
**POST**
```
/api/customer
```
**PATCH**
```
/api/customer/{id}
```
**DELETE**
```
/api/customer/{id}
```

#### Data Format
| Field Name | Data Type | Required | Unique | Validation      |
|--|--|-------|--------|-----------------|
| Name | `string` | true  | true   | max length 255  |
| Website | `string` | false | false  | max length 255  |

#### Additional Information
- If a customer is deleted all associated users must also be deleted
- The customer response objects must not contain associated users
- Creating or updating a customer must not affect any user

### User
#### URL Path
**GET**
```
/api/customer/{id}/user
/api/customer/{id}/user/{id}
```
**POST**
```
/api/customer/{id}/user
```
**PATCH**
```
/api/customer/{id}/user/{id}
```
**DELETE**
```
/api/customer/{id}/user/{id}
```

#### Data Format
| Field Name    | Data Type  | Required | Unique | Validation                       |
|---------------|------------|----------|--------|----------------------------------|
| DisplayName   | `string`   | true     | true   | max length 255                   |
| FirstName     | `string`   | true     | false  | max length 255                   |
| LastName      | `string`   | true     | false  | max length 255                   |
| Email         | `string`   | true     | true   | typical email address validation |
| Date of Birth | `DateTime` | true     | false  |  |

#### Additional Information
- A user must always belong to a single customer
- The user response objects must only contain the associated customer's Id, no additional customer information

## Assessment Criteria

- The provided code should be near production quality
- The provided code should be testable
- The API should return plausible HTTP codes

We will not provide a strict time limit, but we recommend to spend between 2-6 hours. Please clone this repository and push your results to a private repo when you are ready to share your work with us. 

## Implementation

This repository implements the spec above using:

- **CodingChallenge.Data** — EF Core entities (`Customer`, `User`), the `CodingChallengeDbContext`, and migrations.
- **CodingChallenge.Api** — ASP.NET Core Web API with the CRUD controllers, request DTOs/validation, and OData query support (`Microsoft.AspNetCore.OData`) via `[EnableQuery]`.
- **CodingChallenge.Tests** — xUnit integration tests (`WebApplicationFactory` + in-memory SQLite) covering CRUD, validation, uniqueness conflicts, cascade delete, and OData behavior.

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download) (the projects target `net8.0`)

### How to run

```bash
# from the repository root
dotnet restore
dotnet run --project CodingChallenge.Api
```

The API applies any pending EF Core migrations automatically on startup (`db.Database.Migrate()` in `Program.cs`), creating `codingchallenge.db` (SQLite) next to the API project if it doesn't already exist — no manual migration step is required to just run the app.

Once running, Swagger UI is available at `http://localhost:5013/swagger` (or the HTTPS port from `launchSettings.json`).

### Running the tests

```bash
dotnet test
```

### Working with migrations

Migrations live in `CodingChallenge.Data/Migrations`. If you change an entity or the `OnModelCreating` configuration in `CodingChallengeDbContext`, regenerate them:

```bash
# one-time: install the EF Core CLI tool
dotnet tool install --global dotnet-ef

# add a new migration after changing the model
dotnet ef migrations add <MigrationName> \
  --project CodingChallenge.Data/CodingChallenge.Data.csproj \
  --startup-project CodingChallenge.Api/CodingChallenge.Api.csproj \
  -o Migrations

# apply migrations to the database manually (optional — the app also does this on startup)
dotnet ef database update \
  --project CodingChallenge.Data/CodingChallenge.Data.csproj \
  --startup-project CodingChallenge.Api/CodingChallenge.Api.csproj

# sanity-check that the model matches the latest migration
dotnet ef migrations has-pending-model-changes \
  --project CodingChallenge.Data/CodingChallenge.Data.csproj \
  --startup-project CodingChallenge.Api/CodingChallenge.Api.csproj
```

The `--project` flag points at the project containing the `DbContext`/migrations (`CodingChallenge.Data`); `--startup-project` points at the project that configures the actual provider and connection string (`CodingChallenge.Api`), since the two are separate projects here.

## API Reference

All examples below were run against a fresh database with the API started via:

```bash
dotnet run --project CodingChallenge.Api --urls http://localhost:5013
```

### Customer

**Create — `POST /api/customer`**

```bash
curl -i -X POST http://localhost:5013/api/customer \
  -H "Content-Type: application/json" \
  -d '{"name":"Acme Corp","webSite":"https://acme.example"}'
```

```
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: http://localhost:5013/api/Customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61

{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme.example"}
```

**List — `GET /api/customer`**

```bash
curl -i http://localhost:5013/api/customer
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme.example"}]
```

**Get one — `GET /api/customer/{id}`**

```bash
curl -i http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme.example"}
```

**Update (partial) — `PATCH /api/customer/{id}`**

Only the fields present in the body are changed; omitted fields keep their current value.

```bash
curl -i -X PATCH http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61 \
  -H "Content-Type: application/json" \
  -d '{"webSite":"https://acme-updated.example"}'
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme-updated.example"}
```

**Delete — `DELETE /api/customer/{id}`**

Deleting a customer cascades to all of its users.

```bash
curl -i -X DELETE http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61
```

```
HTTP/1.1 204 No Content
```

### User

Users are nested under their customer.

**Create — `POST /api/customer/{customerId}/user`**

```bash
curl -i -X POST http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/user \
  -H "Content-Type: application/json" \
  -d '{
        "displayName":"jdoe",
        "firstName":"John",
        "lastName":"Doe",
        "email":"john.doe@example.com",
        "dateOfBirth":"1990-01-01"
      }'
```

```
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: http://localhost:5013/api/Customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/User/f37a59ef-b3a6-498d-a28e-0693d4438f99

{"id":"f37a59ef-b3a6-498d-a28e-0693d4438f99","customerId":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","displayName":"jdoe","firstName":"John","lastName":"Doe","email":"john.doe@example.com","dateOfBirth":"1990-01-01T00:00:00"}
```

Note the response only includes the customer's `Id` (`customerId`), never a nested customer object.

**List — `GET /api/customer/{customerId}/user`**

```bash
curl -i http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/user
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{"id":"f37a59ef-b3a6-498d-a28e-0693d4438f99","customerId":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","displayName":"jdoe","firstName":"John","lastName":"Doe","email":"john.doe@example.com","dateOfBirth":"1990-01-01T00:00:00"}]
```

**Get one — `GET /api/customer/{customerId}/user/{id}`**

```bash
curl -i http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/user/f37a59ef-b3a6-498d-a28e-0693d4438f99
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"id":"f37a59ef-b3a6-498d-a28e-0693d4438f99","customerId":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","displayName":"jdoe","firstName":"John","lastName":"Doe","email":"john.doe@example.com","dateOfBirth":"1990-01-01T00:00:00"}
```

**Update (partial) — `PATCH /api/customer/{customerId}/user/{id}`**

```bash
curl -i -X PATCH http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/user/f37a59ef-b3a6-498d-a28e-0693d4438f99 \
  -H "Content-Type: application/json" \
  -d '{"lastName":"Smith"}'
```

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"id":"f37a59ef-b3a6-498d-a28e-0693d4438f99","customerId":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","displayName":"jdoe","firstName":"John","lastName":"Smith","email":"john.doe@example.com","dateOfBirth":"1990-01-01T00:00:00"}
```

**Delete — `DELETE /api/customer/{customerId}/user/{id}`**

```bash
curl -i -X DELETE http://localhost:5013/api/customer/24acc7bb-99f5-4b5c-b783-7255b24f0f61/user/f37a59ef-b3a6-498d-a28e-0693d4438f99
```

```
HTTP/1.1 204 No Content
```

### OData query options

The `GET` list endpoints (`/api/customer` and `/api/customer/{customerId}/user`) support `$select`, `$filter`, `$top`, `$skip`, and `$orderby`. Examples below assume four customers exist: `Acme Corp`, `Beta LLC`, `Contoso Inc`, `Acme Widgets`.

**`$select`**

```bash
curl -s "http://localhost:5013/api/customer?\$select=Name"
```

```json
[{"Name":"Acme Corp"},{"Name":"Beta LLC"},{"Name":"Contoso Inc"},{"Name":"Acme Widgets"}]
```

**`$filter`**

```bash
curl -s "http://localhost:5013/api/customer?\$filter=contains(Name,'Acme')"
```

```json
[{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme-updated.example"},{"id":"2d607b43-9951-45e6-b9d4-67751214e1ee","name":"Acme Widgets","webSite":null}]
```

**`$orderby`**

```bash
curl -s "http://localhost:5013/api/customer?\$orderby=Name%20desc"
```

```json
[{"id":"d38c761e-e85f-427a-8951-3f26b8ff9009","name":"Contoso Inc","webSite":null},{"id":"d91b6476-df14-4b02-8b6a-3e28738bf165","name":"Beta LLC","webSite":null},{"id":"2d607b43-9951-45e6-b9d4-67751214e1ee","name":"Acme Widgets","webSite":null},{"id":"24acc7bb-99f5-4b5c-b783-7255b24f0f61","name":"Acme Corp","webSite":"https://acme-updated.example"}]
```

**`$top` / `$skip`**

```bash
curl -s "http://localhost:5013/api/customer?\$orderby=Name&\$top=2&\$skip=1"
```

```json
[{"id":"2d607b43-9951-45e6-b9d4-67751214e1ee","name":"Acme Widgets","webSite":null},{"id":"d91b6476-df14-4b02-8b6a-3e28738bf165","name":"Beta LLC","webSite":null}]
```

### Error responses

**400 — validation failure** (e.g. missing required `Name`)

```bash
curl -i -X POST http://localhost:5013/api/customer \
  -H "Content-Type: application/json" \
  -d '{"webSite":"https://no-name.example"}'
```

```
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Name":["The Name field is required."]},"traceId":"00-0ceda25bed4bdbf8bd039ac6c3bac017-ebe9a6f525cc85c5-00"}
```

**409 — duplicate unique field** (e.g. `Name` already taken)

```bash
curl -i -X POST http://localhost:5013/api/customer \
  -H "Content-Type: application/json" \
  -d '{"name":"Acme Corp"}'
```

```
HTTP/1.1 409 Conflict
Content-Type: application/json; charset=utf-8

{"title":"A customer with this name already exists.","status":409}
```

**404 — resource not found**

```bash
curl -i http://localhost:5013/api/customer/00000000-0000-0000-0000-000000000000
```

```
HTTP/1.1 404 Not Found
Content-Type: application/json; charset=utf-8

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-feefa06486fb5265e196f68d010176b7-1d24053409135662-00"}
```

