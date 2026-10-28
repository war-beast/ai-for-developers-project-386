import { useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import {
  createEventType,
  fetchEventTypes,
  type EventType,
  type Problem,
} from '@/api/event-types'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  inputClassName,
  TextField,
  type FieldErrors,
} from '@/components/text-field'

type FormState = {
  id: string
  title: string
  description: string
  durationMinutes: number
}

const durationOptions = [
  { value: 15, label: '15 минут' },
  { value: 30, label: '30 минут' },
  { value: 45, label: '45 минут' },
  { value: 60, label: '1 час' },
]

const emptyForm: FormState = {
  id: '',
  title: '',
  description: '',
  durationMinutes: 30,
}

export function AdminPage() {
  const queryClient = useQueryClient()
  const { data: eventTypes = [], isPending } = useQuery({
    queryKey: ['event-types'],
    queryFn: fetchEventTypes,
  })
  const [form, setForm] = useState(emptyForm)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})

  const create = useMutation<EventType, Problem, FormState>({
    mutationFn: createEventType,
    onSuccess: () => {
      setForm(emptyForm)
      setFieldErrors({})
      void queryClient.invalidateQueries({ queryKey: ['event-types'] })
    },
    onError: (problem) => {
      setFieldErrors(
        problem.errors ??
          (problem.code === 'event_type_exists'
            ? { id: [problem.detail ?? 'Такой id уже занят.'] }
            : {}),
      )
    },
  })

  function setField(name: keyof FormState, value: string) {
    setForm((current) => ({
      ...current,
      [name]: name === 'durationMinutes' ? Number(value) : value,
    }))
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    create.mutate(form)
  }

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col gap-8 px-4 py-10">
      <h1 className="text-4xl font-light tracking-tight">Админка</h1>

      <div className="grid gap-8 md:grid-cols-2">
        <section className="flex flex-col gap-4">
          <h2 className="text-xl font-medium">Новый тип встречи</h2>
          <form className="grid gap-4" onSubmit={handleSubmit}>
            <TextField
              id="event-type-id"
              label="Id (slug)"
              value={form.id}
              errors={fieldErrors.id}
              onChange={(value) => setField('id', value)}
            />
            <TextField
              id="event-type-title"
              label="Название"
              value={form.title}
              errors={fieldErrors.title}
              onChange={(value) => setField('title', value)}
            />
            <TextField
              id="event-type-description"
              label="Описание"
              rows={3}
              value={form.description}
              errors={fieldErrors.description}
              onChange={(value) => setField('description', value)}
            />
            <div className="grid gap-1">
              <label htmlFor="event-type-duration" className="text-sm font-medium">
                Длительность, минут
              </label>
              <select
                id="event-type-duration"
                name="event-type-duration"
                value={form.durationMinutes}
                aria-invalid={fieldErrors.durationMinutes?.length ? true : undefined}
                className={inputClassName}
                onChange={(event) => setField('durationMinutes', event.target.value)}
              >
                {durationOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {fieldErrors.durationMinutes?.map((message) => (
                <p key={message} className="text-sm text-destructive">
                  {message}
                </p>
              ))}
            </div>
            <div className="flex items-center gap-3">
              <Button type="submit" size="lg" disabled={create.isPending}>
                Создать
              </Button>
              {create.isError && Object.keys(fieldErrors).length === 0 ? (
                <p className="text-sm text-destructive">
                  Не удалось создать тип встречи.
                </p>
              ) : null}
            </div>
          </form>
        </section>

        <section className="flex flex-col gap-4">
          <h2 className="text-xl font-medium">Типы встреч</h2>
          {isPending ? (
            <p className="text-muted-foreground">Загрузка…</p>
          ) : eventTypes.length === 0 ? (
            <p className="text-muted-foreground">Пока нет типов встреч.</p>
          ) : (
            <ul className="grid gap-3">
              {eventTypes.map((eventType) => (
                <li key={eventType.id}>
                  <Card size="sm">
                    <CardHeader>
                      <CardTitle>{eventType.title}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-1 text-muted-foreground">
                      <span>{eventType.description}</span>
                      <span>
                        {eventType.durationMinutes} минут · {eventType.id}
                      </span>
                    </CardContent>
                  </Card>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </main>
  )
}
