from __future__ import annotations

import pyodbc
from .config import settings


def get_sql_server_connection() -> pyodbc.Connection:
    conn_str = (
        f"DRIVER={{{settings.sql_driver}}};"
        f"SERVER={settings.sql_server};"
        f"DATABASE={settings.sql_database};"
        f"UID={settings.sql_username};"
        f"PWD={settings.sql_password};"
        f"TrustServerCertificate={settings.sql_trust_server_certificate};"
    )
    return pyodbc.connect(conn_str)
