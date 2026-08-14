@echo off

start "ProjectHost (dotnet run)" cmd /c "dotnet run --project ..\..\Macro-Deck-3\host\src\MacroDeckHost"
start "Angular (npm start)" cmd /c "cd /d ..\..\Macro-Deck-3\ui\angular && npm run start"