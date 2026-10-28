import { useState, type FormEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'

import { createBooking, type Booking, type BookingProblem } from '@/api/bookings'
import { fetchEventTypes } from '@/api/event-types'
import { fetchSlots } from '@/api/slots'
import { Button } from '@/components/ui/button'
import { TextField, type FieldErrors } from '@/components/text-field'
import {
  formatDateTime,
  formatDayLabel,
  formatSlotTime,
  weekdayIndex,
} from '@/lib/dates'

const weekdays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

export function EventTypePage() {
  const { typeId = '' } = useParams()
  const queryClient = useQueryClient()
  const [selectedDate, setSelectedDate] = useState<string | null>(null)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)
  const [guestName, setGuestName] = useState('')
  const [guestEmail, setGuestEmail] = useState('')
  const [comment, setComment] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [slotTaken, setSlotTaken] = useState(false)
  const [booking, setBooking] = useState<Booking | null>(null)

  const eventTypesQuery = useQuery({
    queryKey: ['event-types'],
    queryFn: fetchEventTypes,
  })
  const slotsQuery = useQuery({
    queryKey: ['slots', typeId],
    queryFn: () => fetchSlots(typeId),
  })
  const create = useMutation<Booking, BookingProblem, string>({
    mutationFn: (start) =>
      createBooking({
        eventTypeId: typeId,
        start,
        guestName,
        guestEmail,
        ...(comment ? { comment } : {}),
      }),
    onSuccess: (created) => {
      setBooking(created)
      setSelectedSlot(null)
      setSlotTaken(false)
      setFieldErrors({})
      setGuestName('')
      setGuestEmail('')
      setComment('')
      void queryClient.invalidateQueries({ queryKey: ['slots', typeId] })
    },
    onError: (problem) => {
      if (problem.code === 'slot_taken') {
        setSlotTaken(true)
        setSelectedSlot(null)
        setFieldErrors({})
        // Слот заняли между выбором и отправкой: перезапрашиваем выдачу.
        void queryClient.invalidateQueries({ queryKey: ['slots', typeId] })
        return
      }

      setSlotTaken(false)
      setFieldErrors(problem.errors ?? {})
    },
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

  if (booking) {
    return (
      <PageMessage>
        <h1 className="text-4xl font-light tracking-tight">Вы записаны</h1>
        <p className="text-lg">{booking.eventTypeTitle}</p>
        <p className="text-muted-foreground">
          {formatDateTime(booking.start)} · {slotWindow.timeZone}
        </p>
        <p className="text-muted-foreground">
          Подтверждение придёт на {booking.guestEmail}.
        </p>
        <div className="mt-2 flex gap-3">
          <Button asChild>
            <Link to="/book">Записаться ещё</Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/">На главную</Link>
          </Button>
        </div>
      </PageMessage>
    )
  }

  const activeDate =
    selectedDate ??
    slotWindow.days.find((day) => day.slots.length > 0)?.date ??
    slotWindow.days[0]?.date
  const activeDay = slotWindow.days.find((day) => day.date === activeDate)
  const hasSlots = slotWindow.days.some((day) => day.slots.length > 0)

  // Дни окна выкладываются календарём от понедельника: пустые клетки держат столбцы недели.
  const leadingBlanks = slotWindow.days[0]
    ? weekdayIndex(slotWindow.days[0].date)
    : 0
  const trailingBlanks =
    (7 - ((leadingBlanks + slotWindow.days.length) % 7)) % 7

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (selectedSlot) {
      create.mutate(selectedSlot)
    }
  }

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col gap-8 px-4 py-10">
      <header className="flex flex-col gap-2">
        <h1 className="text-4xl font-light tracking-tight">{eventType.title}</h1>
        {eventType.description ? (
          <p className="text-muted-foreground">{eventType.description}</p>
        ) : null}
        <p className="text-sm text-muted-foreground">
          {eventType.durationMinutes} минут · {slotWindow.timeZone}
        </p>
      </header>

      <div className="grid gap-8 md:grid-cols-2">
        <section className="flex flex-col gap-3">
          <h2 className="text-lg font-medium">Выберите день</h2>
          <div
            aria-hidden
            className="grid grid-cols-7 gap-1 text-center text-xs text-muted-foreground"
          >
            {weekdays.map((weekday) => (
              <span key={weekday}>{weekday}</span>
            ))}
          </div>
          <ul aria-label="Дни окна записи" className="grid grid-cols-7 gap-1">
            {Array.from({ length: leadingBlanks }, (_, index) => (
              <li key={`blank-before-${index}`} />
            ))}
            {slotWindow.days.map((day) => (
              <li key={day.date}>
                <Button
                  variant={
                    day.date === activeDate
                      ? 'default'
                      : weekdayIndex(day.date) >= 5
                        ? 'secondary'
                        : 'outline'
                  }
                  aria-pressed={day.date === activeDate}
                  data-date={day.date}
                  className="h-12 w-full cursor-pointer px-1 text-xs"
                  onClick={() => {
                    setSelectedDate(day.date)
                    setSelectedSlot(null)
                  }}
                >
                  {formatDayLabel(day.date)}
                </Button>
              </li>
            ))}
            {Array.from({ length: trailingBlanks }, (_, index) => (
              <li key={`blank-after-${index}`} />
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
            <ul
              aria-label="Слоты"
              className="flex max-h-[60vh] flex-col gap-2 overflow-y-auto p-1"
            >
              {activeDay?.slots.map((slot) => (
                <li key={slot.start}>
                  <Button
                    variant={slot.start === selectedSlot ? 'default' : 'outline'}
                    aria-pressed={slot.start === selectedSlot}
                    className="w-full cursor-pointer"
                    onClick={() => {
                      setSelectedSlot(slot.start)
                      setSlotTaken(false)
                      setFieldErrors({})
                    }}
                  >
                    {formatSlotTime(slot.start)}
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

      {selectedSlot || slotTaken ? (
        <section className="flex flex-col gap-3">
          <h2 className="text-lg font-medium">
            {selectedSlot
              ? `Запись на ${formatDateTime(selectedSlot)}`
              : 'Выберите другое время'}
          </h2>
          <form className="grid gap-4" onSubmit={handleSubmit}>
            <TextField
              id="booking-guestName"
              label="Имя"
              value={guestName}
              errors={fieldErrors.guestName}
              onChange={setGuestName}
            />
            <TextField
              id="booking-guestEmail"
              label="Email"
              type="email"
              value={guestEmail}
              errors={fieldErrors.guestEmail}
              onChange={setGuestEmail}
            />
            <TextField
              id="booking-comment"
              label="Комментарий"
              value={comment}
              errors={fieldErrors.comment}
              rows={3}
              onChange={setComment}
            />
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-3">
                <Button
                  type="submit"
                  size="lg"
                  disabled={create.isPending || !selectedSlot}
                >
                  Забронировать
                </Button>
                {create.isError && Object.keys(fieldErrors).length === 0 && !slotTaken ? (
                  <p className="text-sm text-destructive">
                    Не удалось забронировать слот.
                  </p>
                ) : null}
              </div>
              {slotTaken ? (
                <p role="alert" className="text-sm text-destructive">
                  Это время уже занято
                </p>
              ) : null}
            </div>
          </form>
        </section>
      ) : null}
    </main>
  )
}

function PageMessage({ children }: { children: ReactNode }) {
  return (
    <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col gap-2 px-4 py-10">
      {children}
    </main>
  )
}
