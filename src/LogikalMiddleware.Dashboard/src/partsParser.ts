export interface PartsTable {
  caption: string;
  headers: string[];
  rows: string[][];
}

export function parsePartsListXml(xmlText: string): PartsTable[] {
  // The API returns JSON wrapping XML content, or raw XML
  let xml = xmlText;
  try {
    const parsed = JSON.parse(xmlText);
    if (parsed.content) xml = parsed.content;
    else if (typeof parsed === 'string') xml = parsed;
  } catch {
    // Already raw XML
  }

  const parser = new DOMParser();
  const doc = parser.parseFromString(xml, 'text/xml');
  const tables: PartsTable[] = [];

  doc.querySelectorAll('table').forEach(tableEl => {
    const caption = tableEl.getAttribute('caption') || 'Unknown';
    const headerEl = tableEl.querySelector('header');
    if (!headerEl) return;

    // Build header column names, keyed by id
    const columnMap = new Map<string, string>();
    const headers: string[] = [];
    headerEl.querySelectorAll('column').forEach(col => {
      const id = col.getAttribute('id') || '';
      const name = col.textContent?.trim() || '';
      const unit = col.getAttribute('unit');
      const label = unit ? `${name} (${unit})` : name;
      columnMap.set(id, label);
      headers.push(label);
    });

    const columnIds = Array.from(columnMap.keys());
    const rows: string[][] = [];

    tableEl.querySelectorAll('row').forEach(rowEl => {
      const row: string[] = [];
      for (const colId of columnIds) {
        const cell = rowEl.querySelector(`column[id="${colId}"]`);
        row.push(cell?.textContent?.trim() || '');
      }
      rows.push(row);
    });

    tables.push({ caption, headers, rows });
  });

  return tables;
}
