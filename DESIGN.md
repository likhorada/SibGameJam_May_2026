# DESIGN.md - Golem Craft

## Overview

`SibGameJam_May_2026` is a 3D room-exploration crafting game. The player controls a clay golem, collects alchemical elements, combines them in pairs at room-specific craft tables, and uses crafted artifacts to unlock later rooms and the finale.

## Core Loop

1. Explore connected rooms.
2. Collect base elements from infinite sources with `E`.
3. Open craft tables and drag two elements together.
4. Use crafted elements to activate tables, unlock passages, or complete offering tables.
5. Repeat with deeper cross-room recipes.

## Controls

| Input | Action |
|---|---|
| WASD / Arrows | Move, camera-relative |
| E | Interact by raycast 2.4m, then overlap fallback 1.8m |
| Escape | Close crafting panel |

## Player And Inventory

- Movement uses `Rigidbody`.
- The active camera with the highest depth defines camera-relative movement.
- Inventory has 4 normal item slots.
- Clay is permanent and UI-only. It appears as an extra slot, but is not stored in `Inventory`.

## Rooms

| Room | ID | Description | Table | Sources |
|---|---|---|---|---|
| Level 1 | `room_01` | Forge: workbench, anvil, furnace | Craft Table | iron, flint, coal, wood, air |
| Level 2 | `room_02` | Wet room: boiler, activated with torch | Craft Table | water, saltpeter, lime, vitriol |
| Level 3 | `room_03` | Pantry: master workbench, organic recipes | Craft Table | herbs, berries, horn |
| Level 4 | `room_04` | Alchemist study, unlocks finale | Offering Table x1 | cinnabar, chest, yeast |
| Level 5 | `room_05` | Magic chamber finale | Offering Tables x4 | none |

Rooms are placed in one scene, `Assets/Scenes/Room_v3.unity`, and connected by `RoomTransitionTrigger` colliders.

## Interactable Types

| Type | Behaviour |
|---|---|
| Element Source | Infinite pickup. Adds an element to the first free inventory slot. |
| Craft Table | Opens the craft UI, or first consumes an activation element if locked. |
| Door | Consumes a required element, disables blocking, then hides or opens. |
| Offering Table | Consumes specific elements one at a time, spawns 3D visuals, then invokes completion events. |
| Trash Bin | Clears the 4 normal inventory slots. Clay is unaffected. |
| Interaction Hint | Optional `InteractionHintOnInteract` popup after interaction. Can use state-based rules for objects that expose hint state. |

## Crafting System

- Every recipe has exactly 2 inputs and 1 result.
- Recipes are scoped by `roomId`.
- Input order does not matter. `CraftKey` sorts element ids.
- Cross-room usage is intentional: an element found in one room can be used at another room's table.
- The craft UI accepts dragged items from normal inventory slots, the permanent clay slot, and existing table items.
- When closing the craft panel, regular table items return to inventory if enough slots are free.
- Items with `discardOnTableClose` disappear on close.

## Elements

### Base Sources

| Element | Found In | Notes |
|---|---|---|
| clay | permanent | Always available through the UI-only slot |
| iron | `room_01` | Base metal |
| flint | `room_01` | Spark stone |
| coal | `room_01` | Fuel |
| wood | `room_01` | Basic material |
| air | `room_01` | Universal reactant |
| water | `room_02` | Solvent |
| saltpeter | `room_02` | Reactive mineral |
| lime | `room_02` | Building material |
| vitriol | `room_02` | Acidic mineral |
| herbs | `room_03` | Medicinal plant |
| berries | `room_03` | Organic ingredient |
| horn | `room_03` | Organic material |
| yeast | `room_04` | Fermentation agent |
| cinnabar | `room_04` | Red mineral |
| chest | `room_04` | Mystery element |

### World Visuals

`ElementDefinition.worldPrefab` controls the 3D model used by offering tables. Default generated models live in `Assets/Game/Prefabs/ElementWorldModels/`. Imported wrapper prefabs live in `Assets/Game/Art/Models/` and can show a named child mesh from a level FBX.

## Recipe Table

### `room_01` - Forge

