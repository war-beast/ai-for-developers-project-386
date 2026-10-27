import { useState, type FormEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'

import { createBooking, type Booking, type BookingProblem } from '@/api/bookings'
import { fetchEventTypes } from '@/api/event-types'
import { fetchSlots } from '@/api/slots'
import { Button } from '@/components/ui/button'
import { formatDayLabel, formatSlotTime } from '@/lib/dates'

const inputClassName =
  'h-9 w-full border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive'

type FieldErrors = Record<string, string[]>

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
          {formatDayLabel(booking.start.slice(0, 10))},{' '}
          {formatSlotTime(booking.start)} · {slotWindow.timeZone}
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

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (selectedSlot) {
      create.mutate(selectedSlot)
    }
  }

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

      {selectedSlot ? (
        <section className="flex flex-col gap-3">
          <h2 className="text-lg font-medium">
            Запись на {formatDayLabel(selectedSlot.slice(0, 10))},{' '}
            {formatSlotTime(selectedSlot)}
          </h2>
          <form className="grid gap-4" onSubmit={handleSubmit}>
            <GuestField
              name="guestName"
              label="Имя"
              value={guestName}
              errors={fieldErrors.guestName}
              onChange={setGuestName}
            />
            <GuestField
              name="guestEmail"
              label="Email"
              type="email"
              value={guestEmail}
              errors={fieldErrors.guestEmail}
              onChange={setGuestEmail}
            />
            <GuestField
              name="comment"
              label="Комментарий"
              value={comment}
              errors={fieldErrors.comment}
              multiline
              onChange={setComment}
            />
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-3">
                <Button type="submit" size="lg" disabled={create.isPending}>
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

type GuestFieldProps = {
  name: string
  label: string
  value: string
  errors?: string[]
  type?: 'text' | 'email'
  multiline?: boolean
  onChange: (value: string) => void
}

function GuestField({
  name,
  label,
  value,
  errors,
  type = 'text',
  multiline = false,
  onChange,
}: GuestFieldProps) {
  const id = `booking-${name}`

  return (
    <div className="grid gap-1">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      {multiline ? (
        <textarea
          id={id}
          name={name}
          value={value}
          rows={3}
          aria-invalid={errors ? true : undefined}
          className={`${inputClassName} h-auto py-2`}
          onChange={(event) => onChange(event.target.value)}
        />
      ) : (
        <input
          id={id}
          name={name}
          type={type}
          value={value}
          aria-invalid={errors ? true : undefined}
          className={inputClassName}
          onChange={(event) => onChange(event.target.value)}
        />
      )}
      {errors?.map((message) => (
        <p key={message} className="text-sm text-destructive">
          {message}
        </p>
      ))}
    </div>
  )
}

function PageMessage({ children }: { children: ReactNode }) {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-2 px-4 py-10">
      {children}
    </main>
  )
}
