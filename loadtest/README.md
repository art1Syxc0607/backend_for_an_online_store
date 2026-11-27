# Нагрузочное тестирование (k6)

Нагрузочные тесты бэкенда: k6 на хосте, API + PostgreSQL в Docker.
Меряется **API напрямую** (`http://localhost:8080`), без TLS и nginx — чтобы цифры
отражали бэкенд, а не прокси.

```
loadtest/
  docker-compose.loadtest.yml   # override: публикует api:8080, гасит логи/фоновые сервисы
  k6/
    lib/config.js               # URL, пароль ботов, профили, общие настройки
    lib/auth.js                 # логин одного бота и пакетный логин в setup()
    lib/data.js                 # id товаров из каталога
    lib/flows.js                # флоу: каталог, логин, корзина -> заказ -> оплата
    scenarios/01-catalog.js     # S1: чтение каталога (baseline)
    scenarios/02-auth.js        # S2: логин + профиль (CPU на хешировании пароля)
    scenarios/03-order-flow.js  # S3: запись - корзина, заказ, оплата
    scenarios/04-mixed.js       # S4: 70% каталог / 10% логин / 20% покупка
  scripts/
    Seed-LoadTestUsers.ps1      # пул ботов (регистрация + подтверждение email в БД)
    Reset-LoadTestData.ps1      # чистка заказов/корзин, откат остатков, пересоздание корзин
    Run-LoadTests.ps1           # запуск сценариев + отчёты
    Monitor-Resources.ps1       # CPU/память контейнеров и соединения PostgreSQL в CSV
  results/                      # json-отчёты и логи прогонов (не коммитятся)
```

## 1. Что нужно

- Docker Desktop (Linux containers).
- .NET не нужен: API собирается в образ.
- k6. Если `k6` не в PATH, скрипты найдут его в
  `%LOCALAPPDATA%\Programs\k6\**\k6.exe`.
  Установка: `winget install k6.k6` или zip с https://github.com/grafana/k6/releases.
- В `.env` в корне репозитория должны быть `DB_PASSWORD` и `JWT_KEY` (или `JWT__KEY`).

## 2. Поднять стенд

```powershell
# Образ (на этой машине `docker compose build` падает на bake,
# поэтому собираем напрямую и поднимаем с --no-build)
docker build -f WebApi/Dockerfile -t online-store-api .
docker compose -f docker-compose.yml -f loadtest/docker-compose.loadtest.yml up -d --no-build db api

# проверка
curl http://localhost:8080/health
```

Что делает override:

| Переменная | Зачем |
|---|---|
| `ports: 8080:8080` | прямой доступ к API без Caddy/nginx |
| `Serilog__MinimumLevel__Default=Warning` | меньше дискового I/O на логи |
| `Logging__LogLevel__Microsoft.EntityFrameworkCore=Warning` | убирает лог каждого SQL-запроса |
| `IpInfo__Enabled=false` | логин не ходит во внешний geo-сервис |
| `Agent__Enabled=false` | AI-агент не участвует в замерах |
| `BackgroundServices__ExpiredOrders__Enabled=false` | cleanup не мешает прогонам |
| `RateLimiting__Enabled=false` | весь трафик идёт с одного IP, лимиты логина исказили бы замер |
| `Smtp__Host=127.0.0.1`, `Smtp__Port=1` | письма падают мгновенно вместо таймаутов к Gmail |

## 3. Подготовить данные

```powershell
# боты: loadbot001@load.test ... loadbot040@load.test, пароль LoadTest123
.\loadtest\scripts\Seed-LoadTestUsers.ps1 -Count 40

# чистка транзакционных таблиц + остатки >= 1000 и ReservedQuantity = 0
.\loadtest\scripts\Reset-LoadTestData.ps1
```

Скрипт чистки также пересоздаёт ботам пустые корзины: `AddToCart` и `Checkout`
работают только с уже существующей строкой `Carts` и падают с 404/400, если её нет.

Почему боты подтверждаются в БД: `POST /api/cart/Checkout`, `POST /api/order` и
создание отзыва требуют `IsEmailConfirmed == true`, а эндпоинт подтверждения email
защищён `[Authorize]`. Скрипт регистрирует ботов через API (пароль хешируется
приложением) и затем одним `UPDATE` подтверждает им email.

