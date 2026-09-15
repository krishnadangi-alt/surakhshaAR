"""Application configuration for the SurakshaAR backend."""

import os

APP_NAME = "SurakshaAR Backend API"
APP_VERSION = "1.0.0"

# SQLite database file stored in the backend directory.
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATABASE_URL = os.getenv("DATABASE_URL", f"sqlite:///{os.path.join(BASE_DIR, 'surakhshaar.db')}")