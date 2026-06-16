# Game Development Document

## 1. Project Overview

- Project Name: Game_Programming (early 2D top-down pixel-style prototype)
- Engine Version: Unity 2022.3 LTS
- Prototype Goal: Build a playable baseline loop for the future dungeon game, including map layout, player movement, camera follow, menu entry, story/background context, scene portals, and a multi-class transformation pedestal system.

This document keeps the early development stage record. Later final-submission evidence is recorded separately in `README.md` and `TESTING.md`.

## 2. Completed Features with Dates

> Date format: `YYYY-MM-DD`  
> Dates below are aligned with the repository commit timeline.

### 2.1 Game Prototype Drawing

- **Date:** 2026-04-16
- **Evidence (Commit):** `28aeabd - Create prototype.jpg`; `55170c4 - Initial commit`
- Built the first gameplay concept sketch and prototype direction.
- Established the early top-down dungeon idea, including character position, room layout, and basic interaction points.

### 2.2 Game Description Expansion

- **Date:** 2026-04-16
- **Evidence (Commit):** `7de213e - Create description.md`
- Added the first written game description and world setting.
- Clarified that the player would move through dungeon spaces, interact with objects, and progress toward a larger adventure structure.

### 2.3 Asset Selection

- **Date:** 2026-04-16
- **Evidence (Commit):** `28aeabd - Create prototype.jpg`; `af40e6d - Update README.md`
- Selected early pixel-art assets for the character, dungeon environment, and interactive objects.
- Checked that the chosen assets matched the readable 2D top-down style planned for the prototype.

### 2.4 Test Map Setup and Test Character Selection

- **Date:** 2026-04-17
- **Evidence (Commit):** `2a4a863 - feat(player): Reconstructed the pure code-driven mobile system and implemented the 2D frame animation for the main character.`; `033dd30 - Keep the camera always following the character`
- Created a test map for movement, collision, and camera-follow checks.
- Configured a first playable character to verify movement feel and animation playback.
- Added camera follow so the prototype could be explored as a playable space instead of a static scene.

### 2.5 Game Menu Setup and Background Content Addition

- **Date:** 2026-04-19
- **Evidence (Commit):** `042e1e4 - feat: implement main menu UI and scene management`; `a681163 - add the storyIntro`
- Implemented the first main menu and scene entry flow.
- Added story/background content to give the prototype a clearer game identity before gameplay starts.

### 2.6 Scene Portal and Multi-Class Transformation Pedestal System

- **Date:** 2026-04-20
- **Evidence (Commit):** `f837cc5 - Feature: New scene teleportation portal and multi-class profession change altar system have been added.`; `7fe24a5 - add door-open animation`
- Implemented the first scene portal feature for moving between prototype areas.
- Implemented the early multi-class transformation pedestal interaction.
- Enabled player presentation changes after interacting with the pedestal, giving the prototype its first class-selection mechanic.

## 3. Early System Design Notes

### 3.1 Scene Portal System

- **Trigger Condition:** The player enters a portal trigger area and activates the interaction.
- **Design Purpose:** Split the prototype into multiple functional areas and test whether scene-to-scene flow could support later level progression.
- **Prototype Result:** The portal system proved that the project could move beyond one test map and support a connected route.

### 3.2 Multi-Class Transformation Pedestal System

- **Trigger Condition:** The player enters the pedestal collider area and can interact.
- **Interaction Key:** `E` key is used for interaction.
- **Prototype Result:** The player's class presentation can change after interaction, making the pedestal a clear foundation for later class and weapon selection.
- **Script Location in Current Repository:** `Assets/Scripts/Player/CharacterTransformPedestal.cs`
- **Implemented Points:**
  - Uses trigger enter/exit checks to know whether the player is in range.
  - Uses player identification before allowing transformation.
  - Gives visible feedback by changing the player's class presentation.

## 4. Early File and Script Notes

- `Assets/Scripts/Player/CharacterTransformPedestal.cs`
  - Handles the prototype class-transformation interaction.
- `Assets/Scripts/Environment/LevelPortal.cs`
  - Handles the prototype scene-transition interaction.
- `doc/description.md`
  - Stores the early gameplay concept, player goal, and style direction.
- `README.md`
  - Provides the project entry information and later overall project summary.

## 5. Development Process Record

1. Confirmed the early dungeon-game concept and visual direction.
2. Created a first map and movement test scene.
3. Added player movement, animation, and camera follow.
4. Added menu and story entry content.
5. Implemented the first interactive systems: portal and transformation pedestal.
6. Performed prototype playability checks before expanding into combat, rewards, and boss systems later.

## 6. Prototype Quality Notes

| Area | Observation | Prototype Resolution |
|------|-------------|----------------------|
| Player movement | The player needed responsive movement before any combat or room design could be evaluated. | Code-driven movement and frame animation were added, then checked in the test map. |
| Camera follow | A static camera made the test map feel disconnected from player movement. | Camera follow was added so the player stayed visible while exploring. |
| Menu entry | The project needed a clearer start point than opening a test scene directly. | A basic main menu and scene-management flow were implemented. |
| Story context | The early prototype lacked player motivation and world framing. | Story intro content was added to connect the menu to the dungeon concept. |
| Portal interaction | Scene changes needed to be tested before building larger level flow. | The portal system was added and used as the foundation for later route progression. |
| Class transformation | The original prototype needed a stronger player-choice mechanic. | The pedestal interaction was added to test class presentation switching. |

## 7. Milestone Summary

- **M1 (Done):** Playable prototype with map, player movement, animation, and camera follow.
- **M2 (Done):** Menu and story intro added to provide a complete entry point.
- **M3 (Done):** Portal and class-transformation pedestal added as the first interaction systems.
