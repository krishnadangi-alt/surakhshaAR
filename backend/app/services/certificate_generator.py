"""Certificate, QR, and PDF generation engine.

Produces authoritative PDF certificates, high-resolution PNG images,
and public verification QR codes using ReportLab, Pillow, and pypdfium2.
"""

import hashlib
import hmac
import json
import os
import secrets
import urllib.request
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, NamedTuple, Tuple

try:
    import qrcode
except ImportError:
    qrcode = None

try:
    import pypdfium2 as pdfium
except ImportError:
    pdfium = None

try:
    from reportlab.lib import colors
    from reportlab.lib.pagesizes import landscape, letter
    from reportlab.lib.units import inch
    from reportlab.pdfgen import canvas
except ImportError:
    colors = None
    landscape = None
    letter = None
    inch = None
    canvas = None

# Storage root for generated PDFs and images
STORAGE_DIR = Path(__file__).resolve().parent.parent.parent / "storage" / "certificates"
STORAGE_DIR.mkdir(parents=True, exist_ok=True)

# Secret key for verification hashing
HMAC_SECRET = os.environ.get("SURAKSHA_HMAC_SECRET", "surakshaar_secret_salt_2026").encode()

# Public verification base URL — QR codes encode <BASE>/verify/<cert_id>
# Set VERIFICATION_BASE_URL in backend/.env. Default to Render production URL.
VERIFICATION_BASE_URL = os.environ.get("VERIFICATION_BASE_URL", "https://surakhshaar.onrender.com/verify")

# Cached public IMAGE hosting URLs (for displaying certificate images, not for QR)
KNOWN_PUBLIC_URLS: Dict[str, str] = {
    "SUR-2026-0001": "https://files.catbox.moe/hge6s4.png",
    "SUR-2026-0002": "https://files.catbox.moe/t1l5lb.png",
}


def get_verification_url(certificate_number: str) -> str:
    """Return the public verification URL for a certificate.
    
    This is the authoritative URL embedded in every QR code.
    Format: <VERIFICATION_BASE_URL>/<certificate_number>
    """
    base = os.environ.get("VERIFICATION_BASE_URL", VERIFICATION_BASE_URL).rstrip("/")
    return f"{base}/{certificate_number}"



class CertificateArtifacts(NamedTuple):
    pdf_path: str
    image_path: str
    qr_path: str
    public_image_url: str
    verification_token: str
    verification_hash: str


def generate_verification_token() -> str:
    """Generate a cryptographically secure URL-safe verification token."""
    return secrets.token_urlsafe(32)


def compute_verification_hash(certificate_number: str, token: str, worker_id: int, score: float) -> str:
    """Compute HMAC-SHA256 verification signature for certificate authenticity."""
    message = f"{certificate_number}:{worker_id}:{score:.1f}:{token}".encode()
    return hmac.new(HMAC_SECRET, message, hashlib.sha256).hexdigest()


def generate_qr_code_image(verify_url: str, output_path: str) -> str:
    """Generate a high-contrast QR code image encoding the given URL."""
    if qrcode is None:
        return output_path
    qr = qrcode.QRCode(
        version=1,
        error_correction=qrcode.constants.ERROR_CORRECT_M,
        box_size=6,
        border=2,
    )
    qr.add_data(verify_url)
    qr.make(fit=True)
    img = qr.make_image(fill_color="#0F172A", back_color="#FFFFFF")
    img.save(output_path)
    return output_path


def render_pdf_to_image(pdf_path: str, image_path: str, scale: int = 3) -> str:
    """Render a PDF certificate to a 300-DPI high-resolution PNG image."""
    if pdfium is not None and os.path.exists(pdf_path):
        try:
            pdf = pdfium.PdfDocument(pdf_path)
            img = pdf[0].render(scale=scale).to_pil()
            img.save(image_path, "PNG")
            return str(image_path)
        except Exception:
            pass
    return ""


