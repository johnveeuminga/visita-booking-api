# Visita Booking API - Authentication Module

## Overview

This authentication module provides a comprehensive user authentication system supporting three user types (Guest, Hotel, Admin) with Role-Based Access Control (RBAC). It supports both email/password authentication and Google OAuth 2.0/OIDC 1.0.

## Features

- **Multiple Authentication Methods**:
  - Email/Password authentication
  - Google OAuth 2.0/OIDC 1.0 integration
- **Role-Based Access Control (RBAC)**:
  - Guest: Regular users/customers
  - Hotel: Hotel owners/managers
  - Admin: System administrators
- **JWT Token Authentication**:
  - Access tokens (short-lived)
  - Refresh tokens (long-lived)
  - Automatic token refresh
- **Security Features**:
  - Password hashing with BCrypt
  - Token-based authentication
  - Role-based authorization policies
  - Session management

## Database Schema

### Core Tables

1. **Users** - Main user table
2. **Roles** - Available roles (Guest, Hotel, Admin)
3. **UserRoles** - Junction table for user-role assignments
4. **RefreshTokens** - Token management
5. **UserSessions** - Session tracking

## Authentication Endpoints

### Base URL: `/api/auth`

### 1. Health Check

**GET** `/api/auth/health`

- **Description**: Check authentication service status
- **Authentication**: None required
- **Response**: `200 OK`

```json
{
  "success": true,
  "message": "Authentication service is healthy.",
  "data": "OK"
}
```

### 2. User Registration

**POST** `/api/auth/register`

- **Description**: Register a new user account
- **Authentication**: None required
- **Request Body**:

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "confirmPassword": "SecurePassword123",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Guest"
}
```

- **Response**: `200 OK` or `400 Bad Request`

```json
{
  "success": true,
  "message": "Registration successful.",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "provider": "Local",
    "isEmailVerified": false,
    "isActive": true,
    "roles": ["Guest"],
    "createdAt": "2025-01-01T00:00:00Z",
    "lastLoginAt": null
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenExpiry": "2025-01-01T01:00:00Z"
}
```

### 3. User Login

**POST** `/api/auth/login`

- **Description**: Authenticate user with email and password
- **Authentication**: None required
- **Request Body**:

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "rememberMe": false
}
```

- **Response**: `200 OK` or `401 Unauthorized`

```json
{
  "success": true,
  "message": "Login successful.",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "provider": "Local",
    "isEmailVerified": true,
    "isActive": true,
    "roles": ["Guest"],
    "createdAt": "2025-01-01T00:00:00Z",
    "lastLoginAt": "2025-01-01T12:00:00Z"
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenExpiry": "2025-01-01T01:00:00Z"
}
```

### 4. Google Authentication

**POST** `/api/auth/google`

- **Description**: Authenticate user with Google ID token
- **Authentication**: None required
- **Request Body**:

```json
{
  "idToken": "google-id-token-here",
  "role": "Guest"
}
```

- **Response**: `200 OK` or `400 Bad Request`

```json
{
  "success": true,
  "message": "Google authentication successful.",
  "user": {
    "id": 2,
    "email": "user@gmail.com",
    "firstName": "Jane",
    "lastName": "Doe",
    "fullName": "Jane Doe",
    "provider": "Google",
    "isEmailVerified": true,
    "isActive": true,
    "roles": ["Guest"],
    "createdAt": "2025-01-01T00:00:00Z",
    "lastLoginAt": "2025-01-01T12:00:00Z"
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenExpiry": "2025-01-01T01:00:00Z"
}
```

### 5. Token Refresh

**POST** `/api/auth/refresh`

- **Description**: Get new access token using refresh token
- **Authentication**: None required
- **Request Body**:

```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

- **Response**: `200 OK` or `401 Unauthorized`

```json
{
  "success": true,
  "message": "Token refreshed successfully.",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "provider": "Local",
    "isEmailVerified": true,
    "isActive": true,
    "roles": ["Guest"],
    "createdAt": "2025-01-01T00:00:00Z",
    "lastLoginAt": "2025-01-01T12:00:00Z"
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-base64-encoded-refresh-token",
  "tokenExpiry": "2025-01-01T01:00:00Z"
}
```

### 6. Logout

**POST** `/api/auth/logout`

- **Description**: Logout user and revoke refresh token
- **Authentication**: None required
- **Request Body**:

```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

