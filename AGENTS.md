# AGENTS.md

Unity 6000.4.5f1, URP. 3D golem-crafting game. No build/test/lint pipeline is configured. Open and verify in Unity Hub.

## Entry Point
- Scene: `Assets/Scenes/Room_v3.unity` (only enabled scene in Build Settings)
- Wiring: `SceneInstaller` wires systems on `Start()`
- Main verification path: Unity Editor Play Mode

## Core Systems
| System | File | Key Fact |
|---|---|---|
| Inventory | `Assets/Scripts/Inventory/Inventory.cs` | 4 normal slots. Permanent clay slot is UI-only. |
| CraftingSystem | `Assets/Scripts/Craft/CraftingSystem.cs` | Dictionary lookup by `CraftKey` using `roomId` plus 2 element ids. Input order is normalized. |
| GameAudio | `Assets/Scripts/Music/GameAudio.cs` | Plays generated fallback sounds unless a `GameAudioProfile` provides custom clips. |
| GameOverController | `Assets/Scripts/Scene/GameOverController.cs` | Static `IsGameOver` pauses movement/interaction checks. |
| PauseMenuController | `Assets/Scripts/UI/PauseMenuController.cs` | Runtime Esc menu. `SceneInstaller` may auto-create it on the root Canvas if the Inspector field is empty. |
| Interaction hints | `Assets/Scripts/Interaction/InteractionHintOnInteract.cs` | Optional component for temporary text popups after interaction, including conditional rules for objects with hint state. |

## Data
- `ElementDefinition`: `Create > Golem Craft/Element Definition` (`id`, icon, optional personal UI background, world prefab, `discardOnTableClose`)
- `CraftRecipeDatabase`: `Create > Golem Craft/Craft Recipe Database` (`roomId` + 2 inputs -> 1 result)
- `GameAudioProfile`: `Create > Golem Craft/Audio/Game Audio Profile` (custom clips per `GameSoundId`)
- Element world models: `Assets/Game/Prefabs/ElementWorldModels/`
- Imported element wrappers: `Assets/Game/Art/ElementModels/`
- Default audio profile: `Assets/Game/Audio/DefaultGameAudioProfile.asset`

## Interaction Flow
`E` key -> `PlayerInteractor` raycast (2.4m) -> sphere fallback (1.8m) -> `IInteractable.Interact()` -> optional `InteractionHintOnInteract`.

## Crafting Mechanics
- Elements are always combined in pairs.
- Recipes are room-scoped.
- Input order does not matter.
- Cross-room element usage is allowed.
- When a craft panel closes, normal table items return to inventory if there is room; items with `discardOnTableClose` are discarded.

## Room Structure
| Room | ID | Description | Table | Sources | Recipes |
|---|---|---|---|---|---|
| 1 | `room_01` | Forge | Craft Table | iron, flint, coal, wood, air | yes |
| 2 | `room_02` | Wet room, boiler activated with torch | Craft Table | water, saltpeter, lime, vitriol | yes |
| 3 | `room_03` | Pantry, master workbench | Craft Table | herbs, berries, horn | yes |
| 4 | `room_04` | Alchemist study | Offering Table x1 | cinnabar, chest, yeast | no |
| 5 | `room_05` | Magic chamber finale | Offering Tables x4 | none | no |

## UI And Visuals
- UI is generated at runtime by `InventoryUI`, `CraftingPanelUI`, `TableItemUI`, `PauseMenuController`, and `UIFactory`.
- `InventoryUI` and `CraftingPanelUI` expose Inspector fields for panel style, sprites, colors, slot sizes, icon sizes, and text sizes.
- Pause menu is currently runtime-built: empty `SceneInstaller.pauseMenuController` is valid because `SceneInstaller` finds or creates `PauseMenuController` under `rootCanvas` during `Start()`.
- `ElementDefinition.Ui Background Mode` has priority over panel-level item backgrounds and follows the element in inventory, craft table UI, and drag previews.
- Future finished panel art should be assigned as sprites in these Inspector fields. Use sliced sprites when panel borders need to scale cleanly.

## Audio
- Existing code calls `GameAudio.Play(GameSoundId.X)`.
- If no custom clip is configured, generated fallback sounds play.
- Pause menu has separate sliders: `SFX` controls `GameAudio.SetMasterVolume`, `Music` controls room ambience through `AmbientMusicSwitcher.SetMusicVolume`.
- To override sounds, add entries to `Assets/Game/Audio/DefaultGameAudioProfile.asset` or another `GameAudioProfile`, then assign that profile through `SceneInstaller` or `PF_GameAudioController`.

## Key Progression
- `room_04`: place 3 key artifacts on the offering table to unlock `room_05`.
- `room_05`: place 1 key element on each of 4 offering tables to trigger the finale.

## Verification
- No automated tests.
- `dotnet build Assembly-CSharp.csproj` can catch C# compile errors when the generated `.csproj` is current.
- Final gameplay verification must be done in Unity Editor Play Mode.

## Key Quirks
- Unity Input System package is installed, but gameplay uses legacy `Input` API.
- `RoomTransitionTrigger` uses `#if UNITY_6000_0_OR_NEWER` for `linearVelocity`.
- `Esc` closes an open craft panel first. `CraftingPanelUI.LastEscapeCloseFrame` prevents the same key press from also opening pause.
- Craft table item backgrounds default to transparent; use `ElementDefinition.Ui Background Mode` for per-element backgrounds or `CraftingPanelUI.Table Item Background Mode` for panel fallback.
- `.csproj`, `.sln`, `Library/`, `Logs/`, `Temp/`, `UserSettings/`, `.dotnet/`, and generated local build folders should not be treated as hand-authored gameplay assets.
- Some systems use static singleton state (`Inventory.Instance`, `CraftingSystem.Instance`, `GameOverController.IsGameOver`, `GameAudio`). Reset/scene reload behavior should be checked in Play Mode.
