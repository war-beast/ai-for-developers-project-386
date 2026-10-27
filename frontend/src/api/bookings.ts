import { api } from './client'
import type { components } from './schema'

export type Booking = components['schemas']['Booking']
export type CreateBookingRequest = components['schemas']['CreateBookingRequest']
export type BookingProblem = components['schemas']['ProblemDetails']

export async function createBooking(
  body: CreateBookingRequest,
): Promise<Booking> {
  const { data, error } = await api.POST('/api/bookings', { body })

  if (error) {
    throw error
  }

  return data
}
