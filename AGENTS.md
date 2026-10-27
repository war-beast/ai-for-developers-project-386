# CallCalendar — ASP.NET Core MVC + React/Vite, 3-слойная архитектура

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
- `openapi.yaml` эмитится командой `mise run openapi` (не правится руками).
- Клиентский SDK для TypeScript: сгенерированные типы в `frontend/src/api/schema.d.ts`
  (обновление — `mise run sdk`), запросы через `openapi-fetch`.
- Фронтенд: `frontend/` — Vite + React + TypeScript + shadcn/ui, состояние через TanStack Query.

## Окружение

- Все версии инструментов зафиксированы в `mise.toml`; зависимости ставит `mise install`.
- Единая команда проверок: `mise run check` (сборка + линтеры + типы + тесты). Она же вызывается из CI.
- Фоновые серверы (бекенд, Vite) описаны как демоны mise (`[daemons]` в mise.toml):
  `mise daemons start backend frontend`, `mise daemons logs backend`, `mise daemons stop ...`.
- Гит-хуки настроены через hk (`hk.pkl`), включая проверку commit-msg.

## Conventional Commits (обязательно)

Текст каждого коммита начинается с префикса: `feat:` — новая функция,
`fix:` — исправление бага; остальные типы (`chore:`, `docs:`, `refactor:`, `test:`)
используются по смыслу. Проверка выполняется хуком commit-msg (hk), сборки должны
соответствовать этому формату.

## Ветки и PR

- Каждую новую задачу начинаем в новой ветке: `feat/<краткое-имя>` (git не
  допускает `:` в именах веток, поэтому префикс `feat:` пишется как `feat/`).
- Ветки `feat/` создаются только из `dev`.
- После завершения задачи — коммитим изменения (Conventional Commits) и делаем
  PR в ветку `dev` на GitHub.

## Уточнение при неоднозначности

Если формулировка задачи вызывает какие-либо вопросы, или если в ходе работы
возникают странные, неожиданные или неоднозначные детали — приостановите
выполнение до того, как предпринять действие, и запросите у пользователя
уточнение.

Где это уместно, представьте возможные варианты ответов в виде явного списка
опций на выбор.

## Agent skills

### Issue tracker

Issues live as GitHub issues in `war-beast/ai-for-developers-project-386`, worked via the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

Default five canonical triage labels (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
