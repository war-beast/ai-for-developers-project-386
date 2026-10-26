import { Link } from 'react-router'
import { Calendar } from 'lucide-react'

import { Button } from '@/components/ui/button'

export function SiteHeader() {
  return (
    <header className="sticky top-0 z-50 bg-primary text-primary-foreground">
      <div className="mx-auto flex h-14 w-full max-w-3xl items-center justify-between gap-4 px-4">
        <Link
          to="/"
          className="flex items-center gap-2 font-semibold tracking-wide"
        >
          <Calendar className="size-5" aria-hidden />
          Календарь
        </Link>
        <Button
          asChild
          size="lg"
          className="rounded-none bg-primary-foreground text-primary hover:bg-primary-foreground/90"
        >
          <Link to="/book">Записаться на звонок</Link>
        </Button>
      </div>
    </header>
  )
}
