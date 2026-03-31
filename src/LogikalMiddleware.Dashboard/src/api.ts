import type { Project, Elevation, FileProject, FileElevation } from './types';

const BASE = '/api';

export async function searchProjects(term: string): Promise<Project[]> {
  const res = await fetch(`${BASE}/projects/search?term=${encodeURIComponent(term)}`);
  if (!res.ok) throw new Error(`Search failed: ${res.statusText}`);
  return res.json();
}

export async function getElevations(projectGuid: string): Promise<Elevation[]> {
  const res = await fetch(`${BASE}/projects/${projectGuid}/elevations`);
  if (!res.ok) throw new Error(`Failed to get elevations: ${res.statusText}`);
  return res.json();
}

export function getThumbnailUrl(elevationGuid: string): string {
  return `${BASE}/elevations/${elevationGuid}/thumbnail`;
}

export async function getPartsList(elevationGuid: string): Promise<string> {
  const res = await fetch(`${BASE}/elevations/${elevationGuid}/partslist`);
  if (!res.ok) throw new Error(`Failed to get parts list: ${res.statusText}`);
  return res.text();
}

export async function getHealth(): Promise<{ status: string; framework?: string }> {
  const res = await fetch(`${BASE}/health`);
  if (!res.ok) throw new Error('Health check failed');
  return res.json();
}

export async function getFileProjects(search?: string): Promise<FileProject[]> {
  const params = search ? `?search=${encodeURIComponent(search)}` : '';
  const res = await fetch(`${BASE}/files/projects${params}`);
  if (!res.ok) throw new Error(`Failed to get file projects: ${res.statusText}`);
  return res.json();
}

export async function getFileElevations(folder: string): Promise<FileElevation[]> {
  const res = await fetch(`${BASE}/files/elevations?folder=${encodeURIComponent(folder)}`);
  if (!res.ok) throw new Error(`Failed to get file elevations: ${res.statusText}`);
  return res.json();
}
