## Game Title

**Knight Legend** (Unity product name: `2D game`)

## One-Sentence Game Idea

A 2D top-down shooter where you move, aim with the mouse, defeat waves of enemies, earn score, and clear levels before running out of health.

## Controls

| Action | Input |
|--------|--------|
| Move | `W` / `A` / `S` / `D` |
| Aim | Mouse position |
| Fire | Left mouse button |
| Pause | `Esc` |

Input uses Unity's **Input System** (`InputAction` bindings configured per scene on the player and UI manager).

## Unity Version

- **Unity 2022.3 LTS** — `2022.3.62f3c1` (revision `1623fc0bbb97`)

## How to Run the Project

1. Install **Unity Hub** and add editor **2022.3.62f3c1** (or compatible 2022.3 LTS).
2. In Unity Hub, click **Add** → select the folder `2D shooting` (this directory).
3. Open the project and wait for the first import to finish.
4. Open a scene under `Assets/_Scenes/`:
   - `MainMenu.unity` — start here for menu flow
   - `Level1.unity`, `Level2.unity`, `Level3.unity` — playable levels
5. Press **Play** in the Editor.

**Tip:** Click the **Game** view so it has focus before using keyboard/mouse controls.

## Current Status

| Area | Status |
|------|--------|
| Player movement & mouse aim | Working |
| Shooting (projectiles, fire rate, spread) | Working |
| Enemy AI (follow, scroll, shoot modes) | Working |
| Enemy spawners (timed, capped / infinite) | Working |
| Health & damage (teams, shield-first style in template) | Working |
| Score, high score (`PlayerPrefs`) | Working |
| UI (main menu, pause, game over, victory pages) | Working |
| Three playable levels + main menu | In project |
| Object pooling | Not implemented |
| Roguelite meta (coins, upgrades, run persistence) | Not in this subproject |

This folder is a **self-contained Unity project** for a classic top-down shooting template. It is separate from the main `Game_Programming` roguelite prototype (talent room, weapons, global HP UI) in the parent repo.

## Planned Features

- Object pooling for projectiles, enemies, and hit effects
- Weapon / enemy data via `ScriptableObject` instead of hardcoded stats
- Clearer win conditions and level pacing per scene
- Optional gamepad aim/fire polish
- Performance pass (fewer `Instantiate`/`Destroy` spikes in heavy fights)

## Credits

| Role | Detail |
|------|--------|
| **Developer** | Starry160 — [Game_Programming](https://github.com/Starry160/Game_Programming) |
| **Organization** | DIICSU_2D (per Unity `ProjectSettings`) |
| **Engine** | Unity 2022.3 LTS |
| **UI text** | TextMesh Pro (Unity package) |
| **Fonts** | Liberation Sans (OFL); Manaspc UI font — see `Assets/Art/UI Elements/Fonts/manaspc/license.txt` |
| **Other** | Developer Console (imported asset package under `Assets/Imported Custom Asset Packages/`) |

Art, audio, and prefabs in `Assets/Art/`, `Assets/Audio/`, and `Assets/Prefabs/` are project learning assets. Verify third-party licenses before any public release.

## Development Log

| Date | Notes |
|------|--------|
| 2026-05-09 | Initial README: project overview, controls, scene list, core scripts. |
| 2026-05-09 | Documented Input System bindings (WASD, mouse aim/fire, Esc pause). |
| 2026-05-09 | Listed gameplay loop: score, enemy defeat count, game over / level clear via `GameManager` + `UIManager`. |
| 2026-05-21 | README expanded: title, status table, planned features, credits, dev log sections. |

---

## Quick Reference

### Scenes

- `Assets/_Scenes/MainMenu.unity`
- `Assets/_Scenes/Level1.unity`
- `Assets/_Scenes/Level2.unity`
- `Assets/_Scenes/Level3.unity`

### Core Scripts (`Assets/Scripts/`)

- `Player/Controller.cs` — movement and facing
- `ShootingProjectiles/ShootingController.cs`, `Projectile.cs` — firing
- `Enemies/Enemy.cs`, `EnemySpawner.cs` — enemy behavior and spawning
- `Health&Damage/Health.cs`, `Damage.cs` — damage and lives
- `Utility/GameManager.cs` — score, win/lose, scene flow
- `UI/UIManager.cs` — UI pages and pause

### License and Usage

For learning and coursework. Do not redistribute third-party assets without checking their licenses.
