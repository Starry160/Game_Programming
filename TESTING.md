# Knight Legend Test Document

## 1. Purpose

This document defines the manual testing plan for the main Unity project in the `Game_Programming` folder. It focuses on the final submitted game, **Knight Legend**, and does not include the archived classroom projects `2D shooting` or `Interactive Solar System for Kids Project`.

The goal is to verify that the main gameplay loop is playable, stable, and understandable from start to finish:

`MainMenu -> StoryIntro -> InitialRoom -> TalentRoom -> Level_01 -> Level_02 -> Final Boss -> Final Room`

## 2. Test Environment

| Item | Test Setup |
|------|------------|
| Engine | Unity 2022.3 LTS |
| Editor Version | 2022.3.62f3c1 or compatible 2022.3 LTS |
| Platform | Windows |
| Input | Keyboard and mouse |
| Start Scene | `Assets/Scenes/MainMenu.unity` |
| Main Project Scope | Root `Assets`, `Packages`, and `ProjectSettings` under `D:\Game_Programming` |

## 3. Controls Under Test

| Action | Input |
|--------|-------|
| Move | `W`, `A`, `S`, `D` |
| Aim | Mouse position |
| Attack | Left mouse button |
| Interact | `E` |
| Pause / Resume | `Esc` |

## 4. Test Scope

The test scope includes:

- Main menu and story intro flow
- Player movement, aiming, facing, and attack input
- Class transformation and weapon selection
- Sword, staff, and bow combat behavior
- Talent and stat persistence across scenes
- Room battle start, door locking, enemy clearing, and reward reveal
- Potions, chest drops, projectile damage, and hit feedback
- Portal interaction and scene transitions
- Pause menu and return-to-menu confirmation
- Final boss phase behavior, boss health UI, and defeat flow
- Victory trophy and result panel
- Death result panel and run-stat display

The test scope excludes:

- The `2D shooting` classroom project
- The `Interactive Solar System for Kids Project` classroom project
- Automated unit tests, because this project currently relies on manual Unity playtesting

## 5. Entry Criteria

Testing can begin when:

- The project opens in Unity without import-blocking errors.
- `MainMenu.unity` can be opened.
- All required scenes are included in Build Settings in the expected order.
- The Game view has focus before testing keyboard and mouse input.
- Console is cleared before each major test pass.

## 6. Exit Criteria

The build is considered ready for submission when:

- The full game can be completed from `MainMenu` to `Final Room`.
- No blocking errors appear in the Unity Console during the main path.
- Player combat, enemy combat, portals, rewards, pause menu, death, and victory are functional.
- Any remaining issues are minor, documented, and do not prevent grading of the main gameplay loop.

## 7. Test Cases

### TC-01 Main Menu Start Flow

**Priority:** High  
**Scene:** `MainMenu`

**Steps:**
1. Open `Assets/Scenes/MainMenu.unity`.
2. Press Play.
3. Click the Start button.

**Expected Result:**
- A new run starts.
- Previous run state is reset.
- The game loads the story intro scene.
- No Console errors occur.

### TC-02 Story Intro Progression

**Priority:** High  
**Scene:** `StoryIntro`

**Steps:**
1. Start from the main menu.
2. Advance through each story page.
3. Continue after the final story page.

**Expected Result:**
- Story text updates correctly.
- The final page advances to the first gameplay scene.
- Text is readable and does not overlap important UI.

### TC-03 Player Movement and Aim

**Priority:** High  
**Scene:** `InitialRoom`, `Level_01`, or `Level_02`

**Steps:**
1. Move with `W`, `A`, `S`, and `D`.
2. Move diagonally.
3. Aim left and right with the mouse.
4. Attack while aiming on both sides.

**Expected Result:**
- Player movement is responsive.
- The player does not rotate incorrectly or slide uncontrollably.
- The character faces the mouse while attacking.
- The character faces movement direction while not attacking.

### TC-04 Class Transformation Pedestal

**Priority:** High  
**Scene:** `InitialRoom`

**Steps:**
1. Walk into a transformation pedestal trigger.
2. Confirm that the interaction hint appears.
3. Press `E`.
4. Leave and re-enter the pedestal trigger.

**Expected Result:**
- Interaction only works while the player is in range.
- The selected class applies the correct animator and weapon index.
- The target portal is unlocked after transformation.
- No null reference errors appear.

### TC-05 Weapon Selection Persistence

**Priority:** High  
**Scene:** `InitialRoom` to `TalentRoom` to `Level_01`

**Steps:**
1. Select a class at the pedestal.
2. Enter the next scene through the portal.
3. Attack in the next scene.

