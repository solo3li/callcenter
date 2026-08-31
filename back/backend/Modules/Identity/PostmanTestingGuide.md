# Identity Module - Postman Testing Guide

This guide provides details on how to test all endpoints in the Identity Module using Postman.

## 1. Register User
**Endpoint:** `POST /api/auth/register`
**Description:** Registers a new user account in the system and returns access and refresh tokens.

**Headers:**
- `Content-Type`: `application/json`

**Body (raw JSON):**
```json
{
  "email": "testuser@example.com",
  "password": "Password123!",
  "displayName": "Test User",
  "companyName": "Test Co",
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Response (200 OK):**
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "84c8a...",
  "user": {
    "id": "...",
    "email": "testuser@example.com",
    "displayName": "Test User"
  }
}
```

---

## 2. Login User
**Endpoint:** `POST /api/auth/login`
**Description:** Authenticates an existing user and returns a new access and refresh token.

**Headers:**
- `Content-Type`: `application/json`

**Body (raw JSON):**
```json
{
  "email": "testuser@example.com",
  "password": "Password123!"
}
```

**Expected Response (200 OK):**
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "84c8a...",
  "user": {
    "id": "...",
    "email": "testuser@example.com",
    "displayName": "Test User"
  }
}
```

---

## 3. Get Current User (Me)
**Endpoint:** `GET /api/auth/me`
**Description:** Retrieves the authenticated user's profile information.

**Headers:**
- `Authorization`: `Bearer {{accessToken}}`  *(Replace `{{accessToken}}` with the token received from login/register)*

**Expected Response (200 OK):**
```json
{
  "id": "...",
  "email": "testuser@example.com",
  "displayName": "Test User",
  "companyName": "Test Co",
  ...
}
```

---

## 4. Refresh Token
**Endpoint:** `POST /api/auth/refresh`
**Description:** Refreshes an expired access token using a valid refresh token.

**Headers:**
- `Content-Type`: `application/json`

**Body (raw JSON):**
```json
{
  "accessToken": "{{accessToken}}",
  "refreshToken": "{{refreshToken}}"
}
```
*(Replace `{{accessToken}}` and `{{refreshToken}}` with the actual token strings)*

**Expected Response (200 OK):**
```json
{
  "accessToken": "eyJhbG... (new token)",
  "refreshToken": "72f1a... (new refresh token)"
}
```
> **Note:** If you get a `401 Unauthorized`, ensure the `accessToken` and `refreshToken` are exactly as provided by the login/register response and that the refresh token is not expired.

---

## 5. Logout User
**Endpoint:** `POST /api/auth/logout`
**Description:** Logs out the user by revoking the current refresh token.

**Headers:**
- `Authorization`: `Bearer {{accessToken}}`

**Expected Response (200 OK):**
No body returned, status `200 OK`.

---

## Setup Instructions for Postman

1. **Create an Environment** in Postman called `CallCenter Local`.
2. Add a variable `baseUrl` with the initial value `http://localhost:5000`.
3. Add a variable `accessToken` (leave blank initially).
4. Add a variable `refreshToken` (leave blank initially).
5. For all requests above, you can write a **Test Script** in Postman for the Login/Register endpoints to automatically capture the tokens:
   
   ```javascript
   if (pm.response.code === 200) {
       let data = pm.response.json();
       pm.environment.set("accessToken", data.accessToken);
       pm.environment.set("refreshToken", data.refreshToken);
   }
   ```
6. Now you can use `{{baseUrl}}/api/auth/login` as the URL and `{{accessToken}}` in the headers without copying manually!
