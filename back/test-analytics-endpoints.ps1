$baseUrl = "http://localhost:5000"

# --- HELPER FUNCTION ---
function Invoke-Api {
    param (
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Body = $null,
        [string]$AuthToken = $null
    )
    $uri = "$baseUrl$Endpoint"
    $headers = @{
        "Content-Type" = "application/json"
    }
    if ($AuthToken) {
        $headers["Authorization"] = "Bearer $AuthToken"
    }
    
    $params = @{
        Uri = $uri
        Method = $Method
        Headers = $headers
    }
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
    }
    
    try {
        $response = Invoke-RestMethod @params
        $response | Format-List
        return $response
    }
    catch {
        Write-Host "ERROR on $Method $uri : $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "Response Body: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
        } else {
            Write-Host "Response Body: " -ForegroundColor Yellow
            $reader = new-object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.ReadToEnd() | Write-Host -ForegroundColor Yellow
        }
    }
}

$rand = Get-Random
$email = "admin_$rand@example.com"
$password = "P@ssw0rd123!"

# --- AUTHENTICATION ---
Write-Host "=== AUTHENTICATING ==="
Write-Host "Registering test user: $email"
Invoke-Api -Method Post -Endpoint "/api/auth/register" -Body @{
    email = $email
    password = $password
    firstName = "Test"
    lastName = "Admin"
    displayName = "Test Admin"
    companyName = "Test Co"
}

Write-Host "Logging in..."
$loginResponse = Invoke-Api -Method Post -Endpoint "/api/auth/login" -Body @{
    email = $email
    password = $password
}
$token = $loginResponse.accessToken
if (!$token) {
    Write-Host "Failed to obtain auth token" -ForegroundColor Red
    exit
}

# --- STATS ---
Write-Host "`n--- ANALYTICS STATS ---"

Write-Host "GET /api/stats/today"
Invoke-Api -Method Get -Endpoint "/api/stats/today" -AuthToken $token

Write-Host "GET /api/stats/queue"
Invoke-Api -Method Get -Endpoint "/api/stats/queue" -AuthToken $token

Write-Host "GET /api/stats/agents"
Invoke-Api -Method Get -Endpoint "/api/stats/agents" -AuthToken $token

Write-Host "GET /api/stats/period"
Invoke-Api -Method Get -Endpoint "/api/stats/period?from=$(Get-Date (Get-Date).AddDays(-30) -Format 'yyyy-MM-ddTHH:mm:ssZ')&to=$(Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ')" -AuthToken $token

Write-Host "GET /api/stats/summary"
Invoke-Api -Method Get -Endpoint "/api/stats/summary" -AuthToken $token

Write-Host "GET /api/stats/hourly"
Invoke-Api -Method Get -Endpoint "/api/stats/hourly?date=$(Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ')" -AuthToken $token

Write-Host "GET /api/stats/intents"
Invoke-Api -Method Get -Endpoint "/api/stats/intents" -AuthToken $token

Write-Host "GET /api/health"
Invoke-Api -Method Get -Endpoint "/api/health" -AuthToken $token

Write-Host "`nAll Analytics endpoints tested!"