def upload_certificate_image_public(image_path: str, certificate_number: str = "cert") -> str:
    """Upload certificate image to a permanent public host (Catbox/tmpfiles) for global smartphone scanning.
    
    Returns a live public direct image URL (e.g. https://files.catbox.moe/xxxx.png).
    Falls back to LAN/local endpoint if network is offline.
    """
    if certificate_number in KNOWN_PUBLIC_URLS:
        return KNOWN_PUBLIC_URLS[certificate_number]
    if not os.path.exists(image_path):
        return ""

    # 1. Catbox.moe permanent hosting
    try:
        with open(image_path, "rb") as f:
            img_bytes = f.read()
        boundary = "----WebKitFormBoundary" + secrets.token_hex(16)
        parts = [
            f'--{boundary}\r\nContent-Disposition: form-data; name="reqtype"\r\n\r\nfileupload\r\n'.encode("utf-8"),
            f'--{boundary}\r\nContent-Disposition: form-data; name="fileToUpload"; filename="{certificate_number}.png"\r\nContent-Type: image/png\r\n\r\n'.encode("utf-8"),
            img_bytes,
            f'\r\n--{boundary}--\r\n'.encode("utf-8"),
        ]
        body = b"".join(parts)
        req = urllib.request.Request(
            "https://catbox.moe/user/api.php",
            data=body,
            headers={"Content-Type": f"multipart/form-data; boundary={boundary}", "User-Agent": "Mozilla/5.0"}
        )
        with urllib.request.urlopen(req, timeout=12) as resp:
            url = resp.read().decode("utf-8").strip()
            if url.startswith("http"):
                KNOWN_PUBLIC_URLS[certificate_number] = url
                return url
    except Exception:
        pass

    # 2. tmpfiles.org direct download link fallback
    try:
        boundary = "----WebKitFormBoundary" + secrets.token_hex(16)
        parts = [
            f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="{certificate_number}.png"\r\nContent-Type: image/png\r\n\r\n'.encode("utf-8"),
            img_bytes,
            f'\r\n--{boundary}--\r\n'.encode("utf-8"),
        ]
        body = b"".join(parts)
        req = urllib.request.Request(
            "https://tmpfiles.org/api/v1/upload",
            data=body,
            headers={"Content-Type": f"multipart/form-data; boundary={boundary}", "User-Agent": "Mozilla/5.0"}
        )
        with urllib.request.urlopen(req, timeout=12) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            raw_url = data.get("data", {}).get("url", "")
            if raw_url:
                direct = raw_url.replace("tmpfiles.org/", "tmpfiles.org/dl/")
                KNOWN_PUBLIC_URLS[certificate_number] = direct
                return direct
    except Exception:
        pass

    # 3. Offline LAN fallback
    base = os.environ.get("PUBLIC_CERTIFICATE_BASE_URL", "http://172.16.48.160:8000")
    return f"{base.rstrip('/')}/api/v1/certificates/public/{certificate_number}/image"


