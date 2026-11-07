# Xbox 360 Active Downloads

A small Windows app with one button to open **Active Downloads** on a network-connected Xbox 360.

## Run

1. Download and extract the app ZIP from Releases.
2. Put your own `JRPC.dll` and `xdevkit.dll` beside `XboxActiveDownloads.exe`.
3. Turn on the Xbox and make sure it is reachable from your PC.
4. Open the app, enter the Xbox IP address or console name, and click **Open Active Downloads**.

The app remembers the address after a successful connection in `xbox-address.txt`, beside the executable. It opens the downloads screen; it does not start, cancel, or change downloads.

## Requirements

- Windows with .NET Framework 4.5 or later.
- Xbox 360 SDK / Xbox 360 Neighbourhood installed and configured on the PC.
- Your own compatible `JRPC.dll` and `xdevkit.dll`.
- An Xbox 360 with the JRPC remote-call plugin already running and a working debug connection.
- PC and console on a reachable local network.

This is intended for consoles already configured for Xbox 360 Neighbourhood and JRPC. It does not configure or modify the console.

**External DLLs, the Xbox SDK, console plugins, and game files are not included.** Obtain dependencies separately under their respective terms.

## Build

Put the two DLLs in a local `lib` folder, then run:

```powershell
.\build.ps1
```

To use a different dependency folder:

```powershell
.\build.ps1 -DependenciesDirectory 'C:\path\to\your\dependencies'
```

The build uses the .NET Framework C# compiler included with Windows. It produces `bin\XboxActiveDownloads.exe` and `dist\XboxActiveDownloads-v1.0.0.zip`. The ZIP contains the app and usage instructions; supply the external DLLs separately.

The app also accepts `--open <console-address>` to open the menu without displaying its window. It returns exit code 0 on success and 1 on failure.

## How it works

The app calls `XamShowMarketplaceUI` on the console's system thread through JRPC, using the Active Downloads entry point (12). The user interface stays responsive while connecting.

## Troubleshooting

If the app cannot connect, check that the Xbox is on, its address is correct, Xbox 360 Neighbourhood can reach it, and the JRPC plugin is loaded. Keep both dependency DLLs beside the executable.

This is an independent utility, not affiliated with Microsoft or Xbox.
