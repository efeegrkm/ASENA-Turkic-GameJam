# Modular Interaction System

A plug-and-play, event-driven interaction system for Unity. Designed for rapid prototyping during game jams. You can interact with any object without writing new code for each interaction type.

## Features
- **Interface-based (IInteractable):** Fully decoupled. The player script doesn't need to know what it's interacting with.
- **UnityEvents:** Setup actions (play sounds, open doors, add to inventory) directly from the Unity Inspector.
- **Layer Masking:** Optimized with Physics.Raycast to only check objects on specific layers.

## Setup

### 1. Setup the Player
1. Add the `PlayerInteractor.cs` script to your Player or Main Camera.
2. In the Inspector, assign the **Camera Transform** to the script.
3. Set the **Interact Range** (e.g., 3 units).
4. Set the **Interact Layer**. *(Note: It is highly recommended to create a new Unity Layer named "Interactable" and select it here).*
5. Set your preferred **Interact Key** (Default: E).

### 2. Setup an Interactable Object (e.g., A Door or Switch)
1. Ensure your object has a `Collider` (BoxCollider, MeshCollider, etc.).
2. Set the object's Layer to your designated **Interactable** layer (matches step 1.4).
3. Add the `InteractableObject.cs` script to the object.
4. Type a **Prompt Text** in the Inspector (e.g., "Open Door", "Turn on Light", "Pick up Key").

### 3. Configure the Action as you wish 
In the `InteractableObject` script, you will see a UnityEvent section called **On Interact ()**. 
Click the **(+)** button to add an action when the player interacts with this object.
