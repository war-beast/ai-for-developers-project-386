/** Ошибки полей формы в том виде, в каком их отдаёт API: поле → сообщения. */
export type FieldErrors = Record<string, string[]>

type TextFieldProps = {
  id: string
  label: string
  value: string | number
  errors?: string[]
  type?: 'text' | 'email' | 'number'
  rows?: number
  onChange: (value: string) => void
}

export const inputClassName =
  'h-9 w-full border border-input bg-background px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive'

/** Поле формы с подписью и сообщениями об ошибках рядом с ним. */
export function TextField({
  id,
  label,
  value,
  errors,
  type = 'text',
  rows,
  onChange,
}: TextFieldProps) {
  const invalid = errors?.length ? true : undefined

  return (
    <div className="grid gap-1">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      {rows ? (
        <textarea
          id={id}
          name={id}
          value={value}
          rows={rows}
          aria-invalid={invalid}
          className={`${inputClassName} h-auto py-2`}
          onChange={(event) => onChange(event.target.value)}
        />
      ) : (
        <input
          id={id}
          name={id}
          type={type}
          value={value}
          aria-invalid={invalid}
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
