@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building client proxy
docker compose --verbose build --force-rm --no-cache proxy

echo Done
exit 0