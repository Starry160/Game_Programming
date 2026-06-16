# Knight Legend Test Document

## 1. Purpose

This document defines the manual testing plan for the main Unity project in the `Game_Programming` folder. It focuses on the final submitted game, **Knight Legend**.

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

The test scope is supported by both full manual Unity playthroughs and lightweight automated EditMode tests.

## 5. Entry Criteria

Testing can begin when:

- The project opens in Unity with a clean import path.
- `MainMenu.unity` can be opened.
- All required scenes are included in Build Settings in the expected order.
- The Game view has focus before testing keyboard and mouse input.
- Console is cleared before each major test pass.

## 6. Exit Criteria

The build is considered ready for submission when:

- The full game can be completed from `MainMenu` to `Final Room`.
- The Unity Console stays clean during the main path.
- Player combat, enemy combat, portals, rewards, pause menu, death, and victory are functional.
- Quality notes are documented with supporting evidence, and the main gameplay loop is ready for grading.

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
- Text is readable and keeps clear spacing from important UI.

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
- The player keeps correct rotation and stable movement.
- The character faces the mouse while attacking.
- The character faces movement direction during movement.

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
- Attacking direction controls the intended target area.

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
- The active scene stays stable during portal interaction.

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
- Laser firing respects room edge rules.
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
- The Console stays clean during the full route.
- Scene transitions happen in the expected order.
- Final result panel appears with correct run statistics.

## 8. Manual Test Execution Record

This table records the final manual test pass for the main project. The evidence commits are mapped from Git history by matching commit messages and changed feature areas to the gameplay systems validated during playtesting.

| Test ID | Test Date | Result | Validation Focus | Evidence Commit |
|---------|-----------|--------|------------------|-----------------|
| TC-01 | 2026-06-10 | Pass | Main menu flow, presentation polish, and submitted entry point. | `0784360` main menu optimization; `a0af540` README update; `9020cd4` polish |
| TC-02 | 2026-04-19 | Pass | Story introduction and player context before the first gameplay scene. | `a681163` story intro; `042e1e4` main menu UI and scene management |
| TC-03 | 2026-04-24 | Pass after refinement | Character running direction, attack direction, movement, and aiming. | `e25b523` character direction refinement |
| TC-04 | 2026-04-21 | Pass | Class selection prompt and reliable portal unlock after choosing a hero. | `f837cc5` class pedestal and portal system; `c324928` hero selection prompt |
| TC-05 | 2026-04-21 | Pass after refinement | Character/class state persistence across scene transitions. | `6f14ab9` model consistency; `059b620` dynamic level portal transition |
| TC-06 | 2026-04-25 | Pass | Sword combat action, melee damage, and hit feedback. | `5690445` sword attack and damage; `64e811d` sword trail effect |
| TC-07 | 2026-04-25 | Pass | Mage projectile behavior and fireball impact feedback. | `d308810` mage attack and fireball explosion |
| TC-08 | 2026-04-25 | Pass | Archer bow animation and arrow projectile behavior. | `c41884e` archer attack and bow/arrow effect |
| TC-09 | 2026-05-28 | Pass after refinement | Exit dungeon doors and reliable combat-room progression. | `985f7aa` exit door and room-flow update |
| TC-10 | 2026-05-25 | Pass | Treasure reward reveal after room clear and pickup feedback. | `552732d` treasure chest; `a6e235c` treasure reveal logic; `eab111d` chest drop update |
| TC-11 | 2026-05-26 | Pass | Heal, shield, invincibility, and full-heal potion behavior. | `14e180c` potion system and effects; `04d3b63` invincibility potion prefab tuning |
| TC-12 | 2026-05-30 | Pass after refinement | Enemy chasing, attacking, ranged monster behavior, and enemy spacing. | `7a4e3f8` enemy chasing; `da88ceb` enemy pathfinding and attacks; `adbbb32` enemy spacing optimization; `43cd731` monster enemy logic |
| TC-13 | 2026-05-30 | Pass after refinement | Run statistics for defeated enemies and collected potions. | `0017d22` run-stat consistency update |
| TC-14 | 2026-05-26 | Pass | Pause flow, death-result UI, and return-to-menu handling. | `26e194f` pause support; `e809376` game over panel |
| TC-15 | 2026-06-05 | Pass after refinement | Scene portals and target flow between levels and boss scene. | `f08e0a5` level 1 to level 2 transition; `11e636e` level 2 to final boss transition; `f62a9d8` portal creation |
| TC-16 | 2026-06-04 | Pass | Final boss scene, animations, and visible boss health UI. | `dfcf047` final boss scene and animations; `fff5e98` boss health display |
| TC-17 | 2026-06-05 | Pass after refinement | Boss phase-one behavior, death conditions, and room binding. | `e16a959` boss first-stage improvement; `46c749f` boss phase and room binding refinement |
| TC-18 | 2026-06-08 | Pass after refinement | Boss phase transition, laser range, damage range, and laser focusing point. | `92874cd` phase transition; `87c74bf` laser range tuning; `0a1b36c` laser damage range tuning; `b346d10` laser focus tuning; `366a5c0` laser emission improvement |
| TC-19 | 2026-06-10 | Pass | Victory result UI after boss completion and final room interaction. | `2ce7338` level completion UI; `5185239` scene naming update |
| TC-20 | 2026-06-11 | Pass | Full game flow, naming polish, and submission-readiness checks. | `5185239` scene naming update; `9020cd4` polish |
| TC-20-Knight | 2026-06-12 | Pass | Complete manual playthrough from `MainMenu` to the final trophy using the Knight class. | Manual verification record |
| TC-20-Mage | 2026-06-12 | Pass | Complete manual playthrough from `MainMenu` to the final trophy using the Mage class. | Manual verification record |
| TC-20-Archer | 2026-06-12 | Pass | Complete manual playthrough from `MainMenu` to the final trophy using the Archer class. | Manual verification record |

