import React, { useEffect, useState } from 'react';

// ── Types ─────────────────────────────────────────────────────────────

interface VerificationData {
  certificate_number: string;
  valid: boolean;
  worker_name: string;
  employee_id: string | null;
  role: string | null;
  module_name: string;
  score: number | null;
  competency_status: string | null;
  issued_at: string | null;
  valid_until: string | null;
  status: string;
  verify_url: string | null;
  public_image_url: string | null;
  has_image: boolean;
}

interface PublicCertificateVerifyViewProps {
  certificateId?: string;
  onBackToAdmin?: () => void;
}

type VerifyStatus = 'VERIFIED' | 'PENDING' | 'REVOKED' | 'EXPIRED' | 'INVALID';

function resolveStatus(data: VerificationData): VerifyStatus {
  const s = (data.status || '').toUpperCase();
  if (s === 'REVOKED') return 'REVOKED';
  if (s === 'EXPIRED') return 'EXPIRED';
  if (s === 'PENDING_REVIEW' || s === 'PENDING') return 'PENDING';
  if (!data.valid) {
    if (data.valid_until) {
      const exp = new Date(data.valid_until);
      if (exp < new Date()) return 'EXPIRED';
    }
    return 'INVALID';
  }
  return 'VERIFIED';
}

const HINDI_MONTHS: Record<number, string> = {
  0: 'जनवरी',
  1: 'फ़रवरी',
  2: 'मार्च',
  3: 'अप्रैल',
  4: 'मई',
  5: 'जून',
  6: 'जुलाई',
  7: 'अगस्त',
  8: 'सितंबर',
  9: 'अक्टूबर',
  10: 'नवंबर',
  11: 'दिसंबर',
};

function formatDateHindi(iso: string | null): string {
  if (!iso) return '—';
  try {
    const d = new Date(iso);
    if (isNaN(d.getTime())) return '—';
    const day = d.getDate();
    const month = HINDI_MONTHS[d.getMonth()] || `${d.getMonth() + 1}`;
    const year = d.getFullYear();
    return `${day} ${month} ${year}`;
  } catch {
    return '—';
  }
}

const MODULE_HINDI_NAMES: Record<string, string> = {
  'Fire & Explosion Response': 'आग एवं विस्फोट से निपटने की प्रक्रिया',
  'Gas Leak & Confined Space Protocol': 'गैस रिसाव एवं सीमित स्थान सुरक्षा प्रोटोकॉल',
  'Underground Mine Safety Protocol': 'भूमिगत खदान सुरक्षा प्रोटोकॉल',
  'Hazard Identification & Evacuation': 'खतरे की पहचान एवं निकासी प्रक्रिया',
};

// ── SVG Components ────────────────────────────────────────────────────

const AshokaEmblemSvg = () => (
  <svg width="44" height="54" viewBox="0 0 100 125" fill="#1e293b" xmlns="http://www.w3.org/2000/svg">
    <path d="M50 5 C45 5 40 8 38 13 C35 11 31 12 29 16 C25 15 21 18 20 22 C18 27 20 32 23 35 C22 39 24 43 28 45 C28 50 31 54 36 56 C37 60 41 63 46 64 L46 72 C41 73 35 77 34 83 L66 83 C65 77 59 73 54 72 L54 64 C59 63 63 60 64 56 C69 54 72 50 72 45 C76 43 78 39 77 35 C80 32 82 27 80 22 C79 18 75 15 71 16 C69 12 65 11 62 13 C60 8 55 5 50 5 Z" fill="#2d3748" opacity="0.95"/>
    <circle cx="50" cy="80" r="4.5" fill="#1e293b"/>
    <rect x="26" y="85" width="48" height="5" rx="1.5" fill="#1e293b"/>
    <rect x="22" y="92" width="56" height="4" rx="1.5" fill="#334155"/>
    <rect x="18" y="98" width="64" height="3" rx="1" fill="#475569"/>
    <text x="50" y="112" fontSize="9" fontWeight="900" textAnchor="middle" fill="#0f172a" fontFamily="'Noto Sans Devanagari', sans-serif">सत्यमेव जयते</text>
  </svg>
);

