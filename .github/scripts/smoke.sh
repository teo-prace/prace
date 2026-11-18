#!/usr/bin/env bash
# Teste de fumaça: confere se as páginas principais respondem com o código esperado.
# Uso: smoke.sh <url-base> [tentativas]
#   smoke.sh http://localhost:8080 30
#   smoke.sh https://prace.dev 12
# Sai com erro (1) se algum endereço não responder como esperado depois de todas as tentativas.
set -uo pipefail

BASE="${1:?informe a URL base, ex.: https://prace.dev}"
TRIES="${2:-12}"
WAIT=10

# caminho  código esperado
CHECKS=(
  "/healthz 200"
  "/pt 200"
  "/en 200"
  "/pt/services 200"
  "/en/cases 200"
  "/pt/contact 200"
  "/sitemap.xml 200"
  "/robots.txt 200"
  "/pagina-que-nao-existe 404"
)

check_all() {
  local ok=0
  for item in "${CHECKS[@]}"; do
    read -r path expected <<< "$item"
    code=$(curl -s -o /dev/null -m 15 -w "%{http_code}" "$BASE$path")
    if [ "$code" = "$expected" ]; then
      echo "  ok    $code $path"
    else
      echo "  FALHA $code $path (esperado $expected)"
      ok=1
    fi
  done
  return $ok
}

for attempt in $(seq 1 "$TRIES"); do
  echo "Tentativa $attempt/$TRIES em $BASE"
  if check_all; then
    echo "Tudo certo."
    exit 0
  fi
  [ "$attempt" -lt "$TRIES" ] && sleep "$WAIT"
done

echo "O site não respondeu como esperado em $BASE."
exit 1
