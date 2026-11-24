#Requires -Version 5.1
<#
.SYNOPSIS
    Запускает нагрузочные сценарии k6 и складывает отчёты в loadtest/results.

.DESCRIPTION
    Profile = smoke  -> быстрый прогон (2 VU, ~35 сек на сценарий)
    Profile = load   -> боевой профиль из сценария
    Profile = all    -> smoke, затем load

.EXAMPLE
    .\Run-LoadTests.ps1 -Profile all
    .\Run-LoadTests.ps1 -Profile load -Only 01-catalog.js,03-order-flow.js
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:8080',
    [string]$K6 = '',
    [ValidateSet('smoke', 'load', 'all')]
    [string]$Profile = 'all',
    [string[]]$Only = @(),
    [string]$ResultsDir = '',
    [switch]$ResetBeforeEach
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$scenariosDir = Join-Path $repoRoot 'loadtest\k6\scenarios'

if (-not $ResultsDir) {
    $ResultsDir = Join-Path $repoRoot 'loadtest\results'
}
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

function Resolve-K6 {
    param([string]$explicit)

    if ($explicit) {
        if (-not (Test-Path $explicit)) { throw "k6 not found at $explicit" }
        return (Resolve-Path $explicit).Path
    }

    $cmd = Get-Command k6 -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'Programs\k6'),
        'C:\Program Files\k6'
    )
    foreach ($root in $candidates) {
        if (Test-Path $root) {
            $exe = Get-ChildItem $root -Recurse -Filter 'k6.exe' -ErrorAction SilentlyContinue |
                Select-Object -First 1
            if ($exe) { return $exe.FullName }
        }
    }

    throw 'k6.exe not found. Install it (winget install k6.k6) or pass -K6 <path>.'
}

$k6Exe = Resolve-K6 $K6
Write-Host "k6: $k6Exe" -ForegroundColor Cyan

$health = $null
try {
    $health = Invoke-WebRequest "$BaseUrl/health" -UseBasicParsing -TimeoutSec 5
}
catch { }
if ($null -eq $health -or $health.StatusCode -ne 200) {
    throw "API is not available at $BaseUrl. Start the stack first (see loadtest/README.md)."
}

$all = @(
    '01-catalog.js',
    '02-auth.js',
    '03-order-flow.js',
    '04-mixed.js'
)
if ($Only.Count -gt 0) {
    $all = @($all | Where-Object { $Only -contains $_ })
}
if ($all.Count -eq 0) {
    throw 'No scenarios selected.'
}

$profiles = @()
if ($Profile -eq 'smoke' -or $Profile -eq 'all') { $profiles += 'smoke' }
if ($Profile -eq 'load' -or $Profile -eq 'all') { $profiles += 'load' }

$results = New-Object System.Collections.Generic.List[object]

foreach ($currentProfile in $profiles) {
    foreach ($scenario in $all) {
        $path = Join-Path $scenariosDir $scenario
        if (-not (Test-Path $path)) { throw "Scenario not found: $path" }

        if ($ResetBeforeEach) {
            & (Join-Path $PSScriptRoot 'Reset-LoadTestData.ps1')
        }

        $name = [System.IO.Path]::GetFileNameWithoutExtension($scenario)
        $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $out = Join-Path $ResultsDir ("{0}-{1}-{2}.json" -f $name, $currentProfile, $stamp)
        $log = Join-Path $ResultsDir ("{0}-{1}-{2}.log" -f $name, $currentProfile, $stamp)

        Write-Host ''
        Write-Host ("=== {0} [{1}] ===" -f $name, $currentProfile) -ForegroundColor Green

        & $k6Exe run --quiet --summary-export $out `
            -e PROFILE=$currentProfile `
            -e BASE_URL=$BaseUrl `
            $path 2>&1 | Tee-Object -FilePath $log | Out-Host

        $exitCode = $LASTEXITCODE
        $results.Add([pscustomobject]@{
                Scenario = $name
                Profile  = $currentProfile
                ExitCode = $exitCode
                Summary  = $out
            })
    }
}

Write-Host ''
Write-Host '=== RESULTS ===' -ForegroundColor Cyan
$results | Format-Table -AutoSize | Out-Host

$failed = @($results | Where-Object { $_.ExitCode -ne 0 })
if ($failed.Count -gt 0) {
    Write-Host "Thresholds failed in $($failed.Count) run(s): $($failed.Scenario -join ', ')" -ForegroundColor Red
    exit 1
}

Write-Host 'All thresholds passed.' -ForegroundColor Green
