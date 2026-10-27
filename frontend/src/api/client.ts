import createClient from 'openapi-fetch'
import type { paths } from './schema'

/**
 * Привязка контракта TypeSpec к рантайму: типы приходят из generated SDK.
 * В dev через proxy Vite на бекенд, в prod — по абсолютному BASE_URL.
 */
export const api = createClient<paths>({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
})
