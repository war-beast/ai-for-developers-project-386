import { api } from './client'
import type { components } from './schema'

export type EventType = components['schemas']['EventType']
export type CreateEventTypeRequest = components['schemas']['CreateEventTypeRequest']
export type Problem = components['schemas']['ProblemDetails']

export async function fetchEventTypes(): Promise<EventType[]> {
  const { data } = await api.GET('/api/event-types')
  return data ?? []
}

export async function createEventType(
  body: CreateEventTypeRequest,
): Promise<EventType> {
  const { data, error } = await api.POST('/api/event-types', { body })

  if (error) {
    throw error
  }

  return data
}
