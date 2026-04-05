#!/usr/bin/env bash
# Carga los datos iniciales en la base de datos carrito_compras:
#   - Productos desde los CSVs del directorio Resources/
#   - Usuario administrador inicial
#
# Uso: bash seed.sh [host] [port] [user]
# Defaults: localhost 5432 postgres
#
# Usuario creado:
#   UserName : admin
#   Password : admin123  (hash SHA-256 almacenado en BD)

HOST=${1:-localhost}
PORT=${2:-5432}
USER=${3:-postgres}
DB=carrito_compras
DIR="$(cd "$(dirname "$0")" && pwd)"

# --- Productos ---
echo "==> Importando productos..."
COLUMNS='"Sku","Name","Description","AvailableUnits","UnitPrice","Image"'

for FILE in "$DIR/productsEA.csv" "$DIR/productsSP.csv" "$DIR/productsWE.csv"; do
  echo "    $FILE"
  psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" \
    -c "\copy \"Products\" ($COLUMNS) FROM '$FILE' DELIMITER ',' CSV HEADER;"
done

# --- Usuario administrador ---
echo "==> Creando usuario inicial..."

# SHA-256 de "admin123"
HASH="240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9"

psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" <<SQL
INSERT INTO "Users" ("UserName", "Name", "Password")
VALUES ('admin', 'Administrador', '$HASH')
ON CONFLICT DO NOTHING;
SQL

echo ""
echo "Seed completado."
echo "  Productos cargados desde los 3 CSVs."
echo "  Usuario 'admin' disponible (contrasena: admin123). Cambiala despues del primer acceso."