const ScallopedSeal = ({ color, icon }: { color: string; icon: React.ReactNode }) => (
  <div style={{ position: 'relative', width: 84, height: 84, display: 'flex', alignItems: 'center', justifyContent: 'center', filter: 'drop-shadow(0 6px 14px rgba(22,163,74,0.25))' }}>
    <svg width="84" height="84" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M50 0
        C53 5 57 5 61 2
        C66 6 70 7 73 3
        C77 8 81 10 85 7
        C88 13 91 16 96 14
        C97 20 100 23 103 23
        C103 29 104 33 108 34
        C106 40 106 45 109 48
        C106 53 104 58 106 63
        C102 67 99 71 99 77
        C94 80 90 83 89 89
        C83 91 79 93 76 98
        C70 98 65 99 61 103
        C55 101 50 101 45 103
        C40 99 35 98 29 98
        C26 93 22 91 16 89
        C15 83 11 80 6 77
        C6 71 3 67 -1 63
        C1 58 -1 53 -4 48
        C-1 45 -1 40 -3 34
        C1 33 2 29 2 23
        C5 23 8 20 9 14
        C14 16 17 13 20 7
        C24 10 28 8 32 3
        C35 7 39 6 44 2
        Z" fill={color} transform="translate(2, 2) scale(0.96)"/>
    </svg>
    <div style={{ position: 'absolute', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      {icon}
    </div>
  </div>
);

// ── Page Header ───────────────────────────────────────────────────────

function PageHeader({ onBackToAdmin }: { onBackToAdmin?: () => void }) {
  return (
    <header style={{
      background: '#fff',
      borderBottom: '1px solid #f1f5f9',
      padding: '16px 20px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
    }}>
      <div>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: 0 }}>
          <span style={{ fontSize: 24, fontWeight: 800, color: '#0f172a', letterSpacing: -0.5 }}>Suraksha</span>
          <span style={{ fontSize: 24, fontWeight: 800, color: '#ea580c' }}>A</span>
          <span style={{ fontSize: 24, fontWeight: 800, color: '#16a34a' }}>R</span>
        </div>
        <div style={{ fontSize: 11, fontWeight: 500, color: '#475569', marginTop: 4 }}>
          Learn Safe | Work Safe | Build a Safer Jharkhand
        </div>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        <div style={{ textAlign: 'right', lineHeight: 1.25 }}>
          <div style={{ fontSize: 15, fontWeight: 800, color: '#0f172a', fontFamily: "'Noto Sans Devanagari', sans-serif" }}>
            झारखंड सरकार
          </div>
          <div style={{ fontSize: 10.5, fontWeight: 500, color: '#475569', fontFamily: "'Noto Sans Devanagari', sans-serif" }}>
            श्रम, रोजगार एवं प्रशिक्षण विभाग
          </div>
        </div>
        <div><AshokaEmblemSvg /></div>
        {onBackToAdmin && (
          <button
            onClick={onBackToAdmin}
            style={{
              marginLeft: 8,
              padding: '6px 12px',
              borderRadius: 8,
              border: '1px solid #e2e8f0',
              background: '#f8fafc',
              color: '#334155',
              fontSize: 12,
              fontWeight: 600,
              cursor: 'pointer',
            }}
          >
            ← Admin
          </button>
        )}
      </div>
    </header>
  );
}

// ── Status Banner ─────────────────────────────────────────────────────

