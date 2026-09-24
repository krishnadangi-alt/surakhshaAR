"""Public Certificate Verification HTML Template.

Generates the pixel-perfect, mobile-responsive certificate verification
webpage matching the Directorate of Industrial Safety & Health /
Government of Jharkhand official layout.
"""

from datetime import datetime
from typing import Any, Dict, Optional


HINDI_MONTHS = {
    1: "जनवरी",
    2: "फ़रवरी",
    3: "मार्च",
    4: "अप्रैल",
    5: "मई",
    6: "जून",
    7: "जुलाई",
    8: "अगस्त",
    9: "सितंबर",
    10: "अक्टूबर",
    11: "नवंबर",
    12: "दिसंबर",
}

MODULE_HINDI_NAMES = {
    "Fire & Explosion Response": "आग एवं विस्फोट से निपटने की प्रक्रिया",
    "Gas Leak & Confined Space Protocol": "गैस रिसाव एवं सीमित स्थान सुरक्षा प्रोटोकॉल",
    "Underground Mine Safety Protocol": "भूमिगत खदान सुरक्षा प्रोटोकॉल",
    "Hazard Identification & Evacuation": "खतरे की पहचान एवं निकासी प्रक्रिया",
}


def format_date_hindi(dt: Optional[datetime]) -> str:
    """Format datetime into standard Hindi date, e.g. '12 जून 2024'."""
    if not dt:
        return "—"
    try:
        day = dt.day
        month = HINDI_MONTHS.get(dt.month, str(dt.month))
        year = dt.year
        return f"{day} {month} {year}"
    except Exception:
        return str(dt)


