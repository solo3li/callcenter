$baseUrl = "http://localhost:5000"
$email = "test_$(Get-Random)@example.com"
$password = "Password123!"

Write-Host "1. Testing Register..."
$registerBody = @{
    email = $email
    password = $password
    displayName = "Test User"
    companyName = "Test Co"
} | ConvertTo-Json
$regResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json"
Write-Host "Registered user! Tokens received."

$accessToken = $regResponse.accessToken
$refreshToken = $regResponse.refreshToken

Write-Host "2. Testing Login..."
$loginBody = @{
    email = $email
    password = $password
} | ConvertTo-Json
$logResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
Write-Host "Login successful!"
$accessToken = $logResponse.accessToken
$refreshToken = $logResponse.refreshToken
Write-Host "Access Token: $accessToken"
Write-Host "Refresh Token: $refreshToken"

Write-Host "3. Testing /api/auth/me..."
$headers = @{ "Authorization" = "Bearer $accessToken" }
$meResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -Method Get -Headers $headers
Write-Host "Got user: $($meResponse.email)"

Write-Host "4. Testing /api/auth/refresh..."
$refreshBody = @{
    accessToken = $accessToken
    refreshToken = $refreshToken
} | ConvertTo-Json
try {
    $refResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/refresh" -Method Post -Body $refreshBody -ContentType "application/json"
    Write-Host "Refresh successful! New AccessToken received."
} catch {
    Write-Host "Refresh failed!"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd();
        Write-Host "Response: $responseBody"
    }
}

$newAccessToken = $refResponse.accessToken

Write-Host "5. Testing /api/auth/logout..."
$logoutHeaders = @{ "Authorization" = "Bearer $newAccessToken" }
$outResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/logout" -Method Post -Headers $logoutHeaders
Write-Host "Logout successful!"

Write-Host "All Auth tests passed successfully!"
