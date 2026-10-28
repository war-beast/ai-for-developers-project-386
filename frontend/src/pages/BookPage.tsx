import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router'

import { fetchEventTypes } from '@/api/event-types'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

export function BookPage() {
  const { data: eventTypes = [], isPending } = useQuery({
    queryKey: ['event-types'],
    queryFn: fetchEventTypes,
  })

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col gap-6 px-4 py-10">
      <div className="flex flex-col gap-2">
        <h1 className="text-4xl font-light tracking-tight">Запись на звонок</h1>
        <p className="text-muted-foreground">
          Выберите тип встречи. Свободное время появится на следующем шаге.
        </p>
      </div>

      {isPending ? (
        <p className="text-muted-foreground">Загрузка…</p>
      ) : eventTypes.length === 0 ? (
        <p className="text-muted-foreground">Пока нет типов встреч</p>
      ) : (
        <ul className="grid gap-4 sm:grid-cols-2">
          {eventTypes.map((eventType) => (
            <li key={eventType.id}>
              <Card className="h-full">
                <CardHeader>
                  <CardTitle>{eventType.title}</CardTitle>
                  <CardDescription>{eventType.description}</CardDescription>
                </CardHeader>
                <CardContent className="mt-auto flex items-center justify-between gap-4">
                  <span className="text-muted-foreground">
                    {eventType.durationMinutes} минут
                  </span>
                  <Button asChild size="sm">
                    <Link to={`/book/${eventType.id}`}>Записаться</Link>
                  </Button>
                </CardContent>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}
