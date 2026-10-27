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

type FormState = {
  id: string
  title: string
  description: string
  durationMinutes: number
}

type FieldErrors = Record<string, string[]>

const emptyForm: FormState = {
  id: '',
  title: '',
  description: '',
  durationMinutes: 30,
}

const inputClassName =
  'h-9 w-full border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive'

type FieldProps = {
  name: keyof FormState
  label: string
  value: string | number
  errors?: string[]
  type?: 'text' | 'number' | 'textarea'
  onChange: (value: string) => void
}

function Field({
  name,
  label,
  value,
  errors,
  type = 'text',
  onChange,
}: FieldProps) {
  const id = `event-type-${name}`

  return (
    <div className="grid gap-1">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      {type === 'textarea' ? (
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
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-8 px-4 py-10">
      <h1 className="text-4xl font-light tracking-tight">Админка</h1>

      <section className="flex flex-col gap-4">
        <h2 className="text-xl font-medium">Новый тип встречи</h2>
        <form className="grid gap-4" onSubmit={handleSubmit}>
          <Field
            name="id"
            label="Id (slug)"
            value={form.id}
            errors={fieldErrors.id}
            onChange={(value) => setField('id', value)}
          />
          <Field
            name="title"
            label="Название"
            value={form.title}
            errors={fieldErrors.title}
            onChange={(value) => setField('title', value)}
          />
          <Field
            name="description"
            label="Описание"
            type="textarea"
            value={form.description}
            errors={fieldErrors.description}
            onChange={(value) => setField('description', value)}
          />
          <Field
            name="durationMinutes"
            label="Длительность, минут"
            type="number"
            value={form.durationMinutes}
            errors={fieldErrors.durationMinutes}
            onChange={(value) => setField('durationMinutes', value)}
          />
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
          <p className="text-muted-foreground">
            Пока нет типов встреч. Создайте первый в форме выше.
          </p>
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
    </main>
  )
}
