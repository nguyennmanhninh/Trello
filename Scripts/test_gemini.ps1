# Test Gemini API models
$apiKey = "AIzaSyDvx269hBCqAyNXcl69HvxQtB8WJWajpbc"

$models = @(
    "gemini-pro",
    "gemini-1.5-pro",
    "gemini-1.5-flash",
    "gemini-2.0-flash-exp"
)

$apis = @("v1", "v1beta")

$body = @{
    contents = @(
        @{
            parts = @(
                @{
                    text = "Say hello"
                }
            )
        }
    )
} | ConvertTo-Json -Depth 10

foreach ($api in $apis) {
    foreach ($model in $models) {
        $url = "https://generativelanguage.googleapis.com/$api/models/${model}:generateContent?key=$apiKey"
        
        Write-Host "`n=== Testing $api / $model ===" -ForegroundColor Cyan
        
        try {
            $response = Invoke-WebRequest -Uri $url -Method POST -Body $body -ContentType "application/json" -UseBasicParsing
            Write-Host "✅ SUCCESS - Status: $($response.StatusCode)" -ForegroundColor Green
            Write-Host "Response: $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))"
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            Write-Host "❌ FAILED - Status: $statusCode" -ForegroundColor Red
            
            if ($statusCode -eq 404) {
                Write-Host "   Model not found" -ForegroundColor Yellow
            }
            elseif ($statusCode -eq 429) {
                Write-Host "   Rate limit exceeded" -ForegroundColor Yellow
            }
            elseif ($statusCode -eq 503) {
                Write-Host "   Service unavailable" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host "`n✅ Test completed!" -ForegroundColor Green
