# CryptoCompanion Security Demo: SQL Injection Attack Script
# This script demonstrates the difference between vulnerable and secure endpoints.

$baseUrl = "http://localhost:5000/api/SecurityDemo" # Adjust port if necessary

Write-Host "`n--- STEP 1: Standard Query ---" -ForegroundColor Cyan
Write-Host "Searching for 'BTC' on the VULNERABLE endpoint..."
Invoke-RestMethod -Uri "$baseUrl/vulnerable-search?coinSymbol=BTC" | Format-Table

Write-Host "`n--- STEP 2: The Attack (Partial Bypass) ---" -ForegroundColor Red
Write-Host "Injecting malicious payload: BTC' OR '1'='1"
# This payload makes the WHERE clause always true, potentially returning ALL assets.
$attackPayload = "BTC' OR '1'='1"
$encodedPayload = [System.Web.HttpUtility]::UrlEncode($attackPayload)

try {
    $results = Invoke-RestMethod -Uri "$baseUrl/vulnerable-search?coinSymbol=$encodedPayload"
    Write-Host "ATTACK SUCCESSFUL! Found $($results.Count) results (should have been only 1)." -ForegroundColor Red
    $results | Format-Table
} catch {
    Write-Host "Attack failed or connection error." -ForegroundColor Gray
}

Write-Host "`n--- STEP 3: The Defense (Parameterized Query) ---" -ForegroundColor Green
Write-Host "Attempting the same attack on the SECURE endpoint..."

try {
    $results = Invoke-RestMethod -Uri "$baseUrl/secure-search?coinSymbol=$encodedPayload"
    Write-Host "DEFENSE SUCCESSFUL! Found $($results.Count) results. Parameterized query treated the payload as a string." -ForegroundColor Green
} catch {
    Write-Host "Secure endpoint handled the input correctly or returned error." -ForegroundColor Gray
}

Write-Host "`n--- DEMO COMPLETE ---`n"
