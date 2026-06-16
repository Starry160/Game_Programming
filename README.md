# Game_Programming

## Game Title

**Knight Legend** - Unity product name: `Game_Programming`

## One-Sentence Game Idea

A 2D top-down pixel action game where you pick a class at a transformation pedestal, upgrade talents, fight through rooms with sword/staff/bow combat, and survive a two-phase final boss before claiming the victory trophy.

## Controls

| Action | Input |
|--------|-------|
| Move | `W` / `A` / `S` / `D` (or gamepad left stick) |
| Aim | Mouse position (or gamepad right stick) |
| Attack | Left mouse button |
| Interact (portals, pedestal) | `E` |
| Pause | `Esc` |

Input uses Unity's **Input System** (`InputAction` bindings created in code for movement/aim; mouse and `Esc`/`E` read directly via `Mouse.current` / `Keyboard.current`).

## Unity Version

- **Unity 2022.3 LTS** - `2022.3.62f3c1` (revision `1623fc0bbb97`)

## How to Run the Project

1. Install **Unity Hub** and add editor **2022.3.62f3c1** (or a compatible 2022.3 LTS).
2. In Unity Hub, click **Add** -> select the `Game_Programming` folder (this directory).
3. Open the project and wait for the first asset import to finish.
4. Open `Assets/Scenes/MainMenu.unity` to start from the menu flow.
5. Press **Play** in the Editor.

**Tip:** Click the **Game** view so it has focus before using keyboard/mouse controls.

## Game Flow

The run progresses through the scenes in this order:

`MainMenu` -> `StoryIntro` -> `InitialRoom` -> `TalentRoom` -> `Level_01` -> `Level_02` -> `Final Boss` -> `Final Room`

- The player picks a class/weapon at a transformation pedestal and upgrades talents before combat.
- Survival time is counted from the moment the player enters `Level_01` (via the `TalentRoom` portal).
- Defeating the final boss leads to `Final Room`, where touching the victory trophy ends the run.

## Core Features

| Feature | Description |
|---------|-------------|
| Class transformation | Step on a pedestal to choose a class; choice persists across scenes via `GlobalData`. |
| Weapons | Sword (melee arc), Staff (fireball), Bow (arrow) - selected with the class. |
| Talent upgrades | Talent room modifies player stats (max HP / shield) carried across the run. |
| Potions | Heal, Full Heal, Shield, and temporary Invincibility pickups. |
| Combat | Mouse-aimed attacks; fan-shaped melee damage and projectile firing. |
| Final boss | Two phases with melee, arm-launch, and laser attacks, plus an immune/guard mechanic. |
| Run stats | Tracks enemies defeated, potions collected, and survival time. |
| End screens | Separate **victory** and **death** result panels share one `GameOverPanel`. |
| Presentation | Cinematic camera intro, boss health bar, hit feedback, portal transitions. |

## Current Status

| Area | Status |
|------|--------|
| Player movement & mouse aim | Working |
| Class transformation & weapon switching | Working |
| Sword / staff / bow attacks | Working |
| Enemy AI (chaser, monster shooter) | Working |
| Two-phase final boss (melee / arm / laser) | Working |
| Potions (heal / shield / invincibility / full heal) | Working |
| Run stats (kills, potions, survival time) | Working |
| Victory & death result panels | Working |
| Scene flow + level portals | Working |

## Related Classroom Projects

The final submitted game in this repository is **Knight Legend**, located at the repository root. The following earlier classroom exercise projects are maintained as separate repositories:

- [2D Shooting](https://github.com/Starry160/2D-Shooting)
- [Interactive Solar System for Kids](https://github.com/Starry160/Interactive-Solar-System-for-Kids)

## Supporting Documents

- [Testing Evidence](TESTING.md)
- [Asset References](ASSET_REFERENCES.md)
- [AI Usage Statement](AI_USAGE_STATEMENT.md)

## Project Structure

```
Assets/
  Scenes/        MainMenu, StoryIntro, InitialRoom, TalentRoom,
                 Level_01, Level_02, Final Boss, Final Room
  Scripts/
    Core/        GlobalData, AutoDestroy
    Player/      PlayerController, PlayerAttack, PlayerFacing,
                 PlayerStats, WeaponManager, PlayerClassLoader,
                 CharacterTransformPedestal
    Enemies/     EnemyAI, EnemyHealth, MonsterAI, EnemyPushSeparation,
                 FinalBossController, FinalBossMeleeAttack,
                 FinalBossArmLauncher, FinalBossArmProjectile,
                 FinalBossLaserLauncher, FinalBossLaserBeam
    Environment/ RoomController, DoorController, LevelPortal,
                 TreasureChest, TrophyEndingTrigger
    Items/       PotionItem, FullHealPotionItem, ShieldPotionItem,
                 InvincibilityPotionItem, ChestDropItem, Projectile
    Managers/    MainMenuManager, StoryIntroManager,
                 AudioManager, RunStatsManager
    UI/          PauseMenuController, BossHealthUI,
                 GameOverPanel, UICharacterAnimator
    Camera/      CameraController
    Effects/     HitFeedback
  Animations/, Prefabs/, Art/, Audio/   project assets
```

## Key Scripts

- `Core/GlobalData.cs` - cross-scene run state (class, weapon, HP/shield, stat snapshot).
- `Managers/RunStatsManager.cs` - kills, potions, and survival-time tracking.
- `Player/PlayerController.cs` / `PlayerAttack.cs` - movement, aim, and weapon attacks.
- `Player/CharacterTransformPedestal.cs` - class/weapon selection.
- `Enemies/FinalBossController.cs` - two-phase boss state machine.
- `Environment/LevelPortal.cs` - scene transition portals.
- `Environment/TrophyEndingTrigger.cs` - victory ending in `Final Room`.
- `UI/GameOverPanel.cs` - shared victory/death result screen.

## Credits

| Role | Detail |
|------|--------|
| **Developer** | Starry160 - [Game_Programming](https://github.com/Starry160/Game_Programming) |
| **Engine** | Unity 2022.3 LTS |
| **UI text** | TextMesh Pro (Unity package) |

Art, audio, and prefabs under `Assets/Art/`, `Assets/Audio/`, and `Assets/Prefabs/` are documented learning assets with source and license notes recorded in `ASSET_REFERENCES.md`.

## License and Usage

For learning and coursework. Asset usage follows the license notes documented in `ASSET_REFERENCES.md`.
