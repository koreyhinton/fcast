# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

fcast ("For Clan and Spoils") is a real-time strategy/tactics game built on the Osnowa roguelike framework (Entitas ECS + Zenject). Unity editor **2019.4.20f1**: don't suggest upgrading (an attempted upgrade corrupted the editor). Code must stay within C# 7.3 / .NET Standard 2.0. See README.md for controls and the file manifest.

## Build

```sh
NSMAP=/path/to/orc/lib/src/main/bash/com/koreyhinton/nsmap ./build.sh
```

- Must run before opening the editor, and after any `.sh` change. The generated `.cs` files are gitignored, and the project won't compile without them.
- orc is checked out at `/k/repos/kh/orc` on this machine (not a sibling of the repo, so `NSMAP` is passed in).
- Each new generated file needs a line in `build.sh` (`./X.sh > ./X.cs || exit 1`) and a `.gitignore` entry for the `.cs`. Its `.cs.meta` stays committed, because the GUID is how scenes reference the script.
- There are no tests or command-line Unity builds; behavior is checked by running in the editor.

## Code generation (new code goes in .sh)

New Fcast code is written as bash scripts that emit C# using orc, in the same style as `Assets/Scripts/Fcast/FcastMain.sh`:

- Edit the `.sh`, never the generated `.cs` (its first line says so).
- Layout of a script: bash header comments (literate-program style), the `echo` banner line, `v=${1:-<prefix>}`, a `# maps` section for orc variable mappings, then the C# in a `cat << EOF` heredoc.
- Keep the `v` variable and `# maps` section even when unused; they're for orc namespacing later.
- orc namespacing: a spliced child script takes its prefix as `$1` and prefixes every C# local it declares with `${v}`. The parent `export`s mappings (e.g. `${v}ch_Grid=${v}Grid`). The child runs `. ${NSMAP}/bind ${v} Grid ...` and refers to `${!grid}`. Reusable logic is a separate step script spliced in with backticks, with `<prefix>In`/`<prefix>Out` variables instead of a function signature.
- The heredoc is unquoted, so `$` and backticks meant for C# must be escaped (`\$`, `` \` ``). Avoid `$"..."` interpolated strings.
- Existing hand-written `.cs` files (Osnowa, `FcastGameLoop.cs`, the `*Check.cs` helpers) stay as plain C#. Don't convert them unless asked.

## Game loop architecture

- `FcastMain` (generated from `FcastMain.sh`) is the single per-frame entry point. It sits on the empty root `FcastMain` GameObject in `Assets/Scene.unity`, which starts inactive. `GameInitializer.InitializeGame()` activates it once the world exists. It runs before Osnowa's `TurnManagerBehaviour` (`[DefaultExecutionOrder(-100)]`).
- Each frame has three phases:
  1. **INPUT**: read `Input.*` once.
  2. **SIMULATE**: fixed steps (`StepSeconds`); timers advance by `StepSeconds`. No transforms or `Instantiate` here.
  3. **PRESENT**: read the simulation state only, and set absolute values on views (e.g. `localEulerAngles`, not `Rotate`).
- Features are being migrated piece by piece from the legacy `FcastGameLoop.It(g)`, which is called from `Assets/Osnowa/Osnowa.Core/TurnManager.cs` (once per spawn/raze list entry, guarded by `g.Frame`). When moving a feature, remove it from `FcastGameLoop.cs` so it doesn't run twice.
- Migrated so far: player bounce.

### Code style: imperative

Use plain statements, `if` and loops, with all-caps comment lines as section headers (the user navigates by them in emacs wik-mode). Don't add helper classes, LINQ or lambdas for game logic. Avoid allocating per frame: cache lookups (Entitas groups, prefabs, `Grid`) in fields.
