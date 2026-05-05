# DESIGN.md — Golem Craft

## Overview

**SibGameJam_May_2026** — a 3D room-exploration crafting game made for a game jam.
The player controls a golem made of clay, explores interconnected rooms, collects alchemical elements, and combines them at craft tables to create new substances.

## Core Loop

1. **Explore** rooms via doorways/transitions
2. **Collect** base elements from infinite sources (press **E**)
3. **Craft** at tables by dragging elements together
4. **Use** crafted elements to unlock doors, activate tables, or complete offerings
5. **Repeat** across progressively harder rooms

## Controls

| Input | Action |
|---|---|
| WASD / Arrows | Move (camera-relative) |
| E | Interact (raycast 2.4m, sphere fallback 1.8m) |
| Escape | Close crafting panel |

## Player

- Movement via `Rigidbody` (physics-based, no teleportation)
- Camera is always the highest-depth active camera; movement aligns to it
- **4 inventory slots** for regular items
- **1 permanent clay slot** — clay is always available, shown in UI but not stored in inventory

## Rooms

| Room | ID | Description | Craft Table |
|---|---|---|---|
| Level 1 (starter) | `room_01` | Forge (workbench/anvil/furnace). Basic materials: iron, flint, coal, wood, air | Craft Table |
| Level 2 | `room_02` | Wet room (boiler). Water-based chemistry: water, saltpeter, lime, vitriol. Requires **torch** to activate. | Craft Table |
| Level 3 | `room_03` | Pantry (master workbench). Organic/fermentation: herbs, berries, horn. Yeast available from room_04 for crafting here. | Craft Table |
| Level 4 | `room_04` | Alchemist's study. Offering table for 3 key artifacts to unlock room_05. Elements: cinnabar, chest, yeast. | **Offering Table (×1)** |
| Level 5 | `room_05` | Magic chamber. 4 separate offering tables — place 1 key element on each to trigger the finale. | **4 separate Offering Tables** |

Rooms are laid out along the Z-axis in a single scene (`Room_v3.unity`), connected by `RoomTransitionTrigger` colliders that teleport the player and smoothly or instantly switch the camera.

## Interactable Types

| Type | Behaviour |
|---|---|
| **Element Source** | Infinite pickup — press E to add element to inventory |
| **Craft Table** | Opens crafting UI; may require an activation element first (e.g., torch to activate boiler) |
| **Door** | Consumes a required element from inventory to unlock, then disappears |
| **Offering Table** | Accepts specific items one at a time, spawns 3D visuals, then invokes completion events for animation/unlock logic |
| **Trash Bin** | Clears all 4 inventory slots (clay slot unaffected) |

## Crafting System

- **Elements are always combined in pairs** — every recipe requires exactly 2 inputs and produces 1 result
- Recipes are **room-scoped** — same two elements can produce different results in different rooms
- **Cross-room element usage** — player can pick up elements in one room and carry them to a table in another room (e.g., yeast source is in room_04 but used in recipes at room_03 table)
- **Input order does not matter** — `CraftKey` normalizes alphabetically
- Crafting is done in the **UI overlay** by dragging items onto the table area, then dragging one item onto another
- When the crafting panel closes, regular items return to inventory; items marked `discardOnTableClose` are destroyed

## Elements

### Base Elements (pickup sources)

| Element | Rooms Found | Notes |
|---|---|---|
| **Clay** | Always available | Permanent slot — always in inventory, never consumed |
| **Iron** | room_01 | Base metal |
| **Flint** | room_01 | Spark stone |
| **Coal** | room_01 | Fuel |
| **Wood** | room_01 | Basic material |
| **Air** | room_01 | Universal reactant |
| **Water** | room_02 | Universal solvent |
| **Saltpeter** | room_02 | Reactive mineral |
| **Lime** | room_02 | Building material |
| **Vitriol** | room_02 | Acidic mineral |
| **Herbs** | room_03 | Medicinal plant |
| **Berries** | room_03 | Organic |
| **Horn** | room_03 | Animal material |
| **Yeast** | room_04 | Fermentation agent |
| **Cinnabar** | room_04 | Red mineral (also used in room_01 recipe) |
| **Chest** | room_04 | Mystery element |

