import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { createBooking } from '@/api/bookings'
import { fetchEventTypes } from '@/api/event-types'
import { fetchSlots, type SlotWindow } from '@/api/slots'
import { EventTypePage } from './EventTypePage'

vi.mock('@/api/event-types', () => ({ fetchEventTypes: vi.fn() }))
vi.mock('@/api/slots', () => ({ fetchSlots: vi.fn() }))
vi.mock('@/api/bookings', () => ({ createBooking: vi.fn() }))

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
    vi.mocked(createBooking).mockReset()
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

  it('раскладывает дни календарём от понедельника и выделяет выходные', async () => {
    mockWindow(buildWindow({}))

    renderPage()

    const ribbon = await screen.findByRole('list', { name: 'Дни окна записи' })
    const monday = within(ribbon).getByRole('button', { name: '2 ноя' })
    const saturday = within(ribbon).getByRole('button', { name: '7 ноя' })

    // Окно начинается в среду 28 октября: две пустые клетки держат столбцы Пн и Вт.
    expect(ribbon.children[2]!.textContent).toBe('28 окт')
    expect(within(ribbon).getAllByRole('button')).toHaveLength(14)
    expect(monday.getAttribute('data-variant')).toBe('outline')
    expect(saturday.getAttribute('data-variant')).toBe('secondary')
    expect(screen.getByText('Пн')).toBeTruthy()
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

  it('books the selected slot and shows the confirmation screen', async () => {
    const slot = {
      start: '2026-10-28T09:00:00+03:00',
      end: '2026-10-28T09:30:00+03:00',
    }
    mockWindow(buildWindow({ 0: [slot] }))
    vi.mocked(createBooking).mockResolvedValue({
      id: 'booking-1',
      eventTypeId: 'intro-call',
      eventTypeTitle: 'Знакомство',
      start: slot.start,
      end: slot.end,
      guestName: 'Гость',
      guestEmail: 'guest@example.com',
      createdAt: '2026-10-27T10:00:00+00:00',
    })

    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: '09:00' }))
    fireEvent.change(screen.getByLabelText('Имя'), { target: { value: 'Гость' } })
    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'guest@example.com' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Забронировать' }))

    expect(await screen.findByText('Вы записаны')).toBeTruthy()
    expect(screen.getByText(/Знакомство/)).toBeTruthy()
    expect(screen.getByText(/28 окт, 09:00/)).toBeTruthy()
    expect(screen.getByText(/guest@example.com/)).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Записаться ещё' })).toBeTruthy()
    expect(screen.getByRole('link', { name: 'На главную' })).toBeTruthy()
  })

  it('clears the taken slot, keeps guest data and blocks resubmit until another slot', async () => {
    const taken = {
      start: '2026-10-28T09:00:00+03:00',
      end: '2026-10-28T09:30:00+03:00',
    }
    const free = {
      start: '2026-10-28T10:00:00+03:00',
      end: '2026-10-28T10:30:00+03:00',
    }
    mockWindow(buildWindow({ 0: [taken, free] }))
    vi.mocked(fetchSlots)
      .mockResolvedValueOnce(buildWindow({ 0: [taken, free] }))
      .mockResolvedValue(buildWindow({ 0: [free] }))
    vi.mocked(createBooking).mockRejectedValue({
      type: 'about:blank',
      title: 'Время уже занято',
      status: 409,
      code: 'slot_taken',
    })

    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: '09:00' }))
    fireEvent.change(screen.getByLabelText('Имя'), { target: { value: 'Гость' } })
    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'guest@example.com' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Забронировать' }))

    expect(await screen.findByText('Это время уже занято')).toBeTruthy()
    expect((screen.getByLabelText('Имя') as HTMLInputElement).value).toBe('Гость')
    expect((screen.getByLabelText('Email') as HTMLInputElement).value).toBe(
      'guest@example.com',
    )
    await waitFor(() =>
      expect(screen.queryByRole('button', { name: '09:00' })).toBeNull(),
    )

    // Занятый слот снят с выбора: повторная отправка ждёт новый слот.
    const submit = screen.getByRole('button', { name: 'Забронировать' })
    expect((submit as HTMLButtonElement).disabled).toBe(true)

    fireEvent.click(await screen.findByRole('button', { name: '10:00' }))

    expect(
      (screen.getByRole('button', { name: 'Забронировать' }) as HTMLButtonElement)
        .disabled,
    ).toBe(false)
  })
})
