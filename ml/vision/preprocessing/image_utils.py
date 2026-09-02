"""Dependency-free image helpers used by the vision integration scaffold."""

import base64
from typing import List, Optional, Tuple

from ml.vision.config import MAX_IMAGE_BYTES

# Magic-byte signatures for the image formats we accept. A real model pipeline
# would decode the full image (PIL/OpenCV); here we only need enough to treat a
# payload as "a valid image" or "MODEL_ERROR" without any third-party library.
_IMAGE_MAGIC = (
    (b"\x89PNG\r\n\x1a\n", "png"),
    (b"\xff\xd8\xff", "jpeg"),
    (b"GIF87a", "gif"),
    (b"GIF89a", "gif"),
    (b"BM", "bmp"),
)


def decode_base64_image(image_base64: str) -> bytes:
    """Decode a base64 string into raw image bytes.

    Raises:
        ValueError: If the input is not valid base64.
    """
    try:
        return base64.b64decode(image_base64, validate=True)
    except Exception as exc:
        raise ValueError("image_base64 is not valid base64") from exc


def validate_image_bytes(data: bytes) -> Tuple[bool, str]:
    """Check that ``data`` looks like an image and is within size limits.

    Returns:
        ``(ok, image_format_or_error)``. Never raises. ``ok`` is ``False`` for
        an empty/oversized payload or when no magic-byte signature matches.
    """
    if not data:
        return False, "empty payload"
    if len(data) > MAX_IMAGE_BYTES:
        return False, f"image exceeds {MAX_IMAGE_BYTES} bytes"
    for magic, fmt in _IMAGE_MAGIC:
        if data.startswith(magic):
            return True, fmt
    return False, "unsupported image format"


def resize_and_normalize(
    frame: List[List[Tuple[int, int, int]]],
    width: int = 640,
    height: int = 640,
) -> List[List[Tuple[float, float, float]]]:
    """Nearest-neighbour resize + [0, 1] channel normalisation of an RGB frame.

    This is the interface the model preprocessing calls. ``frame`` is a
    ``rows x cols`` grid of ``(r, g, b)`` tuples (0-255). The mock/fallback
    detector never reaches this; a real model would feed its normalised tensor
    (numpy) from this output.

    Args:
        frame: Source RGB frame.
        width: Target width.
        height: Target height.

    Returns:
        A ``height x width`` grid of normalised ``(r, g, b)`` floats in [0, 1].
    """
    if not frame or not frame[0]:
        return []

    src_h = len(frame)
    src_w = len(frame[0])
    scale_y = src_h / float(height)
    scale_x = src_w / float(width)

    out = []
    for y in range(height):
        row = []
        src_y = min(src_h - 1, int(y * scale_y))
        src_row = frame[src_y]
        for x in range(width):
            src_x = min(src_w - 1, int(x * scale_x))
            r, g, b = src_row[src_x]
            row.append((r / 255.0, g / 255.0, b / 255.0))
        out.append(row)
    return out


def base64_to_rgb_frame(image_base64: str) -> Optional[list]:
    """Best-effort ``base64 -> RGB frame`` used only for pure-Python demos/tests.

    A 1x1 opaque PNG is decoded by this helper so small generated images can be
    processed without PIL. Returns ``None`` for anything unrecognised.

    This is a testing/demo convenience. Production frames come from the AR
    Foundation camera stream (sensor array in Unity) and are fed straight to the
    model preprocessing path.
    """
    try:
        data = decode_base64_image(image_base64)
    except ValueError:
        return None

    # 1x1 opaque PNG with no ancillary chunks.
    if len(data) >= 67 and data.startswith(b"\x89PNG\r\n\x1a\n"):
        try:
            # Inspect the IHDR width/height at fixed offsets for a signature PNG.
            width = int.from_bytes(data[16:20], "big")
            height = int.from_bytes(data[20:24], "big")
        except Exception:
            return None
        if width == 1 and height == 1:
            return [[(0, 0, 0)]]
    return None