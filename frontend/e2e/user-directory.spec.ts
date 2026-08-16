import { expect, test } from '@playwright/test'

test.describe.configure({ mode: 'serial' })

const ada = {
  name: 'Ada Lovelace',
  age: '36',
  city: 'London',
  state: 'Greater London',
  pincode: 'WC1E',
}

test.describe('User directory', () => {
  test('starts on the List page with an empty state and no console errors', async ({ page }) => {
    const consoleErrors: string[] = []
    page.on('console', (message) => {
      if (message.type() === 'error') consoleErrors.push(message.text())
    })
    page.on('pageerror', (error) => consoleErrors.push(error.message))

    await page.goto('/')

    await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible()
    await expect(page.getByText('No users yet')).toBeVisible()
    await expect(page.getByRole('table')).toBeHidden()

    expect(consoleErrors).toEqual([])
  })

  test('adds a user and shows it in the list', async ({ page }) => {
    await page.goto('/')
    await page.getByRole('link', { name: 'Add' }).click()

    await expect(page).toHaveURL(/\/add$/)
    await expect(page.getByRole('heading', { name: 'Add user' })).toBeVisible()

    await page.getByLabel('Name').fill(ada.name)
    await page.getByLabel('Age').fill(ada.age)
    await page.getByLabel('City').fill(ada.city)
    await page.getByLabel('State').fill(ada.state)
    await page.getByLabel('Pincode').fill(ada.pincode)

    await page.getByRole('button', { name: 'Save user' }).click()

    await expect(page.getByText('User added.')).toBeVisible()
    await expect(page).toHaveURL(/\/$/)

    const row = page.getByRole('row', { name: new RegExp(ada.name) })
    await expect(row).toBeVisible()
    for (const value of [ada.age, ada.city, ada.state, ada.pincode]) {
      await expect(row.getByText(value, { exact: true })).toBeVisible()
    }
  })

  test('persists the new user across a full page reload', async ({ page }) => {

    await page.goto('/')
    await expect(page.getByRole('row', { name: new RegExp(ada.name) })).toBeVisible()
  })

  test('blocks an invalid submission with inline messages', async ({ page }) => {
    await page.goto('/add')

    await page.getByLabel('Name').fill('A')
    await page.getByLabel('Age').fill('999')
    await page.getByRole('button', { name: 'Save user' }).click()

    await expect(page.getByText('Name must be between 2 and 100 characters.')).toBeVisible()
    await expect(page.getByText('Age must be between 0 and 120.')).toBeVisible()
    await expect(page.getByText('City is required.')).toBeVisible()
    await expect(page.getByText('State is required.')).toBeVisible()
    await expect(page.getByText('Pincode must be between 4 and 10 characters.')).toBeVisible()

    await expect(page.getByLabel('Name')).toHaveAttribute('aria-invalid', 'true')

    await expect(page).toHaveURL(/\/add$/)
  })

  test('serves a deep link to /add directly', async ({ page }) => {

    await page.goto('/add')

    await expect(page.getByRole('heading', { name: 'Add user' })).toBeVisible()
  })
})
