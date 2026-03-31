import { test, expect } from '@playwright/test';

test('search by name, view elevations, and verify downloads', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('header h1')).toHaveText('Logikal Dashboard');
  console.log('  PASS: Dashboard loaded');

  await expect(page.locator('.status.connected')).toBeVisible({ timeout: 60_000 });
  console.log('  PASS: Connected');

  // Search by name
  const input = page.locator('.search-wrapper input');
  await input.fill('Cedar');

  const dropdown = page.locator('.dropdown');
  await expect(dropdown).toBeVisible({ timeout: 30_000 });
  const items = page.locator('.dropdown-item:not(.dropdown-empty)');
  expect(await items.count()).toBeGreaterThan(0);
  console.log('  PASS: Name search shows results');

  await items.first().click();
  await expect(page.locator('.breadcrumb')).toBeVisible({ timeout: 10_000 });
  await expect(page.locator('h2')).toBeVisible();
  console.log(`  PASS: Project loaded: "${await page.locator('h2').textContent()}"`);

  // Verify elevations table
  await expect(page.locator('table')).toBeVisible({ timeout: 60_000 });
  const rows = page.locator('table tbody tr');
  expect(await rows.count()).toBeGreaterThan(0);
  console.log(`  PASS: ${await rows.count()} elevation(s) in table`);

  // Verify download buttons exist
  const downloadBtns = page.locator('.download-buttons button');
  expect(await downloadBtns.count()).toBe(3);
  console.log('  PASS: 3 download buttons present');

  // Verify button labels
  const btnTexts = await downloadBtns.allTextContents();
  expect(btnTexts).toContain('Download Elevations CSV');
  expect(btnTexts).toContain('Download All Thumbnails');
  expect(btnTexts).toContain('Download Parts List');
  console.log('  PASS: Download button labels correct');

  // Go home and search by job number (available immediately, no enrichment wait)
  await page.locator('header h1').click();
  await expect(page.locator('.empty')).toBeVisible({ timeout: 10_000 });
  console.log('  PASS: Home navigation works');

  // Search by job number
  await input.fill('3508');
  await expect(dropdown).toBeVisible({ timeout: 30_000 });
  const jnItems = page.locator('.dropdown-item:not(.dropdown-empty)');
  expect(await jnItems.count()).toBeGreaterThan(0);
  const text = await jnItems.first().textContent();
  console.log(`  PASS: Job number search found: "${text}"`);
  expect(text).toContain('3508');
  console.log('  PASS: Job number visible in search result');

  // Click to open and verify job number on project page
  await jnItems.first().click();
  await expect(page.locator('.breadcrumb')).toBeVisible({ timeout: 10_000 });
  const headerText = await page.locator('.breadcrumb').textContent();
  expect(headerText).toContain('3508');
  console.log('  PASS: Job number visible in breadcrumb');

  const metaText = await page.locator('.meta').textContent() || '';
  expect(metaText).toContain('3508');
  console.log('  PASS: Job number visible in project details');

  console.log('\n  === ALL CHECKS PASSED ===');
});
