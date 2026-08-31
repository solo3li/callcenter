$baseUrl = "http://localhost:5000"
$rand = Get-Random
$email = "admin_$rand@example.com"
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
        $jsonBody = $Body | ConvertTo-Json -Depth 10
    }
    
    try {
        if ($Body) {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -Body $jsonBody
        } else {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
        }
        return $response
    }
    catch {
        Write-Host "ERROR on $Method $uri : $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errBody = $reader.ReadToEnd()
            Write-Host "Response Body: $errBody" -ForegroundColor Yellow
        }
        throw $_
    }
}

Write-Host "=== AUTHENTICATING ==="
Invoke-Api -Method Post -Endpoint "/api/auth/register" -Body @{
    email = $email
    password = $password
    firstName = "Test"
    lastName = "Admin"
    displayName = "Test Admin"
    companyName = "Test Co"
}
$loginResponse = Invoke-Api -Method Post -Endpoint "/api/auth/login" -Body @{
    email = $email
    password = $password
}
$token = $loginResponse.accessToken
$userId = $loginResponse.user.id

Write-Host "`n=== SETUP DUMMY DATA ==="
$callSessionId = [guid]::NewGuid().ToString()

# Insert dummy call session into Postgres
$sql = "INSERT INTO call_sessions (id, user_id, status, direction, started_at, created_at, livekit_room_name) VALUES ('$callSessionId', '$userId', 'active', 'inbound', NOW(), NOW(), 'test-room');"
$containerExec = "docker compose exec -T db psql -U admin -d callcenter -c ""$sql"""
Invoke-Expression $containerExec

# Insert dummy human agent
$humanAgentId = [guid]::NewGuid().ToString()
$sqlAgent = "INSERT INTO human_agents (id, owner_user_id, name, status, created_at) VALUES ('$humanAgentId', '$userId', 'Test Agent', 'available', NOW());"
$containerExecAgent = "docker compose exec -T db psql -U admin -d callcenter -c ""$sqlAgent"""
Invoke-Expression $containerExecAgent

# Insert dummy recording
$recordingId = [guid]::NewGuid().ToString()
$sqlRec = "INSERT INTO call_recordings (id, call_session_id, storage_provider, object_key, status, duration_seconds, size_bytes, created_at) VALUES ('$recordingId', '$callSessionId', 's3', 'test.wav', 'completed', 10, 1024, NOW());"
$containerExecRec = "docker compose exec -T db psql -U admin -d callcenter -c ""$sqlRec"""
Invoke-Expression $containerExecRec

Write-Host "`n=== CALL SESSIONS ==="
Write-Host "GET /api/calls"
$calls = Invoke-Api -Method Get -Endpoint "/api/calls" -AuthToken $token

Write-Host "GET /api/calls/active"
$activeCalls = Invoke-Api -Method Get -Endpoint "/api/calls/active" -AuthToken $token

Write-Host "GET /api/calls/$callSessionId"
$callSession = Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId" -AuthToken $token

Write-Host "PATCH /api/calls/$callSessionId/metadata"
Invoke-Api -Method Patch -Endpoint "/api/calls/$callSessionId/metadata" -Body @{
    metadataJson = "{ `"key`": `"value`" }"
} -AuthToken $token

Write-Host "GET /api/calls/$callSessionId/participants"
$participants = Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/participants" -AuthToken $token

Write-Host "`n=== SIP DESTINATIONS ==="
Write-Host "POST /api/sip/destinations"
$sipDestination = Invoke-Api -Method Post -Endpoint "/api/sip/destinations" -Body @{
    name = "Test Sip Dest $rand"
    callTo = "sip:test@example.com"
    description = "Test Sip"
} -AuthToken $token

$sipId = $sipDestination.id

Write-Host "GET /api/sip/destinations"
$sipDestinations = Invoke-Api -Method Get -Endpoint "/api/sip/destinations" -AuthToken $token

Write-Host "PATCH /api/sip/destinations/$sipId"
Invoke-Api -Method Patch -Endpoint "/api/sip/destinations/$sipId" -Body @{
    name = "Test Sip Dest $rand Updated"
} -AuthToken $token

Write-Host "GET /api/sip/destinations/options"
$options = Invoke-Api -Method Get -Endpoint "/api/sip/destinations/options" -AuthToken $token



Write-Host "`n=== TRANSFERS & HANDOFFS ==="
Write-Host "POST /api/calls/$callSessionId/transfers (destination)"
$destTransfer = Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/transfers" -Body @{
    targetType = "destination"
    targetName = "Test Sip Dest $rand Updated"
    reason = "User requested transfer to dest"
} -AuthToken $token

Write-Host "POST /api/calls/$callSessionId/transfers (agent)"
$agentTransfer = Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/transfers" -Body @{
    targetType = "agent"
    targetName = "Test Agent"
    reason = "User requested transfer to agent"
} -AuthToken $token

Write-Host "GET /api/calls/$callSessionId/transfers"
$transfersList = Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/transfers" -AuthToken $token

$transferId = $agentTransfer.transfer.id
if ($transferId) {
    Write-Host "GET /api/calls/$callSessionId/transfers/$transferId"
    Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/transfers/$transferId" -AuthToken $token

    Write-Host "POST /api/calls/$callSessionId/transfers/$transferId/accept"
    Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/transfers/$transferId/accept" -Body @{ humanAgentId = $humanAgentId } -AuthToken $token

    Write-Host "POST /api/calls/$callSessionId/transfers/$transferId/complete"
    Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/transfers/$transferId/complete" -AuthToken $token
    
    Write-Host "`n=== HANDOFFS ==="
    Write-Host "POST /api/calls/$callSessionId/handoffs/$transferId"
    $handoff = Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/handoffs/$transferId" -Body @{ summary = "Test Handoff" } -AuthToken $token
    $handoffId = $handoff.id

    if ($handoffId) {
        Write-Host "GET /api/calls/$callSessionId/handoffs/$handoffId"
        Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/handoffs/$handoffId" -AuthToken $token

        Write-Host "POST /api/calls/$callSessionId/handoffs/$handoffId/deliver"
        Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/handoffs/$handoffId/deliver" -AuthToken $token

        Write-Host "POST /api/calls/$callSessionId/handoffs/$handoffId/accept"
        Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/handoffs/$handoffId/accept" -AuthToken $token
    }
}

Write-Host "GET /api/calls/$callSessionId/handoffs"
Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/handoffs" -AuthToken $token

Write-Host "`n=== RECORDINGS ==="
Write-Host "GET /api/calls/$callSessionId/recordings"
Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/recordings" -AuthToken $token

Write-Host "GET /api/calls/$callSessionId/recordings/$recordingId"
Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/recordings/$recordingId" -AuthToken $token

Write-Host "GET /api/calls/$callSessionId/recordings/$recordingId/download"
Invoke-Api -Method Get -Endpoint "/api/calls/$callSessionId/recordings/$recordingId/download" -AuthToken $token

Write-Host "DELETE /api/calls/$callSessionId/recordings/$recordingId"
Invoke-Api -Method Delete -Endpoint "/api/calls/$callSessionId/recordings/$recordingId" -AuthToken $token

Write-Host "POST /api/calls/$callSessionId/end"
Invoke-Api -Method Post -Endpoint "/api/calls/$callSessionId/end" -AuthToken $token

Write-Host "DELETE /api/sip/destinations/$sipId"
Invoke-Api -Method Delete -Endpoint "/api/sip/destinations/$sipId" -AuthToken $token

Write-Host "`nAll Call Operations endpoints tested!"
