import { expect, test, type APIRequestContext } from '@playwright/test'

type SlotWindow = {
  days: { date: string; slots: { start: string }[] }[]
}

test('гость бронирует слот, повторная бронь того же слота даёт конфликт', async ({
  page,
  request,
}) => {
  const id = `booking-${Date.now()}`
  const created = await request.post('/api/event-types', {
    data: {
      id,
      title: 'Бронь',
      description: 'Проверка записи',
      durationMinutes: 45,
    },
  })
  expect(created.ok()).toBeTruthy()

  const firstSlot = await firstFreeSlot(request, id)

  await page.goto(`/book/${id}`)
  await page.locator(`button[data-date="${firstSlot.date}"]`).click()
  const slots = page.getByRole('list', { name: 'Слоты' })
  await slots.getByRole('button', { name: firstSlot.time }).click()

  await page.getByLabel('Имя').fill('Гость')
  await page.getByLabel('Email').fill('guest@example.com')
  await page.getByLabel('Комментарий').fill('Перезвоните')
  await page.getByRole('button', { name: 'Забронировать' }).click()

  await expect(
    page.getByRole('heading', { level: 1, name: 'Вы записаны' }),
  ).toBeVisible()
  await expect(page.getByText('Бронь')).toBeVisible()
  await expect(page.getByText(/guest@example\.com/)).toBeVisible()

  // После брони слот исчезает из выдачи.
  await page.goto(`/book/${id}`)
  await page.locator(`button[data-date="${firstSlot.date}"]`).click()
  await expect(
    slots.getByRole('button', { name: firstSlot.time }),
  ).toHaveCount(0)

  // Второй гость занимает следующий слот, пока первый заполняет форму.
  const nextSlot = await firstFreeSlot(request, id)
  await slots.getByRole('button', { name: nextSlot.time }).click()
  await page.getByLabel('Имя').fill('Второй гость')
  await page.getByLabel('Email').fill('second@example.com')

  const other = await request.post('/api/bookings', {
    data: {
      eventTypeId: id,
      start: nextSlot.start,
      guestName: 'Другой гость',
      guestEmail: 'other@example.com',
    },
  })
  expect(other.ok()).toBeTruthy()

  await page.getByRole('button', { name: 'Забронировать' }).click()

  await expect(page.getByText('Это время уже занято')).toBeVisible()
  await expect(page.getByLabel('Имя')).toHaveValue('Второй гость')
  await expect(page.getByLabel('Email')).toHaveValue('second@example.com')
  await expect(
    slots.getByRole('button', { name: nextSlot.time }),
  ).toHaveCount(0)
})

async function firstFreeSlot(request: APIRequestContext, eventTypeId: string) {
  const response = await request.get(`/api/event-types/${eventTypeId}/slots`)
  expect(response.ok()).toBeTruthy()
  const window = (await response.json()) as SlotWindow
  const day = window.days.find((candidate) => candidate.slots.length > 0)
  const slot = day?.slots[0]
  if (!day || !slot) {
    throw new Error('в окне записи нет свободных слотов')
  }

  return { date: day.date, time: slot.start.slice(11, 16), start: slot.start }
}
