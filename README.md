# Календарь звонков


[![hexlet-check](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/hexlet-check.yml/badge.svg)](https://github.com/war-beast/ai-for-developers-project-386/actions)
[![check](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/check.yml/badge.svg)](https://github.com/war-beast/ai-for-developers-project-386/actions/workflows/check.yml)

Сервис записи на звонок: владелец публикует доступное время, а гость без
регистрации выбирает тип встречи и свободный слот в ближайшие 14 дней.

Учебный проект Хекслета: https://ru.hexlet.io/programs/ai-for-developers
Как это должно работать: https://files.hexlet.app/a/2ipc5m

## Стек

- Бекенд: ASP.NET Core 10 (MVC), солюшен `CallCalendar.slnx`: `backend/CallCalendar` (слой представления, REST `api/`), `CallCalendar.BLL` (бизнес-логика), `CallCalendar.DAL` (доступ к данным), `CallCalendar.Tests` (xUnit.v3 + Moq)
- Фронтенд: `frontend/` — Vite + React + TypeScript, shadcn/ui, TanStack Query, react-router. Это единственный пользовательский интерфейс приложения
- Контракт API: TypeSpec (`api/main.tsp`) → OpenAPI (`api/openapi.yaml`) → клиентский SDK для TypeScript (`frontend/src/api/schema.d.ts`, генератор openapi-typescript, запросы через openapi-fetch)
- Инструменты: mise (версии в `mise.toml`), git-хуки через hk (`hk.pkl`), демоны через pitchfork

## Что уже работает

- Главная страница `/` — рассказывает про сервис
- `/book` — каталог типов встреч: название, описание, длительность
- `/admin` — список типов и форма создания; ошибки полей приходят из API
- `/admin/meetings` — заглушка: предстоящие встречи появятся в следующем шаге
- `GET/POST /api/event-types` — список и создание типов встреч
- `GET /health` — проверка живости бекенда

Данные хранятся в памяти процесса: после рестарта бекенда созданные типы
встреч пропадают. Базы данных и сохранения состояния нет.

## Установка

Нужен [mise](https://mise.jdx.dev) (ставит node, pnpm, .NET SDK, линтеры) и git.

```bash
git clone https://github.com/war-beast/ai-for-developers-project-386.git
cd ai-for-developers-project-386
mise install      # установка всех инструментов из mise.toml
mise run install  # зависимости dotnet/node (frontend, api)
```

## Использование

Запуск через демоны (UI на :5173, API на :5150):

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

Полный прогон проверок (его же делает CI): сборка, линтеры, типы, тесты
бекенда и фронтенда, дымовая проверка API:

```bash
mise run check
```

Браузерный сценарий Playwright (поднимает реальные бекенд и Vite):

```bash
pnpm --dir frontend test:e2e
```

## Документация

- `GLOSSARY.md` — словарь предметной области
- `docs/adr/` — принятые архитектурные решения

---

<details>
<summary>Автоматические тесты Хекслета</summary>

Тесты запускаются на каждый коммит. За запуск отвечает файл `.github/workflows/hexlet-check.yml` — не удаляйте и не переименовывайте ни его, ни репозиторий.

</details>

## О Хекслете

[Хекслет](https://ru.hexlet.io/) — школа программирования: авторские программы обучения с практикой, поддержкой наставников и реальными проектами, которые остаются в резюме. Этот репозиторий — один из таких проектов.
