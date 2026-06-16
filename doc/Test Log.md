# Game Test Document

## 1. Test Objectives

- Verify the usability, stability, and interaction correctness of the early prototype features.
- Cover the prototype-stage core loop: map, character, menu, portal, and transformation pedestal.
- Record concrete observations from early checks instead of leaving blank testing fields.

## 2. Test Environment

- Engine: Unity 2022.3 LTS
- Platform: Windows 10
- Input: Keyboard (`E` key interaction included)
- Test Scene: Development test map with portal and transformation pedestal
- Prototype Focus: Movement, camera follow, menu entry, story entry, interaction triggers, and scene transition basics

## 3. Test Scope

1. Gameplay prototype playability validation
2. Game description content display and consistency validation
3. Asset loading and visual display validation
4. Test map and test character validation
5. Game menu and background content validation
6. Scene portal and multi-class transformation pedestal system validation

## 4. Test Methods

- Functional testing: Trigger features step by step and compare with expected results.
- Interaction testing: Focus on collision triggers, key interactions, and state transitions.
- Regression testing: Re-run core cases after new prototype features to prevent early functional regressions.

## 5. Test Cases

### TC-01 Startup and Prototype Entry

- **Precondition:** Project can run normally.
- **Steps:**
  1. Launch the game.
  2. Enter the main playable scene or prototype entry scene.
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
  - Camera follow keeps the character visible during movement.

### TC-03 Asset Display Validation

- **Precondition:** Character, ground, and interactive object assets are imported.
- **Steps:**
  1. Enter the test scene.
  2. Check key assets such as character, pedestal, portal, and menu background.
- **Expected Result:**
  - Assets are displayed correctly with assigned sprites/materials.
  - Sorting and layer order are clean and readable.
  - Prototype visuals match the planned pixel dungeon style.

### TC-04 Menu Function Validation

- **Precondition:** Menu scene or menu panel is available.
- **Steps:**
  1. Open the game menu.
  2. Operate major menu options such as Start Game.
  3. Verify background/story content display.
- **Expected Result:**
  - Menu opens and responds correctly.
  - Start flow enters the intended early gameplay or story scene.
  - Background text content is complete and readable.

### TC-05 Scene Portal Validation

- **Precondition:** A triggerable portal exists in the scene.
- **Steps:**
  1. Move the character into the portal trigger area.
  2. Use the required interaction input if needed.
  3. Observe scene transition behavior.
- **Expected Result:**
  - Portal is triggered correctly.
  - Character enters the target area or target scene.
  - Loading remains stable with a clean screen transition.

### TC-06 Transformation Pedestal Trigger Validation

- **Precondition:** Pedestal exists in the scene, and the script and target presentation/animator settings are assigned.
- **Steps:**
  1. Move the character into the pedestal trigger area.
  2. Press `E`.
  3. Observe character presentation or animation behavior.
- **Expected Result:**
  - System detects interaction and executes transformation.
  - Player presentation changes successfully.
  - No null reference errors or runtime interruption occur.

### TC-07 Pedestal Boundary and Repeated Interaction Validation

- **Precondition:** Same as TC-06.
- **Steps:**
  1. Press `E` outside the pedestal trigger area.
  2. Press `E` inside the trigger area.
  3. Leave the trigger area, re-enter it, and repeat the interaction.
- **Expected Result:**
  - `E` key interaction stays limited to the pedestal trigger area.
  - `E` key inside range triggers transformation.
  - Repeated interaction behavior matches the current prototype design.

## 6. Recorded Prototype Quality Notes

| Note ID | Title | Found In Stage | Priority | Observation | Status |
|---------|-------|----------------|----------|-------------|--------|
| PQ-001 | Prototype needs a playable entry path | Early menu / scene setup | High | Opening only a test scene was not enough to explain the game flow, so a menu and story entry were added. | Improved |
| PQ-002 | Player must stay visible during exploration | Test map | High | The prototype needed camera follow to support map exploration and movement checks. | Verified |
| PQ-003 | Interaction should be range-limited | Transformation pedestal | High | Pressing `E` should only transform the player when inside the pedestal trigger area. | Verified |
| PQ-004 | Portal transition needs stable target flow | Portal test area | High | Scene transition had to be tested early because later level flow depends on this system. | Verified |
| PQ-005 | Pixel assets need readable sorting | Test map / menu background | Medium | Character, map, portal, and pedestal assets needed clear sorting so interactive objects stayed visible. | Recorded |
| PQ-006 | Prototype documentation should match actual features | Development documents | Medium | Early documents should describe prototype systems instead of final gameplay that did not exist yet at that stage. | Verified |

## 7. Current Test Conclusion (Prototype Phase)

- The prototype formed a runnable and interactable early loop.
- The core chain `enter scene -> control character -> trigger interaction -> observe state change or scene transition` was validated.
- Menu, story entry, camera follow, portal interaction, and transformation pedestal behavior gave the project a strong base for later combat and level-flow work.
- Later full-game regression testing and final submission evidence are recorded separately in `TESTING.md`.

## 8. Extended Test Coverage Planned After Prototype

- Edge-case coverage: rapid key presses, boundary collision, repeated cross-scene teleport.
- Compatibility coverage: multiple resolutions and window mode switches.
- User-experience coverage: menu flow clarity and interaction feedback timing.
- Expansion coverage: class-specific combat, room rewards, enemy behavior, boss battle, death result, and victory result.
