-- Отключаем проверку внешних ключей для создания таблиц
SET session_replication_role = 'replica';

-- Таблица CartItems
CREATE TABLE IF NOT EXISTS "CartItems" (
    "Id" SERIAL PRIMARY KEY,
    "CartId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CartItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE SET NULL
);

-- Таблица Carts
CREATE TABLE IF NOT EXISTS "Carts" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_Carts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "IX_Carts_UserId" UNIQUE ("UserId")
);

-- Таблица Categories
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL
);

-- Таблица OrderItems
CREATE TABLE IF NOT EXISTS "OrderItems" (
    "Id" SERIAL PRIMARY KEY,
    "OrderId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "ProductNameAtPurchase" TEXT NOT NULL,
    "PriceAtPurchase" DECIMAL(18,2) NOT NULL,
    "PurchasePriceAtPurchase" DECIMAL(18,2) NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE SET NULL
);

-- Таблица Orders
CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "ShippingAddress" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "PaidAt" TIMESTAMP,
    CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Таблица Payments
CREATE TABLE IF NOT EXISTS "Payments" (
    "Id" SERIAL PRIMARY KEY,
    "OrderId" INTEGER NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "Method" INTEGER NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "ExternalTransactionId" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL,
    "PaidAt" TIMESTAMP,
    "ErrorMessage" TEXT,
    CONSTRAINT "FK_Payments_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders"("Id") ON DELETE CASCADE
);

-- Таблица Products
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Price" DECIMAL(18,2) NOT NULL,
    "PurchasePrice" DECIMAL(18,2) NOT NULL,
    "StockQuantity" INTEGER NOT NULL,
    "ReservedQuantity" INTEGER NOT NULL,
    "AmountOfReceived" INTEGER NOT NULL,
    "AmountOfPaid" INTEGER NOT NULL,
    "AmountOfCanceled" INTEGER NOT NULL,
    "Sku" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    "CategoryId" INTEGER,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories"("Id") ON DELETE SET NULL
);

-- Таблица Reviews
CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    "Rating" INTEGER NOT NULL,
    "IsVerifiedPurchase" BOOLEAN NOT NULL,
    "Status" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    "AdminResponse" TEXT,
    "AdminResponseAt" TIMESTAMP,
    CONSTRAINT "FK_Reviews_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Таблица Users
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "UserName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "BlockReason" TEXT,
    "BlockedAt" TIMESTAMP,
    "IsEmailConfirmed" BOOLEAN NOT NULL,
    "EmailConfirmedAt" TIMESTAMP,
    "EmailConfirmationToken" TEXT,
    "EmailConfirmationTokenExpiry" TIMESTAMP,
    CONSTRAINT "IX_Users_Email" UNIQUE ("Email"),
    CONSTRAINT "IX_Users_UserName" UNIQUE ("UserName")
);

-- Таблица миграций EF
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

-- Включаем проверку внешних ключей
SET session_replication_role = 'origin';

-- Вставка данных
-- Categories
INSERT INTO "Categories" ("Id", "Name", "Description", "CreatedAt") VALUES 
(1, 'Электроника', 'Смартфоны, ноутбуки, планшеты', '2026-08-27 15:46:21.392514'),
(2, 'Одежда', 'Мужская и женская одежда', '2026-08-27 15:46:21.3925324'),
(3, 'Книги', 'Художественная и техническая литература', '2026-08-27 15:46:21.3925327'),
(4, 'Дом и сад', 'Мебель, инструменты, растения', '2026-08-27 15:46:21.3925329'),
(5, 'test category', 'test', '2026-08-28 07:56:27.7332188');