def _draw_certificate_canvas(
    pdf_path: str,
    qr_path: str,
    certificate_number: str,
    worker_name: str,
    employee_id: str,
    module_name: str,
    score: float,
    competency_status: str,
    issue_date: datetime,
    valid_until: datetime,
    verification_hash: str,
) -> None:
    """Render the official Directorate General of Mines Safety certificate canvas to PDF."""
    if canvas is None:
        return

    c = canvas.Canvas(pdf_path, pagesize=landscape(letter))
    width, height = landscape(letter)

    # Background
    c.setFillColor(colors.HexColor("#FDFBF7"))  # Warm parchment off-white
    c.rect(0, 0, width, height, fill=True, stroke=False)

    # Outer Ornate Border
    c.setStrokeColor(colors.HexColor("#D97706"))  # Amber / Gold
    c.setLineWidth(5)
    c.rect(20, 20, width - 40, height - 40, fill=False, stroke=True)

    # Inner Elegant Border
    c.setStrokeColor(colors.HexColor("#0F172A"))  # Deep Navy
    c.setLineWidth(1.5)
    c.rect(26, 26, width - 52, height - 52, fill=False, stroke=True)

    # Thin Accent Inset
    c.setStrokeColor(colors.HexColor("#FDE68A"))  # Soft Gold
    c.setLineWidth(0.75)
    c.rect(30, 30, width - 60, height - 60, fill=False, stroke=True)

    # Top Header
    c.setFillColor(colors.HexColor("#0F172A"))
    c.setFont("Helvetica-Bold", 14)
    c.drawCentredString(width / 2, height - 56, "GOVERNMENT OF INDIA • MINISTRY OF MINES")

    c.setFillColor(colors.HexColor("#059669"))  # Safety Green
    c.setFont("Helvetica-Bold", 10)
    c.drawCentredString(width / 2, height - 72, "DIRECTORATE GENERAL OF MINES SAFETY • SURAKSHA-AR REGISTRY")

    # Main Certificate Title
    c.setFillColor(colors.HexColor("#D97706"))
    c.setFont("Helvetica-Bold", 24)
    c.drawCentredString(width / 2, height - 110, "CERTIFICATE OF SAFETY COMPETENCY")

    # Subtitle
    c.setFillColor(colors.HexColor("#475569"))
    c.setFont("Helvetica-Oblique", 11)
    c.drawCentredString(width / 2, height - 130, "This is to certify that industrial trainee")

    # Worker Name
    c.setFillColor(colors.HexColor("#0F172A"))
    c.setFont("Helvetica-Bold", 26)
    c.drawCentredString(width / 2, height - 165, worker_name)

    # Worker ID & Role
    c.setFillColor(colors.HexColor("#64748B"))
    c.setFont("Helvetica", 11)
    c.drawCentredString(width / 2, height - 183, f"Employee ID: {employee_id}  •  Industrial Mine Worker")

    # Description
    c.setFillColor(colors.HexColor("#334155"))
    c.setFont("Helvetica", 10.5)
    c.drawCentredString(
        width / 2,
        height - 212,
        "has successfully demonstrated practical, validated safety proficiency under simulated emergency conditions for:"
    )

    # Qualification Module Banner
    c.setFillColor(colors.HexColor("#F0FDF4"))  # Mint background
    c.setStrokeColor(colors.HexColor("#86EFAC"))
    c.setLineWidth(1)
    c.roundRect(width / 2 - 220, height - 265, 440, 38, 6, fill=True, stroke=True)

    c.setFillColor(colors.HexColor("#065F46"))
    c.setFont("Helvetica-Bold", 14)
    c.drawCentredString(width / 2, height - 248, module_name)

    # Competency & Score Badge
    score_text = f"Official Score: {score:.1f} / 100  •  Competency Grade: {competency_status}"
    c.setFillColor(colors.HexColor("#0F172A"))
    c.setFont("Helvetica-Bold", 11)
    c.drawCentredString(width / 2, height - 290, score_text)

    # Date Information Box (Center-Bottom)
    issue_str = issue_date.strftime("%d %B %Y") if issue_date else "N/A"
    valid_str = valid_until.strftime("%d %B %Y") if valid_until else "N/A"

    c.setFillColor(colors.HexColor("#475569"))
    c.setFont("Helvetica", 9.5)
    c.drawCentredString(width / 2, height - 325, f"Date of Assessment & Issuance: {issue_str}    |    Valid Until: {valid_str}")

    # Left Section: QR Code & Verification Info
    if os.path.exists(qr_path):
        c.drawImage(qr_path, 50, 48, width=105, height=105)

    c.setFillColor(colors.HexColor("#0F172A"))
    c.setFont("Helvetica-Bold", 9)
    c.drawString(165, 130, f"Certificate ID: {certificate_number}")
    c.setFillColor(colors.HexColor("#64748B"))
    c.setFont("Helvetica", 8)
    c.drawString(165, 115, "Scan QR with Google Scanner / Phone")
    c.drawString(165, 102, f"Auth Hash: {verification_hash[:20]}...")
    c.setFillColor(colors.HexColor("#059669"))
    c.setFont("Helvetica-Bold", 8)
    c.drawString(165, 88, "✓ Authenticated Digital Credential")

    # Right Section: Official Signatures & Seal Representation
    c.setStrokeColor(colors.HexColor("#94A3B8"))
    c.setLineWidth(0.75)
    c.line(width - 250, 95, width - 55, 95)

    c.setFillColor(colors.HexColor("#0F172A"))
    c.setFont("Helvetica-Bold", 9.5)
    c.drawCentredString(width - 152, 80, "Authorized Safety Assessment Officer")
    c.setFillColor(colors.HexColor("#64748B"))
    c.setFont("Helvetica", 8)
    c.drawCentredString(width - 152, 68, "Directorate General of Mines Safety (DGMS)")

    # Save PDF
    c.showPage()
    c.save()


