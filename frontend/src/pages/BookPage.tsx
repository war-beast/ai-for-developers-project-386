import { Link } from 'react-router'

export function BookPage() {
  return (
    <main className="mx-auto flex min-h-dvh max-w-3xl flex-col items-center justify-center gap-4 px-4 text-center">
      <h1 className="text-3xl font-semibold tracking-tight">Запись на звонок</h1>
      <p className="text-muted-foreground">
        Выбор типа встречи и свободного времени появится на следующем шаге.
      </p>
      <Link className="text-sm underline underline-offset-4" to="/">
        На главную
      </Link>
    </main>
  )
}
