#!/usr/bin/env bash
# Carga los datos iniciales en la base de datos carrito_compras:
#   - Productos desde los CSVs del directorio Resources/
#   - Usuario administrador inicial
#
# Uso: bash seed.sh
# La conexión se lee desde el archivo .env ubicado en el mismo directorio.
# Copiá .env.example a .env y completá con tus valores antes de correr este script.

DIR="$(cd "$(dirname "$0")" && pwd)"
ENV_FILE="$DIR/.env"

if [ ! -f "$ENV_FILE" ]; then
  echo "Error: no se encontró el archivo .env en $DIR"
  echo "Copiá .env.example a .env y completá tus credenciales:"
  echo "  cp $DIR/.env.example $DIR/.env"
  exit 1
fi

# Cargar variables del .env (ignorar comentarios y líneas vacías)
export $(grep -v '^\ *#' "$ENV_FILE" | grep -v '^\ *$' | xargs)

# psql toma PGHOST, PGPORT, PGUSER, PGPASSWORD y PGDATABASE como variables de entorno automáticamente

# Verificar si las tablas ya tienen datos
PRODUCTS_EXISTS=$(psql -tAc 'SELECT EXISTS(SELECT 1 FROM "Products");' 2>/dev/null)
USERS_EXISTS=$(psql -tAc 'SELECT EXISTS(SELECT 1 FROM "Users");' 2>/dev/null)

if [ "$PRODUCTS_EXISTS" = "t" ] && [ "$USERS_EXISTS" = "t" ]; then
  echo "Las tablas ya tienen datos. No se escribió nada."
  exit 0
fi

# En Git Bash / Cygwin el psql es el binario de Windows y necesita rutas con formato Windows.
# cygpath -m convierte /c/Code/... a C:/Code/... (barras hacia adelante, compatible con psql).
to_native_path() {
  if command -v cygpath &>/dev/null; then
    cygpath -m "$1"
  else
    echo "$1"
  fi
}

# --- Productos ---
echo "==> Importando productos..."

for FILE in "$DIR/productsEA.csv" "$DIR/productsSP.csv" "$DIR/productsWE.csv"; do
  echo "    $FILE"
  NATIVE_FILE="$(to_native_path "$FILE")"
  psql <<SQL
\copy "Products" ("Sku","Name","Description","AvailableUnits","UnitPrice","Image") FROM '$NATIVE_FILE' DELIMITER ',' CSV HEADER;
SQL
done

# --- Usuario administrador ---
echo "==> Creando usuario inicial..."

# SHA-256 de "admin123"
HASH="240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9"

psql <<SQL
INSERT INTO "Users" ("UserName", "Name", "Password")
VALUES ('admin', 'Administrador', '$HASH')
ON CONFLICT DO NOTHING;
SQL

echo ""
echo "Seed completado."
echo "  Productos cargados desde los 3 CSVs."
echo "  Usuario 'admin' disponible (contrasena: admin123). Cambiala despues del primer acceso."
