# Identity Module Endpoints - Postman Testing Guide

This guide provides detailed instructions and JSON payloads for testing all endpoints within the **Identity Module** using Postman. The base URL for all requests is `http://localhost:5000`.

> [!NOTE]
> All endpoints (except `/api/auth/register` and `/api/auth/login`) require a Bearer token.
> First, use the login or register endpoint to get an `AccessToken`, and set it in Postman under **Authorization** -> **Bearer Token**.

---

## 1. Auth Endpoints (`/api/auth`)

### 1.1. Register a new user
- **Method:** `POST`
- **URL:** `/api/auth/register`
- **Body (JSON):**
```json
{
  "email": "test@example.com",
  "password": "Password123!",
  "displayName": "Test User",
  "companyName": "Acme Corp"
}
```

### 1.2. Login
- **Method:** `POST`
- **URL:** `/api/auth/login`
- **Body (JSON):**
```json
{
  "email": "test@example.com",
  "password": "Password123!"
}
```

### 1.3. Refresh Token
- **Method:** `POST`
- **URL:** `/api/auth/refresh`
- **Body (JSON):**
```json
{
  "accessToken": "YOUR_EXPIRED_ACCESS_TOKEN",
  "refreshToken": "YOUR_REFRESH_TOKEN"
}
```

### 1.4. Logout
- **Method:** `POST`
- **URL:** `/api/auth/logout`
- **Body:** Empty

### 1.5. Get Current User (Me)
- **Method:** `GET`
- **URL:** `/api/auth/me`

### 1.6. Agent Login (For Human Agents)
- **Method:** `POST`
- **URL:** `/api/auth/agent-login`
- **Body (JSON):**
```json
{
  "accessKey": "YOUR_AGENT_ACCESS_KEY"
}
```

---

## 2. API Keys Endpoints (`/api/api-keys`)

### 2.1. List API Keys
- **Method:** `GET`
- **URL:** `/api/api-keys`

### 2.2. Create API Key
- **Method:** `POST`
- **URL:** `/api/api-keys`
- **Body (JSON):**
```json
{
  "name": "Production Key",
  "scopes": ["calls.read", "calls.write"],
  "expiresAt": "2027-12-31T23:59:59Z"
}
```
*Note: Make sure to save the `rawKey` from the response, as it will never be displayed again.*

### 2.3. Update API Key Scopes
- **Method:** `PATCH`
- **URL:** `/api/api-keys/{id}/scopes`
- **Body (JSON):**
```json
{
  "scopes": ["calls.read", "calls.write", "workflows.read"]
}
```

### 2.4. Delete API Key
- **Method:** `DELETE`
- **URL:** `/api/api-keys/{id}`

---

## 3. Human Agents Endpoints (`/api/human-agents`)

### 3.1. List Human Agents
- **Method:** `GET`
- **URL:** `/api/human-agents`

### 3.2. Get Human Agent by ID
- **Method:** `GET`
- **URL:** `/api/human-agents/{id}`

### 3.3. Create Human Agent
- **Method:** `POST`
- **URL:** `/api/human-agents`
- **Body (JSON):**
```json
{
  "name": "Agent Smith",
  "email": "smith@example.com",
  "status": "available"
}
```

### 3.4. Update Human Agent
- **Method:** `PATCH`
- **URL:** `/api/human-agents/{id}`
- **Body (JSON):**
```json
{
  "name": "Agent Smith Updated",
  "email": "smith_updated@example.com"
}
```

### 3.5. Update Agent Status
- **Method:** `PATCH`
- **URL:** `/api/human-agents/{id}/status`
- **Body (JSON):**
```json
{
  "status": "offline"
}
```

### 3.6. Delete Human Agent
- **Method:** `DELETE`
- **URL:** `/api/human-agents/{id}`

### 3.7. Create Access Key for Agent
- **Method:** `POST`
- **URL:** `/api/human-agents/{humanAgentId}/access-keys`
- **Body (JSON):**
```json
{
  "name": "Laptop Key",
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

### 3.8. List Agent Access Keys
- **Method:** `GET`
- **URL:** `/api/human-agents/{humanAgentId}/access-keys`

### 3.9. Delete Agent Access Key
- **Method:** `DELETE`
- **URL:** `/api/human-agents/{humanAgentId}/access-keys/{keyId}`

### 3.10. List Agent Sessions
- **Method:** `GET`
- **URL:** `/api/human-agents/{humanAgentId}/sessions`

### 3.11. Get Current Agent Session
- **Method:** `GET`
- **URL:** `/api/human-agents/{humanAgentId}/sessions/current`

---

## 4. Licenses Endpoints (`/api/licenses`)

### 4.1. List Licenses
- **Method:** `GET`
- **URL:** `/api/licenses`

### 4.2. Get License by ID
- **Method:** `GET`
- **URL:** `/api/licenses/{id}`

### 4.3. Create License
- **Method:** `POST`
- **URL:** `/api/licenses`
- **Body (JSON):**
```json
{
  "userId": "UUID-OF-USER",
  "partnerId": "UUID-OF-PARTNER",
  "name": "Enterprise License",
  "status": "active"
}
```

### 4.4. Update License
- **Method:** `PUT`
- **URL:** `/api/licenses/{id}`
- **Body (JSON):**
```json
{
  "name": "Enterprise License Updated",
  "status": "inactive"
}
```

### 4.5. Delete License
- **Method:** `DELETE`
- **URL:** `/api/licenses/{id}`

---

## 5. Partners Endpoints (`/api/partners`)

### 5.1. List Partners
- **Method:** `GET`
- **URL:** `/api/partners`

### 5.2. Get Partner by ID
- **Method:** `GET`
- **URL:** `/api/partners/{id}`

### 5.3. Get My Partner Profile
- **Method:** `GET`
- **URL:** `/api/partners/me`

### 5.4. Update Partner
- **Method:** `PUT`
- **URL:** `/api/partners/{id}`
- **Body (JSON):**
```json
{
  "name": "Partner Corp",
  "website": "https://partner.example.com",
  "supportEmail": "support@partner.example.com"
}
```

### 5.5. List Partner Customers
- **Method:** `GET`
- **URL:** `/api/partners/{partnerId}/customers`

### 5.6. Add Customer to Partner
- **Method:** `POST`
- **URL:** `/api/partners/{partnerId}/customers`
- **Body (JSON):**
```json
{
  "externalCustomerId": "CUST-001",
  "customerName": "Customer LLC",
  "planId": "UUID-OF-PLAN"
}
```

### 5.7. Get Partner Relationship
- **Method:** `GET`
- **URL:** `/api/partner-relationships/{id}`

### 5.8. Update Partner Relationship
- **Method:** `PUT`
- **URL:** `/api/partner-relationships/{id}`
- **Body (JSON):**
```json
{
  "status": "suspended",
  "metadataJson": "{\"tier\": \"premium\"}"
}
```

### 5.9. Delete Partner Relationship
- **Method:** `DELETE`
- **URL:** `/api/partner-relationships/{id}`

### 5.10. Provision Customer
- **Method:** `POST`
- **URL:** `/api/partners/{partnerId}/provision`
- **Body (JSON):**
```json
{
  "externalCustomerId": "CUST-002",
  "customerName": "New Customer Inc",
  "adminEmail": "admin@newcustomer.com",
  "planId": "UUID-OF-PLAN"
}
```

### 5.11. Get Provision Status
- **Method:** `GET`
- **URL:** `/api/partners/{partnerId}/provision/{externalCustomerId}`

### 5.12. Get Partner Stats
- **Method:** `GET`
- **URL:** `/api/partners/{partnerId}/stats`
