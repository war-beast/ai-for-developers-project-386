import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { fetchBookings } from '@/api/bookings'
import { MeetingsPage } from './MeetingsPage'

vi.mock('@/api/bookings', () => ({ fetchBookings: vi.fn() }))

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MeetingsPage />
    </QueryClientProvider>,
  )
}

describe('MeetingsPage', () => {
  beforeEach(() => {
    vi.mocked(fetchBookings).mockReset()
  })

  it('shows time, type, guest, contact and comment of each meeting', async () => {
    vi.mocked(fetchBookings).mockResolvedValue([
      {
        id: 'booking-1',
        eventTypeId: 'intro-call',
        eventTypeTitle: 'Знакомство',
        start: '2026-10-28T09:00:00+03:00',
        end: '2026-10-28T09:30:00+03:00',
        guestName: 'Гость',
        guestEmail: 'guest@example.com',
        comment: 'Перезвоните',
        createdAt: '2026-10-27T10:00:00+00:00',
      },
      {
        id: 'booking-2',
        eventTypeId: 'hour-call',
        eventTypeTitle: 'Час',
        start: '2026-10-29T11:00:00+03:00',
        end: '2026-10-29T12:00:00+03:00',
        guestName: 'Второй гость',
        guestEmail: 'second@example.com',
        createdAt: '2026-10-27T10:00:00+00:00',
      },
    ])

    renderPage()

    const list = await screen.findByRole('list', { name: 'Предстоящие встречи' })
    const rows = within(list).getAllByRole('listitem')
    expect(rows).toHaveLength(2)
    expect(rows[0]!.textContent).toContain('28 окт, 09:00')
    expect(rows[0]!.textContent).toContain('Знакомство')
    expect(rows[0]!.textContent).toContain('Гость')
    expect(rows[0]!.textContent).toContain('guest@example.com')
    expect(rows[0]!.textContent).toContain('Перезвоните')
    expect(rows[1]!.textContent).toContain('29 окт, 11:00')
    expect(screen.getByText(/Europe\/Moscow/)).toBeTruthy()
  })

  it('shows an empty state without upcoming meetings', async () => {
    vi.mocked(fetchBookings).mockResolvedValue([])

    renderPage()

    expect(await screen.findByText('Предстоящих встреч пока нет')).toBeTruthy()
  })
})
