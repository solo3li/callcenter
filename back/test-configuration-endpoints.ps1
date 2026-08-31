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

Write-Host "1. Registering admin user..."
$regResponse = Invoke-Api -Method Post -Endpoint "/api/auth/register" -Body @{ email = $email; password = $password; displayName = "Admin User" }
Write-Host "RegResponse is: $regResponse" -ForegroundColor Cyan
$token = $regResponse.accessToken
if (-not $token) {
    Write-Host "Failed to get token!" -ForegroundColor Red
    exit 1
}

# ---------------------------------------------------------
# WORKFLOWS
# ---------------------------------------------------------
Write-Host "`n--- WORKFLOWS ---"
Write-Host "POST /api/workflows"
$wf = Invoke-Api -Method Post -Endpoint "/api/workflows" -Body @{ name = "Test Workflow"; description = "A test workflow" } -AuthToken $token

Write-Host "GET /api/workflows"
Invoke-Api -Method Get -Endpoint "/api/workflows" -AuthToken $token

Write-Host "GET /api/workflows/{id}"
Invoke-Api -Method Get -Endpoint "/api/workflows/$($wf.id)" -AuthToken $token

Write-Host "PUT /api/workflows/{id}"
$wf = Invoke-Api -Method Put -Endpoint "/api/workflows/$($wf.id)" -Body @{ name = "Updated Workflow"; description = "Updated desc"; isActive = $true } -AuthToken $token

Write-Host "POST /api/workflows/{id}/versions"
$wfVer = Invoke-Api -Method Post -Endpoint "/api/workflows/$($wf.id)/versions" -Body @{ definitionJson = "{}" } -AuthToken $token

Write-Host "GET /api/workflows/{id}/versions"
Invoke-Api -Method Get -Endpoint "/api/workflows/$($wf.id)/versions" -AuthToken $token

Write-Host "GET /api/workflow-versions/{id}"
Invoke-Api -Method Get -Endpoint "/api/workflow-versions/$($wfVer.id)" -AuthToken $token

Write-Host "POST /api/workflow-versions/{id}/publish"
Invoke-Api -Method Post -Endpoint "/api/workflow-versions/$($wfVer.id)/publish" -AuthToken $token

# ---------------------------------------------------------
# ACTION DEFINITIONS
# ---------------------------------------------------------
Write-Host "`n--- ACTION DEFINITIONS ---"
Write-Host "POST /api/actions"
$action = Invoke-Api -Method Post -Endpoint "/api/actions" -Body @{ name = "testAction_$rand"; displayName = "Test Action"; description = "A test action"; actionType = 3; inputSchemaJson = "{}"; outputSchemaJson = "{}"; configurationJson = "{}" } -AuthToken $token

Write-Host "GET /api/actions"
Invoke-Api -Method Get -Endpoint "/api/actions" -AuthToken $token

Write-Host "GET /api/actions/{id}"
Invoke-Api -Method Get -Endpoint "/api/actions/$($action.id)" -AuthToken $token

Write-Host "PATCH /api/actions/{id}"
$action = Invoke-Api -Method Patch -Endpoint "/api/actions/$($action.id)" -Body @{ name = "testAction2_$rand"; displayName = "Test Action 2"; description = "Updated desc"; inputSchemaJson = "{}"; outputSchemaJson = "{}"; configurationJson = "{}"; isActive = $true } -AuthToken $token

# ---------------------------------------------------------
# PERSONAS
# ---------------------------------------------------------
Write-Host "`n--- PERSONAS ---"
Write-Host "POST /api/personas"
$persona = Invoke-Api -Method Post -Endpoint "/api/personas" -Body @{ name = "Test Persona"; description = "A test persona" } -AuthToken $token

Write-Host "GET /api/personas"
Invoke-Api -Method Get -Endpoint "/api/personas" -AuthToken $token

Write-Host "GET /api/personas/{id}"
Invoke-Api -Method Get -Endpoint "/api/personas/$($persona.id)" -AuthToken $token

