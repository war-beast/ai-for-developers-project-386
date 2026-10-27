import { expect, test } from '@playwright/test'

test('владелец создаёт тип встречи, гость видит его в каталоге', async ({
  page,
}) => {
  const id = `intro-call-${Date.now()}`

  await page.goto('/admin')
  await page.getByLabel('Id (slug)').fill(id)
  await page.getByLabel('Название').fill('Знакомство')
  await page.getByLabel('Описание').fill('Короткий звонок')
  await page.getByLabel('Длительность, минут').fill('30')
  await page.getByRole('button', { name: 'Создать' }).click()

  // Тип появился в списке админки.
  await expect(page.getByText(id)).toBeVisible()

  await page.getByRole('link', { name: 'Записаться' }).click()

  // Гость видит его в каталоге.
  await expect(page.getByText('Знакомство')).toBeVisible()
  await expect(page.getByText('Короткий звонок')).toBeVisible()
  await expect(page.getByText('30 минут')).toBeVisible()
})
