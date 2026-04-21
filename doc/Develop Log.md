# Game Development Document

## 1. Project Overview

- Project Name: Game_Programming (2D top-down pixel-style prototype)
- Engine Version: Unity 2022.3 LTS
- Core Goal: Build a playable baseline loop from prototype to functional demo, including map, character, menu, world background, scene portals, and a multi-class transformation pedestal system.

## 2. Completed Features with Dates

> Date format: `YYYY-MM-DD`  
> Dates below are aligned with the repository commit timeline.

### 2.1 Game Prototype Drawing

- **Date:** 2026-04-16
- **Evidence (Commit):** `28aeabd - Create prototype.jpg`; `55170c4 - Initial commit`
- Built the core gameplay prototype (character movement, basic interaction, object placement).
- Completed the initial scene visual layout for gameplay pace and interaction flow validation.

### 2.2 Game Description Expansion

- **Date:** 2026-04-16
- **Evidence (Commit):** `7de213e - Create description.md`
- Added core world setting and gameplay description, clarifying player goals and progression direction.
- Recorded this information in project description documents for future iterations and presentations.

### 2.3 Asset Selection

- **Date:** 2026-04-16
- **Evidence (Commit):** `28aeabd - Create prototype.jpg`; `af40e6d - Update README.md`
- Selected base assets for character, environment, and interactable objects.
- Kept asset style consistent with the project theme to ensure readability and visual identity in prototype stage.

### 2.4 Test Map Setup and Test Character Selection

- **Date:** 2026-04-17
- **Evidence (Commit):** `2a4a863 - feat(player): Reconstructed the pure code-driven mobile system and implemented the 2D frame animation for the main character.`; `033dd30 - Keep the camera always following the character`
- Created a test map for feature verification and integration checks.
- Configured a test character for movement, collision, interaction, and animation switching validation.

### 2.5 Game Menu Setup and Background Content Addition

- **Date:** 2026-04-19
- **Evidence (Commit):** `042e1e4 - feat: implement main menu UI and scene management`; `a681163 - add the storyIntro`
- Implemented the basic game menu structure (start and feature entry points).
- Added background/lore content to improve narrative completeness and player immersion.

### 2.6 Scene Portal and Multi-Class Transformation Pedestal System

- **Date:** 2026-04-20
- **Evidence (Commit):** `f837cc5 - Feature: New scene teleportation portal and multi-class profession change altar system have been added.`; `7fe24a5 - add door-open animation`
- Implemented the core scene portal feature for cross-area transitions.
- Implemented the core multi-class transformation pedestal logic for proximity-based class transformation.
- Enabled character animation-controller switching after transformation to reflect class differences.

## 3. Key System Design Notes

### 3.1 Scene Portal System

- **Trigger Condition:** Player enters a portal trigger area to execute scene transition logic.
- **Design Purpose:** Split functional areas, reduce single-scene complexity, and support future level expansion.
- **Extension Suggestions:**
  - Add portal requirements (key, level, quest progress).
  - Add transition effects (fade in/out, loading hints).

### 3.2 Multi-Class Transformation Pedestal System

- **Trigger Condition:** Player enters the pedestal collider area and can interact.
- **Interaction Key:** `E` key is currently used to trigger class transformation.
- **Execution Result:** Switches the player's `RuntimeAnimatorController` on `Animator` to change class presentation.
- **Current Script:** `Assets/Scripts/CharacterTransformPedestal.cs`
- **Implemented Points:**
  - Uses `OnTriggerEnter2D` / `OnTriggerExit2D` to manage interaction state.
  - Uses `CompareTag("Player")` for player identification.
  - Uses the New Input System to read keyboard input and trigger transformation.

## 4. Current File and Script Notes

- `Assets/Scripts/CharacterTransformPedestal.cs`
  - Handles pedestal interaction detection and animator-controller switching.
- `doc/description.md`
  - Stores gameplay concept, player goal, and style direction.
- `README.md`
  - Provides the project quick introduction entry.

## 5. Development Process Record

1. Confirmed prototype scope and gameplay boundaries.
2. Built map and character test scene.
3. Added UI menu and world/background content.
4. Implemented core interaction systems (portal + pedestal).
5. Performed integration checks and playability validation.

## 6. Known Issues and Optimization Suggestions

- The transformation pedestal can currently be triggered repeatedly; consider single-use or cooldown logic.
- Icon-hiding and trigger-disabling logic is already reserved in comments and can be enabled by design decision.
- Menu features can be extended (settings, save/load, return to main menu).
- Portal logic should include destination-point management and repeated-load protection.

## 7. Milestone Suggestions

- **M1 (Done):** Playable prototype + basic interaction systems.
- **M2 (In Progress):** Stronger class differentiation (stats, skills, weapon preference).
- **M3 (Planned):** Complete level flow with enemy-growth-system linkage.
