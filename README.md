# Backend for an Online Store

Учебный интернет-магазин: backend на **ASP.NET Core 8** (Clean Architecture, CQRS/MediatR) и
frontend на **React 19 + TypeScript + Vite**.

Проект покрывает полный сценарий магазина: регистрация и вход по JWT, каталог и фильтры,
корзина, оформление и оплата заказа, избранное, отзывы с фото/видео, админ-панель и
AI-ассистент по каталогу.

> **Статус:** учебный/демонстрационный проект. Платёжный шлюз и часть интеграций —
> заглушки (см. раздел [Известные ограничения](#известные-ограничения)).

---

## Содержание

- [Стек](#стек)
- [Архитектура](#архитектура)
- [Возможности](#возможности)
- [Структура репозитория](#структура-репозитория)
- [Требования](#требования)
- [Быстрый старт (локально)](#быстрый-старт-локально)
- [Конфигурация](#конфигурация)
- [Запуск в Docker](#запуск-в-docker)
- [API](#api)
- [База данных](#база-данных)
- [Фоновые задачи и кэш](#фоновые-задачи-и-кэш)
- [Тесты](#тесты)
- [Известные ограничения](#известные-ограничения)
- [Troubleshooting](#troubleshooting)

---

## Стек

### Backend

| Компонент | Версия / назначение |
|---|---|
| .NET | 8.0 (проекты `Domain`, `Application`, `Infrastructure`, `WebApi`) |
| ASP.NET Core | Web API, JWT Bearer, Swagger |
| EF Core + Npgsql | 8.0.14 / 8.0.0, PostgreSQL |
| MediatR | 14.x — CQRS (команды/запросы) |
| FluentValidation | 12.x — валидация команд и запросов |
| AutoMapper | 16.x — маппинг DTO |
| Serilog | 10.x — консоль + файловые логи |
| BCrypt.Net-Next | хеширование паролей |
| MailKit | отправка email (SMTP) |
| Microsoft.Agents.AI + OpenAI SDK | AI-ассистент через Groq (OpenAI-совместимый API) |
| IPinfo | геолокация по IP |

### Frontend

| Компонент | Версия / назначение |
|---|---|
| React | 19 |
| TypeScript | 6 |
| Vite | 8 |
| React Router | 8 |
| TanStack Query | серверное состояние |
| Axios | HTTP-клиент (`baseURL = /api`) |
| Tailwind CSS | 4 |
| ESLint | 10 |

### Тесты

xUnit, FluentAssertions, Moq, EF InMemory, Testcontainers.PostgreSql + Respawn
(интеграционные тесты).

---

## Архитектура

Направление зависимостей строгое:

```
WebApi  ──►  Infrastructure  ──►  Application  ──►  Domain
   │                                  │
   └──────────────►───────────────────┘
```

| Проект | Ответственность | Зависит от |
|---|---|---|
| `Domain` | Сущности, enum'ы, доменные исключения, бизнес-инварианты | — |
| `Application` | MediatR CQRS (`Commands/`, `Queries/`), DTO, валидаторы, behaviors, интерфейсы репозиториев/сервисов | `Domain` |
| `Infrastructure` | EF Core (`AppDbContext`), миграции, репозитории, `UnitOfWork`, JWT, email, платежи, IP-геолокация, AI-агент, фоновые задачи | `Application`, `Domain` |
| `WebApi` | Контроллеры (`api/*`), middleware, DI-композиция, Swagger, Serilog, seeding | `Application`, `Infrastructure` |

Поток обработки запроса:

```
Controller → IMediator.Send(command/query) → ValidationBehavior → CachingBehavior
           → Handler → Repository / IUnitOfWork → PostgreSQL
```

Контроллеры не работают с `DbContext`/репозиториями напрямую.

Слои `Application` не должны ссылаться на `Infrastructure`. Единственное историческое
исключение — интерфейс `IDeviceInfoService`, объявленный в пространстве имён
`Infrastructure.Services` (см. [Известные ограничения](#известные-ограничения)).

---

## Возможности

- **Аутентификация и пользователи**
  - регистрация, вход, JWT Bearer;
  - подтверждение email по токену, повторная отправка письма;
  - сброс пароля по 6-значному коду (BCrypt-хеш, ограничение попыток, одноразовое использование);
  - роли `User` и `Admin`, блокировка/активность пользователя в доменной модели.
- **Каталог**
  - товары, категории, фильтры, сортировка, пагинация;
  - популярные товары, товары с низким остатком (админ);
  - загрузка изображений/видео товара.
- **Корзина и заказы**
  - корзина на пользователя, изменение количества, очистка;
  - checkout с резервированием остатков (пессимистичные блокировки `FOR NO KEY UPDATE`);
  - история заказов, отмена, получение заказа;
  - жизненный цикл заказа: создан → оплачен → отправлен → доставлен → получен.
- **Платежи**
  - инициация платежа, переиспользование свежего `Pending`-интента (< 30 минут);
  - подтверждение оплаты, идемпотентность, проверка владельца;
  - частичный уникальный индекс: один `Pending`-платёж на заказ.
- **Избранное** — добавление/удаление (идемпотентно), список, проверка.
- **Отзывы** — один отзыв на пользователя и товар, рейтинг, медиафайлы, ответ администратора.
- **Админ-панель** — CRUD товаров/категорий, заказы, пользователи, отзывы, дашборд
  (заказы за период, выручка, популярные товары).
- **AI-ассистент** — чат по каталогу (гость и авторизованный пользователь), история диалогов,
  инструменты поиска товаров/категорий.
- **Фон** — очередь email, очистка просроченных заказов, кодов сброса пароля и диалогов агента.

---

## Структура репозитория

```
.
├── Domain/                     # сущности и доменные правила
├── Application/                # CQRS, DTO, валидаторы, интерфейсы
│   ├── Behaviors/              # Validation, Caching, CacheInvalidation
│   ├── Commands/
│   ├── Queries/
│   ├── Validators/
│   └── Interfaces/
├── Infrastructure/             # EF Core, репозитории, сервисы, фоновые задачи
│   ├── Data/                   # AppDbContext, SeedData, ProductSeeder
│   ├── Migrations/             # EF-миграции
│   ├── Repositories/
│   ├── Services/
│   ├── Cleanup/
│   └── Options/
├── WebApi/                     # контроллеры, middleware, Program.cs
│   ├── Controllers/
│   ├── Middleware/
│   └── appsettings.json
├── DomainTests/                # unit-тесты домена
├── ApplicationTests/           # unit-тесты Application (EF InMemory / Moq)
├── InfrastructureTests/        # unit-тесты Infrastructure
├── WebAPITests/                # интеграционные тесты (Testcontainers + Respawn)
├── frontend/                   # React + Vite приложение
├── docker-compose.yml          # PostgreSQL + API + nginx + Caddy
├── Caddyfile                   # TLS reverse proxy
├── .env.example                # шаблон секретов
└── Backend_for_an_online_store.sln
```

### Frontend

Исходники организованы по фичам:

```
frontend/src/
├── app/                        # провайдеры, роутинг, layout
├── features/
│   ├── admin/
│   ├── agent-chat/
│   ├── auth/
│   ├── cart/
│   ├── checkout/
│   ├── current-order/
│   ├── favorites/
│   ├── orders/
│   ├── products/
│   └── reviews/
└── shared/                     # api-клиент, ui-компоненты, утилиты
```

Axios-клиент (`shared/api/client.ts`) ходит на `/api`, подставляет JWT из
`localStorage["access_token"]` и при `401` очищает токен.

---

## Требования

- **.NET SDK 8** (проекты `net8.0`) и **.NET SDK 9** — тестовые проекты на `net9.0`;
  удобнее держать оба.
- **PostgreSQL 16+** (проверено на 18). База по умолчанию: `WebStoreProjectDb`, пользователь
  `postgres`.
- **Node.js 20+** и npm (для frontend).
- **Docker + Docker Compose** — опционально, для контейнерного запуска и интеграционных тестов.
- `dotnet-ef`:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

---

## Быстрый старт (локально)

### 1. PostgreSQL

Создайте базу (если её ещё нет):

```powershell
psql -h localhost -U postgres -c "CREATE DATABASE \"WebStoreProjectDb\";"
```

Строка подключения лежит в `WebApi/appsettings.json`:

```
Host=localhost;Port=5432;Database=WebStoreProjectDb;Username=postgres
```

Пароль **не хранится в файле** — `Infrastructure/DependencyInjection.cs` дописывает
`;Password=<DB_PASSWORD>` из переменной окружения.

### 2. Переменные окружения

PowerShell:

```powershell
$env:DB_PASSWORD  = 'ваш-пароль-postgres'
$env:JWT__KEY     = 'случайная-строка-минимум-32-байта'
```

Обязательные:

- `DB_PASSWORD` — иначе приложение падает на старте;
- `JWT__KEY` — минимум 32 байта (256 бит), иначе `JwtOptions.Load` бросает исключение.

Опционально (нужны только для соответствующих фич):

```powershell
$env:GROQ_API_KEY   = 'gsk_...'          # AI-ассистент
$env:Smtp__Password = 'app-password'     # отправка писем
$env:IpInfo__ApiKey = '...'              # геолокация по IP
```

> `JWT__KEY` — это переменная окружения ASP.NET Core: двойное подчёркивание превращается
> в конфигурацию `Jwt:Key` и перекрывает `appsettings.json`.

### 3. Миграции

```powershell
dotnet ef database update --project Infrastructure --startup-project WebApi
```

EF-инструменты выполняют `Program.cs` до `builder.Build()`, поэтому `DB_PASSWORD` и
`JWT__KEY` должны быть заданы и для этой команды.

Если приложение уже запущено и блокирует `WebApi.exe`, используйте:

```powershell
dotnet ef database update --no-build --project Infrastructure --startup-project WebApi
```

### 4. Backend

```powershell
dotnet run --project WebApi --launch-profile https
```

- API: `https://localhost:7197`
- Swagger (только Development): `https://localhost:7197/swagger`

Используйте профиль `https` — Vite-прокси настроен именно на `https://localhost:7197`.
Профиль `https` идёт первым в `launchSettings.json`, поэтому `dotnet run --project WebApi`
без `--launch-profile` тоже поднимает API на 7197. Профиль `http` (порт 5277) фронтендом
не используется.

При старте выполняется seed:

- администратор `admin@store.com` / `Admin123!`;
- категории;
- 1000 демо-товаров (проверка по количеству, без advisory-блокировки — см.
  [Известные ограничения](#известные-ограничения)).

Базовый seed (админ + категории) выполняется в транзакции под
`pg_advisory_xact_lock(727001)`.

### 5. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Vite отдаёт приложение на `http://localhost:5173` и проксирует `/api` на
`https://localhost:7197` (самоподписанный сертификат разрешён).

Другие команды:

```powershell
npm run build    # tsc -b && vite build
npm run lint     # 0 ошибок (несколько warning'ов)
npm run preview
```

---

## Конфигурация

### Переменные окружения

| Переменная | Обязательна | Назначение |
|---|---|---|
| `DB_PASSWORD` | да | Пароль PostgreSQL; дописывается к connection string |
| `JWT__KEY` | да | Ключ подписи JWT, ≥ 32 байт |
| `JWT__ISSUER` | нет | Issuer токена (по умолчанию `MyAuthServer`) |
| `JWT__AUDIENCE` | нет | Audience токена (по умолчанию `MyAuthClient`) |
| `GROQ_API_KEY` | для агента | Ключ Groq |
| `Smtp__Password` | для email | Пароль приложения Gmail |
| `IpInfo__ApiKey` | для геолокации | Ключ ipinfo.io |
| `Database__MigrateOnStartup` | нет | `true` — применять миграции при старте (в Docker включено) |

В `docker-compose.yml` дополнительно поддерживаются `JWT_KEY` (legacy fallback),
`SMTP_PASSWORD`, `IPINFO_API_KEY`, `DOMAIN`, `POSTGRES_DB`, `POSTGRES_USER`.

### Секции `appsettings.json`

| Секция | Что настраивает |
|---|---|
| `App` | `BaseUrl` (используется в письмах) |
| `ConnectionStrings:DefaultConnection` | PostgreSQL без пароля |
| `Jwt` | `Issuer`, `Audience`; ключ — только через `JWT__KEY` |
| `Smtp` | SMTP-сервер, учётные данные, отправитель |
| `IpInfo` | `ApiKey`, `BaseUrl`, `TimeoutSeconds`, `Enabled` |
| `Agent` | хранение диалогов гостя, интервал очистки |
| `BackgroundServices:ExpiredOrders` | таймаут оплаты (30 мин), интервал проверки (15 мин) |
| `Serilog` | уровни логирования и файловые синки |

Логи пишутся в `logs/all/log-*.txt` и `logs/errors/error-*.txt`.

---

## Запуск в Docker

```powershell
Copy-Item .env.example .env
# заполните DB_PASSWORD, JWT__KEY и при необходимости остальное
docker compose up -d --build
docker compose ps            # api/web/db должны стать healthy
```

Compose-проект называется `online-store` (задано через `name:`), поэтому имена
volume'ов стабильны и не зависят от пути к репозиторию.

Состав:

| Сервис | Описание |
|---|---|
| `db` | PostgreSQL 16-alpine, volume `pgdata`, healthcheck `pg_isready` |
| `api` | ASP.NET Core, non-root (`app`), `Database:MigrateOnStartup=true`, healthcheck `/health`, volumes `uploads` (wwwroot) и `logs` |
| `web` | frontend, собранный Vite и раздаваемый nginx, healthcheck |
| `caddy` | TLS-прокси на 80/443, автоматический сертификат Let's Encrypt, security-заголовки |

Порядок запуска: `db` (healthy) → `api` (healthy) → `web` (healthy) → `caddy`.
У всех сервисов ограничен размер docker-логов (`10 MB × 3`), у `api` —
`stop_grace_period: 30s`, чтобы фоновые задачи успели завершиться.

Домен задаётся в `.env` (`DOMAIN`). Для локального теста `DOMAIN=localhost` — Caddy
сгенерирует самоподписанный сертификат.

> Если volume `uploads`/`logs` уже создавался старой версией compose (от root),
> после перехода на non-root пользователя может потребоваться однократный
> `docker compose exec -u root api chown -R 1654 /app/wwwroot /app/logs`
> (либо удалить volume и дать ему пересоздаться).

Полезные команды:

```powershell
docker compose logs -f api
docker compose exec db psql -U postgres -d WebStoreProjectDb
docker compose down          # остановить
docker compose down -v       # остановить и удалить данные (volume'ы)
```

---

## API

Базовый префикс — `/api`. Аутентификация — `Authorization: Bearer <JWT>`.
Роли: `User`, `Admin`.

### Публичные / каталог

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/products` | Список товаров (пагинация) |
| `GET` | `/api/products/filter` | Фильтры: категория, цена, поиск, сортировка |
| `GET` | `/api/products/{productId}` | Товар по id |
| `GET` | `/api/products/popularProducts` | Популярные товары |
| `GET` | `/api/categories` | Все категории |
| `GET` | `/api/categories/{id}` | Категория по id |
| `GET` | `/api/review/{productId}` | Отзывы товара |

### Аутентификация — `/api/auth`

| Метод | Путь | Доступ | Описание |
|---|---|---|---|
| `POST` | `/register` | аноним | Регистрация, возвращает JWT |
| `POST` | `/login` | аноним | Вход |
| `POST` | `/change-password` | аноним | Запрос кода сброса пароля на email |
| `POST` | `/reset-password` | аноним | Сброс пароля по коду |
| `GET` | `/profile` | `[Authorize]` | Профиль текущего пользователя |
| `GET` | `/confirm-email` | `[Authorize]` | Подтверждение email по `userId` + `token` |
| `GET` | `/resend-confirmation-email` | `[Authorize]` | Повторная отправка письма |

### Корзина — `/api/cart` (`[Authorize]`)

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/` | Корзина пользователя |
| `POST` | `/` | Добавить товар |
| `PUT` | `/items` | Изменить количество |
| `DELETE` | `/items/{productId}` | Удалить позицию |
| `DELETE` | `/clear` | Очистить корзину |
| `POST` | `/Checkout` | Оформить заказ из корзины |

### Заказы — `/api/order` (`[Authorize]`)

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/history` | История заказов (пагинация, период) |
| `GET` | `/{orderId}` | Заказ по id (с проверкой владельца) |
| `POST` | `/` | Создать заказ |
| `POST` | `/initiate` | Инициировать оплату |
| `POST` | `/confirm` | Подтвердить оплату |
| `POST` | `/receive/{orderId}` | Подтвердить получение |
| `PUT` | `/Cancel` | Отменить заказ |

### Избранное — `/api/favorites` (`[Authorize]`)

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/` | Список избранного |
| `POST` | `/{productId}` | Добавить |
| `DELETE` | `/{productId}` | Удалить (идемпотентно) |
| `GET` | `/{productId}/check` | Проверить наличие |

### Отзывы — `/api/review`

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/` | Отзывы текущего пользователя |
| `GET` | `/{productId}` | Отзывы товара (публично) |
| `POST` | `/` | Оставить отзыв |
| `PUT` | `/` | Редактировать отзыв |
| `DELETE` | `/{reviewId:int}` | Удалить отзыв |
| `POST` | `/{reviewId}/files` | Загрузить фото/видео |
| `DELETE` | `/{reviewId}/files` | Удалить файлы |

### AI-агент — `/api/agent` (анонимно)

| Метод | Путь | Описание |
|---|---|---|
| `POST` | `/chat` | Сообщение ассистенту (гость или пользователь) |
| `GET` | `/conversations` | Список диалогов |
| `GET` | `/conversations/{sessionId}` | Диалог с сообщениями |
| `PUT` | `/conversations/{sessionId}/title` | Переименовать диалог |
| `DELETE` | `/conversations/{conversationId}` | Удалить диалог |

Гостю выдаётся cookie `guest_id`; диалоги гостя хранятся 7 дней.

### Админка (`[Authorize(Roles = "Admin")]`)

| Префикс | Возможности |
|---|---|
| `/api/admin/product` | CRUD товаров, файлы, `GET filtered`, `GET low-stock/{limit}` |
| `/api/admin/category` | CRUD категорий |
| `/api/admin/order` | Список/детали заказов, `ship/{id}`, `deliver/{id}` |
| `/api/admin/users` | Список/детали, `promote`, `demote` |
| `/api/admin/reviews` | Модерация, ответ администратора |
| `/api/admin/dashboard` | Заказы за период, выручка, популярные товары |

---

## База данных

### Миграции

- Миграции лежат в `Infrastructure/Migrations`, применяются **вручную**
  (локально) или автоматически при `Database:MigrateOnStartup=true` (Docker).
- Всегда указывайте `--project Infrastructure --startup-project WebApi` — design-time
  фабрики нет, а `Microsoft.EntityFrameworkCore.Design` подключён только в `WebApi`.

```powershell
# применить
dotnet ef database update --project Infrastructure --startup-project WebApi

# добавить новую
dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi

# проверить, что модель и snapshot совпадают
dotnet ef migrations has-pending-model-changes --project Infrastructure --startup-project WebApi
```

### Основные таблицы

`Users`, `Categories`, `Products`, `Carts`, `CartItems`, `Orders`, `OrderItems`,
`Payments`, `Favorites`, `Reviews`, `PasswordResetCodes`, `AgentConversations`,
`AgentMessages`.

### Конкурентность и инварианты

- **Optimistic concurrency** через системную колонку PostgreSQL `xmin` (EF Core
  concurrency token) — для `Product`, `Order`, `Payment`, `Review`, `Category`, `User`,
  `PasswordResetCode`, `AgentConversation`.
- **Pessimistic locks** (`SELECT ... FOR NO KEY UPDATE` / `FOR UPDATE`) — только там, где
  это требуется инвариантом: checkout/резервирование остатков, инициация и подтверждение
  платежа, сброс пароля, удаление товара.
- **Уникальные индексы:**
  - `Users.Email`, `Users.UserName`;
  - `Categories.NormalizedName` — вычисляемая колонка `lower(btrim("Name"))`;
  - `Products.NormalizedSku` — вычисляемая колонка `lower(btrim("Sku"))`;
  - `Favorites (UserId, ProductId)`;
  - `Reviews (UserId, ProductId)` — один отзыв на товар;
  - `Payments (OrderId) WHERE Status = 0` — один `Pending`-платёж на заказ.
- **Базовый seed** (`SeedData`: админ, категории) защищён advisory-блокировкой
  `pg_advisory_xact_lock(727001)` и выполняется в транзакции, чтобы несколько инстансов не
  создавали дубликаты. `ProductSeeder` этой блокировкой не покрыт.
- PostgreSQL-ошибки маппятся в HTTP в `ExceptionHandlingMiddleware`:
  `23505 → 409 DUPLICATE_ENTRY`, `23503 → 409 REFERENCE_CONFLICT`,
  `40001/40P01 → 409 CONCURRENCY_RETRY`, `DbUpdateConcurrencyException → 409`.

---

## Фоновые задачи и кэш

| Компонент | Назначение |
|---|---|
| `EmailBackgroundTaskQueue` + `EmailBackgroundService` | Очередь email (bounded, 100), один reader |
| `CleanupBackgroundService` | Планировщик периодических задач |
| `ExpiredOrdersCleanupTask` | Отмена неоплаченных заказов и возврат остатков |
| `PasswordResetCodesCleanupTask` | Удаление просроченных кодов сброса |
| `ExpiredAgentConversationsCleanupTask` | Удаление старых диалогов гостя |
| `IMemoryCache` + `CachingBehavior` | Кэширование запросов (`ICacheableQuery`) и инвалидация (`ICacheInvalidateCommand`) |

---

## Тесты

```powershell
# все тесты
dotnet test Backend_for_an_online_store.sln

# только unit-проекты
dotnet test DomainTests
dotnet test ApplicationTests
dotnet test InfrastructureTests

# один класс
dotnet test WebAPITests --filter "FullyQualifiedName~AuthFlowTests"
```

Особенности:

- `DomainTests`, `ApplicationTests`, `InfrastructureTests` — EF InMemory/Moq, без внешних
  сервисов.
- `WebAPITests` — Testcontainers.PostgreSql + Respawn, **нужен Docker**, тесты медленные.
  После Respawn сид-админ удаляется, поэтому админ-тесты повышают пользователя и логинятся
  заново.
- Тестовые проекты таргетят `net9.0`, приложение — `net8.0`.

---

## Известные ограничения

### Безопасность

- **Загрузка файлов** проверяет только клиентский `Content-Type`, а расширение берётся из
  имени файла (`WebApi/Common/FileUploadValidation.cs`,
  `Infrastructure/Services/LocalFileStorageService.cs`). Разрешён `image/svg+xml`.
  Файлы раздаются из `wwwroot` через `UseStaticFiles`. Это позволяет stored XSS —
  в продакшене нужен белый список расширений, проверка сигнатур, `nosniff`/CSP и отдельный
  домен для uploads.
- **Rate limiting не настроен** — `login`, `register`, сброс пароля и `/api/agent/chat`
  не ограничены. Атрибут `[EnableRateLimiting]` в `UserController` закомментирован.
- **JWT живёт 1 месяц** (`JwtService`), refresh-токенов и отзыва нет; сброс пароля не
  инвалидирует выданные токены. `AuthResponseDto.ExpiresIn` при этом равен `AddHours(1)`.
- **Ссылки в письмах** строятся из `Request.Host` (`UserController`, `OrderController`,
  `CartController`) при `UseForwardedHeaders` с очищенными `KnownProxies` — возможна
  подмена хоста в письмах (сброс пароля, подтверждение email). Надёжнее использовать
  `App:BaseUrl` из конфигурации.
- `GET /api/auth/confirm-email` помечен `[Authorize]`, поэтому переход по ссылке из
  письма в браузере возвращает `401`. Нужен анонимный эндпоинт или редирект на SPA.
- `UnauthorizedAccessException` (System) не маппится в `ExceptionHandlingMiddleware` —
  в ряде сценариев клиент получает `500` вместо `403`.

### Данные

- `CartItem.ProductId`, `OrderItem.ProductId` и `Review.UserId` — не-nullable, но связи
  сконфигурированы как `DeleteBehavior.SetNull`. Удаление товара, лежащего в корзине,
  или пользователя с отзывами приводит к нарушению `NOT NULL` (`23502`).
- Платёжные стратегии — демо-заглушки (`Task.Delay`, `Success = true`), реальный шлюз не
  подключён.

### Frontend

- `npm run lint` проходит: 0 ошибок, остаются только warning'и
  `react-hooks/exhaustive-deps` и `react-refresh/only-export-components`.
- `vite build` предупреждает о чанке > 500 kB — это не ошибка; при необходимости
  можно включить code splitting.

### Инфраструктура

- Базовые образы зафиксированы на .NET 8. Поддержка .NET 8 LTS завершилась 10.11.2026,
  поэтому образы больше не получают security-патчи. Планируемый переход — .NET 10 LTS
  (не .NET 9: его поддержка закончилась в мае 2026).
- Compose использует `postgres:16-alpine`, локальная разработка — PostgreSQL 18;
  при переносе дампов между версиями учитывайте разницу мажоров.
- `WebApi/wwwroot` исключён из образа: локальные загрузки не попадают в Docker,
  для них используется volume `uploads`.

### Прочее

- `UpdateProfileCommandHandler` — пустая заготовка; эндпоинта обновления профиля нет.
- Блокировка/разблокировка пользователей в `AdminUsersController` закомментирована.
- Пустые тесты-заглушки: `ApplicationTests/Cart/CheckoutHandlerTests.cs` (пустой файл),
  `CancelOrderHandlerTests.cs`, `UpdateProductHandlerTests.cs`, `WebAPITests/UnitTest1.cs`.
- `CategoryRepository.DeleteAllAsync` не используется.
- `ApplicationTests` ссылается на `Infrastructure` (необычно для unit-проекта).
- В корне лежат посторонние файлы: `OrderTests.txt`, `WebApi/web_store.db`,
  `WebApi/web_store.sql`, `WebApi/data.sql` (артефакты SQLite), а также `package.json`/
  `node_modules` в корне (Tailwind-зависимости, дублирующие `frontend`).
- CI (`.github/workflows`) не настроен.

---

## Troubleshooting

| Симптом | Причина / решение |
|---|---|
| `28P01: password authentication failed` | Неверный `DB_PASSWORD`. Проверьте `$env:DB_PASSWORD` и пользователя PostgreSQL |
| `JWT signing key is not configured` | Не задан `JWT__KEY` (≥ 32 байт) |
| `Password not found. Set DB_PASSWORD...` | Не задан `DB_PASSWORD` |
| `dotnet ef` падает на `Program.cs` | EF выполняет startup-проект; задайте `DB_PASSWORD` и `JWT__KEY` перед командой |
| `The process cannot access the file 'WebApi.exe'` | Запущено приложение. Остановите его или используйте `dotnet ef ... --no-build` |
| Frontend не видит API | Запускайте backend в профиле `https` (7197), а не `http` (5277) |
| `database "WebStoreProjectDb" does not exist` | Создайте БД и примените миграции |
| Агент отвечает ошибкой | Не задан `GROQ_API_KEY` |
| Письма не отправляются | Пустой `Smtp:Password` / `Smtp__Password` |
| Тесты `WebAPITests` падают | Не запущен Docker / нет прав на Testcontainers |

---

## Лицензия

Учебный проект; отдельная лицензия не указана.