## 4. Запуск

```powershell
# smoke (2 VU, ~35 сек на сценарий) + load, отчёты в loadtest/results
.\loadtest\scripts\Run-LoadTests.ps1 -Profile all

# только конкретные сценарии
.\loadtest\scripts\Run-LoadTests.ps1 -Profile load -Only 01-catalog.js,03-order-flow.js

# чистка данных перед каждым сценарием
.\loadtest\scripts\Run-LoadTests.ps1 -Profile load -ResetBeforeEach
```

Вручную:

```powershell
k6 run -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/01-catalog.js
k6 run -e PROFILE=smoke -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/04-mixed.js
k6 run --summary-export loadtest/results/catalog.json -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/01-catalog.js
```

Параметры через `-e`:

| Переменная | По умолчанию | Смысл |
|---|---|---|
| `BASE_URL` | `http://localhost:8080` | точка входа |
| `PROFILE` | `load` | `load` или `smoke` |
| `BOTS` | `40` | сколько ботов подготовлено |
| `LOADTEST_PASSWORD` | `LoadTest123` | пароль ботов |
| `BOT_PREFIX` / `BOT_DOMAIN` | `loadbot` / `load.test` | шаблон email |

## 5. Профили нагрузки

| Сценарий | VU | Длительность | Что проверяет |
|---|---|---|---|
| 01-catalog | 0→50→0 | 4 мин | чтение каталога, MemoryCache, p95 < 400 мс |
| 02-auth | 0→20→0 | 2 мин 10 с | логин (PBKDF2/BCrypt), p95 < 800 мс |
| 03-order-flow | 0→20→0 | 2 мин 40 с | блокировки строк, заказ + оплата, p95 < 1500 мс |
| 04-mixed | 0→50→0 | 4 мин | смешанный профиль, p95 < 800 мс |

Пороги заданы в `options.thresholds`: k6 вернёт код 99, если p95 или доля ошибок
хуже целевых — это же подходит для CI.

### Результаты прогона 2026-11-26

Стенд: Ryzen 7 5700X (8C/16T), 32 GB, Docker Desktop (WSL2), k6 на хосте.
Все профили `load`, с `-ResetBeforeEach`, 40 ботов.

| Сценарий | VU | Длительность | Запросов | RPS | p50 | p95 | p99 | Ошибки |
|---|---|---|---|---|---|---|---|---|
| 01-catalog | 50 | 4 мин | 43 213 | 179.7 | 1.0 мс | 2.0 мс | 6.0 мс | 0% |
| 02-auth | 20 | 2 мин 10 с | 2 840 | 21.8 | 61 мс | 151 мс | 165 мс | 0% |
| 03-order-flow | 20 | 2 мин 40 с | 5 819 | 35.7 | 6.6 мс | 1.01 с | 1.01 с | 0% |
| 04-mixed | 50 | 4 мин | 28 549 | 118.2 | 1.2 мс | 147 мс | 1.01 с | 0% |

Ресурсы во время `04-mixed` (`Monitor-Resources.ps1`): API — 56% CPU в среднем
(пик 160%, то есть ~1.6 ядра) и 544–567 MB RSS; PostgreSQL — 10.5% CPU
(пик 21.9%), 40–42 MB, 6 соединений. Узкое место — CPU API, не БД.

p95/p99 в сценариях с оплатой упираются в искусственную задержку 1 с
`PaymentService.ConfirmPaymentAsync`; реальную работу API видно по `med`.

## 6. Что смотреть во время прогона

```powershell
# CPU/память API и PostgreSQL + число соединений, раз в 5 секунд
.\loadtest\scripts\Monitor-Resources.ps1 -DurationMinutes 6 -OutFile .\loadtest\results\monitor-mixed.csv

# остановить досрочно: создать файл .monitor-stop рядом с CSV
New-Item -ItemType File -Force .\loadtest\results\.monitor-stop

# соединения и ожидания PostgreSQL
docker exec -i online-store-db-1 psql -U postgres -d WebStoreProjectDb -c "SELECT state, wait_event_type, count(*) FROM pg_stat_activity GROUP BY 1,2 ORDER BY 3 DESC;"

# ошибки API
docker logs --tail 100 online-store-api-1
```