def generate_certificate_artifacts(
    certificate_number: str,
    worker_name: str,
    employee_id: str,
    module_name: str,
    score: float,
    competency_status: str,
    issue_date: datetime,
    valid_until: datetime,
    verification_token: str = None,
    public_verify_base_url: str = None,
) -> CertificateArtifacts:
    """Generate authentic PDF certificate, high-resolution PNG image, and public scannable QR code.

    Returns:
        CertificateArtifacts(pdf_path, image_path, qr_path, public_image_url, verification_token, verification_hash)
    """
    if verification_token is None:
        verification_token = generate_verification_token()
    if public_verify_base_url is None:
        public_verify_base_url = os.environ.get("VERIFICATION_BASE_URL", "http://localhost:8000/api/v1/certificates/verify")

    pdf_filename = f"{certificate_number}.pdf"
    pdf_path = str(STORAGE_DIR / pdf_filename)
    image_filename = f"{certificate_number}.png"
    image_path = str(STORAGE_DIR / image_filename)
    qr_filename = f"qr_{certificate_number}.png"
    qr_path = str(STORAGE_DIR / qr_filename)

    verification_hash = compute_verification_hash(certificate_number, verification_token, 0, score)

    # The QR code ALWAYS encodes the public verification URL — never an image URL
    verify_url = get_verification_url(certificate_number)
    generate_qr_code_image(verify_url, qr_path)

    # Certificate image public URL (for display, not QR)
    known_url = KNOWN_PUBLIC_URLS.get(certificate_number, "")
    public_image_url = known_url

    # Pass 1: Draw PDF Canvas with verification QR
    _draw_certificate_canvas(
        pdf_path=pdf_path,
        qr_path=qr_path,
        certificate_number=certificate_number,
        worker_name=worker_name,
        employee_id=employee_id,
        module_name=module_name,
        score=score,
        competency_status=competency_status,
        issue_date=issue_date,
        valid_until=valid_until,
        verification_hash=verification_hash,
    )

    # Pass 2: Render High-Resolution PNG Image
    render_pdf_to_image(pdf_path, image_path, scale=3)

    # Pass 3: Attempt to publish certificate IMAGE to permanent host (for display, not for QR)
    if not public_image_url:
        uploaded_url = upload_certificate_image_public(image_path, certificate_number)
        if uploaded_url:
            public_image_url = uploaded_url
            KNOWN_PUBLIC_URLS[certificate_number] = uploaded_url

    return CertificateArtifacts(
        pdf_path=pdf_path,
        image_path=image_path,
        qr_path=qr_path,
        public_image_url=public_image_url,
        verification_token=verification_token,
        verification_hash=verification_hash,
    )


def generate_certificate_pdf(
    certificate_number: str,
    worker_name: str,
    employee_id: str,
    module_name: str,
    score: float,
    competency_status: str,
    issue_date: datetime,
    valid_until: datetime,
    verification_token: str,
    public_verify_base_url: str = None,
) -> Tuple[str, str, str]:
    """Backwards-compatible wrapper returning (pdf_path, verification_token, verification_hash)."""
    artifacts = generate_certificate_artifacts(
        certificate_number=certificate_number,
        worker_name=worker_name,
        employee_id=employee_id,
        module_name=module_name,
        score=score,
        competency_status=competency_status,
        issue_date=issue_date,
        valid_until=valid_until,
        verification_token=verification_token,
        public_verify_base_url=public_verify_base_url,
    )
    return artifacts.pdf_path, artifacts.verification_token, artifacts.verification_hash
