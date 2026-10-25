---
name: fcast-review
description: Review fcast changes (staged by default, or a given commit/branch/path) for bugs and for breaking the project's orc bash-assembled code, Unity 2019.4 and game-loop conventions. Use when the user asks to review fcast changes or runs /fcast-review.
---

# fcast review

Review the target changes and report findings ranked most severe first. Read `CLAUDE.md` first: it holds the conventions this review checks.

## 1. Pick the target

- No argument: the staged changes (`git diff --cached`, `git diff --cached --stat`).
- A commit, range or branch: `git diff <target>` (a branch is diffed against `master`).
- A path: the changes under that path.

Also run `git status --short`. Some findings depend on files that are **not** in the target, e.g. a staged file that relies on an unstaged or untracked one.

## 2. Regenerate before judging generated code

Generated `.cs` files are gitignored, so the diff only shows the `.sh` source. For each changed `X.sh` under `Assets/`:

```sh
NSMAP=/k/repos/kh/orc/lib/src/main/bash/com/koreyhinton/nsmap \
  bash -c 'cd <dir of X.sh> && ./X.sh' > <scratchpad>/X.cs
```

Review that output as the real C#. Diff it against the working-tree `X.cs`; if they differ, `build.sh` hasn't been rerun (report as low).

**Compile check** when Unity DLLs are available (the 2019.4 editor on the SSD at `/media/korey/Extreme SSD/Unity-download/Editor/Data`). Compile in the scratchpad, never in the repo:

```sh
mcs -langversion:7.2 -target:library \
  -r:"<Data>/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll" \
  -r:"<Data>/Managed/UnityEngine/UnityEngine.CoreModule.dll" \
  -r:Assets/Entitas/Entitas.dll -r:Library/ScriptAssemblies/OsnowaExample.dll \
  -out:<scratchpad>/check.dll <scratchpad>/X.cs
```

`OsnowaExample.dll` is from the last editor compile, so it can be stale. Add `-r:` for other `UnityEngine.*Module.dll` files as needed (e.g. `InputLegacyModule` for `Input`). A compile error is high severity; say which DLLs were used.

## 3. Checklist

### orc / bash-assembled files
- **Committable as a set.** If a staged file references a generated type, the `.sh`, its `build.sh` line, the `.gitignore` entry and the `.cs.meta` must all be staged too. A commit missing one breaks a fresh clone.
- **Generated `.cs` never committed or edited by hand.** It must be in `.gitignore`. Any edit to the `.cs` that isn't in the `.sh` gets lost on the next build.
- **`.cs.meta` committed, GUID stable.** Scenes refer to scripts by that GUID; a changed or missing one leaves "missing script" in the scene.
- **`build.sh` covers every `.sh`**: one `./X.sh > ./X.cs || exit 1` line each.
- **Heredoc escaping.** In `cat << EOF`, every `$` or backtick meant for C# must be `\$` / `` \` ``. Look for `$"..."` interpolation, `$` in strings or comments, and backticks in comments. An unescaped one silently becomes a bash expansion: diff the generated output to confirm.
- **Script layout kept**: bash header comments, the `echo` banner, `v=${1:-<prefix>}`, a `# maps` section, then the heredoc. Don't flag unused `v` or empty `# maps`; they're intentional.
- **Namespacing** (when spliced children exist): child locals are `${v}`-prefixed; every shared variable has both a parent `export` and a child `bind` entry; bash `${!name}` indirection resolves to a declared C# name.
- **Executable bit.** `git ls-files -s` should show `100755` for `.sh` files that `build.sh` runs. The repo uses `core.fileMode false` (exFAT), so modes don't get set automatically and a fresh clone on ext4 fails with "Permission denied".

### Unity 2019.4 / C#
- C# 7.3 only: no target-typed `?:` or `new()`, no switch expressions, no records, no `??=`, no nullable reference types, no default interface methods.
- **Hand-edited `.unity` / `.prefab` YAML**: fileIDs unique within the file, every `{fileID: N}` reference resolves, `m_Component` lists match the components' `m_GameObject`, script GUIDs match a `.meta` in the repo, and new `[SerializeField]` fields have their scene entry.
- Asmdef boundaries: everything under `Assets/` outside Plugins/Tests is the `OsnowaExample` assembly.
- Namespace collisions inside `namespace Osnowa.*` (prefer `global::Fcast...`).

### Game loop (`FcastMain`)
- **INPUT**: `Input.*` read only here, once per frame.
- **SIMULATE**: no `transform`, `Instantiate`, `Destroy`, `FindObjectOfType` or `Resources.Load`. Timers advance by `StepSeconds`; no `DateTime`, `Time.time` or frame counters. Logic that must happen once per press handles the multiple-steps-per-frame case.
- **PRESENT**: reads simulation state only and sets absolute values (`localEulerAngles`, `position`), not deltas (`Rotate`, `Translate`). It must not recalculate simulation logic.
- **Migrations**: when a feature moves into `FcastMain`, its old copy in `FcastGameLoop.cs` is removed, along with now-dead locals, interval checks and per-frame allocations. Otherwise both run. The "Migrated so far" note in `CLAUDE.md` is kept current.
- Null and lifetime safety: check before dereferencing entity views (`hasView`); entities can be destroyed while still in groups or queues.
- No per-frame allocations: no `new List`/`Dictionary`, LINQ, closures or string concatenation in `Update`.

### Style
- Imperative: statements, `if` and loops. Flag new helper classes, LINQ or lambdas in game logic.
- ALL-CAPS comment section headers kept for new logic sections (the user navigates them with emacs wik-mode).
- `.md` docs (README, CLAUDE.md) still match the code.

## 4. Report

Verify each finding before reporting it: reproduce it by regenerating, compiling or reading the relevant code. Drop anything you can't confirm, or label it "unverified".

Output a numbered list, most severe first. Each item has:
- **Severity** (high: breaks build, compile, scene or runtime; medium: wrong behavior or a convention that will bite; low: cleanup or docs).
- **Location**: `path:line`, or the `.sh` line for generated code.
- **What's wrong and the concrete failure** (inputs/state → result).
- **Suggested fix** (one line).

End with what was checked and passed in one or two lines (e.g. "generated output compiles; scene references resolve"). Don't edit files unless the user asks.
