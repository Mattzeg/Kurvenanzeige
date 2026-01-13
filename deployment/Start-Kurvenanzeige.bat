@echo off
REM =====================================================
REM Kurvenanzeige - S7-1500 PLC Monitor
REM =====================================================
REM
REM Feature: Real-time PLC data visualization and archiving
REM
REM Features:
REM   - Live dashboard with analog/digital values
REM   - Trend charts with historical data
REM   - SQLite data archiving (7-day retention)
REM   - Automatic PLC reconnection
REM   - 5-10 second update interval
REM
REM =====================================================

echo.
echo =====================================================
echo Kurvenanzeige - S7-1500 PLC Monitor
echo =====================================================
echo.
echo Starting application...
echo.
echo Features:
echo   * Live Dashboard - Real-time process data
echo   * Trend Charts - Historical data visualization
echo   * Data History - Searchable data table
echo   * Settings - PLC configuration
echo.
echo Web Interface will open at: http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.
echo =====================================================
echo.

cd /d "%~dp0"

REM Set production environment
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://localhost:5000

REM Start the application and open browser
start http://localhost:5000
Kurvenanzeige.Web.exe

pause