def render_public_verification_page(
    cert_data: Optional[Dict[str, Any]],
    certificate_id: str,
    status_type: str = "VERIFIED",
) -> str:
    """Render the official SurakshaAR Public Verification HTML page.

    status_type can be: 'VERIFIED', 'PENDING', 'REVOKED', 'EXPIRED', 'INVALID'
    """
    is_verified = (status_type == "VERIFIED" and cert_data is not None)

    worker_name = (cert_data.get("worker_name") or "—") if cert_data else "—"
    cert_num = (cert_data.get("certificate_number") or certificate_id) if cert_data else certificate_id
    raw_module = (cert_data.get("module_name") or "Fire & Explosion Response") if cert_data else "—"
    module_hindi = MODULE_HINDI_NAMES.get(raw_module, raw_module)

    issued_at = cert_data.get("issued_at") if cert_data else None
    valid_until = cert_data.get("valid_until") if cert_data else None

    issue_date_str = format_date_hindi(issued_at)
    valid_date_str = format_date_hindi(valid_until)

    pdf_url = f"/api/v1/certificates/{cert_num}/pdf" if cert_data else "#"

    # Status Configs
    configs = {
        "VERIFIED": {
            "card_bg": "#edf7ed",
            "seal_color": "#16a34a",
            "heading": "प्रमाणपत्र सत्यापित है",
            "heading_color": "#15803d",
            "sub": "यह प्रमाणपत्र वैध है और SurakshaAR के अभिलेख में उपलब्ध है।",
            "pill_bg": "#dcfce7",
            "pill_color": "#15803d",
            "pill_text": "✓ सफलतापूर्वक पूरा किया गया",
            "icon_svg": """<path stroke-linecap="round" stroke-linejoin="round" stroke-width="8.5" stroke="#ffffff" fill="none" d="M28 50 L42 64 L72 34"/>""",
        },
        "PENDING": {
            "card_bg": "#fffbeb",
            "seal_color": "#d97706",
            "heading": "प्रमाणपत्र सत्यापन लंबित है",
            "heading_color": "#b45309",
            "sub": "यह प्रमाणपत्र समीक्षा एवं आधिकारिक सत्यापन प्रक्रिया में है।",
            "pill_bg": "#fef3c7",
            "pill_color": "#b45309",
            "pill_text": "⏳ समीक्षाधीन (Pending Review)",
            "icon_svg": """<circle cx="50" cy="50" r="22" stroke="#ffffff" stroke-width="6" fill="none"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="6" stroke="#ffffff" fill="none" d="M50 36 V50 L60 56"/>""",
        },
        "REVOKED": {
            "card_bg": "#fef2f2",
            "seal_color": "#dc2626",
            "heading": "प्रमाणपत्र रद्द किया गया",
            "heading_color": "#b91c1c",
            "sub": "यह प्रमाणपत्र सुरक्षा नियमों के तहत रद्द (Revoked) किया गया है।",
            "pill_bg": "#fee2e2",
            "pill_color": "#b91c1c",
            "pill_text": "⚠ रद्द (Revoked)",
            "icon_svg": """<path stroke-linecap="round" stroke-linejoin="round" stroke-width="6" stroke="#ffffff" fill="none" d="M50 26 L74 68 H26 Z M50 42 V54 M50 60 V62"/>""",
        },
        "EXPIRED": {
            "card_bg": "#f8fafc",
            "seal_color": "#64748b",
            "heading": "प्रमाणपत्र की वैधता समाप्त",
            "heading_color": "#334155",
            "sub": "इस प्रमाणपत्र की निर्धारित समयावधि पूर्ण हो चुकी है (Expired)।",
            "pill_bg": "#f1f5f9",
            "pill_color": "#475569",
            "pill_text": "⚠ वैधता समाप्त (Expired)",
            "icon_svg": """<circle cx="50" cy="50" r="22" stroke="#ffffff" stroke-width="6" fill="none"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="6" stroke="#ffffff" fill="none" d="M50 34 V50 L62 50"/>""",
        },
        "INVALID": {
            "card_bg": "#fef2f2",
            "seal_color": "#dc2626",
            "heading": "प्रमाणपत्र अमान्य है",
            "heading_color": "#991b1b",
            "sub": "यह प्रमाणपत्र SurakshaAR के राष्ट्रीय/राज्य अभिलेख में उपलब्ध नहीं है।",
            "pill_bg": "#fee2e2",
            "pill_color": "#991b1b",
            "pill_text": "✕ अमान्य (Invalid)",
            "icon_svg": """<path stroke-linecap="round" stroke-linejoin="round" stroke-width="8" stroke="#ffffff" fill="none" d="M34 34 L66 66 M66 34 L34 66"/>""",
        },
    }

    cfg = configs.get(status_type, configs["INVALID"])

    # State Emblem of India SVG (Ashoka Lion Capital with Satyameva Jayate)
    ashoka_emblem_svg = """<svg width="46" height="58" viewBox="0 0 100 125" fill="#1e293b" xmlns="http://www.w3.org/2000/svg">
        <path d="M50 5 C45 5 40 8 38 13 C35 11 31 12 29 16 C25 15 21 18 20 22 C18 27 20 32 23 35 C22 39 24 43 28 45 C28 50 31 54 36 56 C37 60 41 63 46 64 L46 72 C41 73 35 77 34 83 L66 83 C65 77 59 73 54 72 L54 64 C59 63 63 60 64 56 C69 54 72 50 72 45 C76 43 78 39 77 35 C80 32 82 27 80 22 C79 18 75 15 71 16 C69 12 65 11 62 13 C60 8 55 5 50 5 Z" fill="#2d3748" opacity="0.95"/>
        <circle cx="50" cy="80" r="4.5" fill="#1e293b"/>
        <rect x="26" y="85" width="48" height="5" rx="1.5" fill="#1e293b"/>
        <rect x="22" y="92" width="56" height="4" rx="1.5" fill="#334155"/>
        <rect x="18" y="98" width="64" height="3" rx="1" fill="#475569"/>
        <text x="50" y="112" font-size="9" font-weight="900" text-anchor="middle" fill="#0f172a" font-family="'Noto Sans Devanagari', sans-serif">सत्यमेव जयते</text>
    </svg>"""

    # Scalloped Rosette Seal SVG (24 scalloped petals)
    scalloped_seal_svg = f"""<svg width="84" height="84" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
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
            Z" fill="{cfg['seal_color']}" transform="translate(2, 2) scale(0.96)"/>
        <g stroke="#ffffff" fill="none">
            {cfg['icon_svg']}
        </g>
    </svg>"""

    return f"""<!DOCTYPE html>
<html lang="hi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>SurakshaAR — {cfg['heading']} ({cert_num})</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&family=Noto+Sans+Devanagari:wght@400;500;600;700;800;900&display=swap" rel="stylesheet">
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            -webkit-tap-highlight-color: transparent;
        }}
        body {{
            background-color: #f8fafc;
            font-family: 'Inter', 'Noto Sans Devanagari', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            color: #0f172a;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
        }}
        .app-container {{
            width: 100%;
            max-width: 480px;
            min-height: 100vh;
            background: #ffffff;
            display: flex;
            flex-direction: column;
            box-shadow: 0 0 35px rgba(0, 0, 0, 0.05);
        }}
        /* Top Navigation Header */
        header {{
            padding: 16px 20px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            background: #ffffff;
            border-bottom: 1px solid #f1f5f9;
        }}
        .brand-left {{
            display: flex;
            flex-direction: column;
        }}
        .brand-title {{
            font-size: 24px;
            font-weight: 800;
            letter-spacing: -0.5px;
            line-height: 1.1;
            color: #0f172a;
        }}
        .brand-title .orange {{
            color: #ea580c;
        }}
        .brand-title .green {{
            color: #16a34a;
        }}
        .brand-tagline {{
            font-size: 11px;
            font-weight: 500;
            color: #475569;
            margin-top: 4px;
        }}
        .govt-right {{
            display: flex;
            align-items: center;
            gap: 10px;
        }}
        .govt-text {{
            text-align: right;
            line-height: 1.25;
        }}
        .govt-title {{
            font-size: 15px;
            font-weight: 800;
            color: #0f172a;
            font-family: 'Noto Sans Devanagari', sans-serif;
        }}
        .govt-sub {{
            font-size: 10.5px;
            font-weight: 500;
            color: #475569;
            font-family: 'Noto Sans Devanagari', sans-serif;
        }}
        /* Content Area */
        main {{
            padding: 20px 16px 36px;
            display: flex;
            flex-direction: column;
            gap: 16px;
            flex: 1;
        }}
        /* Banner Card */
        .status-banner {{
            background: {cfg['card_bg']};
            border-radius: 20px;
            padding: 30px 16px 26px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            position: relative;
            overflow: hidden;
            border: 1px solid rgba(0, 0, 0, 0.03);
        }}
        .status-banner::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: radial-gradient(circle at 50% 20%, rgba(255, 255, 255, 0.6) 0%, rgba(255, 255, 255, 0) 70%);
            pointer-events: none;
        }}
        .seal-wrapper {{
            margin-bottom: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            filter: drop-shadow(0 6px 14px rgba(22, 163, 74, 0.25));
        }}
        .banner-heading {{
            font-size: 24px;
            font-weight: 800;
            color: {cfg['heading_color']};
            font-family: 'Noto Sans Devanagari', sans-serif;
            margin-bottom: 6px;
            letter-spacing: -0.3px;
        }}
        .banner-sub {{
            font-size: 13.5px;
            font-weight: 500;
            color: #334155;
            font-family: 'Noto Sans Devanagari', sans-serif;
            line-height: 1.45;
            max-width: 360px;
        }}
        /* Details Card */
        .details-card {{
            background: #ffffff;
            border-radius: 20px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }}
        .detail-row {{
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 15px 18px;
            border-bottom: 1px solid #f1f5f9;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .row-left {{
            display: flex;
            align-items: center;
            gap: 14px;
        }}
        .icon-circle {{
            width: 36px;
            height: 36px;
            border-radius: 50%;
            background: #e0f2fe;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #0284c7;
            flex-shrink: 0;
        }}
        .icon-circle.green {{
            background: #dcfce7;
            color: #16a34a;
        }}
        .row-label {{
            font-size: 14px;
            font-weight: 500;
            color: #64748b;
            font-family: 'Noto Sans Devanagari', sans-serif;
        }}
        .row-value {{
            font-size: 15.5px;
            font-weight: 800;
            color: #0f172a;
            text-align: right;
            font-family: 'Noto Sans Devanagari', 'Inter', sans-serif;
            letter-spacing: -0.2px;
            max-width: 220px;
            word-break: break-word;
        }}
        .status-pill {{
            display: inline-flex;
            align-items: center;
            gap: 6px;
            background: {cfg['pill_bg']};
            color: {cfg['pill_color']};
            border-radius: 9999px;
            padding: 6px 14px;
            font-size: 13.5px;
            font-weight: 700;
            font-family: 'Noto Sans Devanagari', sans-serif;
        }}
        /* PDF Download Button */
        .pdf-btn-container {{
            padding: 16px 18px 20px;
        }}
        .pdf-button {{
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            width: 100%;
            padding: 14px;
            background: #0284c7;
            color: #ffffff;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 700;
            font-family: 'Noto Sans Devanagari', sans-serif;
            text-decoration: none;
            cursor: pointer;
            box-shadow: 0 4px 14px rgba(2, 132, 199, 0.35);
            transition: all 0.15s ease-in-out;
        }}
        .pdf-button:hover {{
            background: #0369a1;
            transform: translateY(-1px);
        }}
        .pdf-button:active {{
            transform: translateY(0);
        }}
        /* Footer */
        footer {{
            margin-top: auto;
            padding: 20px 16px 10px;
            border-top: 1px solid #e2e8f0;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            text-align: center;
        }}
        .footer-icon {{
            color: #16a34a;
            display: flex;
            align-items: center;
        }}
        .footer-text {{
            font-size: 12.5px;
            font-weight: 500;
            color: #475569;
            font-family: 'Noto Sans Devanagari', sans-serif;
        }}
    </style>
</head>
<body>
    <div class="app-container">
        <!-- Top Official Header -->
        <header>
            <div class="brand-left">
                <div class="brand-title">Suraksha<span class="orange">A</span><span class="green">R</span></div>
                <div class="brand-tagline">Learn Safe | Work Safe | Build a Safer Jharkhand</div>
            </div>
            <div class="govt-right">
                <div class="govt-text">
                    <div class="govt-title">झारखंड सरकार</div>
                    <div class="govt-sub">श्रम, रोजगार एवं प्रशिक्षण विभाग</div>
                </div>
                <div>{ashoka_emblem_svg}</div>
            </div>
        </header>

        <!-- Main Content -->
        <main>
            <!-- Status Card -->
            <section class="status-banner">
                <div class="seal-wrapper">
                    {scalloped_seal_svg}
                </div>
                <h1 class="banner-heading">{cfg['heading']}</h1>
                <p class="banner-sub">{cfg['sub']}</p>
            </section>

            <!-- Details Card -->
            <section class="details-card">
                <!-- Row 1: Name -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                            </svg>
                        </div>
                        <span class="row-label">नाम</span>
                    </div>
                    <div class="row-value">{worker_name}</div>
                </div>

                <!-- Row 2: Certificate ID -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z"/>
                            </svg>
                        </div>
                        <span class="row-label">प्रमाणपत्र आईडी</span>
                    </div>
                    <div class="row-value" style="font-family: monospace; font-size: 15px;">{cert_num}</div>
                </div>

                <!-- Row 3: Training Module -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M18 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zM6 4h5v8l-2.5-1.5L6 12V4z"/>
                            </svg>
                        </div>
                        <span class="row-label">प्रशिक्षण मॉड्यूल</span>
                    </div>
                    <div class="row-value">{module_hindi}</div>
                </div>

                <!-- Row 4: Status -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle green">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                            </svg>
                        </div>
                        <span class="row-label">स्थिति</span>
                    </div>
                    <div class="row-value">
                        <span class="status-pill">{cfg['pill_text']}</span>
                    </div>
                </div>

                <!-- Row 5: Issue Date -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z"/>
                            </svg>
                        </div>
                        <span class="row-label">जारी करने की तिथि</span>
                    </div>
                    <div class="row-value">{issue_date_str}</div>
                </div>

                <!-- Row 6: Validity Date -->
                <div class="detail-row">
                    <div class="row-left">
                        <div class="icon-circle">
                            <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zm-7-2c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm0-8c1.66 0 3 1.34 3 3s-1.34 3-3 3-3-1.34-3-3 1.34-3 3-3zm-.5 1.5v2.25l1.8 1.08.75-1.23-1.3-0.78V10.5h-1.25z"/>
                            </svg>
                        </div>
                        <span class="row-label">वैधता की तिथि</span>
                    </div>
                    <div class="row-value">{valid_date_str}</div>
                </div>

                <!-- PDF Button -->
                {f'''<div class="pdf-btn-container">
                    <a href="{pdf_url}" target="_blank" rel="noopener noreferrer" class="pdf-button">
                        <svg width="20" height="20" fill="currentColor" viewBox="0 0 24 24">
                            <path d="M20 2H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-8.5 7.5c0 .83-.67 1.5-1.5 1.5H9v2H7.5V7H10c.83 0 1.5.67 1.5 1.5v1zm5 2c0 .83-.67 1.5-1.5 1.5h-2.5V7H15c.83 0 1.5.67 1.5 1.5v3zm4-3H19v1h1.5V11H19v2h-1.5V7h3v1.5zM9 9.5h1v-1H9v1zm4.5 2H14v-3h-.5v3zM4 6H2v14c0 1.1.9 2 2 2h14v-2H4V6z"/>
                        </svg>
                        <span>प्रमाणपत्र देखें (PDF)</span>
                    </a>
                </div>''' if is_verified else ''}
            </section>

            <!-- Bottom Disclaimer Footer -->
            <footer>
                <div class="footer-icon">
                    <svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" viewBox="0 0 24 24">
                        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                        <polyline points="9 12 11 14 15 10"/>
                    </svg>
                </div>
                <div class="footer-text">
                    यह प्रमाणपत्र SurakshaAR, झारखंड सरकार द्वारा जारी किया गया है।
                </div>
            </footer>
        </main>
    </div>
</body>
</html>
"""
