# PowerShell script to execute the SQL migration
# This script adds missing columns to the Loans table

$serverName = "192.185.11.98"
$databaseName = "dinspire_mf_dev"
$userId = "dinspire_mfdev"
$password = "4Sq9Vqzxs9s*vvG?"
$sqlFile = Join-Path $PSScriptRoot "AddMissingLoansColumns.sql"

Write-Host "Connecting to SQL Server: $serverName" -ForegroundColor Cyan
Write-Host "Database: $databaseName" -ForegroundColor Cyan

try {
    # Read the SQL file
    $sqlScript = Get-Content $sqlFile -Raw
    
    # Create connection string
    $connectionString = "Server=$serverName;Database=$databaseName;User Id=$userId;Password=$password;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;"
    
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    
    # Execute the SQL script
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.CommandTimeout = 60
    
    Write-Host "Executing SQL script..." -ForegroundColor Yellow
    $result = $command.ExecuteNonQuery()
    
    Write-Host "SQL script executed successfully!" -ForegroundColor Green
    Write-Host "Rows affected: $result" -ForegroundColor Green
    
    $connection.Close()
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error occurred: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
    exit 1
}
