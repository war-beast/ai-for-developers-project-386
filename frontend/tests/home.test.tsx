// @vitest-environment jsdom
import { afterEach, expect, test } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'

import App from '@/App'

afterEach(cleanup)

function renderAt(path: string) {
  render(
    <MemoryRouter initialEntries={[path]}>
      <App />
    </MemoryRouter>,
  )
}

test('главная рассказывает про сервис', () => {
  renderAt('/')

  expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
    'Календарь звонков',
  )
  expect(screen.getByText(/ближайшие\s+14 дней/)).toBeTruthy()
})

test('главная ведёт на страницу записи', () => {
  renderAt('/')

  const link = screen.getByRole('link', { name: 'Записаться на звонок' })
  expect(link.getAttribute('href')).toBe('/book')
})

test('страница записи — заглушка со ссылкой на главную', () => {
  renderAt('/book')

  expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
    'Запись на звонок',
  )
  expect(screen.getByRole('link', { name: 'На главную' }).getAttribute('href')).toBe('/')
})
