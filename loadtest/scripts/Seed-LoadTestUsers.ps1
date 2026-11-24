#Requires -Version 5.1
<#
.SYNOPSIS
    Создаёт пул ботов-пользователей для нагрузочных тестов k6.

.DESCRIPTION
    1. Проверяет, что API доступен (/health).
    2. Для каждого бота: пробует логин; если не выходит - регистрирует через API.
    3. Подтверждает email ботам напрямую в БД (checkout/order требуют
       подтверждённый email, а эндпоинт подтверждения защищён [Authorize]).
    4. Проверяет, что логин работает.

.EXAMPLE
    .\Seed-LoadTestUsers.ps1 -Count 40
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:8080',
    [int]$Count = 40,
    [string]$Password = 'LoadTest123',
    [string]$BotPrefix = 'loadbot',
    [string]$BotDomain = 'load.test',
    [string]$DbName = 'WebStoreProjectDb',
    [string]$DbUser = 'postgres'
)

$ErrorActionPreference = 'Stop'

function Get-BotEmail([int]$index) {
    return ('{0}{1:D3}@{2}' -f $BotPrefix, $index, $BotDomain)
}

function Get-BotName([int]$index) {
    return ('{0}{1:D3}' -f $BotPrefix, $index)
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Url,
        $Body,
        [hashtable]$Headers
    )

    $params = @{
        Uri                = $Url
        Method             = $Method
        UseBasicParsing    = $true
        TimeoutSec         = 60
    }
    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 5)
        $params.ContentType = 'application/json'
    }
    if ($Headers) {
        $params.Headers = $Headers
    }

    try {
        $response = Invoke-WebRequest @params
        return @{ Status = [int]$response.StatusCode; Body = $response.Content }
    }
    catch {
        $status = 0
        $body = ''
        if ($null -ne $_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                if ($null -ne $stream) {
                    $body = (New-Object System.IO.StreamReader($stream)).ReadToEnd()
                }
            }
            catch { }
        }
        return @{ Status = $status; Body = $body }
    }
}

function Get-DbContainer {
    $name = docker ps --filter 'label=com.docker.compose.service=db' --format '{{.Names}}' 2>$null |
        Select-Object -First 1
    if (-not $name) {
        throw 'PostgreSQL container not found. Start it first: docker compose -f docker-compose.yml -f loadtest/docker-compose.loadtest.yml up -d db api'
    }
    return [string]$name
}

Write-Host "API: $BaseUrl" -ForegroundColor Cyan
$health = Invoke-ApiRequest -Method Get -Url "$BaseUrl/health"
if ($health.Status -ne 200) {
    throw "API is not available at $BaseUrl (health status: $($health.Status)). Start the stack first."
}

$created = 0
$existing = 0
$failures = New-Object System.Collections.Generic.List[string]

for ($i = 1; $i -le $Count; $i++) {
    $email = Get-BotEmail $i
    $body = @{ Email = $email; Password = $Password }

    $login = Invoke-ApiRequest -Method Post -Url "$BaseUrl/api/auth/login" -Body $body
    if ($login.Status -eq 200) {
        $existing++
        continue
    }

    $registerBody = @{
        Email    = $email
        Password = $Password
        UserName = (Get-BotName $i)
    }
    $register = Invoke-ApiRequest -Method Post -Url "$BaseUrl/api/auth/register" -Body $registerBody
    if ($register.Status -eq 200 -or $register.Status -eq 201) {
        $created++
    }
    else {
        $failures.Add("$email -> $($register.Status) $($register.Body)")
    }
}

Write-Host "Registered: $created, already existed: $existing, failures: $($failures.Count)" -ForegroundColor Yellow
if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    throw 'Some bots could not be created.'
}

# Подтверждаем email напрямую в БД: эндпоинт /api/auth/confirm-email требует [Authorize],
# а checkout/order/review требуют IsEmailConfirmed == true.
$container = Get-DbContainer
$pattern = "%@$BotDomain"
$sql = @"
UPDATE "Users"
SET "IsEmailConfirmed" = true,
    "EmailConfirmedAt" = NOW(),
    "EmailConfirmationToken" = NULL,
    "EmailConfirmationTokenExpiryAt" = NULL,
    "IsActive" = true
WHERE "Email" LIKE '$pattern';
"@
$sql | docker exec -i $container psql -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -f - | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw 'Failed to confirm bot emails in the database.'
}

# Проверка логина первого и последнего бота.
foreach ($index in @(1, $Count)) {
    $check = Invoke-ApiRequest -Method Post -Url "$BaseUrl/api/auth/login" -Body @{ Email = (Get-BotEmail $index); Password = $Password }
    if ($check.Status -ne 200) {
        throw "Login check failed for $(Get-BotEmail $index): $($check.Status) $($check.Body)"
    }
}

Write-Host "OK: $Count bots are ready (password: $Password)." -ForegroundColor Green
