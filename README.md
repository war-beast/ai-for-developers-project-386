# Календарь звонков


[![hexlet-check](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/hexlet-check.yml/badge.svg)](https://github.com/war-beast/ai-for-developers-project-386/actions)
[![check](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/check.yml/badge.svg)](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/check.yml)

Разработайте совместно с ИИ сервис для бронирования календаря

Учебный проект Хекслета: https://ru.hexlet.io/programs/ai-for-developers
Как это должно работать: https://files.hexlet.app/a/2ipc5m

## Стек

- Бекенд: ASP.NET Core 10 (MVC), солюшен `CallCalendar.slnx`: `src/CallCalendar` (представление, там же REST `api/`), `CallCalendar.BLL` (бизнес-логика), `CallCalendar.DAL` (доступ к данным), `CallCalendar.Tests` (xUnit.v3 + Moq)
- Контракт API: TypeSpec (`api/main.tsp`) → OpenAPI (`api/openapi.yaml`) → клиентский SDK для TypeScript (`frontend/src/api/schema.d.ts`, генератор openapi-typescript, запросы через openapi-fetch)
- Фронтенд: Vite + React + TypeScript, shadcn/ui, TanStack Query
- Инструменты: mise (версии в `mise.toml`), git-хуки через hk (`hk.pkl`), демоны через pitchfork

## Установка

Нужен [mise](https://mise.jdx.dev) (ставит node, pnpm, .NET SDK, линтеры) и git.

```bash
git clone https://github.com/war-beast/ai-for-developers-project-386.git
cd ai-for-developers-project-386
mise install      # установка всех инструментов из mise.toml
mise run install  # зависимости dotnet/node (frontend, api)
```

## Использование

Запуск через демоны (бекенд на :5150, фронтенд на :5173):

```bash
mise daemons start backend frontend
mise daemons logs backend   # смотреть вывод
mise daemons stop backend frontend
```

Генерация контракта и SDK:

```bash
mise run openapi   # TypeSpec -> openapi.yaml
mise run sdk       # openapi.yaml -> frontend/src/api/schema.d.ts
```

Полный прогон проверок (его же делает CI): сборка, линтеры, типы, тесты:

```bash
mise run check
```

---

<details>
<summary>Автоматические тесты Хекслета</summary>

Тесты запускаются на каждый коммит. За запуск отвечает файл `.github/workflows/hexlet-check.yml` — не удаляйте и не переименовывайте ни его, ни репозиторий.

</details>

## О Хекслете

[Хекслет](https://ru.hexlet.io/) — школа программирования: авторские программы обучения с практикой, поддержкой наставников и реальными проектами, которые остаются в резюме. Этот репозиторий — один из таких проектов.
