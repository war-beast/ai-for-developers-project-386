import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/api/client'
import { BookPage } from './BookPage'

vi.mock('@/api/client', () => ({ api: { GET: vi.fn() } }))

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <BookPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('BookPage', () => {
  beforeEach(() => {
    vi.mocked(api.GET).mockReset()
  })

  it('shows event type cards from the catalog', async () => {
    vi.mocked(api.GET).mockResolvedValue({
      data: [
        {
          id: 'intro-call',
          title: 'Знакомство',
          description: 'Короткий звонок',
          durationMinutes: 30,
        },
      ],
      response: new Response(),
    })

    renderPage()

    expect(await screen.findByText('Знакомство')).toBeTruthy()
    expect(screen.getByText('Короткий звонок')).toBeTruthy()
    expect(screen.getByText('30 минут')).toBeTruthy()
  })

  it('shows empty state when the catalog is empty', async () => {
    vi.mocked(api.GET).mockResolvedValue({ data: [], response: new Response() })

    renderPage()

    expect(await screen.findByText('Пока нет типов встреч')).toBeTruthy()
  })
})
