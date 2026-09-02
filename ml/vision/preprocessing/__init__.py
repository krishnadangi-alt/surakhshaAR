"""Input preprocessing for the vision module: frame capture/resize/normalize.

Dependency-free by design: the integration scaffold must run with the Python
standard library only. When a real model is plugged in these helpers can be
replaced (or wrapped) with a numpy/OpenCV backend of the same contract.
"""

from .image_utils import decode_base64_image, resize_and_normalize, validate_image_bytes

__all__ = ["decode_base64_image", "resize_and_normalize", "validate_image_bytes"]