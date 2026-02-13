# Tracker App - Setup Guide

This guide outlines the steps to set up and run the tracker application after cloning the repository.

## Prerequisites
- **.NET 10.0 SDK** (Preview or Latest)
- **Node.js** (v20 or newer recommended)
- **Podman Desktop** (Optional, if using container features)

## Project Structure
- `src/Tracker.AppHost`: The Aspire AppHost orchestrator.
- `src/tracker-web`: The React frontend (Vite).
- `src/Tracker.Api`: The .NET backend API.

## First-Time Setup

1.  **Clone the repository**:
    ```bash
    git clone <repository-url>
    cd project002
    ```

2.  **Restore dependencies**:
    ```bash
    dotnet restore
    ```

3.  **Install Frontend Dependencies**:
    Navigate to the web project and install npm packages.
    ```bash
    cd src/tracker-web
    npm install
    ```
    *(Return to project root)*
    ```bash
    cd ../..
    ```

## Running the Application

To run the full stack (Frontend + Backend + Database) orchestrated by .NET Aspire:

```bash
dotnet run --project src/Tracker.AppHost/Tracker.AppHost.csproj
```

The app will start and provide URLs for the Aspire Dashboard and the Application itself.
- **App URL**: Typically `https://localhost:17042`
- **Dashboard URL**: Check console output (dynamic port)

## Troubleshooting
- **Database Errors**: If you encounter schema errors, delete `src/Tracker.Api/tracker.db` and restart the app to re-seed.
- **Port Conflicts**: Check `launchSettings.json` in `src/Tracker.AppHost` if default ports are taken.
