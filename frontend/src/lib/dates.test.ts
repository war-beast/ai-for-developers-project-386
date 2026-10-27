import { describe, expect, it } from 'vitest'

import { formatDateTime, formatDayLabel } from './dates'

describe('dates', () => {
  it('formats a day without the weekday: «3 ноя»', () => {
    expect(formatDayLabel('2026-11-03')).toBe('3 ноя')
  })

  it('formats date and time in the owner time zone: «3 ноя, 10:00»', () => {
    expect(formatDateTime('2026-11-03T10:00:00+03:00')).toBe('3 ноя, 10:00')
  })
})
