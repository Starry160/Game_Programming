# Game Test Document

## 1. Test Objectives

- Verify the usability, stability, and interaction correctness of currently implemented features.
- Cover the prototype-stage core loop: map, character, menu, portal, and transformation pedestal.

## 2. Test Environment

- Engine: Unity 2022.3 LTS
- Platform: Windows 10
- Input: Keyboard (`E` key interaction included)
- Test Scene: Development test map (including portal and transformation pedestal)

## 3. Test Scope

1. Gameplay prototype playability validation
2. Game description content display/consistency validation
3. Asset loading and visual display validation
4. Test map and test character validation
5. Game menu and background content validation
6. Scene portal and multi-class transformation pedestal system validation

## 4. Test Methods

- Functional testing: Trigger features step by step and compare with expected results.
- Interaction testing: Focus on collision triggers, key interactions, and state transitions.
- Regression testing: Re-run core cases after new features to prevent functional regressions.

## 5. Test Cases

### TC-01 Startup and Prototype Entry

- **Precondition:** Project can run normally.
- **Steps:**
  1. Launch the game.
  2. Enter the main playable scene.
- **Expected Result:**
  - Game starts successfully without blocking errors.
  - User can enter an operable scene and control the character.

### TC-02 Test Map and Character Validation

- **Precondition:** Test map is already configured.
- **Steps:**
  1. Load the test map.
  2. Verify the character is spawned correctly.
  3. Control character movement.
- **Expected Result:**
  - Map elements render correctly.
  - Test character moves normally without freezing or disappearing.

### TC-03 Asset Display Validation

- **Precondition:** Character, ground, and interactive object assets are imported.
- **Steps:**
  1. Enter the test scene.
  2. Check key assets (character, pedestal, portal, menu background).
- **Expected Result:**
  - Assets are displayed correctly with no missing (pink) materials.
  - Sorting/layer order is correct without obvious overlap issues.

### TC-04 Menu Function Validation

- **Precondition:** Menu scene or menu panel is available.
- **Steps:**
  1. Open the game menu.
  2. Operate major menu options (e.g., Start Game).
  3. Verify background content display.
- **Expected Result:**
  - Menu opens and responds correctly.
  - Background text content is complete and readable.

### TC-05 Scene Portal Validation

- **Precondition:** A triggerable portal exists in the scene.
- **Steps:**
  1. Move the character into the portal trigger area.
  2. Observe scene transition behavior.
- **Expected Result:**
  - Portal is triggered correctly.
  - Character enters the target area or target scene.
  - No repeated loading, black screen, or freeze issues.

### TC-06 Transformation Pedestal Trigger Validation

- **Precondition:** Pedestal exists in the scene, and the script and target animator controller are assigned.
- **Steps:**
  1. Move the character into the pedestal trigger area.
  2. Press `E`.
  3. Observe character animation behavior.
- **Expected Result:**
  - System detects interaction and executes transformation.
  - Player `Animator Controller` is switched successfully.
  - No null reference errors or runtime interruption.

### TC-07 Pedestal Boundary and Repeated Interaction Validation

- **Precondition:** Same as TC-06.
- **Steps:**
  1. Press `E` outside the pedestal trigger area.
  2. Press `E` inside the trigger area, leave it, then re-enter and repeat.
- **Expected Result:**
  - `E` key outside range does not trigger transformation.
  - `E` key inside range triggers transformation.
  - Repeated interaction behavior matches current design (currently repeatable by default).

## 6. Defect Report Template (Recommended)

- Defect ID:
- Defect Title:
- Found In Version:
- Severity: Blocker / Critical / Normal / Minor
- Reproduction Steps:
- Expected Result:
- Actual Result:
- Screenshot or Log:
- Status: Open / Fixed / Verified

## 7. Current Test Conclusion (Phase)

- The current build forms a runnable and interactable prototype loop.
- The core gameplay chain (enter scene -> control character -> trigger interaction) is validated.
- Next phase should add an automated regression checklist and performance tests (FPS, load time, memory usage).

## 8. Suggestions for Next Test Round

- Add edge-case tests: rapid key presses, boundary collision, repeated cross-scene teleport.
- Add compatibility tests: multiple resolutions and window mode switches.
- Add user-experience tests: menu flow clarity and interaction feedback timing.
