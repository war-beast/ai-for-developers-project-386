import { useQuery } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { api, type CallSlot } from '@/api/client'

async function fetchSlots(): Promise<CallSlot[]> {
  const { data, error } = await api.GET('/api/calendar/slots')
  if (error ?? data === undefined) {
    throw new Error(`slots request failed: ${String(error)}`)
  }
  return data
}

export default function App() {
  const { data, isPending, isError } = useQuery({
    queryKey: ['calendar-slots'],
    queryFn: fetchSlots,
  })

  return (
    <main className="mx-auto flex min-h-dvh max-w-3xl flex-col items-center justify-center gap-6 px-4">
      <h1 className="text-3xl font-semibold tracking-tight">Календарь звонков</h1>
      {isError && <p className="text-destructive">Не удалось загрузить слоты</p>}
      {isPending && <p>Загрузка…</p>}
      {data?.map((slot) => (
        <Card key={slot.id} className="w-full">
          <CardHeader>
            <CardTitle>{slot.title}</CardTitle>
            <CardDescription>{slot.startsAt.slice(0, 16).replace('T', ' ')}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button>Забронировать</Button>
          </CardContent>
        </Card>
      ))}
    </main>
  )
}
