# AGENTS.md

Unity 6000.4.5f1, URP. 3D golem-crafting game. No build/test/lint configured. Open in Unity Hub.

## Entry Point
- **Scene**: `Assets/Scenes/Room_v3.unity` (only enabled scene in Build Settings)
- **Wiring**: `SceneInstaller` (MonoBehaviour) wires systems on `Start()`

## Core Systems
| System | File | Key Fact |
|---|---|---|
| Inventory | `Scripts/Inventory/Inventory.cs` | 4 normal slots. Permanent clay slot is UI-only. |
| CraftingSystem | `Scripts/Craft/CraftingSystem.cs` | Dictionary lookup by `CraftKey` (roomId + 2 elementIds). Order-independent. |
| GameOverController | `Scripts/Scene/GameOverController.cs` | Static `IsGameOver` pauses movement/interaction. |

## Data (ScriptableObjects)
- **ElementDefinition**: `Create > Golem Craft/Element Definition` (id, icon, prefab, `discardOnTableClose`)
- **CraftRecipeDatabase**: `Create > Golem Craft/Craft Recipe Database` (roomId + 2 inputs → 1 result)
- **Element world models**: `Assets/Game/Prefabs/ElementWorldModels/` prefabs use `ElementWorldModel` to generate simple primitive 3D visuals at runtime. Some elements override this with wrapper prefabs in `Assets/Game/Art/Models/` that reference named child meshes from level FBX files.
- **Offering visuals**: `OfferingTableInteractable` spawns visuals without parenting them under scaled table roots; spawned visuals can follow assigned anchors via `FollowTransform`, so anchors may live under animated table parts.

## Interaction Flow
E key → `PlayerInteractor` raycast (2.4m) → sphere fallback (1.8m) → `IInteractable.Interact()`

## Crafting Mechanics
- **Elements are always combined in pairs** — every recipe requires exactly 2 inputs and produces 1 result
- **Recipes are room-scoped** — same two elements produce different results depending on which table you use
- **Cross-room element usage** — player can pick up elements in one room and carry them to a table in another room (e.g., yeast source is in room_04 but used in recipes at room_03 table)
- **Input order doesn't matter** — `CraftKey` normalizes alphabetically

## Room Structure
| Room | ID | Description | Craft Table | Elements (Sources) | Recipes in DB |
|---|---|---|---|---|---|
| 1 | room_01 | Forge (workbench/anvil/furnace) | Craft Table | iron, flint, coal, wood, air | yes |
| 2 | room_02 | Wet room (boiler, activate with torch) | Craft Table | water, saltpeter, lime, vitriol | yes |
| 3 | room_03 | Pantry (master workbench) | Craft Table | herbs, berries, horn | yes |
| 4 | room_04 | Alchemist's study | **Offering Table (×1)** | cinnabar, chest, yeast | no (offering only) |
| 5 | room_05 | Magic chamber | **4 separate Offering Tables** | — | no (finale) |

## Key Progression
- **room_04**: Place 3 key artifacts on the offering table → unlocks room_05
- **room_05**: Place 1 key element on each of 4 offering tables → game finale

See `DESIGN.md` for full element list and crafting details.

## Verification
No automated tests. Verify in Unity Editor play mode.

## Key Quirks
- Unity Input System package installed but gameplay uses legacy `Input` API
- `RoomTransitionTrigger` uses `#if UNITY_6000_0_OR_NEWER` for `linearVelocity`
- `.csproj`/`.sln`/`Library/`/`Logs/`/`UserSettings/` are gitignored