function StatusBanner({ status }: { status: VerifyStatus }) {
  const configs = {
    VERIFIED: {
      bg: '#edf7ed',
      sealColor: '#16a34a',
      headingColor: '#15803d',
      title: 'प्रमाणपत्र सत्यापित है',
      sub: 'यह प्रमाणपत्र वैध है और SurakshaAR के अभिलेख में उपलब्ध है।',
      icon: (
        <svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3.2" strokeLinecap="round" strokeLinejoin="round">
          <polyline points="20 6 9 17 4 12" />
        </svg>
      ),
    },
    PENDING: {
      bg: '#fffbeb',
      sealColor: '#d97706',
      headingColor: '#b45309',
      title: 'प्रमाणपत्र सत्यापन लंबित है',
      sub: 'यह प्रमाणपत्र समीक्षा एवं आधिकारिक सत्यापन प्रक्रिया में है।',
      icon: (
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </svg>
      ),
    },
    REVOKED: {
      bg: '#fef2f2',
      sealColor: '#dc2626',
      headingColor: '#b91c1c',
      title: 'प्रमाणपत्र रद्द किया गया',
      sub: 'यह प्रमाणपत्र सुरक्षा नियमों के तहत रद्द (Revoked) किया गया है।',
      icon: (
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
          <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
          <line x1="12" y1="9" x2="12" y2="13" />
          <line x1="12" y1="17" x2="12.01" y2="17" />
        </svg>
      ),
    },
    EXPIRED: {
      bg: '#f8fafc',
      sealColor: '#64748b',
      headingColor: '#334155',
      title: 'प्रमाणपत्र की वैधता समाप्त',
      sub: 'इस प्रमाणपत्र की निर्धारित समयावधि पूर्ण हो चुकी है (Expired)।',
      icon: (
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </svg>
      ),
    },
    INVALID: {
      bg: '#fef2f2',
      sealColor: '#dc2626',
      headingColor: '#991b1b',
      title: 'प्रमाणपत्र अमान्य है',
      sub: 'यह प्रमाणपत्र SurakshaAR के राष्ट्रीय/राज्य अभिलेख में उपलब्ध नहीं है।',
      icon: (
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      ),
    },
  };

  const c = configs[status];
  return (
    <div style={{
      background: c.bg,
      borderRadius: 20,
      padding: '30px 16px 26px',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      textAlign: 'center',
      position: 'relative',
      border: '1px solid rgba(0, 0, 0, 0.03)',
    }}>
      <div style={{ marginBottom: 14 }}>
        <ScallopedSeal color={c.sealColor} icon={c.icon} />
      </div>
      <h1 style={{
        fontSize: 24,
        fontWeight: 800,
        color: c.headingColor,
        fontFamily: "'Noto Sans Devanagari', sans-serif",
        marginBottom: 6,
        letterSpacing: -0.3,
      }}>
        {c.title}
      </h1>
      <p style={{
        fontSize: 13.5,
        fontWeight: 500,
        color: '#334155',
        fontFamily: "'Noto Sans Devanagari', sans-serif",
        lineHeight: 1.45,
        maxWidth: 360,
      }}>
        {c.sub}
      </p>
    </div>
  );
}

// ── Detail Row ────────────────────────────────────────────────────────

function DetailRow({
  icon,
  label,
  value,
  isStatus = false,
  isGreenIcon = false,
}: {
  icon: React.ReactNode;
  label: string;
  value: React.ReactNode;
  isStatus?: boolean;
  isGreenIcon?: boolean;
}) {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '15px 18px',
      borderBottom: '1px solid #f1f5f9',
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        <div style={{
          width: 36,
          height: 36,
          borderRadius: '50%',
          background: isGreenIcon ? '#dcfce7' : '#e0f2fe',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: isGreenIcon ? '#16a34a' : '#0284c7',
          flexShrink: 0,
        }}>
          {icon}
        </div>
        <span style={{ fontSize: 14, fontWeight: 500, color: '#64748b', fontFamily: "'Noto Sans Devanagari', sans-serif" }}>
          {label}
        </span>
      </div>
      <div style={{ textAlign: 'right' }}>
        {isStatus ? (
          <span style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            background: '#dcfce7',
            color: '#15803d',
            borderRadius: 9999,
            padding: '6px 14px',
            fontSize: 13.5,
            fontWeight: 700,
            fontFamily: "'Noto Sans Devanagari', sans-serif",
          }}>
            {value}
          </span>
        ) : (
          <span style={{
            fontSize: 15.5,
            fontWeight: 800,
            color: '#0f172a',
            fontFamily: "'Noto Sans Devanagari', 'Inter', sans-serif",
            letterSpacing: -0.2,
          }}>
            {value}
          </span>
        )}
      </div>
    </div>
  );
}

