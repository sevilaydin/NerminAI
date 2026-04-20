@echo off
echo NerminAI baslatiliyor...

:: Embedding server
start "Embedding Server" cmd /k "cd /d %~dp0 && python -m uvicorn embedding_server:app --host 127.0.0.1 --port 8000"

:: Biraz bekle
timeout /t 5 /nobreak > nul

:: API
start "NerminAI API" cmd /k "cd /d %~dp0 && dotnet run --project NerminAI.API"

:: Biraz daha bekle
timeout /t 10 /nobreak > nul

:: Streamlit UI
start "NerminAI UI" cmd /k "cd /d %~dp0 && python -m streamlit run streamlit/app.py --server.port 8502"

echo.
echo Servisler baslatildi:
echo   Embedding : http://127.0.0.1:8000
echo   API       : http://localhost:5054
echo   UI        : http://localhost:8502
echo.
pause
