#Requires -Version 5.1
<#
.SYNOPSIS
    Сбрасывает транзакционные данные тестовой БД перед серией нагрузочных прогонов.

.DESCRIPTION
    - Удаляет заказы, платежи, корзины, отзывы и прочие транзакционные таблицы.
    - Сохраняет пользователей (ботов), категории и товары.
    - Возвращает остатки товаров к значению не ниже MinimumStock и обнуляет резервы.

.EXAMPLE
    .\Reset-LoadTestData.ps1
    .\Reset-LoadTestData.ps1 -MinimumStock 5000
#>
[CmdletBinding()]
param(
    [string]$DbName = 'WebStoreProjectDb',
    [string]$DbUser = 'postgres',
    [int]$MinimumStock = 1000
)

$ErrorActionPreference = 'Stop'

function Get-DbContainer {
    $name = docker ps --filter 'label=com.docker.compose.service=db' --format '{{.Names}}' 2>$null |
        Select-Object -First 1
    if (-not $name) {
        throw 'PostgreSQL container not found. Start it first: docker compose -f docker-compose.yml -f loadtest/docker-compose.loadtest.yml up -d db api'
    }
    return [string]$name
}

function Invoke-DbQuery([string]$container, [string]$sql) {
    # SQL передаём через stdin: так PowerShell не ломает кавычки в идентификаторах.
    $result = $sql | docker exec -i $container psql -U $DbUser -d $DbName -At -v ON_ERROR_STOP=1 -f -
    if ($LASTEXITCODE -ne 0) {
        throw "SQL failed: $sql"
    }
    return $result
}

$container = Get-DbContainer

$tables = Invoke-DbQuery $container "SELECT tablename FROM pg_tables WHERE schemaname='public' ORDER BY tablename;"
$tables = @($tables | Where-Object { $_ -and $_.Trim() -ne '' } | ForEach-Object { $_.Trim() })

# Сохраняем: историю миграций, пользователей (ботов), каталог.
$toTruncate = @($tables | Where-Object {
    $_ -ne '__EFMigrationsHistory' -and
    $_ -notlike 'Product*' -and
    $_ -notlike 'Categor*' -and
    $_ -notlike 'User*'
})

$before = @{}
foreach ($table in @('Orders', 'Carts', 'Payments', 'Reviews')) {
    if ($tables -contains $table) {
        $before[$table] = (Invoke-DbQuery $container "SELECT count(*) FROM ""$table"";")
    }
}

if ($toTruncate.Count -gt 0) {
    $list = ($toTruncate | ForEach-Object { '"' + $_ + '"' }) -join ', '
    Invoke-DbQuery $container "TRUNCATE TABLE $list RESTART IDENTITY CASCADE;" | Out-Null
    Write-Host "Truncated $($toTruncate.Count) tables." -ForegroundColor Yellow
}
else {
    Write-Host 'Nothing to truncate.' -ForegroundColor Yellow
}

# Остатки: не ниже минимума, резервы сброшены (заказы больше не держат товар).
Invoke-DbQuery $container "UPDATE ""Products"" SET ""StockQuantity"" = GREATEST(""StockQuantity"", $MinimumStock), ""ReservedQuantity"" = 0;" | Out-Null
Write-Host "Product stock >= $MinimumStock, reserved quantity reset." -ForegroundColor Yellow

foreach ($table in $before.Keys) {
    $after = Invoke-DbQuery $container "SELECT count(*) FROM ""$table"";"
    Write-Host ("{0}: {1} -> {2}" -f $table, $before[$table], $after)
}

$users = Invoke-DbQuery $container "SELECT count(*) FROM ""Users"";"
$products = Invoke-DbQuery $container "SELECT count(*) FROM ""Products"";"
Write-Host "Kept: users=$users, products=$products." -ForegroundColor Green
