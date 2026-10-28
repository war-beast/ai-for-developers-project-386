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
- `/book/:typeId` — свободные слоты типа встречи на 14 дней: лента дней, выбор дня и времени, форма гостя и подтверждение записи
- `/admin` — список типов и форма создания; ошибки полей приходят из API
- `/admin/meetings` — предстоящие встречи всех типов по возрастанию начала; пустое состояние, обновление при возврате на вкладку
- `GET/POST /api/event-types` — список и создание типов встреч
- `GET /api/event-types/{eventTypeId}/slots` — свободные слоты на окно записи
- `POST /api/bookings` — запись гостя на свободный слот; пересекающиеся интервалы отклоняются с `409 slot_taken`
- `GET /api/bookings` — предстоящие записи всех типов (включая идущую) по возрастанию начала
- `GET /health` — проверка живости бекенда

Данные хранятся в памяти процесса: после рестарта бекенда созданные типы
встреч и записи пропадают. Базы данных и сохранения состояния нет.

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
бекенда и фронтенда, проверка актуальности сгенерированных контракта и SDK,
дымовая проверка API и браузерный сценарий Playwright на реальных серверах:

```bash
mise run check
```

Отдельно браузерный сценарий Playwright (поднимает реальные бекенд и Vite):

```bash
mise run e2e
```

## Деплой

### Docker

```bash
docker build -t callcalendar .
docker run --rm -e PORT=8080 -p 8080:8080 callcalendar
```

Образ собирает React-фронтенд и кладёт его в `wwwroot`: UI и `/api` отдаются с
одного origin. Приложение слушает `0.0.0.0:$PORT` (без `PORT` — стандартный
порт ASP.NET Core, 8080), проверка живости — `GET /health`.

### Render

`render.yaml` создаёт web-сервис из `Dockerfile` с проверкой `/health`. Render
передаёт `PORT` автоматически, дополнительных переменных окружения не нужно.

1. Запушить изменения в GitHub.
2. Render Dashboard → **New** → **Blueprint** → выбрать репозиторий и применить
   `render.yaml`.
3. После первого деплоя Render выдаст публичный адрес.
4. Открыть `<адрес>/health` — должно вернуться `ok`.

### MCP-сервер Render

MCP подключён в `opencode.json` как remote-сервер `render` с адресом
`https://mcp.render.com/mcp`. Секретов в конфиге нет: авторизация идёт по OAuth,
токен хранится в профиле OpenCode.

1. Открыть OpenCode.
2. Выполнить `/mcps`, выбрать `render` → **Sign in**.
3. В браузере войти в Render и разрешить доступ.
4. Проверить `opencode mcp list` — сервер `render` в статусе connected.
5. Дальше деплой, логи и адрес сервиса доступны инструментами MCP.

Если OAuth недоступен, используется API-ключ Render в заголовке
`Authorization: Bearer $RENDER_API_KEY`; ключ передаётся через переменную
окружения и в репозиторий не попадает.

### Ограничение хранения данных

Типы встреч и записи хранятся в памяти процесса, базы данных нет. Любой
рестарт, редеплой или засыпание бесплатного инстанса Render стирает все данные.
Публичная ссылка подходит для демонстрации, но не для реальных записей: для
сохранения данных нужна внешняя БД.

## Документация

- `CODING_STANDARDS.md` — стандарты кода и архитектуры
- `CONTRIBUTING.md` — окружение, коммиты, ветки и PR
- `GLOSSARY.md` — словарь предметной области
- `docs/adr/` — принятые архитектурные решения

---

<details>
<summary>Автоматические тесты Хекслета</summary>

Тесты запускаются на каждый коммит. За запуск отвечает файл `.github/workflows/hexlet-check.yml` — не удаляйте и не переименовывайте ни его, ни репозиторий.

</details>

## О Хекслете

[Хекслет](https://ru.hexlet.io/) — школа программирования: авторские программы обучения с практикой, поддержкой наставников и реальными проектами, которые остаются в резюме. Этот репозиторий — один из таких проектов.