**Expected Result:**
- The selected weapon persists after scene loading.
- Sword class uses melee attack.
- Staff class fires fireballs.
- Bow class fires arrows.

### TC-06 Sword Attack

**Priority:** High  
**Scene:** Combat room

**Steps:**
1. Select the sword class.
2. Approach an enemy.
3. Aim toward the enemy.
4. Click to attack.
5. Aim away from the enemy and attack again.

**Expected Result:**
- Sword damage applies only to enemies inside the attack range and forward attack cone.
- Sword visual swing plays.
- Enemy health decreases or enemy is defeated.
- Attacking away from the enemy should not incorrectly hit targets behind the player.

### TC-07 Staff Attack

**Priority:** High  
**Scene:** Combat room

**Steps:**
1. Select the staff class.
2. Aim at an enemy.
3. Click to attack.
4. Fire several shots at different angles.

**Expected Result:**
- Fireball spawns from the staff fire point.
- Fireball travels toward the mouse direction.
- Fireball damages valid enemy targets.
- Fireball is destroyed on enemy or environment impact.

### TC-08 Bow Attack

**Priority:** High  
**Scene:** Combat room

**Steps:**
1. Select the bow class.
2. Aim at an enemy.
3. Click to attack.
4. Observe bow recoil.

**Expected Result:**
- Arrow spawns from the bow fire point.
- Arrow travels toward the mouse direction.
- Arrow damages valid enemy targets.
- Bow recoil animation returns to the default position.

### TC-09 Room Battle Start and Door Locking

**Priority:** High  
**Scene:** `Level_01` or `Level_02`

**Steps:**
1. Enter a room trigger.
2. Try to leave the room during combat.
3. Defeat all enemies in the room.

**Expected Result:**
- Room battle starts when the player enters the room.
- Doors or gates lock during combat.
- Enemies activate and chase or attack the player.
- Doors unlock or disappear after all room enemies are cleared.

### TC-10 Treasure Chest and Rewards

**Priority:** Medium  
**Scene:** Cleared combat room

**Steps:**
1. Clear a room that has a treasure chest.
2. Open or trigger the chest.
3. Pick up dropped rewards.

**Expected Result:**
- Treasure appears only after the room is cleared.
- Rewards move out from the chest before becoming collectible.
- Pickup effects apply correctly.
- Potion count increases in run statistics.

### TC-11 Potion Effects

**Priority:** High  
**Scene:** Any scene with potion drops

**Steps:**
1. Pick up a max health potion.
2. Pick up a full heal potion.
3. Pick up a shield potion.
4. Pick up an invincibility potion.

**Expected Result:**
- Max health potion increases max health and heals the player.
- Full heal potion restores health to maximum.
- Shield potion increases and fills shield.
- Invincibility potion prevents damage for its duration and shows visual feedback.
- Popup text appears and fades correctly.

### TC-12 Enemy AI

**Priority:** High  
**Scene:** `Level_01` or `Level_02`

**Steps:**
1. Start a room battle.
2. Observe melee enemies.
3. Observe ranged monsters.
4. Move behind walls or obstacles.

**Expected Result:**
- Melee enemies chase, stop near the player, and attack on cooldown.
- Ranged monsters wander when idle.
- Ranged monsters reposition when line of sight is blocked.
- Enemy attacks damage the player only when the player is a valid target.

### TC-13 Player Damage, Shield, and Death

**Priority:** High  
**Scene:** Combat room

**Steps:**
1. Let enemies hit the player.
2. Observe shield and health changes.
3. Wait for shield regeneration.
4. Continue taking damage until health reaches zero.

**Expected Result:**
- Shield absorbs damage before health.
- Health UI and shield UI update correctly.
- Shield regeneration starts after the configured delay.
- Death disables player control and shows the result panel.

### TC-14 Pause Menu

**Priority:** Medium  
**Scene:** Any gameplay scene

**Steps:**
1. Press `Esc`.
2. Resume the game.
3. Press `Esc` again.
4. Choose return to main menu.
5. Cancel the confirmation.
6. Confirm return to main menu.

**Expected Result:**
- Pause panel opens and closes correctly.
- Time scale is set to `0` while paused and restored to `1` after resume.
- Confirmation panel works correctly.
- Returning to main menu resets run state.

### TC-15 Portal Interaction and Scene Loading

**Priority:** High  
**Scene:** Any scene with a portal

**Steps:**
1. Walk into a portal trigger.
2. Confirm the interaction hint appears.
3. Press `E`.
4. Observe the player pull-in animation.
5. Confirm the target scene loads.

