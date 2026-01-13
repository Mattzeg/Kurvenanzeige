# Kurvenanzeige - S7-1500 PLC Monitor

ASP.NET Core Blazor Server Anwendung zur Überwachung und Archivierung von Daten einer Siemens S7-1500 SPS.

## Features

- **Live Dashboard** - Echtzeit-Anzeige von Prozessdaten
  - Analoge Werte (Temperaturen, Drücke, etc.)
  - Digitale Signale (Status, Schalter, etc.)
  - Automatische Updates alle 5-10 Sekunden

- **Trend-Diagramme** - Zeitliche Verläufe historischer Daten
  - Konfigurierbare Zeitbereiche
  - Mehrere Tags gleichzeitig
  - Min/Max/Durchschnitt-Berechnung

- **Daten-Historie** - Durchsuchbare Tabelle aller aufgezeichneten Daten
  - Filterung nach Zeitbereich
  - Sortierung und Paginierung
  - Quality-Anzeige

- **SQLite-Archivierung** - Automatische Datenspeicherung
  - 7 Tage Rohdaten
  - Automatische Bereinigung
  - WAL-Mode für bessere Performance

- **Robuste PLC-Verbindung**
  - Automatische Wiederverbindung
  - Exponential Backoff
  - Connection Health Monitoring

## Technologie-Stack

- ASP.NET Core 8.0 Blazor Server
- Entity Framework Core mit SQLite
- S7.Net Plus für S7-1500 Kommunikation
- Bootstrap 5 UI

## Systemvoraussetzungen

- Windows 10/11 oder Windows Server
- Netzwerkverbindung zur S7-1500 SPS
- Port 5000 verfügbar für Web-Interface

## Installation

### Option 1: Self-Contained Deployment (Empfohlen)

Die Anwendung wird mit allen benötigten .NET-Laufzeitkomponenten ausgeliefert.

1. Kopieren Sie den gesamten Ordner an den gewünschten Ort
2. Bearbeiten Sie `appsettings.json` und passen Sie die PLC-Verbindungsparameter an:
   ```json
   "PlcConnection": {
     "IpAddress": "192.168.0.1",  // IP-Adresse Ihrer S7-1500
     "Port": 102,
     "Rack": 0,
     "Slot": 1
   }
   ```
3. Doppelklicken Sie auf `Start-Kurvenanzeige.bat`
4. Der Browser öffnet sich automatisch mit http://localhost:5000

### Option 2: Aus Source kompilieren

Voraussetzungen:
- .NET 8.0 SDK oder neuer

```powershell
# Repository klonen
git clone <repository-url>
cd Kurvenanzeige

# Projekt kompilieren
dotnet build

# Anwendung starten
dotnet run --project src/Kurvenanzeige.Web/Kurvenanzeige.Web.csproj
```

## Konfiguration

### PLC-Verbindung (appsettings.json)

```json
{
  "PlcConnection": {
    "IpAddress": "192.168.0.1",        // IP-Adresse der SPS
    "Port": 102,                        // Standard S7-Port
    "Rack": 0,                          // Rack-Nummer
    "Slot": 1,                          // Slot-Nummer (meist 1 für S7-1500)
    "CpuType": "S71500",                // S71500, S71200, S7300, S7400
    "ConnectTimeout": 5000,             // Verbindungs-Timeout in ms
    "ReadTimeout": 2000,                // Lese-Timeout in ms
    "ReconnectDelay": 5000,             // Verzögerung vor Reconnect
    "MaxReconnectAttempts": 10          // Max. Reconnect-Versuche
  }
}
```

### Datenpunkte konfigurieren

Datenpunkte können in der Datenbank `kurvenanzeige.db` in der Tabelle `DataPointConfigurations` konfiguriert werden.

Standard-Datenpunkte (Beispiele):
- `Temperature_Reactor1` - DB1.DBD0 (REAL, °C)
- `Pressure_Line1` - DB1.DBD4 (REAL, bar)
- `Pump_Running` - DB2.DBX0.0 (BOOL)

## Architektur

```
┌─────────────────────────────────────────────────┐
│           Blazor Server Web UI                  │
│  (Dashboard, Trends, History, Settings)         │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────┐
│         Application Services                    │
│  - DataPollingService (Background Worker)       │
│  - DataArchivingService (Cleanup)               │
│  - UiUpdateService (Real-time Updates)          │
└────────┬───────────────────────┬────────────────┘
         │                       │
┌────────┴────────┐    ┌────────┴─────────┐
│  S7PlcService   │    │  DataRepository  │
│  (S7.Net Plus)  │    │  (EF Core)       │
└────────┬────────┘    └────────┬─────────┘
         │                      │
┌────────┴────────┐    ┌────────┴─────────┐
│   S7-1500 PLC   │    │  SQLite DB       │
└─────────────────┘    └──────────────────┘
```

## Projekt-Struktur

```
Kurvenanzeige/
├── src/
│   ├── Kurvenanzeige.Web/              # Blazor Server UI
│   ├── Kurvenanzeige.Core/             # Business Logic
│   ├── Kurvenanzeige.Infrastructure/   # Data & PLC Access
│   └── Kurvenanzeige.Shared/           # DTOs & Constants
├── deployment/
│   ├── Start-Kurvenanzeige.bat
│   └── appsettings.Production.json
└── README.md
```

## Troubleshooting

### PLC verbindet nicht

1. Prüfen Sie die Netzwerkverbindung zur SPS
2. Stellen Sie sicher, dass Port 102 erreichbar ist
3. Verifizieren Sie Rack und Slot in den Settings
4. Prüfen Sie die CPU-Konfiguration in TIA Portal (PUT/GET erlaubt?)

### Keine Daten im Dashboard

1. Prüfen Sie die PLC-Verbindung (siehe oben)
2. Überprüfen Sie die Datenpunkt-Konfiguration in Settings
3. Schauen Sie in die Log-Ausgaben in der Konsole
4. Prüfen Sie, ob die DB-Nummern und Offsets korrekt sind

### Performance-Probleme

1. Reduzieren Sie die Anzahl der Datenpunkte
2. Erhöhen Sie das Polling-Intervall (DataPolling.PollingIntervalMs)
3. Führen Sie ein VACUUM auf der Datenbank aus

## Development

### Build

```powershell
dotnet build
```

### Run

```powershell
dotnet run --project src/Kurvenanzeige.Web/Kurvenanzeige.Web.csproj
```

### Publish (Self-Contained)

```powershell
$targetDate = Get-Date -Format "yyyy-MM-dd"
$targetPath = "C:\Users\matth\OneDrive\Dokumente\Portable Anwendungen\Kurvenanzeige-$targetDate-S7PlcMonitor"

dotnet publish src/Kurvenanzeige.Web/Kurvenanzeige.Web.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $targetPath

Copy-Item deployment/appsettings.Production.json $targetPath/appsettings.json -Force
Copy-Item deployment/Start-Kurvenanzeige.bat $targetPath/ -Force
```

## Lizenz

Proprietär - Alle Rechte vorbehalten

## Support

Bei Fragen oder Problemen erstellen Sie ein Issue im Repository.
