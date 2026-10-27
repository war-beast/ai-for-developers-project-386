import { expect, test } from '@playwright/test'

test('владелец создаёт тип встречи, гость видит его в каталоге', async ({
  page,
}) => {
  const id = `intro-call-${Date.now()}`
  const title = `Знакомство ${id}`

  await page.goto('/admin')
  await page.getByLabel('Id (slug)').fill(id)
  await page.getByLabel('Название').fill(title)
  await page.getByLabel('Описание').fill('Короткий звонок')
  await page.getByLabel('Длительность, минут').fill('30')
  await page.getByRole('button', { name: 'Создать' }).click()

  // Тип появился в списке админки.
  await expect(page.getByRole('listitem').filter({ hasText: id })).toBeVisible()

  await page.getByRole('link', { name: 'Записаться' }).click()

  // Гость видит его в каталоге.
  const card = page.getByRole('listitem').filter({ hasText: title })
  await expect(card).toContainText('Короткий звонок')
  await expect(card).toContainText('30 минут')
})
