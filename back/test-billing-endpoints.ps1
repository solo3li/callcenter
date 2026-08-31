$baseUrl = "http://localhost:5000"
$email = "billing_admin_$(Get-Random)@example.com"
$password = "P@ssw0rd123!"
$ErrorActionPreference = "Stop"

function Invoke-Api {
    param($Method, $Endpoint, $Body = $null, $AuthToken = $null, $QueryParams = $null)
    
    $uri = "$baseUrl$Endpoint"
    if ($QueryParams) {
        $queryStr = ($QueryParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$([uri]::EscapeDataString($_.Value))" }) -join "&"
        $uri = "$uri?$queryStr"
    }

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
        Write-Host "ERROR on $Method $uri : $($ex.Message)" -ForegroundColor Red
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

Write-Host "`n--- PLANS ---"
Write-Host "POST /api/plans"
$plan = Invoke-Api -Method Post -Endpoint "/api/plans" -AuthToken $token -Body @{ name = "Premium Plan"; description = "Unlimited"; tier = "Enterprise"; isPlatformPlan = $true; entitlementsJson = "{}" }
$planId = $plan.id

Write-Host "GET /api/plans/all"
Invoke-Api -Method Get -Endpoint "/api/plans/all" -AuthToken $token

Write-Host "GET /api/plans"
Invoke-Api -Method Get -Endpoint "/api/plans" -AuthToken $token

Write-Host "GET /api/plans/$planId"
Invoke-Api -Method Get -Endpoint "/api/plans/$planId" -AuthToken $token

Write-Host "PUT /api/plans/$planId"
Invoke-Api -Method Put -Endpoint "/api/plans/$planId" -AuthToken $token -Body @{ name = "Premium Plan v2"; description = "Updated"; tier = "Enterprise"; isActive = $true; entitlementsJson = "{}" }

Write-Host "`n--- SUBSCRIPTIONS ---"
Write-Host "POST /api/subscriptions"
$sub = Invoke-Api -Method Post -Endpoint "/api/subscriptions" -AuthToken $token -Body @{ planId = $planId; startsAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ") }
$subId = $sub.id

Write-Host "GET /api/subscriptions"
Invoke-Api -Method Get -Endpoint "/api/subscriptions" -AuthToken $token

Write-Host "GET /api/subscriptions/$subId"
Invoke-Api -Method Get -Endpoint "/api/subscriptions/$subId" -AuthToken $token

Write-Host "PUT /api/subscriptions/$subId"
Invoke-Api -Method Put -Endpoint "/api/subscriptions/$subId" -AuthToken $token -Body @{ status = "Active" }

Write-Host "POST /api/subscriptions/$subId/cancel"
Invoke-Api -Method Post -Endpoint "/api/subscriptions/$subId/cancel" -AuthToken $token -Body @{}

Write-Host "`n--- USAGE ---"
Write-Host "POST /api/usage (Record Usage via Query Params)"
$usageRecord = Invoke-Api -Method Post -Endpoint "/api/usage?metricType=CallMinutes&quantity=10&unit=minutes" -AuthToken $token -Body $null

Write-Host "GET /api/usage"
Invoke-Api -Method Get -Endpoint "/api/usage" -AuthToken $token

Write-Host "GET /api/usage/summary"
Invoke-Api -Method Get -Endpoint "/api/usage/summary" -AuthToken $token

Write-Host "GET /api/usage/metric/CallMinutes"
Invoke-Api -Method Get -Endpoint "/api/usage/metric/CallMinutes" -AuthToken $token

Write-Host "GET /api/usage/call/{callSessionId}"
# we can just use a dummy guid since it filters by user anyway
$dummyGuid = [guid]::NewGuid().ToString()
Invoke-Api -Method Get -Endpoint "/api/usage/call/$dummyGuid" -AuthToken $token

Write-Host "`n--- CLEANUP ---"
Write-Host "DELETE /api/plans/$planId"
Invoke-Api -Method Delete -Endpoint "/api/plans/$planId" -AuthToken $token


Write-Host "`nAll selected endpoints successfully tested!"
