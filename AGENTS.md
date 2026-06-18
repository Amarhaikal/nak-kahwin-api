# Developer & Agent Guidelines (API)

## 💍 Business & Application Context: Nak Kahwin
**Nak Kahwin** (*"Want to Marry"* in Malay) is a collaborative wedding planning and tracking application designed for couples to plan their wedding details together.

---

## 🛠️ Technology Stack
* **Framework**: .NET 9.0 Web API (ASP.NET Core)
* **Database**: PostgreSQL (managed via Entity Framework Core & Npgsql)
* **Authentication**: JWT Bearer Tokens (15 min expiry) & Database-stored Refresh Tokens (7 days expiry, with rotation)
* **Password Hashing**: BCrypt.Net

---

## 📂 Architecture & Directory Structure
* [AppDbContext.cs](file:///Users/amarhaikal/Documents/coding/nak-kahwin/nak-kahwin-api/AppDbContext.cs): Database schema and model relationship mapping.
* [Controllers/](file:///Users/amarhaikal/Documents/coding/nak-kahwin/nak-kahwin-api/Controllers/): Controller layer handling request routing and HTTP responses.
* [Services/](file:///Users/amarhaikal/Documents/coding/nak-kahwin/nak-kahwin-api/Services/): Business logic layer (e.g., token generation, hashing, and database writes).
* [Models/](file:///Users/amarhaikal/Documents/coding/nak-kahwin/nak-kahwin-api/Models/): Database entity definitions.
* [DTOs/](file:///Users/amarhaikal/Documents/coding/nak-kahwin/nak-kahwin-api/DTOs/): Data Transfer Objects for clean request/response payloads.

---

## 🔐 Authentication Contracts

### Roles
Users must register with one of the following roles:
* `groom`
* `bride`

### API Endpoints
All auth requests are handled under route `/api/auth/`.

| Endpoint | Method | Authorization | Request Body | Response Body | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `/api/auth/register` | `POST` | Anonymous | `RegisterRequest` | `AuthResponse` | Registers a new user and returns a token pair. |
| `/api/auth/login` | `POST` | Anonymous | `LoginRequest` | `AuthResponse` | Authenticates a user and returns a token pair. |
| `/api/auth/refresh` | `POST` | Anonymous | `RefreshTokenRequest` | `AuthResponse` | Rotates/invalidates the old refresh token and returns a new pair. |
| `/api/auth/logout` | `POST` | JWT Bearer | `LogoutRequest` | `{ message: string }` | Revokes the provided refresh token. Requires valid authorization header. |

### Auth Payload Schemas

#### DTOs: Request Payloads
* `RegisterRequest`: `{ "name": "...", "email": "...", "password": "...", "role": "groom" | "bride" }`
* `LoginRequest`: `{ "email": "...", "password": "..." }`
* `RefreshTokenRequest` / `LogoutRequest`: `{ "refreshToken": "..." }`

#### DTOs: Response Payloads
* `AuthResponse`:
  ```json
  {
    "accessToken": "...",
    "refreshToken": "...",
    "userId": "...",
    "name": "...",
    "email": "...",
    "role": "groom" | "bride"
  }
  ```
