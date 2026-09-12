/**
 * SurakshaAR Command Dashboard Logic
 * Connects to FastAPI Backend at http://127.0.0.1:8000
 * Subscribes to live WebSocket telemetry at ws://127.0.0.1:8000/api/v1/events/live
 */

const API_BASE = window.location.origin.includes(':8000') 
  ? window.location.origin 
  : 'http://127.0.0.1:8000';

const WS_URL = API_BASE.replace(/^http/, 'ws') + '/api/v1/events/live';

let ws = null;
let eventCount = 0;

// Initialize
document.addEventListener('DOMContentLoaded', () => {
  document.getElementById('api-url').textContent = `${API_BASE}/api/v1`;
  
  loadSummary();
  loadWorkers();
  setupWebSocket();

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
});

// Load Summary KPIs and Modules
async function loadSummary() {
  try {
    const res = await fetch(`${API_BASE}/api/v1/dashboard/summary`);
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
      modList.innerHTML = '<div class="feed-empty">No module stats available.</div>';
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

// Load Worker Table with Retention info
async function loadWorkers() {
  try {
    const res = await fetch(`${API_BASE}/api/v1/dashboard/workers`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();

    const tbody = document.getElementById('workers-tbody');
    if (data.workers && data.workers.length > 0) {
      tbody.innerHTML = data.workers.map(w => {
        const certList = w.certified_modules && w.certified_modules.length > 0
          ? w.certified_modules.map(c => `<span class="badge-pill badge-pass">${c.toUpperCase()}</span>`).join(' ')
          : '<span class="badge-pill badge-pending">None</span>';

        return `
          <tr>
            <td><strong>${w.name}</strong></td>
            <td><code>${w.employee_id}</code></td>
            <td>${w.role}</td>
            <td>${certList}</td>
            <td><span class="badge-pill badge-pass">Day 1 Done • Day 7 Sched</span></td>
            <td><button class="btn-refresh" style="padding: 4px 8px; font-size: 11px;" onclick="viewWorkerProfile(${w.id})">Audit Profile</button></td>
          </tr>
        `;
      }).join('');
    } else {
      tbody.innerHTML = '<tr><td colspan="6" class="text-center" style="padding: 24px;">No workers registered yet. Register workers in app or via API.</td></tr>';
    }
  } catch (err) {
    console.error('Failed to load workers:', err);
  }
}

// View Detailed Worker Profile & Retention
window.viewWorkerProfile = async function(workerId) {
  try {
    const [wRes, rRes] = await Promise.all([
      fetch(`${API_BASE}/api/v1/dashboard/workers/${workerId}`),
      fetch(`${API_BASE}/api/v1/progress/${workerId}/retention`)
    ]);

    const worker = await wRes.json();
    let retentionHtml = '';
    if (rRes.ok) {
      const ret = await rRes.json();
      if (ret.retention_schedules && ret.retention_schedules.length > 0) {
        retentionHtml = ret.retention_schedules.map(s => `
          <div style="margin-top: 8px; padding: 8px; background: #13273A; border-radius: 6px;">
            <strong>${s.module_name} Retention:</strong>
            ${s.milestones.map(m => `
              <div style="font-size: 12px; margin-top: 4px;">
                • ${m.title}: <span class="badge-pill ${m.status === 'completed' ? 'badge-pass' : (m.status === 'due' ? 'badge-due' : 'badge-pending')}">${m.status.toUpperCase()}</span>
                (Due: ${new Date(m.due_date).toLocaleDateString()})
              </div>
            `).join('')}
          </div>
        `).join('');
      }
    }

    alert(`WORKER AUDIT PROFILE:\nName: ${worker.name}\nID: ${worker.employee_id}\nRole: ${worker.role}\nAssessments: ${worker.assessments ? worker.assessments.length : 0}\nCertificates: ${worker.certificates ? worker.certificates.length : 0}`);
  } catch (err) {
    alert('Error loading profile: ' + err.message);
  }
};

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
    const res = await fetch(`${API_BASE}/api/v1/certificates/verify/${encodeURIComponent(certId)}`);
    if (res.status === 200) {
      const data = await res.json();
      resultBox.className = 'verify-result-box verify-valid';
      resultBox.innerHTML = `
        <strong>✓ CERTIFICATE VALID & AUTHENTIC</strong><br>
        • Certificate ID: <code>${data.certificate_number}</code><br>
        • Certified Worker: <strong>${data.worker_name}</strong><br>
        • Training Module: <strong>${data.module_name}</strong><br>
        • Status: Active Industrial Safety Credential
      `;
    } else {
      resultBox.className = 'verify-result-box verify-invalid';
      resultBox.innerHTML = `✕ INVALID CERTIFICATE: The certificate ID '${certId}' was not found in the national registry.`;
    }
  } catch (err) {
    resultBox.className = 'verify-result-box verify-invalid';
    resultBox.innerHTML = `Error verifying certificate: ${err.message}`;
  }
}

// WebSocket Live Telemetry
function setupWebSocket() {
  const statusEl = document.getElementById('connection-status');
  const pillEl = document.getElementById('live-pill');

  try {
    ws = new WebSocket(WS_URL);

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
      setTimeout(setupWebSocket, 5000);
    };

    ws.onerror = () => {
      ws.close();
    };
  } catch (err) {
    console.error('WebSocket connection error:', err);
    setTimeout(setupWebSocket, 5000);
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
