# Planetbase-AutoConnections
An update of JPFarias's AutoConnections mod 
([Nexus](https://www.nexusmods.com/planetbase/mods/9))
([Source](https://bitbucket.org/joaofarias/planetbase-modding/src/master/AutoConnections/))
, modified to use Harmony instead of Redirector. It relies on my own utilities library (PBUtilities) to
get the list of all modules (AKA to abstract away the reflection).

It uses Unity Mod Manager to hook into the game. To add Planetbase support, add this to
UnityModManagerConfig.xml:

```xml
<GameInfo Name="Planetbase">
	<Folder>Planetbase</Folder>
	<ModsDirectory>Mods</ModsDirectory>
	<ModInfo>Info.json</ModInfo>
	<GameExe>Planetbase.exe</GameExe>
	<EntryPoint>[UnityEngine.UI.dll]UnityEngine.Canvas.cctor:Before</EntryPoint>
	<StartingPoint>[Assembly-CSharp.dll]Planetbase.GameStateLogo.load:After</StartingPoint>
	<MinimalManagerVersion>0.22.3</MinimalManagerVersion>
</GameInfo>
```