- **Response**: `200 OK`

```json
{
  "success": true,
  "message": "Logged out successfully.",
  "data": true
}
```

### 7. Revoke All Tokens

**POST** `/api/auth/revoke-all`

- **Description**: Revoke all refresh tokens for current user
- **Authentication**: Bearer token required
- **Headers**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`
- **Response**: `200 OK` or `401 Unauthorized`

```json
{
  "success": true,
  "message": "All tokens revoked successfully.",
  "data": true
}
```

### 8. Get Current User

**GET** `/api/auth/me`

- **Description**: Get current authenticated user information
- **Authentication**: Bearer token required
- **Headers**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`
- **Response**: `200 OK` or `401 Unauthorized`

```json
{
  "success": true,
  "message": "User information retrieved successfully.",
  "data": {
    "id": "1",
    "email": "user@example.com",
    "name": "John Doe",
    "provider": "Local",
    "roles": ["Guest"]
  }
}
```

### 9. Forgot Password

**POST** `/api/auth/forgot-password`

- **Description**: Send password reset email
- **Authentication**: None required
- **Request Body**:

```json
{
  "email": "user@example.com"
}
```

- **Response**: `200 OK`

```json
{
  "success": true,
  "message": "Password reset email sent if account exists.",
  "data": true
}
```

### 10. Reset Password

**POST** `/api/auth/reset-password`

- **Description**: Reset password with token from email
- **Authentication**: None required
- **Request Body**:

```json
{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123",
  "confirmPassword": "NewSecurePassword123"
}
```

- **Response**: `200 OK` or `400 Bad Request`

```json
{
  "success": true,
  "message": "Password reset successfully.",
  "data": true
}
```

### 11. Verify Email

**GET** `/api/auth/verify-email?email=user@example.com&token=verification-token`

- **Description**: Verify email address with token
- **Authentication**: None required
- **Query Parameters**: `email`, `token`
- **Response**: `200 OK` or `400 Bad Request`

```json
{
  "success": true,
  "message": "Email verified successfully.",
  "data": true
}
```

## Authorization Policies

The API supports several authorization policies:

- **GuestPolicy**: Requires "Guest" role
- **HotelPolicy**: Requires "Hotel" role
- **AdminPolicy**: Requires "Admin" role
- **HotelOrAdminPolicy**: Requires "Hotel" OR "Admin" role
- **AllUsersPolicy**: Requires any authenticated user with valid role

### Usage Example:

```csharp
[Authorize(Policy = "HotelPolicy")]
[HttpGet("hotel-only-endpoint")]
public IActionResult HotelOnlyAction()
{
    return Ok("This endpoint is only accessible by Hotel users");
}
```

## Configuration

### Required Configuration (appsettings.json):

```json
{
  "JWT": {
    "Secret": "your-256-bit-secret-key-here-make-sure-its-long-enough",
    "Issuer": "visita-booking-api",
    "Audience": "visita-booking-users",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=VisitaBookingDB;user=root;password=your-password;"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

## Security Considerations

1. **JWT Secret**: Use a strong, randomly generated secret key (256-bit minimum)
2. **HTTPS**: Always use HTTPS in production
3. **Token Storage**: Store refresh tokens securely on client side
4. **Token Expiry**: Configure appropriate token expiry times
5. **Rate Limiting**: Implement rate limiting for authentication endpoints
6. **Password Policy**: Enforce strong password requirements
7. **Account Lockout**: Consider implementing account lockout after failed attempts

## Error Responses

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

Common HTTP status codes:

- `200 OK`: Success
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required or failed
- `403 Forbidden`: Insufficient permissions
- `500 Internal Server Error`: Server error

## Database Migrations

After setting up the configuration, run the following commands to create the database:

```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

## Testing the API

### Example using cURL:

```bash
# Register a new user
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePassword123",
    "confirmPassword": "SecurePassword123",
    "firstName": "Test",
    "lastName": "User",
    "role": "Guest"
  }'

# Login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePassword123"
  }'

# Access protected endpoint
curl -X GET "https://localhost:7000/api/auth/me" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

This authentication module provides a solid foundation for user authentication and authorization in your Visita Booking API.
