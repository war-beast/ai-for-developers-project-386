import type { components } from './schema'
import { api } from './client'

export type Slot = components['schemas']['Slot']
export type SlotDay = components['schemas']['SlotDay']
export type SlotWindow = components['schemas']['SlotWindow']

export async function fetchSlots(eventTypeId: string): Promise<SlotWindow> {
  const { data, error } = await api.GET('/api/event-types/{eventTypeId}/slots', {
    params: { path: { eventTypeId } },
  })

  if (error || !data) {
    throw error ?? new Error('Не удалось загрузить свободные слоты')
  }

  return data
}
