# fcast
**For Clan and Spoils** real-time strategy/tactical (witcher fan fic game)

A derivative work from Osnowa and inspired by the Quinta Essential rogue-like witcher game that was made using the Osnowa framework.

While Osnowa appears to have a flexible architecture approach, fcast is written by me (a Unity newbie) using a quick and dirty approach just to get something up and working (hopefully) by the end of Hacktoberfest.

A minor upgrade to 2019.4.40f1 LTS (from 2019.4.20f1) was attempted but the editor eventually got into a corrupted state (and could no longer find System std lib files), so was reverted back to using the 2019.4.20f1 editor.

## Licensing

This project is based on the [Osnowa framework](https://github.com/azsdaja/Osnowa) originally licensed under the MIT License. For the original README and framework documentation, please see the Osnowa repository.

All modifications, additions, and new code in this repository made by Korey Hinton are licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).

This means:
- The original Osnowa source code remains MIT-licensed.
- My changes and derivative works (the "fcast" code) are licensed under AGPL-3.0.
- Combined distributions must comply with AGPL-3.0.

## Controls

Aim and build a temple:

```
a <ARROWS> t ESC
```

## Linux development exceptions

1. Just launch the editor directly without the hub (since Unity Hub on Ubuntu failed to use custom file locations)

    ```
    ./Unity -projectPath ../../repos/fcast
    ```

2. The editor and projects are large, and if you forgot to reformat an exFAT external volume first, then files will be incorrectly permissioned in git.

    All you can do is disable fileMode for git-tracked files and be cautious in adding newly tracked files:

    ```
    git config core.fileMode false
    chmod -x newcodefile.cs && git add newcodefile.cs
    ```

## Manifest

```
# hook to call imperative game loop iteration (FcastGameLoop.It())
./Assets/Osnowa/Osnowa.Core/TurnManager.cs

# game loop iteration and supporting structures
./Assets/Scripts/Fcast/FcastGameLoop.cs
./Assets/Scripts/Fcast/TimberChopIntervalCheck.cs
./Assets/Scripts/Fcast/GoldMineIntervalCheck.cs
./Assets/Scripts/Fcast/EventIntervalCheck.cs
./Assets/Scripts/Fcast/InputSequenceCheck.cs
./Assets/Scripts/Fcast/BuildingEventIntervalCheck.cs
./Assets/Scripts/Fcast/BuildingUpdateViewsCheck.cs
./Assets/Scripts/Fcast/ExecCheck.cs
./Assets/Scripts/Fcast/IExec.cs

# game data
./Assets/Scripts/Fcast/FcastGameData.cs

# witcher entity
./Assets/GameAssets/Entities/EntityRecipees/Actors/Player.asset # (see _sprites)
./Assets/Sprites/Spritesheets/Witcher tileset/Witcher tileset.png.meta

# Entitas-generated properties (partial to GameEntity)
./Assets/Scripts/Generated/Game/Components/Game{View,Held,etc}Component.cs

# Scene (re-open this file in the Unity editor "Project" tab after re-import)
./Assets/Scene.unity

# Abilities, ([A]im) choose "Project" tab to see the key binding in "Inspector"
./Assets/GameAssets/Abilities/Aim.asset.meta

# Graceful null handling added to allow imperative building views:
./Assets/Osnowa/Osnowa.Grid/EntityPresenter.cs      # GetVisibleEntities()
./Assets/Osnowa/Osnowa.Unity/UnityEntityDetector.cs # FilterEntities()
```
