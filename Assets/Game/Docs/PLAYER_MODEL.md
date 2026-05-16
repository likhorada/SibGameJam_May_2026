# Player Model Setup

This project loads the player visual at runtime so the scene and `PF_Player` stay lightweight.

## Current Setup

- Source art reference folder: `graphics/модели/Character/`
- Runtime model asset: `Assets/Game/Resources/Characters/Golem/gogolem_re.fbx`
- Runtime animation fallback asset: `Assets/Game/Resources/Characters/Golem/gogolem.fbx`
- Runtime texture/material: `Assets/Game/Resources/Characters/Golem/GolemTexture.png` and `GolemTexture.mat`
- Loader component: `Assets/Scripts/Player/PlayerVisualAutoLoader.cs`
- Embedded clip player on `PF_Player`: `Assets/Scripts/Player/PlayerEmbeddedClipAnimator.cs`
- Additive procedural motion: `Assets/Scripts/Player/PlayerProceduralAnimator.cs`
- Player prefab: `Assets/Game/Prefabs/PF_Player.prefab`

`PF_Player` has `PlayerVisualAutoLoader` with `resourcePath = Characters/Golem/GolemVisual`, `fallbackResourcePath = Characters/Golem/gogolem_re`, and `animationFallbackResourcePath = Characters/Golem/gogolem`.

If a hand-authored `GolemVisual` prefab is added later, it will take priority. Until then Unity loads the imported FBX model directly from `Resources`.

## Что было найдено в `graphics`

В `graphics/модели/Character/` нет отдельных `.anim`, `.controller`, `.bvh` или файлов с именами `Idle` / `Walk`. Анимационные данные находятся внутри FBX:

- `gogolem.fbx`
- `gogolem_re.fbx`

Оба файла содержат FBX `AnimationStack`, `AnimationLayer` и много `AnimationCurve`, то есть это не просто статичная T-pose модель. Проблема была в том, что Unity модель загружалась как визуальный объект, но ни один компонент не проигрывал встроенные клипы.

## Как работает загрузка модели

1. `PF_Player` существует в сцене как gameplay-root: на нем остаются `Rigidbody`, collider, `PlayerMovement` и `PlayerInteractor`.
2. На старте `PlayerVisualAutoLoader.Awake()` пытается загрузить prefab/model из `Resources` по `resourcePath`.
3. Сейчас сначала проверяется `Characters/Golem/GolemVisual`. Это будущий удобный путь для вручную настроенного prefab.
4. Если `GolemVisual` не найден, загрузчик берет `fallbackResourcePath`, сейчас это `Characters/Golem/gogolem_re`.
5. Найденный asset создается дочерним объектом у `PF_Player`.
6. К дочерней модели применяются `localPosition`, `localEulerAngles`, `localScale`.
7. Если `hideBuiltInRenderer = true`, старый куб/primitive renderer на корне игрока скрывается, но collider и physics остаются.
8. Если `playEmbeddedAnimationClips = true`, загрузчик находит `PlayerEmbeddedClipAnimator` на `PF_Player` и передает ему загруженный visual-root.
9. Если `addProceduralAnimator = true`, загрузчик добавляет `PlayerProceduralAnimator` для легкого bob/sway поверх основной анимации.

## Как работает проигрывание FBX-анимаций

`PlayerEmbeddedClipAnimator` не требует Animator Controller.

1. Он получает путь модели, которую загрузил `PlayerVisualAutoLoader`.
2. Через `Resources.LoadAll<AnimationClip>(path)` он достает все embedded clips из FBX.
3. Если в основной модели клипы не найдены, он пробует `animationFallbackResourcePath`.
4. Скрипт пытается выбрать idle clip по `idleNameContains`, сейчас `idle`.
5. Скрипт пытается выбрать walk clip по `walkNameContains`, сейчас `walk`.
6. Если таких имен нет, он берет первый доступный embedded clip. Это важно для текущего FBX, где take names не выглядят как нормальные `Idle` / `Walk`.
7. Скрипт находится на `PF_Player`, поэтому его поля можно настраивать заранее в prefab Inspector.
8. После загрузки модели `PlayerVisualAutoLoader` вызывает `Configure(...)` и передает ему `visualRoot`.
9. Скрипт ищет `Animator` внутри загруженной модели и предпочитает тот, у которого уже есть `Avatar`.
10. Если `Animator` есть, но `Avatar = none`, Unity может показать модель и загрузить клипы, но кости останутся в T-pose. В этом случае нужно переимпортировать FBX с Rig-настройками `Animation Type = Generic` и `Avatar Definition = Create From This Model`.
11. Дальше он создает `PlayableGraph`, два `AnimationClipPlayable` и `AnimationMixerPlayable`.
12. Скорость движения берется из `Rigidbody` игрока.
13. Если игрок стоит, вес уходит в idle/первый клип на медленной скорости.
14. Если игрок движется быстрее `walkThreshold`, вес плавно уходит в walk/первый клип на нормальной скорости.
15. Клип вручную зацикливается через `SetTime(time % clip.length)`, чтобы не замирать после первого проигрывания.

