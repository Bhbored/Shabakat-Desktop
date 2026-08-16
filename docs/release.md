# Release (MSI)

Windows installer for Shabakat. It is **per-machine** (needs admin), installs to `C:\Program Files\Shabakat`, and puts a **Shabakat** shortcut on the public desktop.

The MSI wraps the unpackaged self-contained publish folder. You do **not** change `WindowsPackageType` for this. Keep it `None`.

What you ship is always this one file:

`Installer\bin\Release\Shabakat.msi`

App data (SQLite, license, logos) lives in the user’s AppData folder, **not** under Program Files. Upgrading the MSI does not wipe that data. Data from Visual Studio / `dotnet run` is often in a **different** AppData folder — restore a JSON backup on a real install if you need it.

---

## What you need on the build PC

- .NET 10 SDK
- Windows MAUI workload (`maui-windows`)
- Node.js / npm (Release publish runs Tailwind)
- From the repo root: `npm ci` or `npm install` if `node_modules` is missing

WiX is restored automatically from NuGet when `pack-msi.ps1` builds the installer. You do not install WiX by hand.

---

## 1. Bump the version

Windows Installer only upgrades if the **new MSI version is higher**. Rebuilding `1.0.0` on top of an already-installed `1.0.0` will not replace it. Uninstall first, **or** bump.

Keep these three in sync. WiX needs **three** numbers (`major.minor.patch`). `ApplicationDisplayVersion` can be `1.0.1` (that is fine). `ApplicationVersion` is a single integer that must also go up.

| Place | Property | Example now | Next patch | Next minor |
|---|---|---|---|---|
| `Shabakat.csproj` | `ApplicationDisplayVersion` | `1.0.1` | `1.0.2` | `1.1.0` |
| `Shabakat.csproj` | `ApplicationVersion` | `2` | `3` | `4` |
| `Installer/Package.wxs` | `Package Version` | `1.0.1` | `1.0.2` | `1.1.0` |

### Example: next patch (`1.0.1` → `1.0.2`)

`Shabakat.csproj`:

```xml
<ApplicationDisplayVersion>1.0.2</ApplicationDisplayVersion>
<ApplicationVersion>3</ApplicationVersion>
```

`Installer/Package.wxs` (only the `Version` attribute — do **not** touch `UpgradeCode`):

```xml
<Package Name="Shabakat"
         Manufacturer="Shabakat"
         Version="1.0.2"
         UpgradeCode="bd6fa014-7176-42ae-867f-38441509f355"
```

### How to pick the number

- **Patch** `1.0.1` / `1.0.2` — bugfix, installer fix, small change. Same data, same license.
- **Minor** `1.1.0` — new screens or features, still upgrades in place.
- **Major** `2.0.0` — only if you deliberately want a new line of versions. Still uses the same `UpgradeCode`, so it **replaces** `1.x` (it is not a second app side by side).

Do **not** change `UpgradeCode`. That GUID is what tells Windows “this MSI is Shabakat.” Changing it installs a second copy and leaves the old one behind.

You cannot install an **older** MSI over a newer one. The installer shows *A newer version of Shabakat is already installed.* Uninstall from **Settings → Apps** first if you really need to go backwards.

---

## 2. Build the MSI

PowerShell, **repo root** (`C:\Users\...\Shabakat`):

```powershell
cd C:\Users\Bhbored\Desktop\Shabakat
.\pack-msi.ps1
```

That script does two things:

1. `dotnet publish Shabakat.csproj -c Release -r win-x64 --self-contained true`  
   Output folder: `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\`
2. `dotnet build Installer\Shabakat.Installer.wixproj -c Release`  
   Output MSI: `Installer\bin\Release\Shabakat.msi`

First run after a code change takes a few minutes. Wait until it prints `Installer ready: ...\Shabakat.msi`.

If publish already exists and you only changed WiX files, you can skip republish:

```powershell
dotnet build Installer\Shabakat.Installer.wixproj -c Release
```

For a customer build, always use `.\pack-msi.ps1` so the MSI matches current code.

---

## 3. Install / upgrade on a PC

Copy `Installer\bin\Release\Shabakat.msi` to the PC (USB, chat, etc.). Double-click it. Approve the admin prompt.

| Situation | What to do |
|---|---|
| No Shabakat installed | Run the MSI. Done. |
| Older MSI installed (version is **lower**) | Run the new MSI. It replaces Program Files and the shortcut. AppData stays. |
| Same version already installed | Windows skips the upgrade. Uninstall, then run the MSI, **or** bump the version and rebuild. |
| Newer MSI already installed | Uninstall the newer one first, then install the older MSI. |

Uninstall:

**Settings → Apps → Installed apps → Shabakat → Uninstall**

Uninstall does **not** delete AppData (customers, invoices, license). The next install will see the same database.

---

## 4. Check it actually works

1. Desktop shortcut opens Shabakat (window with the real UI, not stuck on *Loading...*).
2. **Settings → Apps** shows the version you put in `Package.wxs`.
3. If this PC already had a licensed MSI install, customers and license should still be there.
4. If this is the first MSI on the PC, you get the activation screen. That is normal. Restore a JSON backup from the gate if you have one.

Do not test “the release” with `dotnet run`. That is a different folder and often a different AppData path.

---

## 5. What the script is doing (optional)

You rarely need these by hand. They are the same steps as `pack-msi.ps1`:

```powershell
dotnet publish Shabakat.csproj -c Release -r win-x64 --self-contained true

dotnet build Installer\Shabakat.Installer.wixproj -c Release "-p:PublishDir=$PWD\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\"
```

If Tailwind CSS is stale, from the repo root:

```powershell
npm ci
npm run tw:build
```

`pack-msi.ps1` already runs `tw:build` as part of the Release publish.

---

## Common mistakes

- Built a new MSI but left `Version="1.0.0"` — Windows thinks nothing changed. Bump it.
- Edited `UpgradeCode` — two Shabakat entries in Apps. Put the original GUID back.
- Copied the `publish` folder instead of the MSI — that can work as a zip, but it is not the installer and has no desktop shortcut from WiX.
- Expected Visual Studio debug data to appear after MSI install — restore a backup.
- Shortcut opens an installer repair wizard — you are on an old advertised-shortcut MSI. Uninstall, then install a build from after the WorkingDirectory fix.