Write-Host "PATCH /api/personas/{id}"
$persona = Invoke-Api -Method Patch -Endpoint "/api/personas/$($persona.id)" -Body @{ name = "Updated Persona"; description = "Updated desc"; isActive = $true } -AuthToken $token

Write-Host "POST /api/personas/{id}/versions"
$perVer = Invoke-Api -Method Post -Endpoint "/api/personas/$($persona.id)/versions" -Body @{ systemPrompt = "You are a helpful assistant."; configurationJson = "{}" } -AuthToken $token

Write-Host "GET /api/personas/{id}/versions"
Invoke-Api -Method Get -Endpoint "/api/personas/$($persona.id)/versions" -AuthToken $token

Write-Host "GET /api/personas/{personaId}/versions/{versionId}"
Invoke-Api -Method Get -Endpoint "/api/personas/$($persona.id)/versions/$($perVer.id)" -AuthToken $token

Write-Host "POST /api/personas/{personaId}/versions/{versionId}/publish"
Invoke-Api -Method Post -Endpoint "/api/personas/$($persona.id)/versions/$($perVer.id)/publish" -AuthToken $token

Write-Host "POST /api/personas/{personaId}/actions/{actionDefinitionId}"
Invoke-Api -Method Post -Endpoint "/api/personas/$($persona.id)/actions/$($action.id)" -AuthToken $token

Write-Host "GET /api/personas/{personaId}/actions"
Invoke-Api -Method Get -Endpoint "/api/personas/$($persona.id)/actions" -AuthToken $token

Write-Host "PUT /api/personas/default"
Invoke-Api -Method Put -Endpoint "/api/personas/default" -Body @{ personaId = $persona.id } -AuthToken $token

Write-Host "GET /api/personas/default"
Invoke-Api -Method Get -Endpoint "/api/personas/default" -AuthToken $token

# ---------------------------------------------------------
# KNOWLEDGE BASES
# ---------------------------------------------------------
Write-Host "`n--- KNOWLEDGE BASES ---"
Write-Host "POST /api/knowledge-bases"
$kb = Invoke-Api -Method Post -Endpoint "/api/knowledge-bases" -Body @{ name = "Test KB"; description = "A test kb" } -AuthToken $token

Write-Host "GET /api/knowledge-bases"
Invoke-Api -Method Get -Endpoint "/api/knowledge-bases" -AuthToken $token

Write-Host "GET /api/knowledge-bases/{id}"
Invoke-Api -Method Get -Endpoint "/api/knowledge-bases/$($kb.id)" -AuthToken $token

Write-Host "PUT /api/knowledge-bases/{id}"
$kb = Invoke-Api -Method Put -Endpoint "/api/knowledge-bases/$($kb.id)" -Body @{ name = "Updated KB"; description = "Updated desc"; isActive = $true } -AuthToken $token

Write-Host "POST /api/knowledge-bases/{id}/documents"
$doc = Invoke-Api -Method Post -Endpoint "/api/knowledge-bases/$($kb.id)/documents" -Body @{ name = "doc1.txt"; sourceUri = "file://doc1.txt"; contentType = "text/plain"; metadataJson = "{}"; content = "This is a test document." } -AuthToken $token

Write-Host "GET /api/knowledge-bases/{id}/documents"
Invoke-Api -Method Get -Endpoint "/api/knowledge-bases/$($kb.id)/documents" -AuthToken $token

Write-Host "GET /api/knowledge-documents/{id}"
Invoke-Api -Method Get -Endpoint "/api/knowledge-documents/$($doc.id)" -AuthToken $token

Write-Host "POST /api/knowledge-documents/{id}/chunks"
$chunk = Invoke-Api -Method Post -Endpoint "/api/knowledge-documents/$($doc.id)/chunks" -Body @{ content = "This is a chunk."; chunkIndex = 1; metadataJson = "{}" } -AuthToken $token

