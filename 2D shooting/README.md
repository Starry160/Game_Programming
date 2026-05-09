# 2D Shooting

A 2D top-down shooting project built with **Unity 2022.3 LTS**.  
The player moves through each level, aims, shoots enemies, and scores points to complete objectives.

## Highlights

- Top-down movement with mouse aiming and shooting
- Multiple enemy behavior styles (chasing, scrolling/patrol, stationary shooting, etc.)
- Configurable enemy spawners (supports both capped and infinite spawning)
- Basic health and damage system (including invincibility frames)
- UI page flow (main menu, pause, victory, game over)
- Score and high score persistence via `PlayerPrefs`

## Environment

- Unity Editor: `2022.3.62f3c1`
- Platform: Windows (can be expanded to other Unity-supported platforms)

## Quick Start

1. Open the project folder `2D shooting` with Unity Hub.
2. Wait for asset import to complete.
3. Open and run any scene in `Assets/_Scenes/`:
   - `MainMenu.unity`
   - `Level1.unity`
   - `Level2.unity`
   - `Level3.unity`

## Controls (Default)

- Move: `W / A / S / D`
- Aim: mouse position
- Fire: left mouse button
- Pause: `Esc`

> Note: Input is configured with `InputAction` (Unity's new Input System) and serialized directly in scene objects.

## Gameplay Goals

- Defeat enemies to gain score
- Reach the configured kill target to trigger level clear
- Lose all health/lives to trigger game over

## Core Script Structure

`Assets/Scripts/`

- `Player/Controller.cs`: player movement and facing logic
- `ShootingProjectiles/ShootingController.cs`: firing, fire rate, spread
- `ShootingProjectiles/Projectile.cs`: projectile movement
- `Enemies/Enemy.cs`: enemy behavior (movement + shooting)
- `Enemies/EnemySpawner.cs`: timed enemy spawning
- `Health&Damage/Health.cs`: health, taking damage, death, respawn
- `Health&Damage/Damage.cs`: collision/trigger damage handling
- `Utility/GameManager.cs`: score, win/lose flow, scene navigation
- `UI/UIManager.cs`: UI page management and pause handling

## Assets

- Scenes: `Assets/_Scenes/`
- Prefabs: `Assets/Prefabs/`
- Art: `Assets/Art/`
- Audio: `Assets/Audio/`

## Possible Extensions

- Add object pooling (to reduce frequent `Instantiate/Destroy` cost)
- Add a weapon system using ScriptableObject data
- Add wave progression and pacing systems
- Add meta progression and save/load systems

## License and Usage

This project is intended for learning and practice in 2D shooter development.  
If you plan to publish it, please verify the licenses of all third-party assets (art, audio, plugins, etc.).

