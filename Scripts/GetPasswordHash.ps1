# Get SHA256 hash for passwords
param(
    [string]$password = "student123"
)

$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hash = $sha256.ComputeHash($bytes)
$base64 = [Convert]::ToBase64String($hash)

Write-Host "Password: $password"
Write-Host "SHA256 Hash: $base64"
