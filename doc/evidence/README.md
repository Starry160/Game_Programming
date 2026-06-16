# Visual Evidence

This folder contains the final verification screenshots linked from `TESTING.md`. Each file records a specific submission-readiness check for **Knight Legend**, including automated tests, scene build order, clean Console state, gameplay flow, boss behavior, result screens, and pause handling.

| Filename | Evidence | Project Detail Verified |
|----------|----------|-------------------------|
| `01-test-runner-editmode-pass.png` | EditMode test suite pass result | 11 automated tests passed for run state, run statistics, player stats, class setup, and weapon switching. |
| `02-build-settings-scene-order.png` | Build Settings scene order | `MainMenu`, `StoryIntro`, `InitialRoom`, `TalentRoom`, `Level_01`, `Level_02`, `Final Boss`, and `Final Room` are included in the intended route order. |
| `03-console-clean-after-play.png` | Clean Unity Console after play | Final play session reached the result flow with 0 errors and 0 warnings. |
| `04-main-menu.png` | Main menu | The submitted entry point is the finished `Knight Legend` menu, not a test scene. |
| `05-class-selection-talent-room.png` | Class selection and talent room | Selected class/weapon and health/shield UI persist into the talent room. |
| `06-level01-combat.png` | Level_01 combat | Player combat, enemy encounter state, room layout, health/shield state, and pause UI are visible during gameplay. |
| `07-treasure-potion-reward.png` | Treasure and potion reward | Reward flow after combat includes chest reward and potion pickup state. |
| `08-final-boss-phase1.png` | Final boss phase one | Boss encounter starts with health UI and phase-one combat behavior visible. |
| `09-final-boss-phase2-laser.png` | Final boss phase two / laser | Phase-two laser behavior is visible during the final boss encounter. |
| `10-victory-result-stats.png` | Victory result statistics | Final trophy flow displays victory text, kills, potions, survival time, and menu return button. |
| `11-death-result-stats.png` | Death result statistics | Player defeat displays battle result statistics instead of ending silently. |
| `12-pause-menu-confirmation.png` | Pause confirmation | Return-to-menu confirmation protects the player from accidentally losing current progress. |
