# Class Notes - 2026-05-19

## Project Snapshot

- **Project:** Knight Legend / Game_Programming
- **Type:** 2D top-down pixel fighting game
- **GitHub status:** Repository already started at `https://github.com/Starry160/Game_Programming`
- **Smallest playable version:** Player attacks enemies, gains coins, applies one upgrade, and wins.
- **Biggest risk at this stage:** The core loop was not finished yet: enemy takes damage -> enemy dies -> reward drops -> player upgrades -> player wins.

## One Visible Task Before Next Session

Make one enemy die from player attacks and show a clear result in-game.

## What My Game Is

A 2D top-down pixel action game, roguelite-style, where the player moves through rooms, fights enemies, collects rewards, and gets stronger until clearing the run or winning the level.

## What the Player Does

The player moves in top-down view, aims, and attacks with one of three weapons: sword melee, staff fireball, or bow arrow. The player takes damage through health and shield, avoids enemies, earns rewards, and uses upgrades to survive and progress.

## Vertical Slice Plan at This Stage

One playable room or level:

- Spawn player and enemies.
- Let the player damage and kill at least one enemy.
- Drop coins or rewards after enemy death.
- Apply one simple upgrade or stat change.
- Add one win condition, such as clearing all enemies or reaching an exit.
- Include basic HUD and weapon-specific attack feedback.

## Smallest Playable Version

Player attacks -> enemy dies -> player gains coins -> one upgrade applies -> player can win.

No extra biomes, full meta-progression, or many levels were planned for this stage. The goal was to prove the loop: fight -> reward -> get stronger -> win.

## Feature Sorting

### Must-Have

- Top-down player movement, aiming, and attack.
- At least one weapon working end-to-end.
- Enemies that can be damaged and die.
- Player health, with shield if kept.
- Death as fail or restart condition.
- Coins, score, or reward on enemy kill.
- One upgrade that changes gameplay, such as extra damage, max health, or faster attack.
- One win condition, such as clear room, beat boss, or reach exit.
- One playable level or room with spawn points and basic UI.

### Should-Have

- All three weapons: sword, staff, and bow.
- Distinct weapon feel and sound effects.
- Simple enemy variety, such as chaser and shooter.
- Upgrade UI, shop, talent room, or choose-one-of-three upgrade flow.
- Room transition or door to next area.
- Game over and restart or return to menu.
- Basic feedback such as hit flash, death effect, and pickup sound.

### Could-Have

- Multiple levels or procedural rooms.
- More upgrades or weapon synergies.
- Minimap, pause menu polish, or high score save.
- Object pooling for bullets and enemies.
- Talent or meta progression across runs.
- Development tooling support, as long as it does not affect the player-facing game.

### Cut First

- Large content packs with many enemy types, biomes, or story branches.
- Full roguelite meta progression with unlock tree and many characters.
- Multiplayer or networking.
- Advanced AI before one enemy type works.
- Fancy UI animations, localization, and achievements.
- Tutorial project features merged into the main game before the vertical slice works.
- Perfect pixel art pass or full audio suite before the core loop plays once.

## Peer Feedback

- **Feedback:** Try to make the game more attractive and engaging.
- **Planned response:** Improve UI polish and add special effects to strengthen the player experience.
