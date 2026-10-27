import { useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'

import { fetchEventTypes } from '@/api/event-types'
import { fetchSlots } from '@/api/slots'
import { Button } from '@/components/ui/button'
import { formatDayLabel, formatSlotTime } from '@/lib/dates'

export function EventTypePage() {
  const { typeId = '' } = useParams()
  const [selectedDate, setSelectedDate] = useState<string | null>(null)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const eventTypesQuery = useQuery({
    queryKey: ['event-types'],
    queryFn: fetchEventTypes,
  })
  const slotsQuery = useQuery({
    queryKey: ['slots', typeId],
    queryFn: () => fetchSlots(typeId),
  })

  if (eventTypesQuery.isPending || slotsQuery.isPending) {
    return <PageMessage>Загрузка…</PageMessage>
  }

  const eventType = eventTypesQuery.data?.find((type) => type.id === typeId)

  if (!eventType) {
    return (
      <PageMessage>
        <h1 className="text-4xl font-light tracking-tight">Тип встречи не найден</h1>
        <p className="text-muted-foreground">
          Проверьте ссылку или вернитесь в{' '}
          <Link to="/book" className="underline underline-offset-4">
            каталог
          </Link>
          .
        </p>
      </PageMessage>
    )
  }

  if (!slotsQuery.data) {
    return <PageMessage>Не удалось загрузить свободные слоты.</PageMessage>
  }

  const slotWindow = slotsQuery.data
  const activeDate =
    selectedDate ??
    slotWindow.days.find((day) => day.slots.length > 0)?.date ??
    slotWindow.days[0]?.date
  const activeDay = slotWindow.days.find((day) => day.date === activeDate)
  const hasSlots = slotWindow.days.some((day) => day.slots.length > 0)

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-8 px-4 py-10">
      <header className="flex flex-col gap-2">
        <h1 className="text-4xl font-light tracking-tight">{eventType.title}</h1>
        {eventType.description ? (
          <p className="text-muted-foreground">{eventType.description}</p>
        ) : null}
        <p className="text-sm text-muted-foreground">
          {eventType.durationMinutes} минут · {slotWindow.timeZone}
        </p>
      </header>

      <section className="flex flex-col gap-3">
        <h2 className="text-lg font-medium">Выберите день</h2>
        <ul
          aria-label="Дни окна записи"
          className="flex gap-2 overflow-x-auto pb-2"
        >
          {slotWindow.days.map((day) => (
            <li key={day.date}>
              <Button
                variant={day.date === activeDate ? 'default' : 'outline'}
                aria-pressed={day.date === activeDate}
                data-date={day.date}
                onClick={() => {
                  setSelectedDate(day.date)
                  setSelectedSlot(null)
                }}
              >
                {formatDayLabel(day.date)}
              </Button>
            </li>
          ))}
        </ul>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-lg font-medium">Свободное время</h2>
        {!hasSlots ? (
          <p className="text-muted-foreground">
            Свободных слотов в ближайшие 14 дней нет
          </p>
        ) : activeDay && activeDay.slots.length === 0 ? (
          <p className="text-muted-foreground">
            В этот день свободных слотов нет
          </p>
        ) : (
          <ul aria-label="Слоты" className="grid grid-cols-3 gap-2 sm:grid-cols-4">
            {activeDay?.slots.map((slot) => (
              <li key={slot.start}>
                <Button
                  variant={slot.start === selectedSlot ? 'default' : 'outline'}
                  aria-pressed={slot.start === selectedSlot}
                  className="w-full"
                  onClick={() => setSelectedSlot(slot.start)}
                >
                  {formatSlotTime(slot.start)}
                </Button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </main>
  )
}

function PageMessage({ children }: { children: ReactNode }) {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-2 px-4 py-10">
      {children}
    </main>
  )
}