`Monitor-Resources.ps1` пишет CSV с колонками
`timestamp,api_cpu_pct,api_mem_mb,db_cpu_pct,db_mem_mb,pg_connections`
(числа в InvariantCulture, чтобы не ломались в русской локали).

## 7. Как читать результаты

- **01-catalog** в основном попадает в MemoryCache (`ICacheableQuery`, TTL 10 мин),
  поэтому p95 там единицы миллисекунд. Реальную работу БД показывает тег
  `GET /api/products/filter (uncached)` — 10% запросов с уникальным `SearchText`.
- **02-auth** ограничен CPU на хешировании пароля, а не БД.
- **03-order-flow** и **04-mixed** — запись: `Checkout` берёт `FOR UPDATE` на корзину
  и товары, поэтому рост p95 при увеличении VU — ожидаемое поведение.
- `DELETE /api/cart/clear` идемпотентен: 204 и на непустой, и на пустой, и на
  отсутствующей корзине (раньше отсутствующая давала 400). Поэтому `orderFlow`
  начинается с очистки и не спотыкается после `Reset-LoadTestData.ps1`.
- `POST /api/order/confirm` содержит искусственную задержку 1 с
  (`PaymentService.ConfirmPaymentAsync` — эмуляция платёжного шлюза), поэтому p95 в
  сценариях с оплатой включает эту секунду. Остальную работу БД/API удобнее смотреть
  по `med`/`p(90)`, а не по общему p95.
- Пул Npgsql по умолчанию 100 соединений на инстанс: при упоре в него в
  `pg_stat_activity` появится очередь.
- Всё крутится на одной машине (k6 + Docker + PostgreSQL), поэтому абсолютные RPS
  относительны. Сравнивать имеет смысл прогоны «до/после» на одинаковой конфигурации.

## 8. Типовые проблемы

| Симптом | Причина / решение |
|---|---|
| `failed to execute bake` при `docker compose build` | известная проблема на этой машине; собирайте `docker build -f WebApi/Dockerfile -t online-store-api .` и поднимайте с `--no-build` |
| `login failed ... 400` | боты не созданы: запустите `Seed-LoadTestUsers.ps1` |
| `checkout 400` | email не подтверждён или пустая корзина: перезапустите seed-скрипт, проверьте `Reset-LoadTestData.ps1` |
| `checkout 400` / `order 400` про остатки | остатки кончились: `Reset-LoadTestData.ps1 -MinimumStock 5000` |
| `no products returned` | БД пустая: поднимите стенд и дождитесь сидирования (1000 товаров) |

## 9. Rate limiting

Включён по умолчанию (секция `RateLimiting` в `WebApi/appsettings.json`):

| Политика | Эндпоинт | Лимит | Ключ |
|---|---|---|---|
| `auth-login` | `POST /api/auth/login` | 5/мин | IP |
| `auth-register` | `POST /api/auth/register` | 5/10 мин | IP |
| `auth-request-reset` | `POST /api/auth/change-password` | 3/15 мин | IP |
| `auth-submit-reset` | `POST /api/auth/reset-password` | 10/15 мин | IP |
| `email-resend` | `GET /api/auth/resend-confirmation-email` | 1/5 мин | userId |
| global | весь API | 600/мин | IP |

При превышении: `429`, заголовок `Retry-After` (секунды) и тело в формате
`ErrorResponseDto` с `errorCode = "RATE_LIMITED"`. `/health` исключён из лимитов.

- Пороги переопределяются конфигурацией: `RateLimiting__Login__PermitLimit=20`.
- В интеграционных тестах лимитер выключен (`RateLimiting:Enabled=false` в
  `CustomWebApplicationFactory`): счётчики живут в памяти процесса и не
  сбрасываются Respawn между тестами. Сам лимитер проверяет `RateLimitingTests`
  через `RateLimitedWebApplicationFactory`.
- В нагрузочных прогонах выключен (`RateLimiting__Enabled=false`), потому что
  весь трафик идёт с одного IP.
- Счётчики in-memory: при нескольких репликах фактический лимит умножается на их
  число. Для масштабирования нужен Redis-backed limiter или `limit_req` на ingress.
- Rate limiting не заменяет блокировку аккаунта: следующим шагом логично добавить
  счётчик неудачных попыток на пользователя (по аналогии с
  `PasswordResetCode.AttemptsCount`).
