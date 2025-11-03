$passwords = @("admin123", "teacher123")

foreach($pwd in $passwords) {
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($pwd)
    $hash = $sha256.ComputeHash($bytes)
    $base64 = [Convert]::ToBase64String($hash)
    Write-Host "$pwd = $base64"
}
