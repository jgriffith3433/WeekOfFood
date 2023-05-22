@echo off
echo ------------------------------------------------------------
echo ----------------------BUILDING------------------------------
echo ------------------------------------------------------------
echo Building client image
docker compose --verbose build --force-rm --no-cache client

echo Done
exit 0
