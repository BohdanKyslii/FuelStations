# FuelStations — сервіс АЗС і цін на пальне

REST API для керування АЗС, довідником видів пального, цінами та наявністю пального.
Тестове завдання: ASP.NET Core Web API + PostgreSQL + Entity Framework Core + Swagger.

> **EN:** REST API for fuel stations, a fuel type reference table, prices and availability.
> ASP.NET Core 10 Web API, PostgreSQL (Docker locally, Neon in the cloud), EF Core migrations,
> Swagger UI, DTO validation with Ukrainian error messages, `.http` scenarios with automated checks.

## Зміст

- [Стек](#стек)
- [Швидкий старт з Docker](#швидкий-старт-з-docker)
- [Запуск з хмарною БД Neon](#запуск-з-хмарною-бд-neon)
- [Як перевірити](#як-перевірити)
- [Ендпоінти](#ендпоінти)
- [Бізнес-модель і рішення](#бізнес-модель-і-рішення)
- [Схема БД](#схема-бд)
- [Валідація і помилки](#валідація-і-помилки)
- [Структура проєкту](#структура-проєкту)
- [Якість коду](#якість-коду)
- [Що можна покращити](#що-можна-покращити)

## Стек

| Що | Чим |
|---|---|
| Платформа | .NET 10, C#, ASP.NET Core Web API (контролери) |
| БД | PostgreSQL 17 у Docker (розробка), Neon — керований PostgreSQL (Production) |
| ORM | Entity Framework Core 10 + Npgsql, міграції в `FuelStations/Migrations` |
| Документація API | OpenAPI (`/openapi/v1.json`) + Swagger UI (`/swagger`), описи з XML-коментарів |
| Перевірка API | Swagger UI, HTTP-файли Rider з автоматичними перевірками |
| Якість коду | `.editorconfig`, `dotnet format`, Husky.Net (перевірка формату перед комітом) |

## Швидкий старт з Docker

Потрібні **.NET SDK 10** і **Docker Desktop**.

```powershell
git clone <url-репозиторію>
cd FuelStations
docker compose up -d
dotnet run --project FuelStations --launch-profile http
```

- Swagger UI: **http://localhost:5080/swagger**
- PostgreSQL у Docker: `localhost:5433`, база `fuel_stations`, користувач і пароль `fuel`.
- При першому запуску застосунок сам застосує міграції — таблиці створяться автоматично.
- **Тестові дані:** відкрий `FuelStations/demo-data.http` у Rider і натисни **Run All** —
  5 видів пального, 10 АЗС (реальні адреси мережі БРСМ), 40 цін. Запускати на порожній БД.

Повністю очистити БД: `docker compose down -v`, потім `docker compose up -d` і перезапуск API.

## Запуск з хмарною БД Neon

Секретів у репозиторії немає. Рядок підключення до Neon зберігається у файлі
`FuelStations/appsettings.Local.json`, який ігнорується Git. Шаблон — `FuelStations/appsettings.Local.example.json`.

1. Поклади отриманий файл `appsettings.Local.json` у папку `FuelStations/` поруч з `appsettings.json`.
2. Запусти профіль `neon` (середовище Production):

   ```powershell
   dotnet run --project FuelStations --launch-profile neon
   ```

3. У консолі має з'явитися `Environment: Production, database host: ep-...neon.tech`.
   Swagger: **http://localhost:5080/swagger**. Docker для цього режиму не потрібен.

| Профіль | Середовище | БД | Звідки рядок підключення |
|---|---|---|---|
| `http` | Development | Docker, `localhost:5433` | `appsettings.Development.json` |
| `neon` | Production | Neon | `appsettings.Local.json` або змінна оточення `ConnectionStrings__Default` |

Перший запит після паузи може тривати кілька секунд: безкоштовна БД Neon «прокидається».

## Як перевірити

**Swagger UI** (`/swagger`) — усі ендпоінти з описами українською та англійською:
розгорни ендпоінт → **Try it out** → **Execute**.

**HTTP-файли** (Rider / інші IDE JetBrains → **Run All**):

| Файл | Що робить |
|---|---|
| [`FuelStations/demo-data.http`](FuelStations/demo-data.http) | Наповнює порожню БД тестовими даними |
| [`FuelStations/FuelStations.http`](FuelStations/FuelStations.http) | 14 кроків з автоматичними перевірками (`client.test`): створення виду пального й АЗС з двома ID, ціна, фільтри, захист від видалення, 400 / 404. Прибирає за собою, тож запускається повторно |

Приклади, які варто спробувати в Swagger:

| Запит | Очікувано |
|---|---|
| `GET /api/stations/1030` і `GET /api/stations/1031` | Та сама АЗС «Київ Маяковського»: два ID, одна АЗС |
| `GET /api/stations?city=київ&fuelType=DPE_PLUS` | Лише київські АЗС, де ДПЕ Plus **є в наявності** |
| `PATCH /api/stations/1031/fuels/A95BIO` з `{ "price": 86.49 }` | Нова ціна видна і через ID `1030`; `priceUpdatedAt` оновився |
| `DELETE /api/fuel-types/DPE` | 400: вид продається на АЗС — його можна лише деактивувати |
| `POST /api/stations/1030/fuels` з `{ "fuelTypeCode": "A95BIO", "price": 0 }` | 400: «Ціна має бути більшою за нуль.» |
| `GET /api/stations/9999` | 404: «АЗС з ID 9999 не знайдено.» |

## Ендпоінти

### Види пального — `/api/fuel-types`

| Метод | Маршрут | Що робить | Відповіді |
|---|---|---|---|
| `GET` | `/api/fuel-types?includeInactive=false` | Список видів пального | 200 |
| `GET` | `/api/fuel-types/{code}` | Вид за кодом (`dpe_plus` знайде `DPE_PLUS`) | 200, 404 |
| `POST` | `/api/fuel-types` | Створити вид пального | 201, 400 |
| `PUT` | `/api/fuel-types/{code}` | Змінити назву та активність (код незмінний) | 200, 400, 404 |
| `DELETE` | `/api/fuel-types/{code}` | Видалити, якщо вид ніде не продається | 204, 400, 404 |

### АЗС — `/api/stations`

| Метод | Маршрут | Що робить |
|---|---|---|
| `GET` | `/api/stations?city=&isActive=&fuelType=&number=&page=&pageSize=` | Пошук із фільтрами та пагінацією |
| `GET` | `/api/stations/{number}` | АЗС за будь-яким її ID, з усіма ID, пальним і цінами |
| `POST` | `/api/stations` | Створити АЗС з одним або кількома ID |
| `PUT` | `/api/stations/{number}` | Змінити назву, місто, адресу, опис, статус |
| `DELETE` | `/api/stations/{number}` | Видалити АЗС разом з усіма ID і пальним |
| `POST` | `/api/stations/{number}/terminals` | Додати новий ID до наявної АЗС |
| `POST` | `/api/stations/{number}/fuels` | Додати вид пального з ціною |
| `PATCH` | `/api/stations/{number}/fuels/{fuelTypeCode}` | Змінити ціну та/або наявність |

Фільтр `fuelType` повертає лише АЗС, де це пальне **є в наявності**. Пагінація: `page` ≥ 1, `pageSize` 1–100
(за замовчуванням 20); відповідь містить `items`, `page`, `pageSize`, `totalCount`, `totalPages`.

## Бізнес-модель і рішення

- **ID АЗС.** Кожна АЗС має один або кілька чотиризначних ID (`1030`, `1031`) — по одному на вікно каси
  оператора: на екрані вміщується до 8 колонок, тож АЗС з 14 колонками отримує два ID. ID унікальні й незмінні,
  а API адресує АЗС за будь-яким з них.
- **Технічний ключ прихований.** `Stations.Id` потрібен лише БД для зв'язків; у DTO його немає.
- **Види пального — довідник у БД**, а не перелік у коді: новий вид додається через API без перевипуску програми.
  Код (`A95BIO`) — природний первинний ключ, приводиться до верхнього регістру.
- **Деактивація замість видалення.** Вид, що продається хоча б на одній АЗС, не видаляється (і API, і
  `ON DELETE RESTRICT` у БД), а лише деактивується; деактивований вид не можна додати на АЗС.
- **Ціна й наявність належать АЗС**, а не окремому ID: зміна через `1031` видна через `1030`.
- **Дата зміни ціни** оновлюється лише тоді, коли ціна справді змінилася; зміна наявності її не чіпає.
- **Правила продубльовано в БД** (унікальні індекси, check constraints, FK): захист від гонок запитів
  і змін в обхід API.
- **DTO** окремі для створення й редагування: коди пального й ID АЗС передаються лише при створенні,
  тож змінити їх неможливо за побудовою.

## Схема БД

```
FuelTypes                    Stations                     StationTerminals
───────────────────────      ─────────────────────        ──────────────────────────
Code (PK, 'A95BIO') ◄─┐      Id (PK, identity) ◄──┬────── StationId (FK, CASCADE)
Name (UNIQUE)         │      Name (UNIQUE)        │       Number (PK, 1000–9999)
IsActive              │      City (INDEX)         │       Description
CreatedAt             │      Address              │
                      │      Description (NULL)   │
                      │      IsActive             │
                      │      CreatedAt            │
                      │                           │
                      │   StationFuels            │
                      │   ────────────────────────────────
                      │   Id (PK, identity)       │
                      │   StationId (FK) ─────────┘   ON DELETE CASCADE
                      └── FuelTypeCode (FK)           ON DELETE RESTRICT
                          Price numeric(10,2) (> 0)
                          IsAvailable
                          PriceUpdatedAt
                          UNIQUE (StationId, FuelTypeCode)
```

Міграції застосовуються автоматично при старті. Вручну: `dotnet tool restore`, потім у папці `FuelStations`
`dotnet ef database update`.

## Валідація і помилки

Усі помилки — у стандартному форматі **RFC 7807 ProblemDetails** з повідомленнями українською:

```json
{
  "title": "Некоректні дані",
  "status": 400,
  "detail": "Вид пального DPE продається на АЗС, тому його не можна видалити. Деактивуйте його (isActive: false)."
}
```

| Ситуація | Код |
|---|---|
| Порушено правила DTO (ціна ≤ 0, ID не 1000–9999, порожня назва, `pageSize` > 100) | 400, поле `errors` |
| Дублікат коду чи назви виду пального, назви АЗС, ID АЗС, пального на АЗС | 400 |
| Невідомий або деактивований вид пального при додаванні на АЗС | 400 |
| Видалення виду пального, що продається | 400 |
| Неіснуюча АЗС, вид пального або пальне на АЗС | 404 |

## Структура проєкту

```
FuelStations.sln
docker-compose.yml                    # PostgreSQL 17 for local development
dotnet-tools.json                     # local tools: dotnet-ef, Husky.Net
.editorconfig, .husky/                # code style and pre-commit format check
FuelStations/
├── Program.cs                        # environments, DI, migrations on startup, Swagger UI
├── appsettings.json                  # shared settings (no connection string)
├── appsettings.Development.json      # Docker DB connection string
├── appsettings.Local.example.json    # template for the Neon connection string
├── Properties/launchSettings.json    # profiles: http (Docker), neon (Production), port 5080
├── Common/Pagination.cs              # pagination limits
├── Entities/                         # FuelType, Station, StationTerminal, StationFuel
├── Data/AppDbContext.cs              # EF Core schema: keys, indexes, check constraints, FKs
├── Dtos/                             # request DTOs with validation, response DTOs
├── Controllers/                      # ApiControllerBase, FuelTypesController, StationsController
├── Migrations/                       # EF Core migrations
├── demo-data.http                    # demo data via the API
└── FuelStations.http                 # API scenario with automated checks
```

## Якість коду

- `async`/`await` і асинхронні методи EF Core; `AsNoTracking` і проєкції в DTO для читання;
  `AsSplitQuery` для двох колекцій.
- `.editorconfig` + `dotnet format`; Husky.Net не пропускає коміт з невідформатованим кодом.
- Коментарі й XML-документація двомовні (українська, під нею англійська) — їх показує Swagger.
- Історія Git: кожен етап — окрема гілка, злита в `main` через `--no-ff`.

## Що можна покращити

1. Інтеграційні тести (xUnit + `WebApplicationFactory`) за сценарієм `FuelStations.http`.
2. Залишки пального в літрах і автоматичне `IsAvailable = false` при нульовому залишку.
3. Історія цін (`PriceHistory`) для аналітики.
4. Сервісний шар між контролерами й `DbContext`.
5. `CancellationToken` у методах контролерів.
6. Деплой API з публічним Swagger (Dockerfile + хостинг), рядок підключення — змінною оточення.
