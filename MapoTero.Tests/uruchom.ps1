<#
.SYNOPSIS
    Bezpieczne lokalne uruchamianie kompilacji i testów MapoTero bez zamrażania systemu.
.DESCRIPTION
    Buduje i uruchamia testy z najniższym priorytetem CPU (Idle), limitem węzłów MSBuild (-m:1),
    wyłączonym serwerem kompilacji (UseSharedCompilation=false), twardym limitem sterty GC (640 MB)
    dla procesu testów oraz sekwencyjnym uruchamianiem testów (MaxParallelThreads=1), żeby nie
    wypychać aplikacji użytkownika do pliku stronicowania (pagefile) ani nie blokować kliknięć w Windows.
    Domyślnie uruchamia tylko szybkie testy logiki (bez Siec, Ciezkie, UI, Wolne).
#>
param(
    [ValidateSet("Szybkie", "Domyslne", "UI", "Wolne", "Wszystkie")]
    [string]$Tryb = "Szybkie",
    [string]$Filtr = "",
    [switch]$BezBudowania,
    [ValidateRange(1, 4)]
    [int]$Watki = 1,
    [ValidateSet("Idle", "BelowNormal")]
    [string]$Priorytet = "Idle",
    [string]$Projekt = "MapoTero.Tests/MapoTero.Tests.vbproj"
)

$ErrorActionPreference = "Stop"

function UruchomProces {
    param(
        [string]$Argumenty,
        [switch]$LimitPamieciTestu
    )
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    $psi.Arguments = $Argumenty
    $psi.UseShellExecute = $false
    if ($LimitPamieciTestu) {
        # Limit sterty GC nakładany tylko na proces testów (bez spowalniania kompilatora vbc/MSBuild):
        # na laptopie z 8 GB RAM zapobiega wypychaniu przeglądarki, IDE i eksploratora do pliku stronicowania.
        $psi.EnvironmentVariables["DOTNET_GCHeapHardLimit"] = "0x28000000"
        $psi.EnvironmentVariables["DOTNET_GCConserveMemory"] = "9"
    }
    $p = [System.Diagnostics.Process]::Start($psi)
    try {
        $p.PriorityClass = [System.Diagnostics.ProcessPriorityClass]::$Priorytet
    } catch {
        # proces mógł się już zakończyć
    }
    $p.WaitForExit()
    if ($p.ExitCode -ne 0) {
        exit $p.ExitCode
    }
}

if (-not $BezBudowania) {
    $assets = Join-Path (Split-Path $Projekt -Parent) "obj\project.assets.json"
    $noRestore = if (Test-Path $assets) { " --no-restore" } else { "" }
    Write-Host "Budowanie ($Priorytet, -m:1, bez VBCSCompiler)..." -ForegroundColor Cyan
    UruchomProces "build `"$Projekt`" -c Release -m:1 -p:UseSharedCompilation=false$noRestore"
}

if ([string]::IsNullOrWhiteSpace($Filtr)) {
    switch ($Tryb) {
        "Szybkie"   { $Filtr = "Kategoria!=Siec&Kategoria!=Ciezkie&Kategoria!=UI&Kategoria!=Wolne" }
        "Domyslne"  { $Filtr = "Kategoria!=Siec&Kategoria!=Ciezkie" }
        "UI"        { $Filtr = "Kategoria=UI" }
        "Wolne"     { $Filtr = "Kategoria=Wolne" }
        "Wszystkie" { $Filtr = "Kategoria!=Brak" }
    }
}

Write-Host "Testy [$Tryb | filtr: $Filtr | wątki: $Watki | priorytet: $Priorytet]..." -ForegroundColor Cyan
UruchomProces "test `"$Projekt`" -c Release --no-build --filter `"$Filtr`" -- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=$Watki" -LimitPamieciTestu
