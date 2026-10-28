# Стандарты кода CallCalendar

Соглашения по коду и архитектуре. Процесс публикации изменений —
в [CONTRIBUTING.md](./CONTRIBUTING.md).

## Структура солюшена

- `backend/CallCalendar` — слой представления (ASP.NET Core MVC, .NET 10): REST-контроллеры `api/` — это слой View бекенда; пользовательский интерфейс — React SPA в `frontend/`
- `backend/CallCalendar.BLL` — слой бизнес-логики (classlib): папки `Model/`, `Interface/`, `Service/`
- `backend/CallCalendar.DAL` — слой доступа к данным (classlib): папки `Enums/`, `Entity/`, `Interface/`, `Repository/`
- `backend/CallCalendar.Tests` — тестовый проект xUnit.v3 + Moq

Структурные папки BLL и DAL нельзя удалять; каждая из них содержит файл-плейсхолдер
`.gitkeep`, чтобы отображаться в Solution Explorer. Новые классы кладутся в
соответствующую папку по смыслу.

Зависимости строго односторонние: `CallCalendar -> CallCalendar.BLL -> CallCalendar.DAL`.
`CallCalendar.Tests` ссылается на BLL, а для интеграционных тестов API — на `CallCalendar`.

## Форматирование C#

Если в одной строке больше одного вызова метода после точки, то начиная со
второго каждый вызов переносится на новую строку; точка остаётся в начале
строки, отступ — +4 пробела от начала выражения:

```csharp
problemFactory.Create(ProblemCodes.SlotTaken)
    .GetProblemResult();
```

Правило действует для всего C#-кода в `backend/`, включая тесты.
`dotnet format` его не проверяет — соблюдаем вручную.

## Контракт API и фронтенд

- Контракт описывается в **TypeSpec**: `api/main.tsp` (+ `api/tspconfig.yaml`).
- Генерируемые файлы (правятся только генератором, вручную — никогда):
  - `api/openapi.yaml` — OpenAPI из TypeSpec;
  - `frontend/src/api/schema.d.ts` — типы клиентского SDK.
- Обе генерации выполняет одна команда `mise run sdk` (`openapi` → `sdk`).
  `mise run check` повторяет генерацию и падает, если файлы разошлись с коммитом.
- Запросы к API идут только через `openapi-fetch` и типы из `schema.d.ts`.
- Серверный код по контракту пишется вручную: подходящего генератора для
  ASP.NET Core MVC нет (`openapi-generator` даёт заготовки контроллеров,
  несовместимые с 3-слойной архитектурой).
- Фронтенд: `frontend/` — Vite + React + TypeScript + shadcn/ui, состояние через TanStack Query.
