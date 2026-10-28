import { Calendar } from 'lucide-react'
import { Link, NavLink } from 'react-router'

const links = [
  { to: '/admin', label: 'Админка', end: true },
  { to: '/admin/meetings', label: 'Встречи', end: false },
]

export function SiteHeader() {
  return (
    <header className="sticky top-0 z-50 bg-primary text-primary-foreground">
      <div className="mx-auto flex h-14 w-full max-w-5xl items-center justify-between gap-4 px-4">
        <Link
          to="/"
          className="flex items-center gap-2 font-semibold tracking-wide"
        >
          <Calendar className="size-5" aria-hidden />
          Календарь
        </Link>
        <nav className="flex items-center gap-1 text-sm">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) =>
                `px-3 py-1.5 transition-colors hover:bg-primary-foreground/10 ${
                  isActive ? 'bg-primary-foreground/15 font-medium' : ''
                }`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      </div>
    </header>
  )
}