// ── Main Public Component ─────────────────────────────────────────────

export const PublicCertificateVerifyView: React.FC<PublicCertificateVerifyViewProps> = ({
  certificateId,
  onBackToAdmin,
}) => {
  const resolvedId: string | null = (() => {
    if (certificateId) return certificateId;
    if (typeof window === 'undefined') return null;
    const p = window.location.pathname;
    if (p.startsWith('/verify/')) return decodeURIComponent(p.slice(8));
    if (p.startsWith('/certificate/')) return decodeURIComponent(p.slice(13));
    const q = new URLSearchParams(window.location.search).get('verify');
    if (q) return q;
    return null;
  })();

  const [certData, setCertData] = useState<VerificationData | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    if (!resolvedId) {
      setLoading(false);
      setNotFound(true);
      return;
    }
    setLoading(true);
    setNotFound(false);

    fetch(`/api/v1/certificates/verify/${encodeURIComponent(resolvedId)}`)
      .then(async (res) => {
        if (res.status === 404 || res.status === 422) {
          setNotFound(true);
          setLoading(false);
          return;
        }
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data: VerificationData = await res.json();
        setCertData(data);
        setLoading(false);
      })
      .catch(() => {
        setNotFound(true);
        setLoading(false);
      });
  }, [resolvedId]);

  if (loading) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f8fafc' }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{
            width: 44,
            height: 44,
            border: '4px solid #e2e8f0',
            borderTopColor: '#0284c7',
            borderRadius: '50%',
            animation: 'spin 0.8s linear infinite',
            margin: '0 auto 14px',
          }} />
          <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
          <p style={{ color: '#475569', fontSize: 14, fontFamily: "'Noto Sans Devanagari', sans-serif" }}>
            प्रमाणपत्र सत्यापित हो रहा है...
          </p>
        </div>
      </div>
    );
  }

  const status: VerifyStatus = notFound || !certData ? 'INVALID' : resolveStatus(certData);
  const isVerified = status === 'VERIFIED';
  const certNumber = certData?.certificate_number || resolvedId || '—';
  const workerName = certData?.worker_name || '—';
  const rawModule = certData?.module_name || 'Fire & Explosion Response';
  const moduleHindi = MODULE_HINDI_NAMES[rawModule] || rawModule;

  const pdfUrl = `/api/v1/certificates/${encodeURIComponent(certNumber)}/pdf`;

  return (
    <div style={{
      minHeight: '100vh',
      background: '#f8fafc',
      fontFamily: "'Inter', 'Noto Sans Devanagari', sans-serif",
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
    }}>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&family=Noto+Sans+Devanagari:wght@400;500;600;700;800;900&display=swap');
        * { box-sizing: border-box; }
      `}</style>

      <div style={{
        width: '100%',
        maxWidth: 480,
        minHeight: '100vh',
        background: '#ffffff',
        display: 'flex',
        flexDirection: 'column',
        boxShadow: '0 0 35px rgba(0, 0, 0, 0.05)',
      }}>
        <PageHeader onBackToAdmin={onBackToAdmin} />

        <main style={{ padding: '20px 16px 36px', display: 'flex', flexDirection: 'column', gap: 16, flex: 1 }}>
          <StatusBanner status={status} />

          {/* Details Card */}
          <section style={{
            background: '#ffffff',
            borderRadius: 20,
            border: '1px solid #e2e8f0',
            boxShadow: '0 4px 16px rgba(0, 0, 0, 0.04)',
            overflow: 'hidden',
            display: 'flex',
            flexDirection: 'column',
          }}>
            {/* Row 1: Name */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                </svg>
              )}
              label="नाम"
              value={workerName}
            />

            {/* Row 2: Certificate ID */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z"/>
                </svg>
              )}
              label="प्रमाणपत्र आईडी"
              value={<span style={{ fontFamily: 'monospace', fontSize: 15 }}>{certNumber}</span>}
            />

            {/* Row 3: Training Module */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M18 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zM6 4h5v8l-2.5-1.5L6 12V4z"/>
                </svg>
              )}
              label="प्रशिक्षण मॉड्यूल"
              value={moduleHindi}
            />

            {/* Row 4: Status */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
              )}
              label="स्थिति"
              value={isVerified ? "✓ सफलतापूर्वक पूरा किया गया" : (status === 'PENDING' ? "⏳ समीक्षाधीन" : "⚠ अमान्य")}
              isStatus={true}
              isGreenIcon={isVerified}
            />

            {/* Row 5: Issue Date */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z"/>
                </svg>
              )}
              label="जारी करने की तिथि"
              value={formatDateHindi(certData?.issued_at ?? null)}
            />

            {/* Row 6: Validity Date */}
            <DetailRow
              icon={(
                <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zm-7-2c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm0-8c1.66 0 3 1.34 3 3s-1.34 3-3 3-3-1.34-3-3 1.34-3 3-3zm-.5 1.5v2.25l1.8 1.08.75-1.23-1.3-0.78V10.5h-1.25z"/>
                </svg>
              )}
              label="वैधता की तिथि"
              value={formatDateHindi(certData?.valid_until ?? null)}
            />

            {/* PDF Button */}
            {isVerified && (
              <div style={{ padding: '16px 18px 20px' }}>
                <a
                  href={pdfUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: 10,
                    width: '100%',
                    padding: '14px',
                    background: '#0284c7',
                    color: '#ffffff',
                    border: 'none',
                    borderRadius: 12,
                    fontSize: 16,
                    fontWeight: 700,
                    fontFamily: "'Noto Sans Devanagari', sans-serif",
                    textDecoration: 'none',
                    boxShadow: '0 4px 14px rgba(2, 132, 199, 0.35)',
                  }}
                >
                  <svg width="20" height="20" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M20 2H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-8.5 7.5c0 .83-.67 1.5-1.5 1.5H9v2H7.5V7H10c.83 0 1.5.67 1.5 1.5v1zm5 2c0 .83-.67 1.5-1.5 1.5h-2.5V7H15c.83 0 1.5.67 1.5 1.5v3zm4-3H19v1h1.5V11H19v2h-1.5V7h3v1.5zM9 9.5h1v-1H9v1zm4.5 2H14v-3h-.5v3zM4 6H2v14c0 1.1.9 2 2 2h14v-2H4V6z"/>
                  </svg>
                  <span>प्रमाणपत्र देखें (PDF)</span>
                </a>
              </div>
            )}
          </section>

          {/* Footer */}
          <footer style={{
            marginTop: 'auto',
            padding: '20px 16px 10px',
            borderTop: '1px solid #e2e8f0',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 8,
            textAlign: 'center',
          }}>
            <div style={{ color: '#16a34a', display: 'flex', alignItems: 'center' }}>
              <svg width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                <polyline points="9 12 11 14 15 10"/>
              </svg>
            </div>
            <div style={{ fontSize: 12.5, fontWeight: 500, color: '#475569', fontFamily: "'Noto Sans Devanagari', sans-serif" }}>
              यह प्रमाणपत्र SurakshaAR, झारखंड सरकार द्वारा जारी किया गया है।
            </div>
          </footer>
        </main>
      </div>
    </div>
  );
};