import { Link } from 'react-router'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

const guestFeatures = [
  'Выбрать тип встречи',
  'Увидеть свободные слоты на 14 дней',
  'Записаться без регистрации',
]

export function HomePage() {
  return (
    <main className="mx-auto grid w-full max-w-5xl flex-1 content-center gap-8 px-4 py-10 md:grid-cols-2">
      <div className="flex flex-col justify-center gap-6">
        <h1 className="text-5xl font-light tracking-tight">Календарь звонков</h1>
        <p className="text-muted-foreground">
          Сервис записи на звонок: владелец календаря публикует свободное время, а
          гость без регистрации выбирает тип встречи и свободный слот в ближайшие
          14 дней.
        </p>
        <Button asChild size="lg" className="w-fit">
          <Link to="/book">Записаться</Link>
        </Button>
      </div>

      <Card className="self-center">
        <CardHeader>
          <CardTitle>Что можно сделать</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="grid gap-2 text-muted-foreground">
            {guestFeatures.map((feature) => (
              <li key={feature} className="flex gap-2">
                <span aria-hidden>—</span>
                {feature}
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>
    </main>
  )
}
