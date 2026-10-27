import { expect, test } from '@playwright/test'

test('гость открывает тип, выбирает день и видит слоты', async ({
  page,
  request,
}) => {
  const id = `slots-${Date.now()}`

  const created = await request.post('/api/event-types', {
    data: {
      id,
      title: 'Слоты',
      description: 'Проверка окна записи',
      durationMinutes: 45,
    },
  })
  expect(created.ok()).toBeTruthy()

  await page.goto(`/book/${id}`)

  await expect(
    page.getByRole('heading', { level: 1, name: 'Слоты' }),
  ).toBeVisible()
  await expect(page.getByText(/Проверка окна записи/)).toBeVisible()

  const dayButtons = page
    .getByRole('list', { name: 'Дни окна записи' })
    .getByRole('button')
  await expect(dayButtons).toHaveCount(14)

  // Выходные без слотов, поэтому выбираем первый будний день.
  const slots = page.getByRole('list', { name: 'Слоты' })
  for (let index = 0; index < 14; index += 1) {
    await dayButtons.nth(index).click()
    if ((await slots.count()) > 0) {
      break
    }
  }

  // Слоты других сценариев уже могут занимать ранние часы, поэтому берём любой.
  const slot = slots.getByRole('button').first()
  await expect(slot).toBeVisible()

  await slot.click()
  await expect(slot).toHaveAttribute('aria-pressed', 'true')
})