| Input A | Input B | Result |
|---|---|---|
| iron | flint | firesteel |
| firesteel | coal | fire |
| fire | wood | torch |
| clay | iron | ore |
| ore | fire | copper |
| copper | wood | mechanism |
| clay | wood | paper |
| air | clay | dust |

### `room_02` - Wet Room

| Input A | Input B | Result |
|---|---|---|
| water | saltpeter | acid |
| water | berries | must |
| air | saltpeter | explosion |
| water | clay | dust |
| water | vitriol | poison |
| water | lime | gypsum |
| iron | vitriol | copper |

`explosion` triggers game over when crafted.

### `room_03` - Pantry

| Input A | Input B | Result |
|---|---|---|
| fire | water | alcohol |
| alcohol | herbs | tincture |
| alcohol | horn | tincture |
| must | horn | wine |
| water | herbs | elixir |
| water | horn | glue |
| air | herbs | dust |
| clay | iron | mechanism |
| copper | blank_scroll | mechanism |
| paper | wood | blank_scroll |
| mechanism | blank_scroll | ancient_scroll |
| acid | gypsum | base_stone |
| base_stone | elixir | philosopher_stone |
| torch | copper | magic_wand |
| mechanism | air | vessel |
| vessel | fire | spirit |
| tincture | wine | soul_essence |
| soul_essence | elixir | soul |
| wine | herbs | mind |
| clay | copper | body |

## Key Artifact Paths

### Room 04 Artifacts

1. `ancient_scroll`: clay+wood=paper, paper+wood=blank_scroll, clay+iron=ore, ore+fire=copper, copper+wood=mechanism, mechanism+blank_scroll=ancient_scroll.
2. `philosopher_stone`: water+saltpeter=acid, water+lime=gypsum, acid+gypsum=base_stone, water+herbs=elixir, base_stone+elixir=philosopher_stone.
3. `magic_wand`: iron+flint=firesteel, firesteel+coal=fire, fire+wood=torch, clay+iron=ore, ore+fire=copper, torch+copper=magic_wand.

### Room 05 Finale Elements

1. `spirit`: clay+iron=ore, ore+fire=copper, copper+wood=mechanism, mechanism+air=vessel, vessel+fire=spirit.
2. `mind`: water+berries=must, must+horn=wine, fire+water=alcohol, alcohol+herbs=tincture, tincture+wine=mind.
3. `body`: clay+iron=ore, ore+fire=copper, clay+copper=body.
4. `soul`: fire+water=alcohol, alcohol+herbs=tincture, water+berries=must, must+horn=wine, tincture+wine=soul_essence, soul_essence+elixir=soul.

## Game Over

Crafting `explosion` in `room_02` triggers `GameOverController`. The overlay shows a restart button and pauses gameplay with `Time.timeScale = 0`.

## UI

The main UI is generated at runtime:

- `InventoryUI` creates the bottom inventory bar.
- `InventorySlotUI` handles inventory drag/drop.
- `CraftingPanelUI` creates the craft table overlay.
- `TableDropArea` accepts dragged elements.
- `TableItemUI` handles table items and pair crafting.
- `InteractionHintWindow` shows temporary hint popups.

`InventoryUI` and `CraftingPanelUI` expose Inspector fields for future visual polish: panel sprites, colors, slot sizes, icon sizes, and text sizes. `ElementDefinition` can also define a personal UI background that follows that element in inventory, craft table UI, and drag previews. If no personal background is configured, craft table items use the panel-level `Table Item Background Mode` fallback.

## Audio

- `AmbientMusicSwitcher` handles room/zone background music.
- Interaction SFX go through `GameAudio.Play(GameSoundId.X)`.
- `GameAudio` uses generated fallback clips by default.
- `GameAudioProfile` can override any individual `GameSoundId` with a real `AudioClip`.

## Architecture Notes

- `SceneInstaller` wires scene references on `Start()`.
- Static singleton-style access is used by `Inventory`, `CraftingSystem`, `GameOverController`, and `GameAudio`.
- ScriptableObjects hold data: `ElementDefinition`, `CraftRecipeDatabase`, `GameAudioProfile`.
- There are no automated gameplay tests. Verify in Unity Editor Play Mode.
- Unity Input System is installed, but gameplay currently uses legacy `Input`.
