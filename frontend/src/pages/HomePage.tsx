import { Link } from 'react-router'

import { Button } from '@/components/ui/button'

export function HomePage() {
  return (
    <main className="mx-auto flex min-h-dvh max-w-3xl flex-col items-center justify-center gap-6 px-4 text-center">
      <h1 className="text-4xl font-semibold tracking-tight">Календарь звонков</h1>
      <p className="max-w-xl text-muted-foreground">
        Сервис записи на звонок: владелец календаря публикует свободное время, а
        гость без регистрации выбирает тип встречи и свободный слот в ближайшие
        14 дней.
      </p>
      <Button asChild size="lg">
        <Link to="/book">Записаться на звонок</Link>
      </Button>
    </main>
  )
}
