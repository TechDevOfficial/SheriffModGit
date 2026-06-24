# Classic Us – Sheriff Mod

An Among Us (Classic Us) mod that adds the custom **Sheriff** role. 

The Sheriff is a Crewmate equipped with a kill button. Their objective is to locate and eliminate the Impostor. However, if they target an innocent crewmate, they will die instead (misfire).

## Features

- **Early Role Registration**: Overrides Unity scene lifecycle timings to register custom and standard roles early, preventing "role does not exist" race conditions during transitions.
- **Lobby Customize Menu Integration**: Adds configuration options directly to the pre-game lobby laptop settings (Game tab).
- **Lobby Setting Sync & Locks**: 
  - Hosts can customize settings which are synced immediately to all clients.
  - Clients have settings controls locked (arrow buttons hidden) to ensure only the host can modify game settings.
- **Custom Intro Cutscene**: Displays "Sheriff" in orange with custom description text when the game starts.
- **Custom Kill Logic**: Manages the kill button logic, enforcing self-kill (misfire) if a Crewmate is targeted.

## How to Build

### Requirements
- .NET SDK 6.0 or higher.

### Compilation
Build the project using the dotnet CLI:
```powershell
dotnet build -c Release
```

The output DLL is generated in the project's build output directory:
`SheriffMod/bin/Release/net6.0/ClassicUs.SheriffMod.dll`

You must manually copy this DLL to your game's BepInEx plugins directory:
`<GameDir>\BepInEx\plugins\`

## Configuration

Settings can be changed:
1. **In-game**: In the laptop customization menu under the **Game** tab.
2. **Configuration file**: Editing `BepInEx/config/classicus.sheriff.cfg` on disk:
   - `EnableSheriff` (bool) – Toggles the Sheriff role.
   - `SheriffCount` (int, 0–3) – Number of Sheriffs to assign.
   - `SheriffKillCooldown` (float, 5–60s) – Cooldown of the Sheriff's kill button.
