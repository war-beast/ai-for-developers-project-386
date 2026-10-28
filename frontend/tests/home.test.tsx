// @vitest-environment jsdom
import { afterEach, expect, test, vi } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'

import App from '@/App'

vi.mock('@/api/client', () => ({
  api: {
    GET: vi.fn().mockResolvedValue({ data: [], response: new Response() }),
  },
}))

afterEach(cleanup)

function renderAt(path: string) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

test('главная рассказывает про сервис', () => {
  renderAt('/')

  expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
    'Календарь звонков',
  )
  expect(screen.getByText(/ближайшие\s+14 дней/)).toBeTruthy()
})

test('панель навигации ведёт в админку и встречи', () => {
  renderAt('/')

  expect(screen.getByRole('link', { name: 'Календарь' }).getAttribute('href')).toBe('/')
  expect(screen.getByRole('link', { name: 'Админка' }).getAttribute('href')).toBe('/admin')
  expect(
    screen.getByRole('link', { name: 'Встречи' }).getAttribute('href'),
  ).toBe('/admin/meetings')
})

test('главная зовёт в каталог и перечисляет функции гостя', () => {
  renderAt('/')

  expect(screen.getByRole('link', { name: 'Записаться' }).getAttribute('href')).toBe('/book')
  expect(screen.getByText('Выбрать тип встречи')).toBeTruthy()
})

test('страница записи показывает пустой каталог', async () => {
  renderAt('/book')

  expect(screen.getByRole('heading', { level: 1 }).textContent).toBe(
    'Запись на звонок',
  )
  expect(await screen.findByText('Пока нет типов встреч')).toBeTruthy()
})