В Play Mode в Console должна появиться строка вида:

```text
gogolem_re: loaded embedded animation clips
```

Ниже будут перечислены имена и длины клипов, которые Unity реально импортировала из FBX.

Если в этом же логе есть `Animator Avatar: none` или предупреждение `Animator has no Avatar`, значит клипы найдены, но Unity не может применить их к скелету. Открой `Assets/Game/Resources/Characters/Golem/gogolem_re.fbx`, вкладка `Rig`, выставь `Animation Type = Generic`, `Avatar Definition = Create From This Model`, нажми `Apply`, затем сделай то же для `gogolem.fbx`.

## Какие клипы использовать

Для текущего FBX нормальные кандидаты:

- `Armature|IDLE` - стояние.
- `Armature|WALK` - ходьба.
- `Armature|TAKE` - отдельное действие, можно позже подключить под взаимодействие/подбор.

Клипы с именами вида `Arm_L|...`, `Head|...`, `Leg_L|...` скорее всего являются частичными или служебными экспортами по отдельным костям. Для основного движения лучше начинать с клипов `Armature|...`.

`PlayerEmbeddedClipAnimator` выбирает клипы по подстрокам `idleNameContains` и `walkNameContains`. Если позже в FBX появятся другие имена, поменяй эти поля прямо на `PF_Player`.

## How To Replace The Player Model

1. Put the new FBX under `Assets/Game/Resources/Characters/<Name>/`.
2. Put textures/materials beside the FBX or in a nearby `Materials` folder.
3. Open `Assets/Game/Prefabs/PF_Player.prefab`.
4. On `PlayerVisualAutoLoader`, set `fallbackResourcePath` to the Resources path without file extension, for example `Characters/Golem/gogolem_re`.
5. Set `animationFallbackResourcePath` to another FBX with matching skeleton animation clips, if clips live in a separate file.
6. Tune `localPosition`, `localEulerAngles`, and `localScale` if the model appears offset, rotated, or too large.
7. Keep `hideBuiltInRenderer` enabled unless you intentionally want to see the old primitive cube.
8. Enter Play Mode and check movement, collisions, camera framing, interaction ray reach, and the clip list printed in Console.
9. If the model stays in T-pose while the clip list is printed, check the Console line `Animator Avatar`. It must not be `none`.

## How The Animation Works

`PlayerEmbeddedClipAnimator` plays the authored/imported FBX clips. `PlayerProceduralAnimator` is optional and only adds extra body bob/sway.

`PlayerProceduralAnimator` reads the parent `Rigidbody` velocity and blends between:

- idle: a small vertical breathing/bob motion;
- walk: stronger bob plus slight pitch/roll sway.

Tune these fields on a manually added `PlayerProceduralAnimator`, or on a custom `GolemVisual` prefab if you create one:

- `walkThreshold`: minimum planar speed before walk animation blends in.
- `blendSpeed`: how quickly idle/walk changes.
- `idleBobAmplitude` and `idleBobFrequency`: idle motion.
- `walkBobAmplitude` and `walkBobFrequency`: walk motion.
- `walkPitchDegrees` and `walkRollDegrees`: walk sway.

## How To Add Authored Animation Clips Later

1. Import a rigged FBX with clips into `Assets/Game/Resources/Characters/<Name>/`.
2. Create a `GolemVisual.prefab` in the same Resources folder.
3. Add the model as a child of that prefab.
4. Either rely on `PlayerEmbeddedClipAnimator`, or add `Animator` and an Animator Controller with `Idle` and `Walk` states.
5. If using a normal Animator Controller, disable `playEmbeddedAnimationClips` on `PF_Player`.
6. If using the embedded Playables loader, keep `playEmbeddedAnimationClips` enabled and tune `idleNameContains` / `walkNameContains` on the `PF_Player` `PlayerEmbeddedClipAnimator` if clip names are not `Idle` and `Walk`.
7. Either disable `addProceduralAnimator` on `PF_Player`, or keep it for extra squashy movement.
8. Update `PlayerVisualAutoLoader.resourcePath` to `Characters/<Name>/GolemVisual`.

The `GolemVisual` prefab path is preferred because it lets artists tune materials, scale, child objects, and Animator setup without changing scene objects.

## Verification Checklist

1. Open `Assets/Scenes/Room_v3.unity`.
2. Enter Play Mode.
3. Confirm the old cube renderer is hidden and the golem model appears.
4. Move with WASD and check that the embedded idle/walk animation blends start and stop cleanly. If `addProceduralAnimator` is enabled later, also check the extra bob/sway.
5. Press `E` near sources, craft tables, and offering tables to make sure the visual does not block interaction.
6. Check Room 4 and Room 5 camera framing after the model is visible.