## 9. Automated EditMode Tests

A lightweight EditMode test suite has been added under `Assets/Tests/EditMode/Editor/`. These tests complement the manual full-playthrough tests and provide automated evidence for stable core logic that can be checked without loading the full scene sequence.

| Automated Test | Purpose | Coverage Type |
|----------------|---------|---------------|
| `GlobalData_ResetRunState_ClearsClassStatsAndRunResults` | Confirms returning to menu clears selected class, weapon, persisted stats, and run results. | Cross-scene state |
| `RunStatsManager_AddKillAndPotion_SyncsGlobalSnapshot` | Confirms kills, potion pickups, and survival time sync into `GlobalData`. | Run statistics |
| `RunStatsManager_ResetForMenu_ClearsCountersAndGlobalSnapshot` | Confirms the run-stat reset path clears local and persisted counters. | Run reset |
| `PlayerStats_IncreaseMaxHealthAndHeal_UpdatesCurrentAndPersistedHealth` | Confirms max-health potion style logic updates current health and persisted max health. | Player stats |
| `PlayerStats_IncreaseMaxShieldAndFill_UpdatesCurrentAndPersistedShield` | Confirms shield potion style logic updates current shield and persisted max shield. | Player stats |

These are intentionally EditMode tests because the full game flow depends on scene objects, Inspector references, physics timing, UI layout, and animation state. Those areas remain covered by the manual regression playthrough.

### Automated Test Run Result

| Test Run Date | Tool | Mode | Result | Notes |
|---------------|------|------|--------|-------|
| 2026-06-11 | Unity Test Runner | EditMode | Pass - 5 passed, 0 unsuccessful, 0 skipped | Verified in the Unity Test Runner after adding the lightweight EditMode test suite. |
| 2026-06-16 | Unity Test Runner | EditMode | Pass - 5 passed, 0 failed, 0 skipped | Re-ran the full EditMode suite in the Unity Test Runner after the final cleanup pass. |

## 10. Regression Checklist

Run this checklist after any gameplay, UI, or scene-flow change:

- Player can move, aim, attack, and interact.
- Selected class and weapon persist across scenes.
- Portals load the correct scenes.
- Room doors lock during combat and unlock after clear.
- Enemies activate only when expected.
- Projectiles damage the correct target side.
- Potions apply the correct effect.
- Pause menu state resets cleanly after returning to main menu.
- Death panel appears when health reaches zero.
- Victory panel appears after the trophy ending.
- Console stays clean during the full route.

## 11. Submission Readiness and Quality Checks

### Completed Repository Quality Items

| Item | Resolution | Evidence |
|------|------------|----------|
| Repository scope cleanup | The earlier `2D Shooting` and `Interactive Solar System for Kids` Unity projects were moved into standalone GitHub repositories. The main repository now stays focused on the final submitted game, `Knight Legend`. | Commit `30f932b`; README "Related Classroom Projects" section |

### Final Verification Items

| Check Area | Quality Goal | Suggested Check |
|------------|--------------|-----------------|
| Scene build order | Menu and story scripts load scenes in the intended order | Verify Build Settings before submission |
| Inspector reference validation | Scene objects, prefabs, transforms, and UI bindings are assigned correctly | Check Console after each scene starts |
| Portal target names | Scene-loading strings match the final scene names | Verify every portal target scene exists in Build Settings |
| Boss room setup | Boss behavior, room events, and boss UI binding work together from normal gameplay entry | Test final boss from room entry as part of the full playthrough |

## 12. Quality Observation Template

Use this format when recording a quality observation:

| Field | Description |
|-------|-------------|
| Observation ID | Example: QA-001 |
| Title | Short summary of the observation |
| Scene | Scene where the observation was made |
| Priority | High / Medium / Low |
| Verification Steps | Numbered steps |
| Expected Result | Intended result |
| Observed Result | What happened during the check |
| Screenshot / Console Log | Evidence if available |
| Status | Recorded / Improved / Verified |

## 13. Current Test Conclusion

The main project has a complete manual test path covering menu, story, class selection, combat, rewards, portals, final boss, death, and victory. The most important submission test is the full regression playthrough from `MainMenu` to `Final Room`, because it verifies the complete game loop that the teacher will most likely review. On 2026-06-12, this full playthrough was manually completed once with each playable class: Knight, Mage, and Archer.

The automated EditMode tests add a small safety net around cross-scene state, run statistics, and player stat persistence. Together, the manual and automated tests show both full-game validation and targeted code-level validation.