### World Visuals

Every `ElementDefinition` has a generated or imported `worldPrefab`. Default models live in `Assets/Game/Prefabs/ElementWorldModels/` and use `ElementWorldModel` to build recognizable low-poly primitive shapes at runtime. When a suitable textured prop exists in the level FBX, the element may instead point to a wrapper prefab in `Assets/Game/Art/Models/` that displays only the named FBX child mesh.

### Crafted Elements (recipes)

Recipes are stored in `MainCraftRecipeDatabase` in `Assets/Game/Crafting/`. Each room has multiple crafting recipes that combine two input elements into one result. See the database asset for the complete list.

#### New Elements (proposed)

| ID | Name | Type | Purpose |
|---|---|---|---|
| `paper` | Paper | intermediate | clay + wood |
| `mechanism` | Mechanism | intermediate | primary: copper + wood in room_01; alternate schematic assembly in room_03 |
| `blank_scroll` | Blank Scroll | intermediate | paper + wood |
| `ancient_scroll` | Ancient Scroll | **KEY room_04** | mechanism + blank_scroll |
| `base_stone` | Base Stone | intermediate | acid + gypsum |
| `philosopher_stone` | Philosopher's Stone | **KEY room_04** | base_stone + elixir |
| `magic_wand` | Magic Wand | **KEY room_04** | torch + copper |
| `vessel` | Vessel | intermediate | mechanism + air |
| `spirit` | Spirit | **KEY room_05** | vessel + fire |
| `mind` | Mind | **KEY room_05** | tincture + wine |
| `body` | Body | **KEY room_05** | clay + copper |
| `soul_essence` | Soul Essence | intermediate | tincture + wine |
| `soul` | Soul | **KEY room_05** | soul_essence + elixir |

#### Complete Recipe Table

**room_01 (Forge — workbench/anvil/furnace):**
| Input A | Input B | Result | Note |
|---|---|---|---|
| iron | flint | firesteel | keep |
| firesteel | coal | fire | keep |
| fire | wood | torch | keep (needed for room_02) |
| clay | iron | ore | keep |
| ore | fire | copper | new |
| copper | wood | mechanism | new (forge assembly) |
| clay | wood | paper | new |
| air | clay | dust | keep |

**room_02 (Wet Room — boiler, activate with torch):**
| Input A | Input B | Result | Note |
|---|---|---|---|
| water | saltpeter | acid | keep |
| water | berries | must | keep |
| air | saltpeter | explosion | GAME OVER, keep |
| water | clay | dust | keep |
| water | vitriol | poison | keep |
| water | lime | gypsum | keep |
| iron | vitriol | copper | keep (existing recipe) |

**room_03 (Pantry — master workbench):**
| Input A | Input B | Result | Note |
|---|---|---|---|
| fire | water | alcohol | keep |
| alcohol | herbs | tincture | keep |
| alcohol | horn | tincture | keep |
| must | horn | wine | keep |
| water | herbs | elixir | keep |
| water | horn | glue | keep |
| air | herbs | dust | keep |
| clay | iron | mechanism | alternate (clay mold + iron fittings) |
| copper | blank_scroll | mechanism | alternate (schematic-guided assembly) |
| paper | wood | blank_scroll | new |
| mechanism | blank_scroll | ancient_scroll | KEY room_04 |
| acid | gypsum | base_stone | new |
| base_stone | elixir | philosopher_stone | KEY room_04 |
| torch | copper | magic_wand | KEY room_04 |
| mechanism | air | vessel | new |
| vessel | fire | spirit | KEY room_05 |
| tincture | wine | soul_essence | new |
| soul_essence | elixir | soul | KEY room_05 |
| wine | herbs | mind | KEY room_05 |
| clay | copper | body | KEY room_05 |

