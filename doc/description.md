# Knight Legend - Game Concept and Design Overview

## 1. Game Identity

**Knight Legend** is a 2D top-down pixel action game built in Unity. The player controls a hero who chooses a class at a transformation pedestal, upgrades basic survival talents, fights through dungeon rooms, defeats enemies with class-specific weapons, and survives a two-phase final boss before reaching the final trophy.

The project is designed as a vertical slice rather than a large full game. Its goal is to provide one complete and playable experience from menu to victory, showing the main systems that would support a larger dungeon action game.

## 2. Core Game Idea

The player explores a short dungeon sequence made of connected scenes and combat rooms. Before entering the main levels, the player chooses a class and weapon style:

- **Sword / Knight:** close-range melee attack with a forward arc.
- **Staff / Mage:** ranged fireball attack aimed at the mouse.
- **Bow / Archer:** ranged arrow attack with directional projectile firing.

This class choice persists across scenes, so the player keeps the selected play style throughout the run. The main challenge is to survive room battles, collect rewards, manage health and shield, and defeat the final boss.

## 3. Intended Player Experience

The intended experience is a compact dungeon adventure with clear progression:

1. Start from the main menu and story introduction.
2. Enter the initial room and choose a class.
3. Visit the talent room to improve survival stats.
4. Fight through Level_01 and Level_02.
5. Face the final boss in a dedicated arena.
6. Reach the final room and trigger the victory result screen.

The player should always understand the current goal: choose a class, enter a room, clear enemies, collect rewards, move through the portal, and continue toward the boss.

## 4. Core Gameplay Loop

The game loop is:

1. **Prepare:** choose a class and improve stats.
2. **Enter room:** trigger combat and lock the room.
3. **Fight:** use weapon attacks to defeat melee and ranged enemies.
4. **Reward:** open treasure or collect potions.
5. **Progress:** use portals to move to the next scene.
6. **Boss:** defeat the two-phase final boss.
7. **Result:** view victory or death statistics.

This loop supports the assessment requirement for a coherent vertical slice: the project is a connected playable path with mechanics that build toward a complete run.

## 5. Main Game Systems

### 5.1 Player Movement and Input

The player moves with WASD and aims with the mouse. Movement uses Rigidbody2D physics. The project uses Unity's Input System for movement and aim actions, while interaction, pause, and mouse attack input are read directly from keyboard and mouse devices.

### 5.2 Class Transformation and Weapon Selection

The class pedestal lets the player switch animator controller and weapon index. The selected class is stored in `GlobalData`, allowing the same class and weapon to be restored after scene transitions.

### 5.3 Combat

Combat is based on mouse direction and weapon type:

- Sword attacks use a short-range sector check.
- Staff attacks instantiate a fireball projectile.
- Bow attacks instantiate an arrow projectile.

Enemies include melee chasers and ranged monsters. Combat feedback includes hit reactions, audio, animations, health/shield UI, and result statistics.

### 5.4 Room Flow

Rooms use a trigger-based battle system. When the player enters a combat room, doors lock, enemies activate, and the room only clears when all assigned enemies are defeated. After clearing a room, doors open or fade away and rewards can appear.

### 5.5 Items and Rewards

The project includes potions and chest rewards. Potions can improve max health, restore health, increase shield, or grant temporary invincibility. These systems make the run more forgiving and give the player a reason to engage with rewards after combat.

### 5.6 Final Boss

The final boss is the most complex encounter in the project. It has:

- Two health phases.
- Movement windows between attacks.
- Melee attacks.
- Arm projectile attacks.
- Laser attacks.
- Temporary immune/guard behavior.
- A visible health UI and defeat flow.

This boss acts as the final test of the player's chosen class, positioning, and survival management.

### 5.7 Result Screens and Run Statistics

The game tracks enemies defeated, potions collected, and survival time. The same result panel supports both death and victory outcomes, giving the run a clear ending instead of simply stopping after the boss.

## 6. Tools and Resources

- **Engine:** Unity 2022.3 LTS.
- **Programming language:** C#.
- **Main Unity systems:** 2D physics, Animator, Tilemap, TextMesh Pro, UI, scene management, and the Input System.
- **Asset style:** pixel art dungeon assets, pixel UI fonts, music, sound effects, and boss sprites.

External asset sources and license notes are recorded separately in `ASSET_REFERENCES.md`.

## 7. Scope Decisions

The project intentionally focuses on a short but complete game path. It delivers a controlled vertical slice with a clear beginning, middle, boss fight, and ending.

## 8. Legal, Ethical, Accessibility, and Security Considerations

The project uses a mixture of free external assets, Unity packages, self-generated sound effects, and AI-generated icons. Asset references and license notes are documented to support responsible use.

Accessibility considerations include simple controls, readable UI text, visible health/shield bars, and clear room progression. The design also provides clear opportunities for optional enhancements such as remappable controls, difficulty options, colorblind-friendly UI choices, and stronger tutorial prompts.

The security profile is simple because the game is local, uses offline gameplay, and has no user-data collection or network features.

## 9. Submission Scope

Knight Legend is submitted as a complete coursework-scale vertical slice. The final build focuses on the playable dungeon route from the main menu through class selection, combat rooms, treasure rewards, level transitions, the multi-phase final boss, and the victory result screen.

The project evidence includes manual regression testing, lightweight automated EditMode tests, documented asset references, and a cleaned repository structure focused on the final submitted game.

## 10. Design Summary

Knight Legend is a realistic coursework-scale game project. It demonstrates player control, class-based combat, enemy behavior, room progression, rewards, UI feedback, scene transitions, and a multi-phase final boss. The final design is focused on delivering a complete playable dungeon action experience with a clear beginning, progression path, boss climax, and victory ending.
