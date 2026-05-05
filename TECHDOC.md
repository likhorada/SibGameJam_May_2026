# Техническая документация проекта

Проект: Unity 6000.4.5f1, URP, 3D-игра про крафт голема. Основная сцена: `Assets/Scenes/Room_v3.unity`.

## Быстрый старт

1. Открыть проект через Unity Hub.
2. Открыть сцену `Assets/Scenes/Room_v3.unity`.
3. Запустить Play Mode.
4. Проверить Console: `SceneInstaller: scene installed successfully` и `CraftingSystem: recipes loaded = ...`.

Главная точка связывания сцены: `Assets/Scripts/Scene/SceneInstaller.cs`. Он ничего не создает с нуля, а связывает уже расставленные ссылки: `Inventory`, `CraftingSystem`, `CraftRecipeDatabase`, UI и столы крафта.

## Локальная проверка кода

В проект локально установлен .NET SDK 8.0 через `.dotnet/`. Он не требует системной установки и игнорируется git.

Команда проверки компиляции:

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.dotnet_home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_NOLOGO = '1'
.\.dotnet\dotnet.exe build Assembly-CSharp.csproj
```

Для `Assembly-CSharp.csproj` нужны reference assemblies `.NETFramework v4.7.1`. Они лежат локально в `.netfx-reference-assemblies/`, а путь к ним задает `Directory.Build.props`.

Ожидаемые предупреждения: `CS0649` про serialized поля. Это нормально для Unity, потому что поля заполняются через Inspector.

## Основные папки

| Путь | Назначение |
|---|---|
| `Assets/Scripts` | Игровой C# код |
| `Assets/Game/Elements` | `ElementDefinition` ScriptableObject для каждого элемента |
| `Assets/Game/Crafting/MainCraftRecipeDatabase.asset` | База рецептов |
| `Assets/Game/Prefabs/ElementWorldModels` | Простые runtime-модели элементов из primitives |
| `Assets/Game/Art/Models` | Wrapper-prefab'ы, которые показывают выбранные части level FBX |
| `Assets/Game/Art/ElementSprites` | UI-иконки элементов |
| `Assets/Models/v3` | Level FBX, материалы и текстуры художников |

## Элементы

Элемент описывается `ElementDefinition`:

- `id` — стабильный id, используется в рецептах. Пример: `iron`, `ancient_scroll`.
- `displayName` — имя для UI и логов.
- `icon` — UI-спрайт.
- `fallbackColor` — цвет для UI/заглушек.
- `worldPrefab` — 3D-модель, которую offering table показывает в мире.
- `worldScale` — дополнительный scale при спавне.
- `discardOnTableClose` — если true, предмет исчезает при закрытии craft table и не возвращается в инвентарь.

Чтобы добавить новый элемент:

1. Создать `ElementDefinition` через `Create > Golem Craft > Element Definition`.
2. Задать уникальный `id` в нижнем регистре.
3. Назначить `icon`.
4. Назначить `worldPrefab`.
5. Если элемент можно подобрать в комнате, создать объект с `ElementSourceInteractable` и указать этот `ElementDefinition`.

## 3D-модели элементов

Модель элемента назначается в `ElementDefinition.worldPrefab`. Сейчас есть два режима: простая generated primitive-модель и FBX-wrapper. Для обычной настройки размера чаще всего не нужно менять код.

### Primitive-модель

Prefab из `Assets/Game/Prefabs/ElementWorldModels/` содержит `ElementWorldModel` и `elementId`. На runtime скрипт строит форму из Unity primitives. Это надежный fallback: модель не зависит от импортированных FBX.

В primitive-prefab'е важны:

- `elementId` — какую форму строить.
- `modelScale` — базовый размер всей generated-модели.

### FBX-wrapper

Prefab из `Assets/Game/Art/Models/` тоже содержит `ElementWorldModel`, но дополнительно использует:

- `sourceModel` — level FBX prefab.
- `sourceChildName` — имя дочернего объекта внутри FBX.
- `sourceModelFitSize` — целевой размер модели.
- `includeSourceNameVariants` — включает варианты вроде `Wood.001`, `Chest.002`, `IngotCinnabar.010`.

Wrapper не копирует весь уровень в сцену визуально. Он инстанцирует FBX, оставляет включенными только renderers нужного дочернего объекта, центрирует и масштабирует их.

Важно: если wrapper-prefab правится вручную в YAML, `sourceModel` должен ссылаться на root `GameObject` импортированного FBX. Для текущих `Level-*-v3.fbx` это `fileID: 919132149155446097`. `fileID: 100100000` подходит для `m_SourcePrefab` в сцене, но не подходит для serialized `GameObject` поля компонента и приведет к невидимой модели.

### Как менять размер модели

Самый удобный и безопасный способ — менять `worldScale` в `ElementDefinition` конкретного элемента. Это финальный множитель, который применяется при спавне модели на offering table. Он подходит, когда модель в целом хорошая, но на столе выглядит слишком большой или слишком маленькой.

Примеры:

- модель в 2 раза больше: `worldScale = (2, 2, 2)`;
- модель в 2 раза меньше: `worldScale = (0.5, 0.5, 0.5)`;
- лучше почти всегда использовать равномерный scale, где `x = y = z`.

Когда использовать другие поля:

| Поле | Где находится | Когда менять |
|---|---|---|
| `ElementDefinition.worldScale` | `Assets/Game/Elements/<Element>.asset` | Быстрая настройка размера конкретного элемента в игре |
| `ElementWorldModel.modelScale` | prefab модели | Базовый размер всего prefab'а, если он слишком крупный/мелкий везде |
| `ElementWorldModel.sourceModelFitSize` | FBX-wrapper prefab | Размер выбранного FBX-child до финального `worldScale` |
| `ElementWorldModel.sourceModelOffset` | FBX-wrapper prefab | Сместить модель относительно anchor, если она висит не по центру |
| `ElementWorldModel.sourceModelEulerAngles` | FBX-wrapper prefab | Развернуть импортированную модель |
| `ElementWorldModel.sourceModelScale` | FBX-wrapper prefab | Тонкая настройка исходного FBX до auto-fit; обычно не нужна |

Практическое правило:

1. Сначала крутить `ElementDefinition.worldScale`.
2. Если все экземпляры этого prefab'а изначально неадекватного размера, крутить `modelScale`.
3. Для FBX-wrapper'ов, если auto-fit сделал объект неправильного размера еще до `worldScale`, крутить `sourceModelFitSize`.
4. Не менять scale у `PF_OfferingTable` или `itemAnchors` ради размера предметов. Это может снова привести к растяжению/смещению.

### Как менять положение и поворот модели

Если модель видна, но лежит неудобно:

- `sourceModelOffset` — подвинуть импортированную модель внутри wrapper'а.
- `sourceModelEulerAngles` — развернуть модель, например положить факел или повернуть сундук лицом к камере.
- `itemAnchors` на offering table — менять место размещения конкретного слота на столе.

Разница важная: `sourceModelOffset` исправляет сам prefab элемента, а `itemAnchors` исправляют композицию на конкретном столе.

Текущие FBX-wrapper'ы:

| Элемент | FBX child |
|---|---|
| `iron` | `IngotIron` |
| `coal` | `Coal` |
| `wood` | `Wood` |
| `torch` | `WallTorch` |
| `berries` | `Berry` |
| `herbs` | `Grass` |
| `paper` | `Paper_Pile1` |
| `chest` | `Chest` |
| `cinnabar` | `IngotCinnabar` |

Если wrapper-модель не видна:

1. Проверить, что `sourceModel` не `None`.
2. Проверить имя `sourceChildName` по FBX hierarchy.
3. Включить `includeSourceNameVariants`, если модель разбита на части с суффиксами.
4. Проверить, что `sourceModel.fileID` ссылается на root `GameObject` FBX, а не на `100100000`.
5. Уменьшить или увеличить `sourceModelFitSize`.
6. Если нужно быстро вернуть видимость, переключить `worldPrefab` элемента на primitive-prefab из `Assets/Game/Prefabs/ElementWorldModels`.

### Частые вопросы по моделям

| Вопрос | Что делать |
|---|---|
| Модель слишком маленькая на offering table | Увеличить `worldScale` у элемента |
| Модель слишком большая | Уменьшить `worldScale` у элемента |
| Только FBX-wrapper неправильного размера | Настроить `sourceModelFitSize` в `Assets/Game/Art/Models/PF_ImportedElement_*` |
| Модель стоит не в центре anchor | Настроить `sourceModelOffset` |
| Модель повернута боком/вверх ногами | Настроить `sourceModelEulerAngles` |
| Модель растянулась | Проверить, что предмет не parent'ится под объект с неравномерным scale; для offering table включить `spawnedVisualsFollowAnchors` |
| Нужна другая часть FBX | Изменить `sourceChildName`; если частей несколько с суффиксами, включить `includeSourceNameVariants` |
| Нужно быстро откатить проблемный импорт | В `ElementDefinition.worldPrefab` вернуть prefab из `Assets/Game/Prefabs/ElementWorldModels` |

## Инвентарь

`Inventory` хранит 4 обычных слота. Глина отображается в UI как постоянный слот и не занимает обычный слот.

Основные методы:

- `AddElement(element)` — добавить элемент в первый свободный слот.
- `HasElement(element)` — проверить наличие.
- `TryConsumeElement(element)` — удалить один элемент из инвентаря.
- `ClearAllNormalSlots()` — очистить 4 обычных слота.

UI инвентаря работает через `InventoryUI` и `InventorySlotUI`.

## Крафт

Все рецепты парные: 2 входа дают 1 результат. Порядок входов не важен.

Рецепт описывается в `CraftRecipeDatabase`:

- `roomId` — комната, где работает рецепт. Пример: `room_01`.
- `inputA`, `inputB` — два элемента.
- `result` — результат.

Ключ рецепта: `roomId + sorted(inputA.id, inputB.id)`.

Чтобы добавить рецепт:

1. Открыть `Assets/Game/Crafting/MainCraftRecipeDatabase.asset`.
2. Добавить элемент в список `recipes`.
3. Указать `roomId`, `inputA`, `inputB`, `result`.
4. Проверить в Play Mode через соответствующий craft table.

Если один и тот же набор элементов должен давать разные результаты в разных комнатах, добавить отдельные записи с разными `roomId`.

## Craft Table

`CraftTableInteractable` открывает UI крафта.

Важные поля:

- `tableId` — id стола.
- `roomId` — id комнаты для поиска рецептов.
- `craftingPanel` — UI-панель стола.
- `startsActive` — активен ли стол сразу.
- `requiredActivationElement` — элемент для активации.
- `consumeActivationElement` — расходовать ли элемент активации.

Пример: котел во второй комнате может требовать `torch`.

## Offering Table

`OfferingTableInteractable` принимает конкретные элементы по одному и спавнит их 3D-визуалы на anchor points.

Важные поля:

- `requiredItems` — какие элементы нужны.
- `itemAnchors` — куда ставить каждый элемент.
- `spawnedVisualsFollowAnchors` — если включено, spawned visuals следуют за anchors через `FollowTransform`.
- `onCompleted` — UnityEvent, вызывается когда все элементы поставлены.
- `Completed` — C# event для кода.

Визуалы не parent'ятся под root стола, поэтому не наследуют растянутый scale. Если `spawnedVisualsFollowAnchors` включен, они следуют за позициями и поворотами anchors в `LateUpdate`.

## Анимация offering table в room_04

Рекомендуемая схема для стола, который разъезжается и открывает проход:

1. У художников должен быть animated table prefab или объект из `Level-4` с `Animator`.
2. Логический `PF_OfferingTable` можно оставить невидимым и нерастянутым.
3. `itemAnchors` нужно положить дочерними объектами под те части стола, которые реально двигаются в анимации.
4. В `OfferingTableInteractable` оставить `spawnedVisualsFollowAnchors = true`.
5. В `onCompleted` добавить вызов на controller/Animator:
   - `Animator.SetTrigger("Open")`, или
   - метод контроллера вроде `OpenFinalPassage()`.
6. Проход в room_05 лучше открывать по Animation Event в конце клипа: отключить blocking collider, включить trigger перехода, открыть/скрыть визуальную заглушку.

Почему это работает: предметы не являются children растянутого логического стола, но каждый кадр следуют за anchors. Если anchor находится внутри animated table part, предмет двигается вместе с этой частью без деформации scale.

Минимальный контроллер можно сделать таким:

```csharp
using UnityEngine;