#### Complex Artifact Paths

**For room_04 (3 artifacts):**
1. `ancient_scroll`: clay+wood=paper → paper+wood=blank_scroll → clay+iron=ore → ore+fire=copper → copper+wood=mechanism → mechanism+blank_scroll=**ancient_scroll** (6 steps)
2. `philosopher_stone`: water+saltpeter=acid → water+lime=gypsum → acid+gypsum=base_stone → water+herbs=elixir → base_stone+elixir=**philosopher_stone** (5 steps)
3. `magic_wand`: iron+flint=firesteel → firesteel+coal=fire → fire+wood=torch → clay+iron=ore → ore+fire=copper → torch+copper=**magic_wand** (6 steps)

**For room_05 (4 elements):**
1. `spirit`: clay+iron=ore → ore+fire=copper → copper+wood=mechanism → mechanism+air=vessel → vessel+fire=**spirit** (5 steps)
2. `mind`: water+berries=must → must+horn=wine → fire+water=alcohol → alcohol+herbs=tincture → tincture+wine=**mind** (5 steps)
3. `body`: clay+iron=ore → ore+fire=copper → clay+copper=**body** (3 steps)
4. `soul`: fire+water=alcohol → alcohol+herbs=tincture → water+berries=must → must+horn=wine → tincture+wine=soul_essence → soul_essence+elixir=**soul** (6 steps)

## Game Over

Triggered by crafting **Explosion** (Air + Saltpeter in room_02). The screen displays "GAME OVER" with a restart button. `Time.timeScale = 0` freezes all gameplay.

## Key Progression Hooks

- **Torch** → activates locked craft tables (e.g., boiler in room_02)
- **Doors** → require specific elements to pass through (consumed on use)
- **Offering Table (room_04)** → place 3 key artifacts (ancient_scroll, philosopher_stone, magic_wand); `OnCompleted()` unlocks room_05
- **Offering Tables (room_05)** → place 1 key element on each of 4 tables (spirit, mind, body, soul) → game finale
- **Room transitions** → linear progression: room_01 → room_02 → room_03 → room_04 → room_05

## UI

All UI is **programmatically generated** (no prefabs for screens):
- `UIFactory.CreateText()` / `UIFactory.CreateButton()`
- Inventory bar at screen bottom
- Crafting panel is a dynamic overlay with draggable items
- Game over overlay with title, message, restart button

## Audio

- `AmbientMusicSwitcher` — trigger-based zone music. Player enters a collider zone, music crossfades.
- Each room has its own background music source.

## Architecture Notes

- **No DI framework** — `SceneInstaller` manually wires references on `Start()`
- **Singletons** — `Inventory`, `CraftingSystem`, `GameOverController` accessed via static `Instance`
- **ScriptableObjects** for data: `ElementDefinition` (elements), `CraftRecipeDatabase` (recipes)
- **No tests, no CI** — verify by playing in Unity Editor
- **Unity Input System** package installed but gameplay uses legacy `Input.GetKey` API

## Adding Content

### New Element
1. Right-click in Project: `Create > Golem Craft/Element Definition`
2. Set: id (auto-lowercased), display name, icon (sprite), world prefab (optional)
3. Set `discardOnTableClose = true` if element should vanish when crafting panel closes (like clay)

### New Recipe
1. Select `MainCraftRecipeDatabase` in `Assets/Game/Crafting/`
2. Add entry in Inspector: Room ID, Input A, Input B, Result
3. Recipe is available immediately on play

### New Room
1. Add room layout to the main scene (position along Z-axis)
2. Create `RoomCameraController` with camera points
3. Add `RoomTransitionTrigger` colliders at entry/exit points
4. Register table in `SceneInstaller`'s `craftTables` array

### New Interactable
1. Implement `IInteractable` interface (requires `InteractionPrompt` getter and `Interact()` method)
2. Attach to a GameObject with a Collider
3. Player presses E → `PlayerInteractor` raycast/overlap finds it → calls `Interact()`
