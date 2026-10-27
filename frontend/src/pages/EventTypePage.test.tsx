import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { fetchEventTypes } from '@/api/event-types'
import { fetchSlots, type SlotWindow } from '@/api/slots'
import { EventTypePage } from './EventTypePage'

vi.mock('@/api/event-types', () => ({ fetchEventTypes: vi.fn() }))
vi.mock('@/api/slots', () => ({ fetchSlots: vi.fn() }))

function buildWindow(slotsByDay: Record<number, { start: string; end: string }[]>) {
  const days = Array.from({ length: 14 }, (_, index) => {
    const date = new Date(Date.UTC(2026, 9, 28 + index)).toISOString().slice(0, 10)
    return { date, slots: slotsByDay[index] ?? [] }
  })

  return {
    timeZone: 'Europe/Moscow',
    windowStart: days[0]!.date,
    windowEnd: days[13]!.date,
    stepMinutes: 30,
    days,
  } satisfies SlotWindow
}

function mockWindow(window: SlotWindow) {
  vi.mocked(fetchEventTypes).mockResolvedValue([
    {
      id: 'intro-call',
      title: 'Знакомство',
      description: 'Короткий звонок',
      durationMinutes: 30,
    },
  ])
  vi.mocked(fetchSlots).mockResolvedValue(window)
}

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/book/intro-call']}>
        <Routes>
          <Route path="/book/:typeId" element={<EventTypePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('EventTypePage', () => {
  beforeEach(() => {
    vi.mocked(fetchEventTypes).mockReset()
    vi.mocked(fetchSlots).mockReset()
  })

  it('shows the event type header and 14 day ribbon', async () => {
    mockWindow(
      buildWindow({
        0: [{ start: '2026-10-28T09:00:00+03:00', end: '2026-10-28T09:30:00+03:00' }],
      }),
    )

    renderPage()

    expect(await screen.findByRole('heading', { level: 1, name: 'Знакомство' })).toBeTruthy()
    expect(screen.getByText('Короткий звонок')).toBeTruthy()
    expect(screen.getByText(/30 минут/)).toBeTruthy()
    expect(screen.getByText(/Europe\/Moscow/)).toBeTruthy()

    const ribbon = screen.getByRole('list', { name: 'Дни окна записи' })
    expect(within(ribbon).getAllByRole('button')).toHaveLength(14)
  })

  it('selects the first day with slots and switches days', async () => {
    mockWindow(
      buildWindow({
        0: [
          { start: '2026-10-28T09:00:00+03:00', end: '2026-10-28T09:30:00+03:00' },
          { start: '2026-10-28T10:00:00+03:00', end: '2026-10-28T10:30:00+03:00' },
        ],
        1: [],
        2: [{ start: '2026-10-30T11:00:00+03:00', end: '2026-10-30T11:30:00+03:00' }],
      }),
    )

    renderPage()

    const ribbon = await screen.findByRole('list', { name: 'Дни окна записи' })
    const dayButtons = within(ribbon).getAllByRole('button')
    expect(dayButtons[0]!.getAttribute('aria-pressed')).toBe('true')

    const slots = screen.getByRole('list', { name: 'Слоты' })
    expect(within(slots).getAllByRole('button')).toHaveLength(2)
    expect(within(slots).getByText('09:00')).toBeTruthy()

    fireEvent.click(dayButtons[1]!)
    expect(await screen.findByText('В этот день свободных слотов нет')).toBeTruthy()

    fireEvent.click(dayButtons[2]!)
    expect(await screen.findByText('11:00')).toBeTruthy()
  })

  it('marks the clicked slot as selected', async () => {
    mockWindow(
      buildWindow({
        0: [{ start: '2026-10-28T09:00:00+03:00', end: '2026-10-28T09:30:00+03:00' }],
      }),
    )

    renderPage()

    const slot = await screen.findByRole('button', { name: '09:00' })
    expect(slot.getAttribute('aria-pressed')).toBe('false')

    fireEvent.click(slot)

    expect(screen.getByRole('button', { name: '09:00' }).getAttribute('aria-pressed')).toBe('true')
  })

  it('shows an empty message when the whole window has no slots', async () => {
    mockWindow(buildWindow({}))

    renderPage()

    expect(
      await screen.findByText('Свободных слотов в ближайшие 14 дней нет'),
    ).toBeTruthy()
  })

  it('shows a not found message for an unknown event type', async () => {
    vi.mocked(fetchEventTypes).mockResolvedValue([])
    vi.mocked(fetchSlots).mockResolvedValue(buildWindow({}))

    renderPage()

    expect(await screen.findByText('Тип встречи не найден')).toBeTruthy()
  })
})
