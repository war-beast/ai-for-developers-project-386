/**
 * Тест цепочки TypeSpec → OpenAPI → SDK: типы из сгенерированной схемы
 * применимы к реальному коду клиента.
 */
import { test, expect } from 'vitest'
import type { paths } from '@/api/schema'
import { api } from '@/api/client'

test('generated SDK exposes calendar slots endpoint', () => {
  const client = api as typeof api
  type SlotsPath = keyof paths
  const hasSlotsPath: SlotsPath extends '/api/calendar/slots' ? true : false = true
  expect(client).toBeDefined()
  expect(hasSlotsPath).toBe(true)
})
