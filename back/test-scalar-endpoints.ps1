$baseUrl = "http://localhost:5000"
$email = "admin_$(Get-Random)@example.com"
$password = "P@ssw0rd123!"
$ErrorActionPreference = "Stop"

function Invoke-Api {
    param($Method, $Endpoint, $Body = $null, $AuthToken = $null)
    $uri = "$baseUrl$Endpoint"
    $headers = @{
        "Content-Type" = "application/json"
    }
    if ($AuthToken) {
        $headers["Authorization"] = "Bearer $AuthToken"
    }

    $jsonBody = $null
    if ($Body) {
        $jsonBody = $Body | ConvertTo-Json -Depth 10 -Compress
    }

    try {
        if ($Body) {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -Body $jsonBody
        } else {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
        }
        return $response
    } catch {
        $ex = $_.Exception
        Write-Host "ERROR on $Method $Endpoint : $($ex.Message)" -ForegroundColor Red
        if ($ex.Response) {
            $reader = New-Object System.IO.StreamReader($ex.Response.GetResponseStream())
            $respBody = $reader.ReadToEnd()
            Write-Host "Response Body: $respBody" -ForegroundColor Red
        }
        throw
    }
}

Write-Host "1. Registering admin user..."
$regResponse = Invoke-Api -Method Post -Endpoint "/api/auth/register" -Body @{ email = $email; password = $password; displayName = "Admin User" }
$token = $regResponse.accessToken
$userId = (Invoke-Api -Method Get -Endpoint "/api/auth/me" -AuthToken $token).id

Write-Host "`n--- API KEYS ---"
Write-Host "POST /api/api-keys"
$apiKey = Invoke-Api -Method Post -Endpoint "/api/api-keys" -AuthToken $token -Body @{ name = "TestKey"; scopes = @("all"); expiresAt = $null }
$keyId = $apiKey.id

Write-Host "GET /api/api-keys"
$keys = Invoke-Api -Method Get -Endpoint "/api/api-keys" -AuthToken $token

Write-Host "PATCH /api/api-keys/$keyId/scopes"
Invoke-Api -Method Patch -Endpoint "/api/api-keys/$keyId/scopes" -AuthToken $token -Body @{ scopes = @("read-only") }

Write-Host "DELETE /api/api-keys/$keyId"
Invoke-Api -Method Delete -Endpoint "/api/api-keys/$keyId" -AuthToken $token


Write-Host "`n--- HUMAN AGENTS ---"
Write-Host "POST /api/human-agents"
$agent = Invoke-Api -Method Post -Endpoint "/api/human-agents" -AuthToken $token -Body @{ name = "Agent Smith"; email = "smith@example.com"; maxConcurrentCalls = 2 }
$agentId = $agent.id

Write-Host "GET /api/human-agents"
$agents = Invoke-Api -Method Get -Endpoint "/api/human-agents" -AuthToken $token

Write-Host "GET /api/human-agents/$agentId"
$agentDetails = Invoke-Api -Method Get -Endpoint "/api/human-agents/$agentId" -AuthToken $token

Write-Host "PATCH /api/human-agents/$agentId"
Invoke-Api -Method Patch -Endpoint "/api/human-agents/$agentId" -AuthToken $token -Body @{ name = "Agent Smith Updated"; email = "smith@example.com"; maxConcurrentCalls = 3 }

Write-Host "PATCH /api/human-agents/$agentId/status"
Invoke-Api -Method Patch -Endpoint "/api/human-agents/$agentId/status" -AuthToken $token -Body @{ status = 1 }

Write-Host "POST /api/human-agents/$agentId/access-keys"
$agentKey = Invoke-Api -Method Post -Endpoint "/api/human-agents/$agentId/access-keys" -AuthToken $token -Body @{ name = "AgentKey1" }
$agentKeyId = $agentKey.id

Write-Host "GET /api/human-agents/$agentId/access-keys"
Invoke-Api -Method Get -Endpoint "/api/human-agents/$agentId/access-keys" -AuthToken $token

Write-Host "DELETE /api/human-agents/$agentId/access-keys/$agentKeyId"
Invoke-Api -Method Delete -Endpoint "/api/human-agents/$agentId/access-keys/$agentKeyId" -AuthToken $token

Write-Host "GET /api/human-agents/$agentId/sessions"
Invoke-Api -Method Get -Endpoint "/api/human-agents/$agentId/sessions" -AuthToken $token

Write-Host "GET /api/human-agents/$agentId/sessions/current"
try { Invoke-Api -Method Get -Endpoint "/api/human-agents/$agentId/sessions/current" -AuthToken $token } catch { Write-Host "Expected failure if no session exists" -ForegroundColor Yellow }

Write-Host "DELETE /api/human-agents/$agentId"
Invoke-Api -Method Delete -Endpoint "/api/human-agents/$agentId" -AuthToken $token


Write-Host "`n--- LICENSES ---"
Write-Host "POST /api/licenses"
$license = Invoke-Api -Method Post -Endpoint "/api/licenses" -AuthToken $token -Body @{ userId = $userId; startsAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ") }
$licenseId = $license.id

Write-Host "GET /api/licenses"
Invoke-Api -Method Get -Endpoint "/api/licenses" -AuthToken $token

Write-Host "GET /api/licenses/$licenseId"
Invoke-Api -Method Get -Endpoint "/api/licenses/$licenseId" -AuthToken $token

Write-Host "PUT /api/licenses/$licenseId"
Invoke-Api -Method Put -Endpoint "/api/licenses/$licenseId" -AuthToken $token -Body @{ status = "Active"; metadataJson = "{}" }

Write-Host "DELETE /api/licenses/$licenseId"
Invoke-Api -Method Delete -Endpoint "/api/licenses/$licenseId" -AuthToken $token


Write-Host "`n--- PARTNERS ---"
Write-Host "GET /api/partners"
Invoke-Api -Method Get -Endpoint "/api/partners" -AuthToken $token

Write-Host "GET /api/partners/me"
try { $myPartner = Invoke-Api -Method Get -Endpoint "/api/partners/me" -AuthToken $token } catch { Write-Host "Not a partner yet, which is expected" -ForegroundColor Yellow }

Write-Host "`nAll selected endpoints successfully tested!"
