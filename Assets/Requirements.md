# Kirby Character Controller Requirements Document

**Project Goal:** 
To develop a robust, modular, and game designer-friendly 2D character controller for Kirby in Unity. 
The system must support various transformations, each with unique movement capabilities and animation sets, while adhering to SOLID design principles and leveraging Unity's Scriptable Objects.
The controller should provide a fluid and responsive gameplay experience, jumping, walking and running should feel good to control. 
---

## 1. Core System Architecture & Design Principles

* **Modularity:** The system must be highly modular, allowing for easy addition, removal, or modification of Kirby's forms and abilities without requiring significant rewrites of core systems.
* **SOLID Principles:**
    * **Single Responsibility Principle (SRP):** Each class or component should have a single, well-defined responsibility (e.g., a class for input handling, another for movement physics, another for animation management per state).
    * **Open/Closed Principle (OCP):** The system should be open for extension (adding new forms, abilities) but closed for modification (core systems shouldn't need to change to accommodate new forms).
    * **Liskov Substitution Principle (LSP):** Different Kirby forms, if treated as subtypes of a base Kirby, should be substitutable without breaking the game.
    * **Interface Segregation Principle (ISP):** Interfaces should be granular, so classes only implement methods relevant to them. Avoid "fat" interfaces.
    * **Dependency Inversion Principle (DIP):** High-level modules should not depend on low-level modules. Both should depend on abstractions (e.g., interfaces or abstract classes).
* **Scriptable Objects:** Utilize Scriptable Objects extensively for defining character forms, abilities, animation sets, and other configurable data. This allows game designers to easily create and tweak content without coding.
* **Game Designer Friendliness:** The system should be intuitive for game designers to use. This means:
    * Clear inspectors for configuring forms and abilities.
    * Easy assignment of animations.
    * Minimal need to delve into complex code or animation graphs for common tasks.
* **Reflection:** Use Reflection **only if absolutely necessary** and where its benefits clearly outweigh the potential performance and maintainability drawbacks. Prioritize more direct and type-safe solutions.
* **Separation of Concerns:** Clearly separate logic for:
    * Input Handling
    * Movement Physics & Collision
    * State Management
    * Animation Control
    * Ability Management
    * Transformation Management

---

## 2. Character State Management

* **Base States (Regular Kirby):**
    * **Idle:** Standing still. Animations should adapt to:
        * Flat Surface (`Idle.anim`)
        * Sloped Surface (`SlopeL.anim`, `SlopeR.anim`)
        * Deep Sloped Surface (`DeepSlopeL.anim`, `DeepSlopeR.anim`)
    * **Walking:** Moving based on analog input. (`Walk.anim`)
    * **Running:** Faster movement based on analog input. (`Run.anim`)
    * **Jumping:** Initial upward movement. Should feel fluid and good to control. (`Jump.anim`)
        * Should vary on how long you hold the jump button. jump high if you hold it longer, jump low if you tap it.
        * If you jump high, kirby's fall animation activates with a delay, and he looks like he bounces when he lands. 
    * **Falling:** Descending after a jump or when airborne (standard gravity). (`Fall.anim`)
        * Kirby can still move left or right while falling. kirby should only fall past a certain height or after a jump, otherwise he just keeps his running animation.
    * **Crouching:** Lowered stance. Cannot walk while crouched. Animations should adapt to:
        * Flat Surface (`Crouch.anim`)
        * Sloped Surface (Potentially reuse `Squashed_SlopeL.anim`, `Squashed_SlopeR.anim` or similar if `Crouch.anim` isn't slope-specific)
        * Deep Sloped Surface (Potentially reuse `Squashed_DeepSlopeL.anim`, `Squashed_DeepSlopeR.anim` or similar)
    * **Sliding** Pressing attack while crouching slides forward. ('Slide.anim')
    * **Flying:** Sustained aerial movement after a double jump. (`Fly.anim`)
        * Kirby can remain in the air indefinitely by repeatedly "flapping" (jump input) as long as he doesn't touch the ground.
        * Without flapping input, Kirby should gently descend (slow fall).
        * The `Fly.anim` likely covers the visual aspect of both flapping and gentle descent, with physics dictating the movement.
    * **Inhaling:** Transition state to suck in enemies/objects. This is done with the Attack input action (`Inhale.anim`)
    * **Full:** State after successfully inhaling something. (`Full_Idle.anim`)
        * Can walk, run, and jump when full. (`Full_Run.anim`, `Full_Jump.anim`)
        * Animations should adapt to surfaces:
            * Full Idle on Slopes (`Full_SlopeL.anim`, `Full_SlopeR.anim`)
            * Full Idle on Deep Slopes (`Full_DeepSlopeL.anim`, `Full_DeepSlopeR.anim`)
        * Full Falling (`Full_Fall.anim`)
    * **Spitting:** Expelling an inhaled object/enemy. (`Spit.anim`)  
    * **Swallowing:** Absorbing an inhaled enemy for a transformation. Done by crouching while full of an enemy or object. (`Swallow.anim`)
    * **Landing:** Animation that plays upon hitting the ground.
        * Special landing from height: Hold first two frames of `Fall.anim` until close to the ground, then play the end part of `Fall.anim` (bounce).
    * The Attack input action can be used to exit the `Fly` state, and to spit the inhaled object/enemy.
* **State Transitions (Regular Kirby):**
    * `Idle` <-> `Walk` <-> `Run` (based on analog input)
    * `Idle`/`Walk`/`Run` -> `Jump`
    * `Jump` -> `Fall`
    * `Jump` -> `JumpToFly` (after a second jump input) -> `Fly`
    * `Fly` (flap via jump input to ascend/maintain height; no input leads to gentle descent)
    * `Fly` -> `Fall` (via "Exhale"/"Spit" action, using `Spit.anim` or `FlyEnd.anim` then `Fall.anim`)
    * `Idle`/`Walk`/`Run` -> `Crouch`
    * `Crouch` -> `Idle`
    * `Idle`/`Walk`/`Run` -> `Inhale`
    * `Inhale` -> `Full` (on successful inhale)
    * `Full` (Idle/Walk/Run/Jump states)
    * `Full` -> `Spit` -> `Idle`/`Fall` (returns to normal)
    * `Full` -> `Swallow` -> `Idle`/`Fall` (returns to normal, potentially triggering transformation)
    * Any airborne state (including `Fly` if touching ground) -> `Landing` -> `Idle` (or appropriate ground state)

---

## 3. Movement System

* **Ground Movement:**
    * Variable speed for walking and running based on analog input.
    * Smooth acceleration and deceleration.
    * Correctly handle movement on flat, sloped, and deep-sloped surfaces.
* **Jumping:**
    * Variable jump height (e.g., holding jump button longer).
    * Precise jump trajectory.
* **Flying:**
    * **Indefinite Flight Duration:** Kirby can stay airborne by active flapping as long as he doesn't touch the ground.
    * **Controlled Ascent via Flapping:** Repeated "flap" inputs (jump button) allow Kirby to gain or maintain height. Each flap provides an upward boost.
    * **Gentle Descent (Slow Fall):** If no flap input is given while in the `Fly` state, Kirby should gently descend. This descent is slower and more controlled than the standard `Fall` state. Gravity's effect should be significantly dampened or replaced by a consistent gentle downward velocity during this phase of flight.
    * **Prevent Uncontrolled Ascent:** A critical requirement is to prevent bugs where Kirby continuously rises upwards without limit (e.g., due to misapplied forces or lack of velocity dampening after a flap) or where a single flap sends him too high too quickly. Upward movement in flight must be intentional due to player input.
    * **Exiting Flight:** The primary way to consciously exit the `Fly` state and return to standard `Fall` physics is via the "Exhale"/"Spit" action. Touching the ground will also terminate flight.
    * Horizontal control while flying.
* **Falling:**
    * Standard gravity application when in the `Fall` state (e.g., after a jump, or after exhaling from `Fly` state).
    * Terminal velocity.
* **Physics:**
    * Use Unity's 2D physics (Rigidbody2D, Colliders).
    * Ensure responsive and "game-feel" appropriate physics.
* **Velocity Control:** The system must carefully manage velocity changes during state transitions to ensure fluid movement and prevent bugs like unintended indefinite upward flight or abrupt stops/starts. Apply forces and dampen velocities appropriately for each state.

---

## 4. Animation System

* **Animation Control:**
    * Primarily use `Animator.Play("AnimationName")` for playing specific animation clips directly. This offers more direct control and simplifies the Animator Controller.
    * **Shortcuts/Graph Connections:** For simple, always-sequential transitions (e.g., `JumpToFly.anim` immediately followed by `Fly.anim`), these can be connected directly in the Animator graph for convenience. The system should know when an animation played via `.Play()` is expected to auto-transition via the graph.
    * The system needs to manage which animation should be playing based on the current character state and actions.
* **Animation Sets:**
    * Each Kirby form (including regular Kirby) will have its own set of animation clips.
    * Scriptable Objects should be used to define these animation sets, allowing easy mapping of states/actions to specific animation clips for each form.
    * Example structure for an Animation Set (Scriptable Object):
        * Idle Animation
        * Walk Animation
        * Run Animation
        * Jump Animation
        * Fall Animation
        * Fly Animation (may visually represent both flapping and gentle descent)
        * ... and so on for all relevant states/actions of that form.
* **Animation Prioritization:** Handle cases where multiple animations could play (e.g., landing overrides falling).
* **Existing Animations:** The system should be able to utilize the provided list of `.anim` files for regular Kirby:
    * `Crouch.anim`, `DeepSlopeL.anim`, `DeepSlopeR.anim`, `Fall.anim`, `Fly.anim`, `FlyEnd.anim` (transition from Fly to Fall), `Full.anim`, `Full_DeepSlopeL.anim`, `Full_DeepSlopeR.anim`, `Full_Fall.anim`, `Full_Idle.anim`, `Full_Jump.anim`, `Full_Run.anim`, `Full_SlopeL.anim`, `Full_SlopeR.anim`, `Guard.anim` (Note: Guard behavior not detailed in prompts but animation exists), `Idle.anim`, `Inhale.anim`, `Jump.anim`, `JumpToFly.anim`, `Run.anim`, `SlopeL.anim`, `SlopeR.anim`, `Spit.anim`, `Squashed_DeepSlopeL.anim`, `Squashed_DeepSlopeR.anim`, `Squashed_SlopeL.anim`, `Squashed_SlopeR.anim` (Note: Squashed animations might be for damage or specific crouch on slopes), `Swallow.anim`, `Walk.anim`.
    * The existing `KirbyAnim.controller` might serve as a reference but the goal is to simplify reliance on a complex graph.

---

## 5. Transformation System

* **Core Concept:** Kirby can transform into different forms, inheriting abilities from swallowed enemies.
* **Transformation Trigger:** Typically after `Swallow` action if a transformable power was inhaled.
* **Modular Forms:**
    * Each transformation is a self-contained module (likely defined by a Scriptable Object).
    * This Scriptable Object would define:
        * The form's unique animation set (referencing animation clips or an Animation Set SO).
        * The form's available abilities (e.g., specific attacks, modified movement).
        * Movement overrides (e.g., Rider Kirby runs faster).
* **Ability System:**
    * Each form has a defined set of abilities. Some abilities might be shared (e.g., basic jump), while others are unique.
    * Abilities should be implementable as separate components or classes, easily attachable/detachable or activatable/deactivatable based on the current form.
* **Example Transformation - Rider Kirby:**
    * **Abilities:** Can only Run and Jump.
    * **Movement Changes:** Runs faster than regular Kirby.
    * **Animations:** Has a distinct set of animations for its actions (e.g., `Rider_Run.anim`, `Rider_Jump.anim`).
    * Cannot fly, inhale, crouch (unless specified by the Rider Kirby form definition).
* **Extendibility:** The system must be easily extendable to add many more transformations with diverse mechanics and animations in the future. Designers should be able to create new transformation Scriptable Objects, assign animations, and define unique abilities/movement properties.
* **Reverting Form:** A mechanism to revert from a transformation back to regular Kirby (e.g., discarding an ability, taking damage).

---

## 6. Input System

* **Abstraction:** Abstract input handling from character logic. This allows for easier remapping of controls and support for different input devices (keyboard, gamepad).
* **Actions:** Define inputs based on actions (e.g., "Jump", "MoveHorizontal", "Inhale", "UseAbility", "Exhale").
* **Analog Input:** Support analog input for variable movement speed (walk/run).

---

## 7. General Considerations & Nice-to-Haves

* **Performance:** Keep performance in mind, especially if many characters or complex animations are on screen.
* **Debugging Tools:** Consider adding visual debugging tools (e.g., displaying current state, velocity) to aid development and tuning.
* **Camera Follow:** While not part of the controller itself, ensure the controller's movement works well with a typical 2D platformer camera system.
* **Extensibility for Other Abilities:** Design the ability system so it could potentially handle other Kirby mechanics like Star Spit, specific attack types for transformations, etc.

---