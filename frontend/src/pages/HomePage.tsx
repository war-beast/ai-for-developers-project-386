export function HomePage() {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col items-center justify-center gap-6 px-4 text-center">
      <h1 className="text-5xl font-light tracking-tight">Календарь звонков</h1>
      <p className="max-w-xl text-muted-foreground">
        Сервис записи на звонок: владелец календаря публикует свободное время, а
        гость без регистрации выбирает тип встречи и свободный слот в ближайшие
        14 дней.
      </p>
    </main>
  )
}
