/**
 * SurakshaAR Command Dashboard Logic
 * Connects to FastAPI Backend at http://127.0.0.1:8000
 * Subscribes to live WebSocket telemetry at ws://127.0.0.1:8000/api/v1/events/live
 */

const API_BASE = window.location.origin.includes(':8000') 
  ? window.location.origin 
  : 'http://127.0.0.1:8000';

const WS_PATH = '/api/v1/events/live';

let ws = null;
let wsReconnectTimer = null;
let eventCount = 0;

// --- Day 6: Bearer-token session (admin login via POST /api/v1/auth/login) ---
const TOKEN_KEY = 'surakshaar.admin.token';
const TOKEN_EXPIRY_KEY = 'surakshaar.admin.token.expires_at';

function getToken() {
  try {
    const token = sessionStorage.getItem(TOKEN_KEY);
    const exp = Number(sessionStorage.getItem(TOKEN_EXPIRY_KEY) || '0');
    if (!token) return null;
    if (exp && Date.now() > exp) {
      clearToken();
      return null;
    }
    return token;
  } catch (e) {
    return null;
  }
}

function setToken(token, expiresInSec) {
  try {
    sessionStorage.setItem(TOKEN_KEY, token);
    if (expiresInSec) {
      sessionStorage.setItem(TOKEN_EXPIRY_KEY, String(Date.now() + (expiresInSec - 30) * 1000));
    }
  } catch (e) { /* storage unavailable: session still works in-memory for this page */ }
}

function clearToken() {
  try {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_EXPIRY_KEY);
  } catch (e) { /* ignore */ }
}

function wsUrl() {
  const base = API_BASE.replace(/^http/, 'ws') + WS_PATH;
  const token = getToken();
  return token ? `${base}?token=${encodeURIComponent(token)}` : base;
}

// Central fetch helper: injects `Authorization: Bearer <token>` when signed in.
// 401 -> show the login overlay; 403 -> surface an access banner.
async function apiFetch(path, options = {}) {
  const token = getToken();
  const headers = { ...(options.headers || {}) };
  if (token && !headers.Authorization) {
    headers.Authorization = `Bearer ${token}`;
  }
  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (res.status === 401) {
    handleUnauthorized();
  } else if (res.status === 403) {
    showForbiddenBanner();
  }
  return res;
}

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

// Initialize
document.addEventListener('DOMContentLoaded', () => {
  document.getElementById('api-url').textContent = `${API_BASE}/api/v1`;

  document.getElementById('login-form').addEventListener('submit', handleLogin);
  document.getElementById('btn-logout').addEventListener('click', handleLogout);
  document.getElementById('worker-modal-close').addEventListener('click', closeWorkerModal);
  document.getElementById('worker-modal').addEventListener('click', (e) => {
    if (e.target.id === 'worker-modal') closeWorkerModal();
  });

  document.getElementById('btn-refresh').addEventListener('click', () => {
    loadSummary();
    loadWorkers();
  });

  document.getElementById('btn-verify').addEventListener('click', verifyCertificate);
  document.getElementById('cert-input').addEventListener('keydown', (e) => {
    if (e.key === 'Enter') verifyCertificate();
  });

  document.getElementById('btn-clear-feed').addEventListener('click', () => {
    const feed = document.getElementById('feed-container');
    feed.innerHTML = '<div class="feed-empty">Feed cleared. Waiting for new events...</div>';
    eventCount = 0;
    document.getElementById('feed-counter').textContent = `0 events received this session`;
  });

  if (getToken()) {
    hideLogin();
    loadSummary();
    loadWorkers();
    setupWebSocket();
  } else {
    showLogin();
  }
});