-- Users
INSERT INTO "Users" ("Id", "UserName", "Email", "PasswordHash", "Role", "CreatedAt", "IsActive", "BlockReason", "BlockedAt", "IsEmailConfirmed", "EmailConfirmedAt", "EmailConfirmationToken", "EmailConfirmationTokenExpiry") VALUES 
(2, 'Admin', 'admin@store.com', '$2a$11$8XAh/gNUnXZqHNKglC3lSO0F0zXDDVVHGMtYxWc4JTHSySNE6V99.', 1, '2026-08-27 15:46:21.276111', true, NULL, NULL, true, '2026-08-27 15:46:21.2768385', NULL, NULL),
(3, 'art', 'salart@2006.com', '$2a$11$6muoI.Jdpows88UG.7cWbe2ionxma..UaN03ZbEl8nDhDD7jfyKPy', 0, '2026-08-28 08:16:45.9054076', true, NULL, NULL, true, '2026-08-28 15:46:21.2768385', 'T9PdOKLE1id6bU5iw2QUclGn5KKXlZgzodmmg0xkppA', '2026-08-29 08:16:45.9059163'),
(4, 'string', 'user@example.com', '$2a$11$.LVQidYgWmVs.RZq/2DPEO6d.e47/HySvksObs3d862/UBbSTfbm6', 0, '2026-08-28 08:53:19.3468651', true, NULL, NULL, false, NULL, 'Qk3XCpYkLVcYHEid9ZTe5rl7Wu9_j9PCqcvaPwc62aU', '2026-08-29 08:53:19.3472308'),
(5, 'dgfg', 'fgdg5646le@.com', '$2a$11$rjv4yW2CEXV7T105z12yleDt3TQTKmHehl.2svx2zX2T4oSXiSbk.', 0, '2026-08-28 08:53:48.8885042', true, NULL, NULL, false, NULL, 'UoIa1nDz83I9YcGA3wSQboAURRRHo3yFsA-0GR_DVsw', '2026-08-29 08:53:48.8885874'),
(6, 'dgfg1', 'fgdg5646gfgfle@.com', '$2a$11$w4AE8KTtdMvO83/cmRa5M.X./KEBuV3ESL3yaSywRaJdsWoaDDaU2', 0, '2026-08-28 08:54:04.7216091', true, NULL, NULL, false, NULL, 'GjYspTLcRr1LRaQBcVJg27dy8kUbcclVQMWWXJuhj0M', '2026-08-29 08:54:04.7216236'),
(7, 'dgfgg1', 'fgdgg5646gfgfle@.com', '$2a$11$pLd6M98WRSP35qHzY3UwHeW4NsQtI0Di.hTSK29x6VHJzNmOORxiO', 0, '2026-08-28 08:54:11.0122647', true, NULL, NULL, false, NULL, 'MPgef5FMqQEkPkgwGK8aKRfIgcHLKSeK5Vin9YAD6y0', '2026-08-29 08:54:11.0122767'),
(8, 'dgfggh1', 'fgdgg56h46gfgfle@.com', '$2a$11$aLvqW4z/ZqcdfDPy.4iB8eXsS/5G98/.n6BTgBrUNO.o.IDFTKTUi', 0, '2026-08-28 08:54:21.5515823', true, NULL, NULL, false, NULL, '0y_dOZgRoyc8JazH5X5XXkp3QyeeAs0D8oHJG4twjx4', '2026-08-29 08:54:21.5515936');

-- Восстанавливаем последовательности для SERIAL полей
SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id") FROM "Categories"));
SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users"));

-- Products
INSERT INTO "Products" ("Id", "Name", "Description", "Price", "PurchasePrice", "StockQuantity", "ReservedQuantity", "AmountOfReceived", "AmountOfPaid", "AmountOfCanceled", "Sku", "CreatedAt", "UpdatedAt", "CategoryId") VALUES 
(1, 'iPhone 15 Pro', 'Последний iPhone', 999.99, 750.0, 10, 10, 0, 0, 0, NULL, '2026-08-27 15:46:21.4441161', '2026-08-27 15:46:21.4441297', 1),
(2, 'Samsung Galaxy S24', 'Флагман Samsung', 899.99, 650.0, 12, 0, 0, 3, 0, NULL, '2026-08-27 15:46:21.4441499', '2026-08-27 15:46:21.44415', 1),
(3, 'MacBook Pro 14"', 'Профессиональный ноутбук', 1999.99, 1400.0, 5, 0, 0, 0, 0, NULL, '2026-08-27 15:46:21.4441506', '2026-08-27 15:46:21.4441506', 1),
(4, 'Футболка хлопковая', 'Качественная футболка', 29.99, 15.0, 40, 0, 8, 10, 0, NULL, '2026-08-27 15:46:21.444152', '2026-08-27 15:46:21.444152', 2),
(5, 'Джинсы классические', 'Синие джинсы', 79.99, 40.0, 30, 30, 0, 0, 0, NULL, '2026-08-27 15:46:21.4441522', '2026-08-27 15:46:21.4441523', 2),
(6, 'Clean Architecture', 'Книга Роберта Мартина', 49.99, 25.0, 18, 0, 0, 2, 0, NULL, '2026-08-27 15:46:21.4441528', '2026-08-27 15:46:21.4441528', 3),
(7, 'C# 12 и .NET 8', 'Современный C#', 64.99, 35.0, 15, 0, 0, 0, 0, NULL, '2026-08-27 15:46:21.444153', '2026-08-27 15:46:21.444153', 3);

SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products"));

-- Carts
INSERT INTO "Carts" ("Id", "UserId", "UpdatedAt") VALUES 
(1, 3, '2026-09-01 07:43:21.7187649'),
(2, 4, '2026-08-28 08:53:19.3468663'),
(3, 5, '2026-08-28 08:53:48.888505'),
(4, 6, '2026-08-28 08:54:04.7216102'),
(5, 7, '2026-08-28 08:54:11.0122656'),
(6, 8, '2026-08-28 08:54:21.5515833');

SELECT setval('"Carts_Id_seq"', (SELECT MAX("Id") FROM "Carts"));

-- CartItems
INSERT INTO "CartItems" ("Id", "CartId", "ProductId", "Quantity") VALUES 
(6, 1, 3, 3),
(7, 1, 4, 3),
(8, 1, 5, 3);

SELECT setval('"CartItems_Id_seq"', (SELECT MAX("Id") FROM "CartItems"));

-- Orders
INSERT INTO "Orders" ("Id", "UserId", "TotalAmount", "ShippingAddress", "Status", "CreatedAt", "PaidAt") VALUES 
(1, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1081905', NULL),
(2, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1433872', NULL),
(3, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1561971', NULL),
(4, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1677703', NULL),
(5, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1795412', NULL),
(6, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.1908811', NULL),
(7, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.2022849', NULL),
(8, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.2136233', NULL),
(9, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.2249667', NULL),
(10, 2, 999.99, 'string', 0, '2026-08-28 08:59:21.2361181', NULL),
(11, 3, 79.99, 'string', 0, '2026-08-28 14:54:12.9512289', NULL),
(12, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.0773319', NULL),
(13, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.0940904', NULL),
(14, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.109889', NULL),
(15, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.1264311', NULL),
(16, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.1421815', NULL),
(17, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.1576356', NULL),
(18, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.1742823', NULL),
(19, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.1900908', NULL),
(20, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.2061559', NULL),
(21, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.2222934', NULL),
(22, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.2384819', NULL),
(23, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.2557325', NULL),
(24, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.273315', NULL),
(25, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.2906787', NULL),
(26, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.3072865', NULL),
(27, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.3242568', NULL),
(28, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.341473', NULL),
(29, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.3602356', NULL),
(30, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.3771549', NULL),
(31, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.3933364', NULL),
(32, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.410738', NULL),
(33, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.4279515', NULL),
(34, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.4447903', NULL),
(35, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.4615226', NULL),
(36, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.4776025', NULL),
(37, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.4943652', NULL),
(38, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.5112799', NULL),
(39, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.5281175', NULL),
(40, 3, 79.99, 'string', 0, '2026-08-28 14:54:13.5447382', NULL),
(41, 3, 239.92, 'My house', 4, '2026-08-29 06:09:34.4867584', '2026-08-29 06:53:41.7361396'),
(42, 3, 2859.93, 'Me dgfd', 1, '2026-08-29 15:44:07.003579', '2026-08-29 15:48:58.2470315');

SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders"));

-- OrderItems
INSERT INTO "OrderItems" ("Id", "OrderId", "ProductId", "ProductNameAtPurchase", "PriceAtPurchase", "PurchasePriceAtPurchase", "Quantity", "CreatedAt") VALUES 
(1, 1, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1078294'),
(2, 2, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1433831'),
(3, 3, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1561878'),
(4, 4, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1677676'),
(5, 5, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1795384'),
(6, 6, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.1908785'),
(7, 7, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.2022825'),
(8, 8, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.213621'),
(9, 9, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.2249645'),
(10, 10, 1, 'iPhone 15 Pro', 999.99, 750.0, 1, '2026-08-28 08:59:21.2361161'),
(11, 11, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:12.9508473'),
(12, 12, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.0773278'),
(13, 13, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.0940869'),
(14, 14, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.1098861'),
(15, 15, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.1264286'),
(16, 16, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.1421789'),
(17, 17, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.157633'),
(18, 18, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.1742797'),
(19, 19, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.1900882'),
(20, 20, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.2061533'),
(21, 21, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.2222907'),
(22, 22, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.238478'),
(23, 23, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.2557246'),
(24, 24, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.2733115'),
(25, 25, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.2906757'),
(26, 26, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3072837'),
(27, 27, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3242531'),
(28, 28, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3414701'),
(29, 29, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3602319'),
(30, 30, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3771516'),
(31, 31, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.3933331'),
(32, 32, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4107344'),
(33, 33, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4279478'),
(34, 34, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4447868'),
(35, 35, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4615193'),
(36, 36, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4775994'),
(37, 37, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.4943622'),
(38, 38, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.5112762'),
(39, 39, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.5281149'),
(40, 40, 5, 'Джинсы классические', 79.99, 40.0, 1, '2026-08-28 14:54:13.544733'),
(41, 41, 4, 'Футболка хлопковая', 29.99, 15.0, 8, '2026-08-29 06:09:34.4863468'),
(42, 42, 2, 'Samsung Galaxy S24', 899.99, 650.0, 3, '2026-08-29 15:44:07.003212'),
(43, 42, 4, 'Футболка хлопковая', 29.99, 15.0, 2, '2026-08-29 15:44:07.0035448'),
(44, 42, 6, 'Clean Architecture', 49.99, 25.0, 2, '2026-08-29 15:44:07.0035467');

SELECT setval('"OrderItems_Id_seq"', (SELECT MAX("Id") FROM "OrderItems"));

-- Payments
INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Status", "Method", "TransactionId", "ExternalTransactionId", "CreatedAt", "PaidAt", "ErrorMessage") VALUES 
(1, 41, 239.92, 1, 1, 'google_41_0cff960390eb40bd961dbe975bf096f2', 'google_41_0cff960390eb40bd961dbe975bf096f2', '2026-08-29 06:12:54.8854715', '2026-08-29 06:53:41.7327063', NULL),
(2, 42, 2859.93, 1, 3, 'sbp_42_45fd45d5ca054133b1db72960a8b3e1f', 'sbp_42_45fd45d5ca054133b1db72960a8b3e1f', '2026-08-29 15:48:05.3045506', '2026-08-29 15:48:58.2438039', NULL);

SELECT setval('"Payments_Id_seq"', (SELECT MAX("Id") FROM "Payments"));

-- Reviews
INSERT INTO "Reviews" ("Id", "UserId", "ProductId", "Text", "Rating", "IsVerifiedPurchase", "Status", "CreatedAt", "UpdatedAt", "AdminResponse", "AdminResponseAt") VALUES 
(1, 3, 4, 'nice T-shirt', 5, true, 1, '2026-08-29 08:33:34.9476966', '2026-08-29 13:18:42.0360273', 'Thank you for you comment', '2026-08-29 13:18:42.0360059'),
(2, 3, 4, 'just one more comment', 5, true, 1, '2026-08-29 13:14:04.4413178', '2026-08-29 13:14:04.4413397', NULL, NULL);

SELECT setval('"Reviews_Id_seq"', (SELECT MAX("Id") FROM "Reviews"));

-- EF Migrations History
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260826152621_First', '8.0.14');

-- Создаем индексы
CREATE INDEX IF NOT EXISTS "IX_CartItems_CartId" ON "CartItems" ("CartId");
CREATE INDEX IF NOT EXISTS "IX_CartItems_ProductId" ON "CartItems" ("ProductId");
CREATE INDEX IF NOT EXISTS "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");
CREATE INDEX IF NOT EXISTS "IX_Orders_UserId" ON "Orders" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Payments_OrderId" ON "Payments" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_Products_CategoryId" ON "Products" ("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_ProductId" ON "Reviews" ("ProductId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_UserId" ON "Reviews" ("UserId");