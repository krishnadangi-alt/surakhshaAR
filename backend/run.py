"""Entry point for the SurakshaAR backend (development and production).

Configuration comes from the environment / ``backend/.env``:

  SURAKHSHAAR_HOST            bind address (default 0.0.0.0)
  SURAKHSHAAR_PORT            bind port (default 8000)
  SURAKHSHAAR_RELOAD          auto-reload, development only (default 1)

For the real/graded deployment run without auto-reload:

  python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
"""

import uvicorn

from app.config import HOST, PORT, RELOAD

if __name__ == "__main__":
    uvicorn.run("app.main:app", host=HOST, port=PORT, reload=RELOAD)