// --- Auth UI ---
async function handleLogin(e) {
  e.preventDefault();
  const username = document.getElementById('login-username').value.trim();
  const password = document.getElementById('login-password').value;
  const errBox = document.getElementById('login-error');
  const btn = document.getElementById('btn-login');
  errBox.style.display = 'none';
  btn.disabled = true;
  btn.textContent = 'Signing in...';
  try {
    const res = await fetch(`${API_BASE}/api/v1/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok || data.role !== 'admin') {
      const msg = !res.ok
        ? (data.detail || `Login failed (HTTP ${res.status})`)
        : 'Dashboard sign-in requires an admin account.';
      errBox.textContent = Array.isArray(msg) ? msg.map(m => m.msg || m).join('; ') : msg;
      errBox.style.display = 'block';
      return;
    }
    setToken(data.access_token, data.expires_in);
    sessionStorage.setItem('surakshaar.admin.username', data.username || username);
    hideLogin();
    loadSummary();
    loadWorkers();
    setupWebSocket();
  } catch (err) {
    errBox.textContent = `Cannot reach backend: ${err.message}`;
    errBox.style.display = 'block';
  } finally {
    btn.disabled = false;
    btn.textContent = 'Sign In';
  }
}

function handleLogout() {
  clearToken();
  try { sessionStorage.removeItem('surakshaar.admin.username'); } catch (e) { /* ignore */ }
  if (ws) { try { ws.close(); } catch (e) { /* ignore */ } ws = null; }
  if (wsReconnectTimer) { clearTimeout(wsReconnectTimer); wsReconnectTimer = null; }
  showLogin();
}

function showLogin() {
  document.getElementById('login-overlay').style.display = 'flex';
  updateSessionPill();
}

function hideLogin() {
  document.getElementById('login-overlay').style.display = 'none';
  updateSessionPill();
}

function updateSessionPill() {
  let username = null;
  try { username = sessionStorage.getItem('surakshaar.admin.username'); } catch (e) { /* ignore */ }
  const pill = document.getElementById('session-pill');
  const label = document.getElementById('session-user');
  if (getToken()) {
    label.textContent = `Signed in: ${username || 'admin'}`;
    pill.classList.add('session-active');
  } else {
    label.textContent = 'Not signed in';
    pill.classList.remove('session-active');
  }
}

function handleUnauthorized() {
  clearToken();
  showLogin();
}

function showForbiddenBanner() {
  const statusEl = document.getElementById('connection-status');
  if (statusEl) statusEl.textContent = 'Access denied (admin token required)';
}

// Load Summary KPIs and Modules (protected: admin Bearer token via apiFetch)
async function loadSummary() {
  try {
    const res = await apiFetch(`/api/v1/dashboard/summary`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();

    document.getElementById('stat-total-workers').textContent = data.total_workers ?? 0;
    document.getElementById('stat-certified-workers').textContent = data.certified_workers ?? 0;
    document.getElementById('stat-workers-training').textContent = data.workers_in_training ?? 0;
    document.getElementById('stat-pass-rate').textContent = `${data.pass_rate ?? 0}%`;

    // Render Modules
    const modList = document.getElementById('module-stats-list');
    if (data.module_stats && data.module_stats.length > 0) {
      modList.innerHTML = data.module_stats.map(m => `
        <div class="module-item">
          <div class="module-info">
            <h4>${m.module_name}</h4>
            <span class="badge-tag">ID: ${m.module_id}</span>
          </div>
          <div class="module-counts">
            <div class="count-badge">
              <div class="count-num">${m.workers_enrolled}</div>
              <div class="count-text">Enrolled</div>
            </div>
            <div class="count-badge">
              <div class="count-num" style="color: #10B981;">${m.certified}</div>
              <div class="count-text">Certified</div>
            </div>
          </div>
        </div>
      `).join('');
    } else {
      modList.innerHTML = '<div class="feed-empty">No module training data yet.</div>';
    }

    // Render Weaknesses
    const weakList = document.getElementById('weakness-list');
    if (data.common_weaknesses && data.common_weaknesses.length > 0) {
      weakList.innerHTML = data.common_weaknesses.map(w => `
        <div class="weakness-item">
          <div>
            <strong style="color: #F8FAFC;">${formatCategory(w.competency_name)}</strong>
            <div style="font-size: 11px; color: #94A3B8;">Avg Score: ${w.average_score ?? 'N/A'}%</div>
          </div>
          <span class="badge-pill badge-due">${w.count} Occurrences</span>
        </div>
      `).join('');
    } else {
      weakList.innerHTML = '<div class="feed-empty">No critical weaknesses detected across workers.</div>';
    }

  } catch (err) {
    console.error('Failed to load summary:', err);
  }
}

// Load Worker Table with live Day 1/7/30 Retention info (protected: admin token)
async function loadWorkers() {
  try {
    const res = await apiFetch(`/api/v1/dashboard/workers`);
    if (res.status === 401) return;
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();

    const tbody = document.getElementById('workers-tbody');
    if (data.workers && data.workers.length > 0) {
      const rows = await Promise.all(data.workers.map(async (w) => {
        const certList = w.certified_modules && w.certified_modules.length > 0
          ? w.certified_modules.map(c => `<span class="badge-pill badge-pass">${escapeHtml(c).toUpperCase()}</span>`).join(' ')
          : '<span class="badge-pill badge-pending">None</span>';
        const retention = await fetchRetentionSummary(w.id);

        return `
          <tr>
            <td><strong>${escapeHtml(w.name)}</strong></td>
            <td><code>${escapeHtml(w.employee_id)}</code></td>
            <td>${escapeHtml(w.role)}</td>
            <td>${certList}</td>
            <td>${retention}</td>
            <td><button class="btn-refresh" style="padding: 4px 8px; font-size: 11px;" onclick="viewWorkerProfile(${w.id})">Audit Profile</button></td>
          </tr>
        `;
      }));
      tbody.innerHTML = rows.join('');
    } else {
      tbody.innerHTML = '<tr><td colspan="6" class="text-center" style="padding: 24px;">No workers registered yet. Register workers in app or via API.</td></tr>';
    }
  } catch (err) {
    console.error('Failed to load workers:', err);
  }
}


// Day 6: real Day 1/7/30 retention status per worker (GET /progress/{id}/retention).
// Collapses every milestone across all module schedules into one badge summary.
async function fetchRetentionSummary(workerId) {
  try {
    const res = await apiFetch(`/api/v1/progress/${workerId}/retention`);
    if (!res.ok) return '<span style="color: #64748B;">--</span>';
    const data = await res.json();
    const schedules = data.retention_schedules || [];
    const milestones = schedules.flatMap(s => s.milestones || []);
    if (milestones.length === 0) return '<span style="color: #64748B;">No schedule</span>';
    const due = milestones.filter(m => m.status === 'due').length;
    const completed = milestones.filter(m => m.status === 'completed').length;
    const pending = milestones.filter(m => m.status === 'pending').length;
    if (due > 0) return `<span class="badge-pill badge-due">${due} Due</span>`;
    if (pending > 0) return `<span class="badge-pill badge-pending">${completed} Done • ${pending} Sched</span>`;
    return `<span class="badge-pill badge-pass">${completed} Done</span>`;
  } catch (err) {
    console.error('fetchRetentionSummary error:', err);
    return '<span style="color: #64748B;">--</span>';
  }
}

// View Detailed Worker Profile & Retention (Day 6: modal, Bearer token)
window.viewWorkerProfile = async function(workerId) {
  openWorkerModal('Loading worker profile...');
  try {
    const [wRes, rRes] = await Promise.all([
      apiFetch(`/api/v1/dashboard/workers/${workerId}`),
      apiFetch(`/api/v1/progress/${workerId}/retention`)
    ]);
    if (wRes.status === 401 || rRes.status === 401) return;
    const worker = await wRes.json();
    let retentionHtml = '<div class="loading-placeholder">No retention schedule found.</div>';
    if (rRes.ok) {
      const ret = await rRes.json();
      if (ret.retention_schedules && ret.retention_schedules.length > 0) {
        retentionHtml = ret.retention_schedules.map(s => `
          <div style="margin-top: 8px; padding: 8px; background: #13273A; border-radius: 6px;">
            <strong style="color: #38BDF8;">${escapeHtml(s.module_name)} (${escapeHtml(s.module_code)})</strong><br>
            <span style="font-size: 11px; color: #94A3B8;">Anchor: ${s.base_date ? new Date(s.base_date).toLocaleDateString() : 'N/A'}</span><br>
            ${(s.milestones || []).map(m => `
              <span class="badge-pill ${m.status === 'completed' ? 'badge-pass' : (m.status === 'due' ? 'badge-due' : 'badge-pending')}">
                Day ${m.day}: ${m.status.toUpperCase()}${m.score != null ? ` (${m.score}%)` : ''}
              </span>
            `).join(' ')}
          </div>
        `).join('');
      }
    }
    const certs = (worker.certificates || []).map(c =>
      `<span class="badge-pill badge-pass">${escapeHtml(c.certificate_number || 'CERT')}</span>`
    ).join(' ') || '<span style="color: #64748B;">None yet</span>';
    const profile = (worker.competency_profile || []).map(cp => `
      <div style="margin-top: 8px; padding: 8px; background: #13273A; border-radius: 6px;">
        <strong style="color: #F8FAFC;">${escapeHtml(cp.module_name)}</strong>
        <span class="badge-pill ${cp.passed ? 'badge-pass' : 'badge-due'}" style="margin-left: 6px;">
          ${cp.passed ? 'PASSED' : 'NOT PASSED'}
        </span>
        <div style="font-size: 12px; color: #94A3B8; margin-top: 4px;">
          Attempt #${cp.attempt_number} - Score: ${cp.overall_score ?? 'N/A'}%
        </div>
      </div>
    `).join('') || '<div class="loading-placeholder">No assessments yet.</div>';
    openWorkerModal(`
      <div style="margin-bottom: 12px;">
        <strong style="font-size: 16px;">${escapeHtml(worker.name)}</strong>
        <span style="color: #94A3B8;"> - ${escapeHtml(worker.employee_id)} - ${escapeHtml(worker.role)}</span>
      </div>
      <h4 style="color: #38BDF8; margin: 10px 0 4px;">Certificates</h4>
      <div>${certs}</div>
      <h4 style="color: #38BDF8; margin: 12px 0 4px;">Competency Profile</h4>
      <div>${profile}</div>
      <h4 style="color: #38BDF8; margin: 12px 0 4px;">Day 1 / 7 / 30 Retention</h4>
      <div>${retentionHtml}</div>
    `, `${escapeHtml(worker.name)} Profile`);
  } catch (err) {
    openWorkerModal(`<div class="verify-result-box verify-invalid">Error: ${escapeHtml(err.message)}</div>`);
  }
};

function openWorkerModal(html, title) {
  if (title) document.getElementById('worker-modal-title').textContent = title;
  document.getElementById('worker-modal-body').innerHTML = html;
  document.getElementById('worker-modal').style.display = 'flex';
}

function closeWorkerModal() {
  document.getElementById('worker-modal').style.display = 'none';
}

// Verify Certificate
async function verifyCertificate() {
  const input = document.getElementById('cert-input');
  const resultBox = document.getElementById('cert-verify-result');
  const certId = input.value.trim();

  if (!certId) {
    alert('Please enter a certificate ID.');
    return;
  }

  resultBox.style.display = 'block';
  resultBox.className = 'verify-result-box';
  resultBox.innerHTML = 'Verifying with Ministry of Mines cryptographic register...';

  try {
    // Public endpoint: no Bearer token attached (QR scans resolve without credentials).
    const res = await fetch(`${API_BASE}/api/v1/certificates/verify/${encodeURIComponent(certId)}`);
    if (res.status === 200) {
      const data = await res.json();
      resultBox.className = 'verify-result-box verify-valid';
      resultBox.innerHTML = `
        <strong>VALID - CERTIFICATE VALID and AUTHENTIC</strong><br>
        - Certificate ID: <code>${escapeHtml(data.certificate_number)}</code><br>
        - Certified Worker: <strong>${escapeHtml(data.worker_name)}</strong><br>
        - Training Module: <strong>${escapeHtml(data.module_name)}</strong><br>
        - Status: Active Industrial Safety Credential
      `;
    } else if (res.status === 422) {
      resultBox.className = 'verify-result-box verify-invalid';
      resultBox.innerHTML = `Certificate numbers look like <code>SUR-YYYY-NNNN</code> (e.g. from an issued certificate). Check the ID and try again.`;
    } else {
      resultBox.className = 'verify-result-box verify-invalid';
      resultBox.innerHTML = `INVALID CERTIFICATE: The certificate ID '${escapeHtml(certId)}' was not found in the national registry.`;
    }
  } catch (err) {
    resultBox.className = 'verify-result-box verify-invalid';
    resultBox.innerHTML = `Error verifying certificate: ${escapeHtml(err.message)}`;
  }
}

// WebSocket Live Telemetry (Day 6: admin Bearer token via ?token= query param)
function setupWebSocket() {
  const statusEl = document.getElementById('connection-status');
  const pillEl = document.getElementById('live-pill');

  if (ws) { try { ws.close(); } catch (e) { /* ignore */ } ws = null; }
  if (wsReconnectTimer) { clearTimeout(wsReconnectTimer); wsReconnectTimer = null; }
  if (!getToken()) {
    statusEl.textContent = 'Sign in for live stream...';
    return;
  }

  try {
    ws = new WebSocket(wsUrl());

    ws.onopen = () => {
      statusEl.textContent = 'Backend Online (Live Stream)';
      pillEl.style.borderColor = 'rgba(16, 185, 129, 0.5)';
    };

    ws.onmessage = (event) => {
      try {
        const payload = JSON.parse(event.data);
        handleLiveEvent(payload);
      } catch (e) {
        console.error('Invalid WS payload:', event.data);
      }
    };

    ws.onclose = () => {
      statusEl.textContent = 'Reconnecting in 5s...';
      pillEl.style.borderColor = 'rgba(239, 68, 68, 0.5)';
      if (wsReconnectTimer) clearTimeout(wsReconnectTimer);
      wsReconnectTimer = setTimeout(setupWebSocket, 5000);
    };

    ws.onerror = () => {
      try { ws.close(); } catch (e) { /* ignore */ }
    };
  } catch (err) {
    console.error('WebSocket connection error:', err);
    if (wsReconnectTimer) clearTimeout(wsReconnectTimer);
    wsReconnectTimer = setTimeout(setupWebSocket, 5000);
  }
}

function handleLiveEvent(data) {
  const container = document.getElementById('feed-container');
  const empty = document.getElementById('feed-empty');
  if (empty) empty.remove();

  eventCount++;
  document.getElementById('feed-counter').textContent = `${eventCount} events received this session`;

  const timeStr = new Date().toLocaleTimeString();
  const evType = data.event_type || 'telemetry_event';
  const severity = data.severity || 'info';

  let borderClass = '';
  if (evType === 'critical_action' || severity === 'critical') borderClass = 'danger';
  else if (evType === 'assessment_completed' || evType === 'fire_suppressed') borderClass = 'success';
  else if (evType === 'wrong_action') borderClass = 'warning';

  const entry = document.createElement('div');
  entry.className = `feed-entry ${borderClass}`;
  
  let details = '';
  if (data.payload) {
    details = Object.entries(data.payload)
      .filter(([k, v]) => v !== '' && v !== null)
      .map(([k, v]) => `<strong>${k}:</strong> ${v}`)
      .join(' • ');
  }

  entry.innerHTML = `
    <div style="display: flex; justify-content: space-between;">
      <span class="feed-type">${evType.toUpperCase()}</span>
      <span class="feed-time">${timeStr}</span>
    </div>
    <div class="feed-msg">Session: <code>${data.session_id || 'N/A'}</code> • Worker: #${data.worker_id || 1} • Module: #${data.module_id || 1}</div>
    ${details ? `<div style="font-size: 11px; color: #94A3B8; margin-top: 4px;">${details}</div>` : ''}
  `;

  container.prepend(entry);
}

function formatCategory(str) {
  if (!str) return '';
  return str.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
}
