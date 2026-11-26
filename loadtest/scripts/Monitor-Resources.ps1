#Requires -Version 5.1
<#
.SYNOPSIS
    Снимает CPU/память контейнеров API и PostgreSQL плюс число подключений к БД.

.DESCRIPTION
    Полезно запускать параллельно с k6, чтобы видеть, где упирается нагрузка.
    Пишет CSV: timestamp, api_cpu_pct, api_mem_mb, db_cpu_pct, db_mem_mb, pg_connections.

    Остановить досрочно: создать файл .monitor-stop рядом с выходным CSV.

.EXAMPLE
    .\Monitor-Resources.ps1 -DurationMinutes 10
    .\Monitor-Resources.ps1 -DurationMinutes 10 -OutFile .\results\monitor-run3.csv
#>
[CmdletBinding()]
param(
    [int]$IntervalSeconds = 5,
    [double]$DurationMinutes = 15,
    [string]$OutFile = '',
    [string]$ApiContainer = 'online-store-api-1',
    [string]$DbContainer = 'online-store-db-1',
    [string]$DbName = 'WebStoreProjectDb',
    [string]$DbUser = 'postgres'
)

$ErrorActionPreference = 'Stop'

$resultsDir = Resolve-Path (Join-Path $PSScriptRoot '..\results')
if (-not $OutFile) {
    $OutFile = Join-Path $resultsDir 'monitor.csv'
}
$OutFile = [System.IO.Path]::GetFullPath($OutFile)
$stopFile = Join-Path (Split-Path $OutFile) '.monitor-stop'
Remove-Item $stopFile -ErrorAction SilentlyContinue

'timestamp,api_cpu_pct,api_mem_mb,db_cpu_pct,db_mem_mb,pg_connections' |
    Set-Content -Path $OutFile -Encoding UTF8

function ConvertTo-Megabytes([string]$usage) {
    if ($usage -match '([\d\.,]+)\s*([KMGT])i?B') {
        $value = [double]::Parse(($Matches[1] -replace ',', '.'), [System.Globalization.CultureInfo]::InvariantCulture)
        switch ($Matches[2]) {
            'K' { return $value / 1024 }
            'M' { return $value }
            'G' { return $value * 1024 }
            'T' { return $value * 1024 * 1024 }
        }
    }
    return 0
}

function Get-ContainerStats([string]$container) {
    $raw = docker stats --no-stream --format '{{.CPUPerc}};{{.MemUsage}}' $container 2>$null
    if (-not $raw) { return $null }
    $parts = ($raw | Select-Object -First 1) -split ';'
    if ($parts.Count -lt 2) { return $null }
    return @{
        Cpu = [double]::Parse(($parts[0] -replace '%', '').Trim(), [System.Globalization.CultureInfo]::InvariantCulture)
        MemMb = [math]::Round((ConvertTo-Megabytes ($parts[1] -split '/')[0]), 1)
    }
}

$deadline = (Get-Date).AddMinutes($DurationMinutes)
Write-Host "Monitoring $ApiContainer / $DbContainer -> $OutFile (stop file: $stopFile)" -ForegroundColor Cyan

$samples = 0
while ((Get-Date) -lt $deadline -and -not (Test-Path $stopFile)) {
    try {
        $api = Get-ContainerStats $ApiContainer
        $db = Get-ContainerStats $DbContainer
        $connections = ('SELECT count(*) FROM pg_stat_activity WHERE datname=''' + $DbName + ''';' |
            docker exec -i $DbContainer psql -U $DbUser -d $DbName -At 2>$null |
            Select-Object -First 1)
        if ($api -and $db) {
            # InvariantCulture: иначе в русской локали десятичная запятая ломает CSV.
            $line = [string]::Format(
                [System.Globalization.CultureInfo]::InvariantCulture,
                '{0},{1},{2},{3},{4},{5}',
                (Get-Date).ToString('o'), $api.Cpu, $api.MemMb, $db.Cpu, $db.MemMb, ([string]$connections).Trim())
            Add-Content -Path $OutFile -Value $line -Encoding UTF8
            $samples++
        }
    }
    catch {
        # Контейнеры могли перезапускаться - просто пропускаем сэмпл.
    }
    Start-Sleep -Seconds $IntervalSeconds
}

Write-Host "Samples: $samples" -ForegroundColor Green
