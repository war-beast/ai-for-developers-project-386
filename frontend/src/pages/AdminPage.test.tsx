import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/api/client'
import { AdminPage } from './AdminPage'

vi.mock('@/api/client', () => ({ api: { GET: vi.fn(), POST: vi.fn() } }))

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <AdminPage />
    </QueryClientProvider>,
  )
}

describe('AdminPage', () => {
  beforeEach(() => {
    vi.mocked(api.GET).mockReset()
    vi.mocked(api.GET).mockResolvedValue({ data: [], response: new Response() })
    vi.mocked(api.POST).mockReset()
  })

  it('предлагает предустановленные длительности', () => {
    renderPage()

    const duration = screen.getByLabelText('Длительность, минут')

    expect(duration.tagName).toBe('SELECT')
    expect(
      Array.from((duration as HTMLSelectElement).options).map(
        (option) => option.textContent,
      ),
    ).toEqual(['15 минут', '30 минут', '45 минут', '1 час'])
  })

  it('shows field errors returned by the API next to the field', async () => {
    vi.mocked(api.POST).mockResolvedValue({
      error: {
        type: 'about:blank',
        title: 'Поля заполнены неверно',
        status: 400,
        code: 'validation_failed',
        errors: { title: ['Название обязательно.'] },
      },
      response: new Response(null, { status: 400 }),
    })

    renderPage()
    fireEvent.click(screen.getByRole('button', { name: 'Создать' }))

    expect(await screen.findByText('Название обязательно.')).toBeTruthy()
    expect(screen.getByLabelText('Название').getAttribute('aria-invalid')).toBe('true')
  })
})
