# Class Attendance and Presentation Notes - 2026-05-14

## Attendance Evidence

On May 14th, I attended the class. My group was called **Happy Unity**.

During the group discussion, we discussed programming concepts such as function, inheritance, polymorphism, and the role of instances in Unity. Our group planned to study Unity's event-driven system.

## Unity Event-Driven Programming Presentation Plan

**Group:** Happy Unity  
**Date:** 2026-05-14

## Presentation Topic

**How `Assets/Scripts/ShootingProjectiles/ShootingController.cs` controls the spawning and destruction of projectiles through Unity's event-driven model.**

We planned to trace what happens from the moment the player presses the fire button to the moment a projectile disappears from the scene, and use this small example to explain the main ideas of Unity's event-driven architecture.

## 1. How Unity Automatically Calls Event Functions

We planned to explain that Unity owns the game loop, and that scripts implement methods with reserved names. The engine calls these methods at the right time, so we do not need to write a main loop ourselves.

## 2. How Player Input Is Checked Every Frame

We planned to show that Unity provides a per-frame event function, and that the shooting script uses it to continuously poll the current input value. This explains why the game responds smoothly when the player holds the fire button.

## 3. The Role of `InputAction` in the New Input System

We planned to introduce `InputAction` as an abstraction that separates a gameplay action from the specific key or button used to trigger it. We also planned to mention why it has to be enabled and disabled together with the script lifecycle.

## 4. How a Projectile Is Dynamically Spawned at Runtime

We planned to describe how the shooting script holds a prefab and asks Unity to create a copy of it whenever the player fires. This is an example of runtime object creation.

## 5. How a Projectile Is Automatically Destroyed at Runtime

We planned to point out that the shooting script does not destroy projectiles itself. Destruction is handled by a separate script reacting to a Unity physics event when the projectile collides with something. This was used as an example of loose coupling between scripts through events.
