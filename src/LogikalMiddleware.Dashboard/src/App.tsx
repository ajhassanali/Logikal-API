import { useState, useEffect, useCallback, useRef } from 'react';
import { getHealth, getFileProjects, getFileElevations } from './api';
import { parsePartsListXml } from './partsParser';
import type { PartsTable } from './partsParser';
import type { FileProject, FileElevation } from './types';

const BASE = '/api';

type View = 'search' | 'project';

function App() {
  const [view, setView] = useState<View>('search');
  const [connected, setConnected] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [suggestions, setSuggestions] = useState<FileProject[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [searching, setSearching] = useState(false);
  const [selectedProject, setSelectedProject] = useState<FileProject | null>(null);
  const [elevations, setElevations] = useState<FileElevation[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [downloadStatus, setDownloadStatus] = useState('');
  const [partsData, setPartsData] = useState<Record<string, PartsTable[]>>({});
  const [partsLoading, setPartsLoading] = useState<string | null>(null);
  const [expandedParts, setExpandedParts] = useState<string | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const searchRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const check = () => getHealth().then(h => setConnected(h.status === 'connected')).catch(() => setConnected(false));
    check();
    const id = setInterval(check, 10000);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const onSearchChange = useCallback((value: string) => {
    setSearchTerm(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (value.trim().length < 2) {
      setSuggestions([]);
      setShowDropdown(false);
      return;
    }
    debounceRef.current = setTimeout(async () => {
      setSearching(true);
      try {
        const results = await getFileProjects(value.trim());
        setSuggestions(results);
        setShowDropdown(true);
      } catch { setSuggestions([]); }
      finally { setSearching(false); }
    }, 300);
  }, []);

  const formatResult = (p: FileProject) => {
    const parts: string[] = [];
    if (p.jobNumber) parts.push(p.jobNumber.replace(/\s/g, ''));
    if (p.projectCenter) parts.push(p.projectCenter);
    parts.push(p.name);
    return `${parts.join(' | ')} (${p.elevationCount})`;
  };

  const selectProject = useCallback(async (project: FileProject) => {
    setSelectedProject(project);
    setShowDropdown(false);
    setSearchTerm('');
    setSuggestions([]);
    setLoading(true);
    setError('');
    setPartsData({});
    setExpandedParts(null);
    try {
      const result = await getFileElevations(project.folderPath);
      setElevations(result);
      setView('project');
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load elevations');
    } finally { setLoading(false); }
  }, []);

  const goHome = () => {
    setView('search');
    setSelectedProject(null);
    setElevations([]);
    setError('');
    setDownloadStatus('');
    setPartsData({});
    setExpandedParts(null);
  };

  const loadParts = async (elevGuid: string) => {
    if (partsData[elevGuid]) {
      setExpandedParts(expandedParts === elevGuid ? null : elevGuid);
      return;
    }
    setPartsLoading(elevGuid);
    setExpandedParts(elevGuid);
    try {
      const res = await fetch(`${BASE}/elevations/${elevGuid}/partslist`);
      if (!res.ok) throw new Error(`Failed: ${res.statusText}`);
      const text = await res.text();
      const tables = parsePartsListXml(text);
      setPartsData(prev => ({ ...prev, [elevGuid]: tables }));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load parts');
      setExpandedParts(null);
    } finally { setPartsLoading(null); }
  };

  // --- Download functions ---

  const downloadElevationsCsv = () => {
    const header = 'Position,Name,Width (mm),Height (mm),System,State,Color,Area (m²),Qty';
    const rows = elevations.map(e =>
      `"${e.positionNumber}","${e.name}",${e.width},${e.height},"${e.system || ''}","${e.state || ''}","${e.colorWindow || ''}",${e.area},${e.quantity}`
    );
    const csv = [header, ...rows].join('\n');
    downloadBlob(csv, `${selectedProject?.name || 'elevations'}.csv`, 'text/csv');
  };

  const downloadAllThumbnails = async () => {
    if (!connected) return;
    setDownloadStatus('Downloading thumbnails...');
    try {
      const { default: JSZip } = await import('jszip');
      const zip = new JSZip();
      let count = 0;
      for (const e of elevations) {
        try {
          const res = await fetch(`${BASE}/elevations/${e.guid}/thumbnail`);
          if (res.ok) {
            const blob = await res.blob();
            zip.file(`${e.positionNumber}-${e.name || `elevation-${count}`}.png`, blob);
            count++;
            setDownloadStatus(`Thumbnails: ${count}/${elevations.length}...`);
          }
        } catch { /* skip failed thumbnails */ }
      }
      if (count > 0) {
        const content = await zip.generateAsync({ type: 'blob' });
        downloadBlob(content, `${selectedProject?.name || 'thumbnails'}.zip`, 'application/zip');
        setDownloadStatus(`Downloaded ${count} thumbnails`);
      } else {
        setDownloadStatus('No thumbnails available');
      }
    } catch {
      setDownloadStatus('Thumbnail download failed');
    }
  };

  const downloadPartsList = async () => {
    if (!connected) return;
    setDownloadStatus('Downloading parts lists...');
    try {
      const allRows: string[] = [];
      let count = 0;
      for (const e of elevations) {
        try {
          const res = await fetch(`${BASE}/elevations/${e.guid}/partslist`);
          if (res.ok) {
            const text = await res.text();
            const tables = parsePartsListXml(text);
            for (const table of tables) {
              for (const row of table.rows) {
                allRows.push(`"${e.positionNumber} - ${e.name}","${table.caption}",${row.map(c => `"${c}"`).join(',')}`);
              }
            }
            count++;
            setDownloadStatus(`Parts lists: ${count}/${elevations.length}...`);
          }
        } catch { /* skip failed parts lists */ }
      }
      if (allRows.length > 0) {
        const csv = ['Elevation,Category,Data...', ...allRows].join('\n');
        downloadBlob(csv, `${selectedProject?.name || 'partslist'}.csv`, 'text/csv');
        setDownloadStatus(`Downloaded parts for ${count} elevations`);
      } else {
        setDownloadStatus('No parts data available');
      }
    } catch {
      setDownloadStatus('Parts list download failed');
    }
  };

  const downloadBlob = (data: string | Blob, filename: string, type: string) => {
    const blob = data instanceof Blob ? data : new Blob([data], { type });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <div className="app">
      <header>
        <h1 onClick={goHome} style={{ cursor: 'pointer' }}>Logikal Dashboard</h1>
        <span className={`status ${connected ? 'connected' : 'disconnected'}`}>
          {connected ? 'Connected' : 'Disconnected'}
        </span>
      </header>

      <div className="search-box" ref={searchRef}>
        <div className="search-wrapper">
          <input
            type="text"
            placeholder="Search projects by name, customer, job number, or center..."
            value={searchTerm}
            onChange={e => onSearchChange(e.target.value)}
            onFocus={() => suggestions.length > 0 && setShowDropdown(true)}
          />
          {searching && <span className="search-spinner" />}
          {showDropdown && suggestions.length > 0 && (
            <div className="dropdown">
              {suggestions.map(p => (
                <div key={p.folderPath} className="dropdown-item" onClick={() => selectProject(p)}>
                  <span className="dropdown-main">{formatResult(p)}</span>
                  {p.customerName && <span className="dropdown-desc">{p.customerName}</span>}
                </div>
              ))}
            </div>
          )}
          {showDropdown && suggestions.length === 0 && searchTerm.trim().length >= 2 && !searching && (
            <div className="dropdown">
              <div className="dropdown-item dropdown-empty">No projects found</div>
            </div>
          )}
        </div>
      </div>

      {error && <div className="error">{error}</div>}

      {view === 'search' && !loading && (
        <div className="empty">
          <p>Search for a project</p>
          <p className="hint">Type a project name, customer name, job number, or project center</p>
        </div>
      )}

      {view === 'project' && !loading && selectedProject && (
        <>
          <div className="breadcrumb">
            <button onClick={goHome}>Home</button>
            <span>/</span>
            <span>
              {selectedProject.jobNumber ? `${selectedProject.jobNumber} - ` : ''}
              {selectedProject.projectCenter && `${selectedProject.projectCenter} / `}
              {selectedProject.name}
            </span>
          </div>

          <div style={{ marginBottom: 16 }}>
            <h2>
              {selectedProject.jobNumber && <span style={{ color: 'var(--primary)', marginRight: 8 }}>{selectedProject.jobNumber}</span>}
              {selectedProject.name}
            </h2>
            <div className="meta" style={{ fontSize: '0.95rem', marginTop: 4 }}>
              {selectedProject.projectCenter && <span>Center: {selectedProject.projectCenter}</span>}
              {selectedProject.customerName && <span style={{ marginLeft: 16 }}>Customer: {selectedProject.customerName}</span>}
              {selectedProject.offerNumber && <span style={{ marginLeft: 16 }}>Offer: {selectedProject.offerNumber}</span>}
              {selectedProject.userCreated && <span style={{ marginLeft: 16 }}>Created by: {selectedProject.userCreated}</span>}
            </div>
          </div>

          <div className="download-buttons" style={{ marginBottom: 20 }}>
            <button onClick={downloadElevationsCsv}>Download Elevations CSV</button>
            <button
              onClick={downloadAllThumbnails}
              disabled={!connected}
              title={!connected ? 'Requires Logikal connection' : ''}
              className={!connected ? 'btn-disabled' : ''}
            >
              Download All Thumbnails{!connected ? ' (Requires connection)' : ''}
            </button>
            <button
              onClick={downloadPartsList}
              disabled={!connected}
              title={!connected ? 'Requires Logikal connection' : ''}
              className={!connected ? 'btn-disabled' : ''}
            >
              Download Parts List{!connected ? ' (Requires connection)' : ''}
            </button>
          </div>
          {downloadStatus && <div style={{ fontSize: '0.85rem', color: '#64748b', marginBottom: 12 }}>{downloadStatus}</div>}

          {elevations.length > 0 ? (
            <div className="elevations-list">
              {elevations.map(e => (
                <div key={e.guid || e.positionNumber} className="elevation-row">
                  <div className="elevation-main">
                    {connected && (
                      <div className="elevation-thumb">
                        <img
                          src={`${BASE}/elevations/${e.guid}/thumbnail`}
                          alt={`${e.positionNumber} - ${e.name}`}
                          onError={ev => { (ev.target as HTMLImageElement).style.display = 'none'; }}
                        />
                      </div>
                    )}
                    <div className="elevation-info">
                      <div className="elevation-header">
                        <strong>{e.positionNumber}</strong>
                        <span className="elevation-name">{e.name}</span>
                        {e.state && <span className={`elevation-state state-${e.state.toLowerCase()}`}>{e.state}</span>}
                      </div>
                      <div className="elevation-details">
                        {(e.width > 0 || e.height > 0) && <span>{e.width} x {e.height} mm</span>}
                        {e.system && <span>{e.system}</span>}
                        {e.colorWindow && <span>Color: {e.colorWindow}</span>}
                        {e.area > 0 && <span>Area: {e.area} m²</span>}
                        {e.quantity > 1 && <span>Qty: {e.quantity}</span>}
                      </div>
                    </div>
                    {connected && (
                      <button
                        className="btn-parts"
                        onClick={() => loadParts(e.guid)}
                        disabled={partsLoading === e.guid}
                      >
                        {partsLoading === e.guid ? 'Loading...' : expandedParts === e.guid ? 'Hide Parts' : 'View Parts'}
                      </button>
                    )}
                    {!connected && (
                      <span className="parts-hint">Parts require connection</span>
                    )}
                  </div>
                  {expandedParts === e.guid && partsData[e.guid] && (
                    <div className="parts-panel">
                      {partsData[e.guid].map((table, i) => (
                        <div key={i} className="parts-table-section">
                          <h4>{table.caption} ({table.rows.length})</h4>
                          <table>
                            <thead>
                              <tr>
                                {table.headers.map((h, j) => <th key={j}>{h}</th>)}
                              </tr>
                            </thead>
                            <tbody>
                              {table.rows.map((row, ri) => (
                                <tr key={ri}>
                                  {row.map((cell, ci) => <td key={ci}>{cell}</td>)}
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="empty"><p>No elevations found in this project</p></div>
          )}
        </>
      )}

      {loading && <div className="loading">Loading...</div>}
    </div>
  );
}

export default App;