Write-Host "POST /api/knowledge-bases/{id}/search"
Invoke-Api -Method Post -Endpoint "/api/knowledge-bases/$($kb.id)/search" -Body @{ query = "test"; topK = 5 } -AuthToken $token

Write-Host "POST /api/personas/{personaId}/knowledge-bases/{knowledgeBaseId}"
Invoke-Api -Method Post -Endpoint "/api/personas/$($persona.id)/knowledge-bases/$($kb.id)" -AuthToken $token

Write-Host "GET /api/personas/{personaId}/knowledge-bases"
Invoke-Api -Method Get -Endpoint "/api/personas/$($persona.id)/knowledge-bases" -AuthToken $token

# ---------------------------------------------------------
# CALL CONFIGURATIONS
# ---------------------------------------------------------
Write-Host "`n--- CALL CONFIGURATIONS ---"
Write-Host "POST /api/call-configurations"
$cc = Invoke-Api -Method Post -Endpoint "/api/call-configurations" -Body @{ name = "Test CC"; description = "Test call config"; personaId = $persona.id; workflowId = $wf.id; configJson = "{}" } -AuthToken $token

Write-Host "GET /api/call-configurations"
Invoke-Api -Method Get -Endpoint "/api/call-configurations" -AuthToken $token

Write-Host "GET /api/call-configurations/{id}"
Invoke-Api -Method Get -Endpoint "/api/call-configurations/$($cc.id)" -AuthToken $token

Write-Host "PATCH /api/call-configurations/{id}"
$cc = Invoke-Api -Method Patch -Endpoint "/api/call-configurations/$($cc.id)" -Body @{ name = "Updated CC"; description = "Updated desc"; personaId = $persona.id; workflowId = $wf.id; configJson = "{}"; isActive = $true } -AuthToken $token

Write-Host "PUT /api/call-configurations/{id}/actions"
Invoke-Api -Method Put -Endpoint "/api/call-configurations/$($cc.id)/actions" -Body @{ actionDefinitionIds = @($action.id) } -AuthToken $token

# ---------------------------------------------------------
# CLEANUP
# ---------------------------------------------------------
Write-Host "`n--- CLEANUP ---"
Write-Host "DELETE /api/personas/{personaId}/knowledge-bases/{knowledgeBaseId}"
Invoke-Api -Method Delete -Endpoint "/api/personas/$($persona.id)/knowledge-bases/$($kb.id)" -AuthToken $token

Write-Host "DELETE /api/knowledge-chunks/{id}"
Invoke-Api -Method Delete -Endpoint "/api/knowledge-chunks/$($chunk.id)" -AuthToken $token

Write-Host "DELETE /api/knowledge-documents/{id}"
Invoke-Api -Method Delete -Endpoint "/api/knowledge-documents/$($doc.id)" -AuthToken $token

Write-Host "DELETE /api/knowledge-bases/{id}"
Invoke-Api -Method Delete -Endpoint "/api/knowledge-bases/$($kb.id)" -AuthToken $token

Write-Host "DELETE /api/personas/{personaId}/actions/{actionDefinitionId}"
Invoke-Api -Method Delete -Endpoint "/api/personas/$($persona.id)/actions/$($action.id)" -AuthToken $token

Write-Host "DELETE /api/personas/{id}"
Invoke-Api -Method Delete -Endpoint "/api/personas/$($persona.id)" -AuthToken $token

Write-Host "DELETE /api/actions/{id}"
Invoke-Api -Method Delete -Endpoint "/api/actions/$($action.id)" -AuthToken $token

Write-Host "DELETE /api/workflows/{id}"
Invoke-Api -Method Delete -Endpoint "/api/workflows/$($wf.id)" -AuthToken $token

Write-Host "DELETE /api/call-configurations/{id}"
Invoke-Api -Method Delete -Endpoint "/api/call-configurations/$($cc.id)" -AuthToken $token

Write-Host "`nAll selected endpoints successfully tested!"
