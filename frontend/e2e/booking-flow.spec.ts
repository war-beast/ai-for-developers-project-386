import { expect, test } from '@playwright/test'

test('владелец создаёт тип, гость бронирует слот, владелец видит встречу', async ({
  page,
}) => {
  const id = `full-flow-${Date.now()}`
  const email = `${id}@example.com`

  // Владелец создаёт тип встречи в админке.
  await page.goto('/admin')
  await page.getByLabel('Id (slug)').fill(id)
  await page.getByLabel('Название').fill('Полный сценарий')
  await page.getByLabel('Описание').fill('Сквозная проверка')
  await page.getByLabel('Длительность, минут').fill('30')
  await page.getByRole('button', { name: 'Создать' }).click()
  await expect(page.getByText(id)).toBeVisible()

  // Гость находит тип в каталоге и открывает его.
  await page.getByRole('link', { name: 'Записаться' }).click()
  await page.getByRole('link', { name: 'Полный сценарий' }).click()
  await expect(
    page.getByRole('heading', { level: 1, name: 'Полный сценарий' }),
  ).toBeVisible()

  // Выходные в ленте без слотов, поэтому берём первый день со слотами.
  const dayButtons = page
    .getByRole('list', { name: 'Дни окна записи' })
    .getByRole('button')
  const slots = page.getByRole('list', { name: 'Слоты' })
  for (let index = 0; index < 14; index += 1) {
    await dayButtons.nth(index).click()
    if ((await slots.getByRole('button').count()) > 0) {
      break
    }
  }

  const slot = slots.getByRole('button').first()
  const time = (await slot.textContent()) ?? ''
  await slot.click()
  await page.getByLabel('Имя').fill('Гость сценария')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Комментарий').fill('Сквозной сценарий')
  await page.getByRole('button', { name: 'Забронировать' }).click()

  await expect(
    page.getByRole('heading', { level: 1, name: 'Вы записаны' }),
  ).toBeVisible()
  await expect(page.getByText(email)).toBeVisible()

  // Владелец видит созданную через UI запись в списке встреч.
  await page.goto('/admin/meetings')
  const row = page
    .getByRole('list', { name: 'Предстоящие встречи' })
    .getByRole('listitem')
    .filter({ hasText: email })
  await expect(row).toBeVisible()
  await expect(row).toContainText('Полный сценарий')
  await expect(row).toContainText('Гость сценария')
  await expect(row).toContainText('Сквозной сценарий')
  await expect(row).toContainText(time)
})
