param(
    [string]$BackupDir = "C:\Users\hartu\apps\media\Backups",
    [int]$RetentionDays = 7
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupName = "media_api_backup_$timestamp"
$backupPath = Join-Path $BackupDir $backupName
New-Item -ItemType Directory -Force -Path $backupPath | Out-Null

$logFile = Join-Path $backupPath "backup.log"
function Write-Log($msg) {
    $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')  $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line
}

try {
    # ── 1. PostgreSQL database dump ───────────────────────
    Write-Log "Starting PostgreSQL dump..."
    $env:PGPASSWORD = "Media_Db_P@ss_2026!"
    $pgDumpPath = "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe"
    $dbDumpFile = Join-Path $backupPath "media_hmo_dump.sql"

    & $pgDumpPath -h localhost -U media_user -d media_hmo -F p -f $dbDumpFile 2>&1 | ForEach-Object { Write-Log "pg_dump: $_" }

    if ($LASTEXITCODE -ne 0) { throw "pg_dump failed with exit code $LASTEXITCODE" }
    Write-Log "Database dump: $dbDumpFile ($((Get-Item $dbDumpFile).Length) bytes)"

    # ── 2. Media files backup ──────────────────────────
    Write-Log "Starting media files backup..."
    $mediaDir = "C:\Users\hartu\apps\media\wwwroot\media"
    $mediaZip = Join-Path $backupPath "media_files.zip"

    if (Test-Path $mediaDir) {
        Compress-Archive -Path "$mediaDir\*" -DestinationPath $mediaZip -CompressionLevel Optimal
        Write-Log "Media files backup: $mediaZip ($((Get-Item $mediaZip).Length) bytes)"
    } else {
        Write-Log "WARNING: Media directory not found at $mediaDir"
    }

    # ── 3. Cleanup old backups ─────────────────────────
    Write-Log "Cleaning backups older than $RetentionDays days..."
    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    Get-ChildItem -Path $BackupDir -Directory | Where-Object { $_.LastWriteTime -lt $cutoff } | ForEach-Object {
        Write-Log "Removing old backup: $($_.Name)"
        Remove-Item -Recurse -Force $_.FullName
    }

    Write-Log "Backup completed successfully: $backupPath"
    Write-Log "STATUS: SUCCESS"

} catch {
    Write-Log "STATUS: FAILED — $_"
    exit 1
} finally {
    $env:PGPASSWORD = $null
}