**Expected Result:**
- Portal interaction only works when the player is in range and the portal is unlocked.
- Player physics is disabled during the pull-in animation.
- Target scene loads once.
- No yellow portal trace warnings are printed.
- The active scene is not reloaded by accident.

### TC-16 Final Boss Intro and Health UI

**Priority:** High  
**Scene:** `Final Boss`

**Steps:**
1. Enter the final boss room.
2. Start the boss battle.
3. Observe the boss health UI.

**Expected Result:**
- Boss battle starts when the room battle begins.
- Boss health UI appears at battle start.
- Boss health UI updates when the boss takes damage.
- Boss UI fades or hides after the boss room is cleared.

### TC-17 Final Boss Phase One

**Priority:** High  
**Scene:** `Final Boss`

**Steps:**
1. Fight the boss during phase one.
2. Move near and far from the boss.
3. Damage the boss until phase transition starts.

**Expected Result:**
- Boss moves during movement windows.
- Boss chooses valid phase-one attacks.
- Boss can take damage with post-hit invulnerability.
- Phase transition begins when phase-one health reaches zero.

### TC-18 Final Boss Phase Two

**Priority:** High  
**Scene:** `Final Boss`

**Steps:**
1. Complete phase-one transition.
2. Observe phase-two health refill.
3. Dodge arm and laser attacks.
4. Defeat the boss.

**Expected Result:**
- Boss health refills visibly for phase two.
- Boss uses phase-two attacks, including arm and laser behavior.
- Laser does not fire when blocked by room edge rules.
- Boss defeat animation plays.
- Boss collision is disabled after defeat.
- Room can clear after boss defeat.

### TC-19 Victory Trophy and Result Panel

**Priority:** High  
**Scene:** `Final Room`

**Steps:**
1. Enter the final room after defeating the boss.
2. Approach the trophy.
3. Trigger the victory ending.

**Expected Result:**
- Trophy ending can be triggered by the player.
- Run timer stops.
- Victory result panel appears.
- Kills, potions, and survival time are displayed.
- Main menu button returns to the main menu.

### TC-20 Full Regression Playthrough

**Priority:** High  
**Scene:** Full game

**Steps:**
1. Start from `MainMenu`.
2. Complete story intro.
3. Select a class.
4. Enter talent room.
5. Complete `Level_01`.
6. Complete `Level_02`.
7. Defeat the final boss.
8. Trigger the final trophy.

**Expected Result:**
- The complete game flow is playable without restarting the Editor.
- No blocking Console errors occur.
- Scene transitions happen in the expected order.
- Final result panel appears with correct run statistics.

## 8. Regression Checklist

Run this checklist after any gameplay, UI, or scene-flow change:

- Player can move, aim, attack, and interact.
- Selected class and weapon persist across scenes.
- Portals load the correct scenes.
- Room doors lock during combat and unlock after clear.
- Enemies activate only when expected.
- Projectiles damage the correct target side.
- Potions apply the correct effect.
- Pause menu does not remain active after returning to main menu.
- Death panel appears when health reaches zero.
- Victory panel appears after the trophy ending.
- Console has no blocking errors.

## 9. Known Risks and Watch Areas

| Risk | Why It Matters | Suggested Check |
|------|----------------|-----------------|
| Scene build order mismatch | Menu and story scripts load scenes by build index | Verify Build Settings before submission |
| Missing Inspector references | Many systems rely on assigned transforms, prefabs, and UI objects | Check Console after each scene starts |
| Portal target name mistakes | Scene loading uses string scene names | Verify every portal target scene exists in Build Settings |
| Boss room setup | Boss behavior depends on room events and boss UI binding | Test final boss from room entry, not only by opening the scene directly |
| Nested old projects | Old projects can confuse review or batch searches | Keep final submission focused on the root Unity project |

## 10. Defect Report Template

Use this format when recording a bug:

| Field | Description |
|-------|-------------|
| Defect ID | Example: BUG-001 |
| Title | Short summary of the issue |
| Scene | Scene where the issue occurred |
| Severity | Blocker / High / Medium / Low |
| Steps to Reproduce | Numbered steps |
| Expected Result | What should happen |
| Actual Result | What happened instead |
| Screenshot / Console Log | Evidence if available |
| Status | Open / Fixed / Verified |

## 11. Current Test Conclusion

The main project has a complete manual test path covering menu, story, class selection, combat, rewards, portals, final boss, death, and victory. The most important submission test is the full regression playthrough from `MainMenu` to `Final Room`, because it verifies the complete game loop that the teacher will most likely review.
