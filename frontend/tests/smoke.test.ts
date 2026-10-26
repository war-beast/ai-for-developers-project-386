import { test, expect } from 'vitest'

/**
 * Дымовой тест: бекенд поднялся и отвечает.
 * URL бекенда задаётся окружением (mise), по умолчанию — локальный порт.
 */
const baseUrl = process.env.SMOKE_URL ?? 'http://127.0.0.1:5150'

test('backend is up and answers calendar slots', async () => {
  const response = await fetch(`${baseUrl}/api/calendar/slots`)
  expect(response.status).toBe(200)

  const slots = (await response.json()) as Array<{ id: number; title: string }>
  expect(Array.isArray(slots)).toBe(true)
  expect(slots.length).toBeGreaterThan(0)
  expect(typeof slots[0]?.title).toBe('string')
})
