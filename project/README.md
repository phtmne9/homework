# Склад ПВЗ — итоговый проект

Web API для пункта выдачи заказов: приёмка посылок на склад, хранение,
выдача клиенту по телефону и возврат отправителю по истечении срока.

## Домен

```
PickupPoint (ПВЗ)  ──<  Parcel (посылка)  >──  Client (клиент)
```

`Parcel` связывает пункт и получателя. Статусы: `Expected`, `InStock`, `Issued`, `Returned`.
Срок хранения считается от даты приёмки, вместимость ПВЗ ограничена.

## Запуск

Ключ подписи токенов не хранится в репозитории — задайте его переменной окружения:

```
export Jwt__Key="строка-не-короче-32-символов"
dotnet run --project PvzApi
```

Swagger: `http://localhost:5000/swagger`. База SQLite создаётся при первом запуске
и заполняется: 2 ПВЗ, 20 клиентов, 100 посылок.

Учётные записи создаются автоматически:

| Логин | Пароль | Роль |
|---|---|---|
| `admin` | `Admin123` | Admin |
| `operator` | `Operator123` | Operator |

## Эндпоинты

| Метод | Путь | Доступ |
|---|---|---|
| POST | `/api/auth/login` | без авторизации |
| GET | `/api/parcels` | любой вошедший |
| GET | `/api/parcels/{id}` | любой вошедший |
| GET | `/api/parcels/track/{trackNumber}` | без авторизации |
| POST | `/api/parcels` | Admin, Operator |
| POST | `/api/parcels/{id}/issue` | Admin, Operator |
| POST | `/api/parcels/{id}/return` | только Admin |
| GET | `/api/parcels/expiring?days=2` | любой вошедший |
| GET | `/api/directory/points` | любой вошедший |
| POST | `/api/directory/clients` | Admin, Operator |
| POST | `/api/directory/points` | только Admin |
| GET | `/api/files/parcels.csv` | Admin, Operator |
| GET | `/api/files/parcels.xlsx` | Admin, Operator |
| POST | `/api/files/clients/csv` | Admin, Operator |
| POST | `/api/files/clients/xlsx` | Admin, Operator |

Список посылок принимает `page`, `pageSize`, `status`, `pickupPointId`,
`search` (по треку, имени и телефону), `sortBy` (`price`, `weight`, `track`,
по умолчанию дата приёмки) и `desc`.

## Что из курса использовано

| Тема курса | Где в проекте |
|---|---|
| Контроллеры, маршрутизация, HTTP-статусы | `Controllers/` |
| Интерфейс → реализация → контроллер | `Services/` |
| DTO | `Models/Dtos.cs` |
| EF Core Code First, связи, Include | `Data/AppDbContext.cs`, `Services/ParcelService.cs` |
| Пагинация, фильтрация, сортировка | `ParcelService.GetPagedAsync` |
| Загрузка и выгрузка csv и xlsx | `Services/FileService.cs` |
| FluentValidation | `Validators/` |
| Глобальная обработка ошибок | `Middleware/ExceptionMiddleware.cs` |
| JWT и роли | `Services/TokenService.cs`, атрибуты `[Authorize]` |
| CQRS на MediatR | `Features/Parcels/` |
| Логирование пайплайном | `Behaviors/LoggingBehavior.cs` |
| CORS и rate limiting | `Program.cs` |
| Swagger с аннотациями | `Program.cs`, атрибуты `SwaggerOperation` |
| xUnit и Moq | `PvzApi.Tests/` |

## Тесты

```
dotnet test PvzApi.Tests
```

28 тестов: сервис посылок на SQLite в памяти, контроллер на моках, валидаторы,
импорт и экспорт файлов.

## Замечания по реализации

Цена и вес объявлены как `decimal`, но в SQLite хранятся через `HasConversion<double>()` —
SQLite не поддерживает `decimal` в `ORDER BY`, без конверсии сортировка по цене падает.

Импорт файлов отсеивает дубликаты телефонов не только по базе, но и внутри самого
файла: проверка только по базе не видит ещё не сохранённые строки текущей пачки.
