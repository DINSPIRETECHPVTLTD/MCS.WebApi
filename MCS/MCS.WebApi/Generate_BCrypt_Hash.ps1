# ============================================================
# PowerShell Script: Generate BCrypt Hash for Password
# ============================================================
# This script helps generate a BCrypt hash for use in SQL scripts
# Requires: BCrypt.Net-Next NuGet package or online tool
# ============================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Password
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "BCrypt Hash Generator" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Option 1: Try to use BCrypt.Net-Next if available
try {
    # Check if BCrypt.Net-Next is available
    $bcryptAssembly = [System.Reflection.Assembly]::LoadFrom("$PSScriptRoot\BCrypt.Net-Next.dll")
    if ($bcryptAssembly) {
        $hash = [BCrypt.Net.BCrypt]::HashPassword($Password)
        Write-Host "BCrypt Hash Generated:" -ForegroundColor Green
        Write-Host $hash -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Use this hash in your SQL script:" -ForegroundColor Cyan
        Write-Host "SET @PasswordHash = '$hash';" -ForegroundColor White
        exit 0
    }
}
catch {
    Write-Host "BCrypt.Net-Next not found locally." -ForegroundColor Yellow
}

# Option 2: Use online API (if available) or provide instructions
Write-Host "BCrypt.Net-Next not available locally." -ForegroundColor Yellow
Write-Host ""
Write-Host "Options to generate BCrypt hash:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Online Tool:" -ForegroundColor Green
Write-Host "   Visit: https://bcrypt-generator.com/" -ForegroundColor White
Write-Host "   Enter password: $Password" -ForegroundColor White
Write-Host "   Copy the generated hash" -ForegroundColor White
Write-Host ""
Write-Host "2. .NET Code:" -ForegroundColor Green
Write-Host "   Install: Install-Package BCrypt.Net-Next" -ForegroundColor White
Write-Host "   Code: BCrypt.Net.BCrypt.HashPassword(`"$Password`")" -ForegroundColor White
Write-Host ""
Write-Host "3. C# Console App:" -ForegroundColor Green
Write-Host "   using BCrypt.Net;" -ForegroundColor White
Write-Host "   var hash = BCrypt.HashPassword(`"$Password`");" -ForegroundColor White
Write-Host "   Console.WriteLine(hash);" -ForegroundColor White
Write-Host ""
Write-Host "4. Python (if you have bcrypt installed):" -ForegroundColor Green
Write-Host "   import bcrypt" -ForegroundColor White
Write-Host "   hash = bcrypt.hashpw(b'$Password', bcrypt.gensalt())" -ForegroundColor White
Write-Host "   print(hash.decode())" -ForegroundColor White
Write-Host ""

# Option 3: Generate a simple hash for testing (NOT for production)
Write-Host "⚠️  For testing only (NOT production):" -ForegroundColor Red
$bytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash($bytes)
$hashString = [System.BitConverter]::ToString($hashBytes).Replace("-", "")
Write-Host "SHA256 Hash: $hashString" -ForegroundColor Yellow
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan

