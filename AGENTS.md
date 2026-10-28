# CallCalendar — указатель для агентов

Проект: ASP.NET Core MVC (.NET 10) + React/Vite, 3-слойная архитектура.
Правила кода и процесса живут в отдельных файлах и подгружаются OpenCode
автоматически через `instructions` в `opencode.json`:

- [CODING_STANDARDS.md](./CODING_STANDARDS.md) — структура солюшена, форматирование C#,
  контракт API и правила фронтенда. Читать перед любой правкой `backend/`, `api/`, `frontend/`.
- [CONTRIBUTING.md](./CONTRIBUTING.md) — окружение, `mise run check`, Conventional Commits,
  ветки и PR. Читать перед коммитом или PR.

Новые соглашения по коду пишем в `CODING_STANDARDS.md`, по процессу —
в `CONTRIBUTING.md`. `AGENTS.md` не пополняется.

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