public sealed class Room04OfferingTableController : MonoBehaviour
{
    [SerializeField] private Animator tableAnimator;
    [SerializeField] private Collider finalRoomBlocker;
    [SerializeField] private GameObject finalRoomTransition;

    public void Open()
    {
        if (tableAnimator != null)
            tableAnimator.SetTrigger("Open");
    }

    public void UnlockFinalRoom()
    {
        if (finalRoomBlocker != null)
            finalRoomBlocker.enabled = false;

        if (finalRoomTransition != null)
            finalRoomTransition.SetActive(true);
    }
}
```

В `onCompleted` вызвать `Open()`. В конце animation clip вызвать `UnlockFinalRoom()` через Animation Event.

## Двери и переходы

`DoorInteractable` расходует нужный элемент из инвентаря и исчезает/отключается.

`RoomTransitionTrigger` переносит игрока к `targetSpawnPoint` и двигает камеру к `targetCameraPoint`. Игрок определяется по `PlayerMovement`.

При добавлении перехода:

1. Создать trigger collider.
2. Добавить `RoomTransitionTrigger`.
3. Указать `targetSpawnPoint`.
4. Указать `targetCameraPoint`.
5. Проверить, что `RoomCameraController` найден или назначен.

## Game Over

`GameOverController` использует static `IsGameOver`. Когда game over активен, движение и взаимодействие игрока должны останавливаться.

`CraftGameOverTrigger` слушает результат крафта и может запускать game over, если создан опасный элемент.

## UI

UI собирается runtime-кодом:

- `InventoryUI` строит слоты.
- `CraftingPanelUI` строит craft panel.
- `TableDropArea` принимает dragged элементы.
- `TableItemUI` отвечает за предметы на craft table.

Если меняется размер UI, сначала проверять serialized layout поля в `CraftingPanelUI` и `InventoryUI`.

## Практические вопросы по системам

### Как добавить новый источник элемента

1. Создать или выбрать `ElementDefinition`.
2. В сцене создать объект-источник.
3. Добавить `ElementSourceInteractable`.
4. В поле `element` назначить нужный `ElementDefinition`.
5. Убедиться, что у объекта есть collider, до которого достает `PlayerInteractor`.

Если игрок нажимает E, но ничего не происходит, проверить:

- объект находится в радиусе raycast/sphere fallback;
- collider не выключен;
- `ElementSourceInteractable.element` не `None`;
- инвентарь не полон.

### Как добавить новый craft table

1. Создать объект стола с collider.
2. Добавить `CraftTableInteractable`.
3. Создать или продублировать `CraftingPanelUI` для этого стола.
4. В `SceneInstaller.craftTables` добавить новую запись.
5. Указать `Table`, `Panel`, `tableId`, `roomId`.
6. Добавить рецепты с тем же `roomId` в `MainCraftRecipeDatabase`.

Если UI не открывается:

- проверить, что стол добавлен в `SceneInstaller.craftTables`;
- проверить, что `Panel` назначена;
- проверить Console на `CraftTableInteractable has no CraftingPanelUI`;
- убедиться, что `startsActive = true` или стол успешно активирован нужным элементом.

### Как сделать стол, который нужно активировать предметом

В `CraftTableInteractable`:

- выключить `startsActive`;
- назначить `requiredActivationElement`;
- выбрать `consumeActivationElement`.

Пример: если котел должен включаться факелом, назначить `torch` в `requiredActivationElement`. Если факел должен исчезать после активации, оставить `consumeActivationElement = true`.

### Как понять, почему рецепт не работает

Проверять в таком порядке:

1. У craft table правильный `roomId`.
2. В `MainCraftRecipeDatabase` есть рецепт с тем же `roomId`.
3. Оба input назначены и это именно нужные `ElementDefinition`, а не похожие дубликаты.
4. Result назначен.
5. Рецепт не конфликтует с другим рецептом с тем же `roomId` и той же парой input.

Порядок input не важен: `iron + wood` и `wood + iron` считаются одним рецептом.

### Как добавить новую комнату

1. Добавить визуальные объекты комнаты в сцену.
2. Добавить spawn point для входа игрока.
3. Добавить camera point для `RoomCameraController`.
4. Создать trigger перехода с `RoomTransitionTrigger`.
5. Указать `targetSpawnPoint` и `targetCameraPoint`.
6. Если в комнате есть craft table, добавить ему новый `roomId` и рецепты.

Если переход срабатывает странно:

- проверить, что trigger collider имеет `isTrigger`;
- проверить, что у игрока есть `PlayerMovement`;
- проверить, что `targetSpawnPoint` не внутри стены/пола;
- проверить, что `targetCameraPoint` назначен.

### Как открыть проход после события

Самый простой способ:

1. Заблокировать проход collider'ом или неактивным transition trigger.
2. Повесить метод открытия на `onCompleted` у `OfferingTableInteractable`, Animation Event или другой UnityEvent.
3. В методе отключить blocker и включить переход/визуал прохода.

Для анимированного стола лучше запускать анимацию сразу в `onCompleted`, а сам проход открывать Animation Event'ом в конце клипа.

### Как настроить item anchors на offering table

`itemAnchors` задают позиции, куда встанут предметы.

- Чтобы предметы стояли дальше друг от друга, двигать anchors.
- Чтобы предметы ехали вместе с анимированной частью стола, сделать anchor child этой части.
- Не использовать scale anchors для размера предметов; размер менять через `ElementDefinition.worldScale`.
- Для невидимого логического `PF_OfferingTable` важны collider и anchors, а не mesh renderer.

### Как работает постоянная глина

Глина — UI-only постоянный слот. Она не занимает один из 4 обычных слотов инвентаря. Если рецепт требует `clay`, UI крафта должен позволять использовать этот постоянный источник, а не искать глину в обычных слотах.

Если кажется, что глина "не тратится", это ожидаемое поведение. Для одноразовых ингредиентов использовать обычные элементы и `Inventory.TryConsumeElement`.

### Что делать, если инвентарь полон

В инвентаре только 4 обычных слота. Если pickup или закрытие craft table не возвращает предмет:

- освободить слот через trash bin;
- проверить, не лежат ли предметы на craft table UI;
- проверить, что предмет не помечен `discardOnTableClose`;
- проверить Console на `Inventory is full`.

### Как добавить game over на новый рецепт

1. Создать элемент результата, который должен завершать игру.
2. Добавить рецепт в `MainCraftRecipeDatabase`.
3. На объекте с `CraftGameOverTrigger` назначить этот элемент в `gameOverElement`.
4. Проверить, что `GameOverController` есть в сцене.

`CraftGameOverTrigger` слушает событие `CraftingPanelUI.ElementCrafted`, поэтому game over должен срабатывать именно после успешного крафта результата.

### Как диагностировать UI drag-and-drop

Если предмет не перетаскивается или не крафтится:

- проверить, что в сцене есть `EventSystem`;
- проверить, что Canvas назначен в `SceneInstaller`;
- проверить, что `InventoryUI` настроен;
- проверить, что craft panel активируется через `CraftTableInteractable`;
- проверить, что у `TableDropArea` есть raycast target Image.

### Когда нужно Reimport

Делать reimport папки или ассета в Unity, если:

- вручную менялись `.prefab`, `.asset`, `.meta`;
- менялись ссылки на FBX;
- Unity Inspector показывает старые значения;
- модель в Play Mode выглядит так, будто prefab не обновился.

Обычно достаточно reimport конкретной папки, например `Assets/Game/Art/Models`. `Reimport All` дольше, но помогает после больших YAML-изменений.

### Что проверять перед коммитом

1. `git status --short` — понять, какие файлы изменены.
2. Не коммитить `Library/`, `Temp/`, `.dotnet/`, `.netfx-reference-assemblies/`.
3. Запустить compile-check через локальный `.dotnet`.
4. По возможности проверить Play Mode в Unity.
5. Если менялись данные рецептов/элементов, проверить `DESIGN.md`, `DESIGNRU.md`, `TECHDOC.md`.

## Проверочный чеклист

После изменений в данных:

1. Открыть `Room_v3.unity`.
2. Запустить Play Mode.
3. Проверить Console на ошибки загрузки ассетов.
4. Проверить pickup элемента через E.
5. Проверить craft table: открыть, перетащить 2 элемента, получить результат.
6. Проверить offering table: поставить нужные элементы, убедиться что 3D-модели видны.
7. Для animated offering table: убедиться, что anchors двигаются вместе с частями стола, а spawned visuals следуют за ними.
8. Запустить compile-check через локальный `.NET SDK`.

## Частые проблемы

| Симптом | Что проверить |
|---|---|
| Элемент не крафтится | `roomId`, оба input, наличие рецепта в DB |
| Рецепт работает не в той комнате | `roomId` у `CraftTableInteractable` и в `CraftRecipeDatabase` |
| Предмет не появляется на offering table | `worldPrefab`, `itemAnchors`, Console errors |
| FBX-wrapper невидим | `sourceModel`, `sourceChildName`, `includeSourceNameVariants`, `sourceModelFitSize`; при ручном YAML проверить, что `sourceModel.fileID` равен root GameObject FBX, а не `100100000` |
| Модель растягивается | root/parent scale, `spawnedVisualsFollowAnchors`, не parent'ить визуал под растянутый mesh |
| Build через dotnet не стартует | использовать локальный `.dotnet`, `DOTNET_CLI_HOME`, проверить `.netfx-reference-assemblies` |
