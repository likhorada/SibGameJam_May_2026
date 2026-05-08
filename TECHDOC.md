# Техническая документация проекта

Проект: Unity 6000.4.5f1, URP, 3D-игра про крафт голема. Основная сцена: `Assets/Scenes/Room_v3.unity`.

Документ отвечает на практические вопросы: где что лежит, как добавить контент, как настраивать звуки, UI, подсказки, модели и как проверять изменения.

## Быстрый старт

1. Открыть проект через Unity Hub.
2. Открыть сцену `Assets/Scenes/Room_v3.unity`.
3. Запустить Play Mode.
4. Проверить Console: должны появиться сообщения вроде `SceneInstaller: scene installed successfully` и `CraftingSystem: recipes loaded = ...`.

Главная точка связывания сцены - `Assets/Scripts/Scene/SceneInstaller.cs`. Он не создает игру с нуля, а связывает уже расставленные ссылки: `Inventory`, `CraftingSystem`, `CraftRecipeDatabase`, Canvas, UI и столы крафта.

## Локальная проверка C#

В проекте есть локальный .NET SDK в `.dotnet/`. Для проверки компиляции:

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.dotnet_home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_NOLOGO = '1'
.\.dotnet\dotnet.exe build Assembly-CSharp.csproj
```

Ожидаемые предупреждения:

- `CS0649` про serialized поля - нормально для Unity, потому что значения задаются через Inspector.
- Предупреждения MSBuild о версиях Unity assemblies - обычно не относятся к gameplay-коду.

Важно: `.csproj` генерируется Unity. Если добавлены новые `.cs` файлы, Unity может обновить проектные файлы только после открытия/перегенерации проекта.

## Основные папки

| Путь | Назначение |
|---|---|
| `Assets/Scripts` | Игровой C# код |
| `Assets/Game/Elements` | `ElementDefinition` для всех элементов |
| `Assets/Game/Crafting/MainCraftRecipeDatabase.asset` | База рецептов |
| `Assets/Game/Audio` | Профили звуков |
| `Assets/Game/Art/ElementSprites` | UI-иконки элементов |
| `Assets/Game/Prefabs/ElementWorldModels` | Простые runtime-модели элементов |
| `Assets/Game/Art/ElementModels` | Wrapper-prefab'ы для частей FBX |
| `Assets/Game/Prefabs` | Основные prefab'ы интерактивных объектов и UI |
| `Assets/Models/v3` | FBX уровней, материалы и текстуры |

## Архитектура

### SceneInstaller

`SceneInstaller` на `Start()`:

1. Находит или использует назначенные ссылки на `Inventory` и `CraftingSystem`.
2. Передает базу рецептов в `CraftingSystem`.
3. Настраивает `GameAudio`, если назначены `GameAudioProfile` или `AudioSource`.
4. Настраивает `InventoryUI`.
5. Настраивает меню паузы через `PauseMenuController`: использует назначенный scene-instance, ищет существующий объект под Canvas, создает instance из `pauseMenuPrefab` или использует runtime fallback.
6. Проходит по массиву `craftTables` и связывает физические столы с их `CraftingPanelUI`.

Если что-то не работает на старте, первым делом проверять ссылки в `SceneInstaller`.

### PauseMenuController

`PauseMenuController` отвечает за Esc-меню паузы. Основной asset для настройки: `Assets/Game/Prefabs/PF_PauseMenu.prefab`. В prefab лежит компонент `PauseMenuController` с Inspector-полями для размеров, текста, громкости и `UIImageStyle`-стилей backdrop, panel, buttons, slider background/fill/handle.

Сам overlay все еще собирается кодом через `rootCanvas`, `UIFactory`, `Button`, `Slider`, `Image` и `Text`, но теперь визуальные параметры берутся из prefab/scene-instance, а не захардкожены внутри метода сборки.

Почему в Inspector у `SceneInstaller` может быть пустое поле `Pause Menu Controller`, но меню все равно работает:

1. На `Start()` `SceneInstaller.InstallPauseMenu()` проверяет serialized поле `pauseMenuController`.
2. Если туда случайно назначен prefab asset, а не scene-instance, он создает instance этого prefab под `rootCanvas`.
3. Если поле пустое, он ищет `PauseMenuController` среди дочерних объектов `rootCanvas` через `GetComponentInChildren<PauseMenuController>(true)`.
4. Если такого объекта нет и назначен `pauseMenuPrefab`, он создает instance prefab под `rootCanvas`.
5. Если prefab тоже не назначен, он создает простой `GameObject("PauseMenuController")` под `rootCanvas` и добавляет компонент `PauseMenuController`.
6. Затем вызывает `pauseMenuController.Configure(rootCanvas)`.
7. `Configure` растягивает root объекта на весь Canvas, строит скрытый overlay `PauseMenuOverlay` дочерним объектом и применяет стартовые значения громкости.

Поэтому пустая ссылка `Pause Menu Controller` сейчас не ошибка. Для `Room_v3` правильный путь - оставить scene-instance override пустым и назначить `PF_PauseMenu` в поле `Pause Menu Prefab`. Runtime fallback без prefab остается только страховкой, чтобы меню не пропало при неполной настройке сцены.

Текущее поведение:

- `Esc` открывает меню паузы, если не открыт craft panel.
- Если открыт `CraftingPanelUI`, `Esc` сначала закрывает craft panel. `CraftingPanelUI.LastEscapeCloseFrame` не дает тем же нажатием открыть меню паузы в этот же кадр.
- При паузе `PauseMenuController.IsPaused = true`, `Time.timeScale = 0`.
- `PlayerMovement` и `PlayerInteractor` проверяют `PauseMenuController.IsPaused` и перестают читать движение/взаимодействие.
- `Resume` снимает паузу и возвращает `Time.timeScale = 1`.
- `Restart` снимает паузу и перезагружает активную сцену по `buildIndex`, а если его нет - по имени сцены.
- `Quit` в билде вызывает `Application.Quit()`, а в Unity Editor останавливает Play Mode.
- Ползунок `Music` вызывает `AmbientMusicSwitcher.SetMusicVolume` и меняет только эмбиент комнат.
- Ползунок `SFX` вызывает `GameAudio.SetMasterVolume` и меняет только UI/gameplay SFX.

Как подключать и настраивать меню паузы:

1. Открыть `SceneInstaller` в сцене.
2. Поле `Pause Menu Controller` использовать только если в сцене уже есть конкретный instance меню под Canvas и нужно явно выбрать именно его.
3. Поле `Pause Menu Prefab` должно ссылаться на `Assets/Game/Prefabs/PF_PauseMenu.prefab`. В `Room_v3` эта ссылка уже назначена.
4. В prefab `PF_PauseMenu` настраиваются `Panel Size`, `Button Size`, `Slider Size`, `Title Text`, размеры текста, стартовые `Music` / `SFX`.
5. Для кастомного арта раскрыть `Backdrop Style`, `Panel Style`, `Button Style`, `Slider Background Style`, `Slider Fill Style`, `Slider Handle Style` и назначить нужные sprites/colors. Для масштабируемых рамок использовать sliced sprites и `Image Type = Sliced`.
6. После изменения prefab проверить Play Mode: `Esc`, закрытие craft panel через `Esc`, restart, quit в Editor, оба slider'а, движение и `E` во время паузы.
7. Если позже понадобится полностью ручная UI-иерархия с заранее расставленными `Button`/`Slider`, расширить `PauseMenuController` serialized ссылками на готовые дочерние объекты, сохранив текущую prefab-driven сборку как fallback.

### Static singleton-состояние

В проекте несколько систем доступны через статические поля:

- `Inventory.Instance`
- `CraftingSystem.Instance`
- `GameOverController.Instance`
- `GameOverController.IsGameOver`
- `GameAudio` как статический сервис

Это удобно для game jam: любому скрипту легко обратиться к инвентарю или аудио. Риск в том, что такое состояние живет глобально, не передается явно через ссылки и может неожиданно сохраняться между перезапусками Play Mode, если включены нестандартные настройки domain reload. Поэтому после изменений в этих системах обязательно проверять restart, reload scene и повторный запуск Play Mode.

## Инвентарь

`Inventory` хранит только 4 обычных слота. Глина - постоянный UI-слот и не входит в массив слотов.

Ключевые методы:

- `AddElement(element)` - добавить элемент в первый свободный слот.
- `TrySetSlot(index, element)` - положить элемент в конкретный пустой слот.
- `TryClearSlot(index)` - очистить конкретный слот.
- `HasElement(element)` - проверить наличие.
- `TryConsumeElement(element)` - удалить один такой элемент.
- `ClearAllNormalSlots()` - очистить 4 обычных слота.

UI инвентаря:

- `InventoryUI` строит панель и слоты.
- `InventorySlotUI` отвечает за drag/drop.
- Постоянный слот глины использует `DragContext.BeginFromPermanentElement`.

Если инвентарь "ломается", проверить:

1. `Inventory.Instance` существует.
2. В сцене есть `EventSystem`.
3. `SceneInstaller.inventoryUI` и `SceneInstaller.rootCanvas` назначены.
4. Предметы со стола не занимают место в инвентаре, пока панель крафта открыта.
5. При закрытии стола есть свободные слоты для возврата предметов.

## Крафт

Все рецепты парные: 2 входа дают 1 результат. Рецепт определяется по:

- `roomId`;
- id первого элемента;
- id второго элемента.

Порядок входов не важен: `CraftKey` сортирует id элементов. `iron + wood` и `wood + iron` считаются одной парой.

Основные классы:

- `CraftRecipeDatabase` - список рецептов.
- `CraftRecipe` - одна запись рецепта.
- `CraftingSystem` - lookup по `CraftKey`.
- `CraftingPanelUI` - UI стола крафта.
- `TableDropArea` - область, куда бросают элементы.
- `TableItemUI` - элемент, лежащий на столе.

Если рецепт не работает:

1. Проверить `roomId` у `CraftTableInteractable`.
2. Проверить такой же `roomId` в `MainCraftRecipeDatabase`.
3. Проверить, что input A/input B/result назначены именно нужными `ElementDefinition`.
4. Проверить, нет ли дубликата с той же комнатой и той же парой.
5. Проверить Console на `No recipe`.

## Элементы

Элемент описывается `ElementDefinition`.

Поля:

- `id` - стабильный id для рецептов, например `iron`.
- `displayName` - имя в UI.
- `icon` - спрайт для UI.
- `fallbackColor` - цвет для заглушек.
- `uiBackgroundMode` - личный фон элемента в UI.
- `uiBackgroundStyle` - цвет/sprite личного UI-фона, если выбран `CustomStyle`.
- `worldPrefab` - 3D-модель для подношений.
- `worldScale` - финальный множитель размера в мире.
- `discardOnTableClose` - если true, элемент исчезает при закрытии craft table.

Как добавить элемент:

1. Создать `ElementDefinition`: `Create > Golem Craft/Element Definition`.
2. Заполнить `id`, `displayName`, `icon`.
3. Если элементу нужен личный UI-фон, настроить `UI Background`.
4. Назначить `worldPrefab`, если нужен 3D-визуал на offering table.
5. Если элемент должен исчезать при закрытии стола, включить `discardOnTableClose`.
6. Если элемент можно подбирать в комнате, создать объект с `ElementSourceInteractable` и collider.

### Личный UI-фон элемента

`ElementDefinition` может хранить фон, который будет появляться вместе с этим элементом в инвентаре, на craft table и в drag-preview.

`Ui Background Mode`:

- `None` - личного фона нет. Это default для всех существующих элементов.
- `FallbackColor` - фон берется из `fallbackColor` элемента.
- `CustomStyle` - фон берется из `Ui Background Style`: можно назначить sprite, цвет и тип `Image`.

Приоритет такой: сначала личный фон элемента, потом fallback конкретной панели. Поэтому если у элемента включен `Ui Background Mode`, он будет выглядеть одинаково на разных UI-панелях.

## 3D-модели элементов

Есть два типа моделей.

### Primitive prefab

Prefab из `Assets/Game/Prefabs/ElementWorldModels/` использует `ElementWorldModel` и строит простую форму из Unity primitives во время игры. Это надежный fallback, если импортированная модель сломалась.

### FBX-wrapper

Prefab из `Assets/Game/Art/ElementModels/` берет level FBX, ищет дочерний объект по `sourceChildName`, скрывает лишние renderer'ы и показывает только нужную модель.

Важные поля:

| Поле | Где | Когда менять |
|---|---|---|
| `ElementDefinition.worldScale` | `Assets/Game/Elements/<Element>.asset` | Быстро изменить размер конкретного элемента в игре |
| `ElementWorldModel.modelScale` | prefab модели | Изменить базовый размер всего primitive-prefab |
| `sourceModelFitSize` | FBX-wrapper | Подогнать размер выбранного FBX-child |
| `sourceModelOffset` | FBX-wrapper | Сместить модель относительно anchor |
| `sourceModelEulerAngles` | FBX-wrapper | Повернуть модель |
| `itemAnchors` | offering table | Изменить место предмета на конкретном столе |

Практическое правило: сначала менять `ElementDefinition.worldScale`, потом уже prefab модели. Не менять scale у самого offering table ради размера предметов.

## Offering tables

`OfferingTableInteractable` принимает список `requiredItems`. Для каждого элемента можно назначить `itemAnchors`.

Поведение:

- при взаимодействии ищется первый нужный элемент в инвентаре;
- элемент расходуется через `Inventory.TryConsumeElement`;
- на anchor создается `worldPrefab`;
- если `spawnedVisualsFollowAnchors = true`, визуал получает `FollowTransform` и следует за anchor, даже если anchor на анимированной части стола;
- когда все элементы поставлены, вызываются `Completed` и `onCompleted`.

Если предмет не появляется:

1. Проверить `requiredItems`.
2. Проверить `itemAnchors`.
3. Проверить `ElementDefinition.worldPrefab`.
4. Проверить Console.
5. Временно заменить `worldPrefab` на primitive-prefab.

## Аудио взаимодействий

Все игровые SFX вызываются через:

```csharp
GameAudio.Play(GameSoundId.UiClick);
```

или аналогичный `GameSoundId`.

Сейчас старые звуки сохраняются как fallback: если пользовательский клип не настроен, `GameAudio` генерирует короткий звук кодом.

### Как подставить свои звуки

1. Открыть `Assets/Game/Audio/DefaultGameAudioProfile.asset`.
2. В массив `Sounds` добавить запись.
3. Выбрать `Sound Id`, например `CollectElement`, `CraftSuccess`, `DoorOpen`.
4. Назначить `Audio Clip`.
5. Настроить `Volume` и `Pitch Range`.
6. Убедиться, что профиль назначен в `SceneInstaller` или что в сцене есть `Assets/Game/Prefabs/Music/PF_GameAudioController.prefab`.

Только заполненные `Sound Id` заменяются пользовательскими клипами. Все остальные продолжают играть fallback-звуки.

### Основные Sound Id

| Sound Id | Где используется |
|---|---|
| `UiClick` | Кнопки UI |
| `UiDrag` | Начало перетаскивания |
| `UiDrop` | Успешный drop |
| `TableOpen` / `TableClose` | Открытие и закрытие craft table |
| `CraftSuccess` / `CraftFail` | Результат крафта |
| `CollectElement` | Подбор элемента |
| `InventoryFull` | Нет места |
| `Activate` | Активация стола |
| `OfferingPlace` / `OfferingComplete` | Столы подношений |
| `DoorOpen` / `Locked` | Двери и закрытые объекты |
| `Trash` | Мусорка |
| `GameOver` | Game over |

## Фоновая музыка

`AmbientMusicSwitcher` работает по trigger-зонам. Когда игрок входит в collider зоны, соответствующий `AudioSource` становится текущей фоновой музыкой.

Если музыка не переключается:

1. Проверить trigger collider.
2. Проверить tag игрока.
3. Проверить `AudioSource`.
4. Проверить, не конфликтуют ли несколько зон.

## UI: инвентарь и стол крафта

UI создается runtime-кодом, но основные визуальные параметры вынесены в Inspector.

### InventoryUI

Настраивает:

- стиль панели;
- высоту панели;
- стиль слота;
- размер слота;
- стартовую позицию и шаг слотов;
- размер и позицию иконки;
- размер текста.

Иконки элементов увеличены относительно старой версии, чтобы не выглядеть слишком мелко.

Обычные слоты инвентаря поддерживают drag/drop между собой: drop на пустой слот переносит элемент, drop на занятый слот меняет элементы местами. Вечный слот глины не участвует в перестановке, потому что глина не хранится в обычном `Inventory`.

### CraftingPanelUI

Настраивает:

- размер и позицию панели;
- стиль панели;
- размер области стола;
- sprite/color области стола;
- размер item на столе;
- размер и позицию иконки item;
- размер подписи item.
- фон item на столе через `Table Item Background Mode`.

Элемент на столе сейчас выглядит компактнее и спокойнее, чем элемент в инвентаре: он лежит на рабочем фоне стола без обязательной плитки-слота. Это ожидаемый вид. Фон панели/области стола воспринимается как общий фон рабочего пространства, а личный фон элемента добавляется отдельно через `ElementDefinition`, если конкретному элементу нужен собственный бэк.

Если у `ElementDefinition` включен личный `Ui Background Mode`, этот фон имеет приоритет над настройками craft table.

`Table Item Background Mode`:

- `None` - фон item полностью прозрачный. Это текущий рабочий вариант: на столе видны только иконка и подпись.
- `ElementFallbackColor` - фон берется из `Fallback Color` в `ElementDefinition`. Это удобно, если нужно вернуть старые цветные плитки.
- `CustomStyle` - фон берется из `Table Item Style`: можно назначить цвет, sprite и тип изображения.

### Как добавить нормальный визуал панелей

1. Подготовить sprites для панели инвентаря, слота, панели крафта и области стола.
2. Если панели должны масштабироваться, открыть Sprite Editor и настроить borders.
3. В `InventoryUI` назначить sprites в `Panel Visual` и `Slot Visual`.
4. В `CraftingPanelUI` назначить sprites в `Panel Visual` и `Table Area Sprite`.
5. Если нужен фон у предметов на столе, поставить `Table Item Background Mode = CustomStyle` и настроить `Table Item Style`.
6. Если нужен фон только у конкретного элемента, открыть его `ElementDefinition`, поставить `Ui Background Mode = CustomStyle` и настроить `Ui Background Style`.
7. Для sliced-панелей оставить `Image Type = Sliced`.
8. Проверить Play Mode на разных разрешениях.

## Подсказки при взаимодействии

Для подсказок используются:

- `InteractionHintOnInteract` - компонент на интерактивном объекте;
- `InteractionHintWindow` - окно, которое показывает текст;
- `Assets/Game/Prefabs/PF_InteractionHintWindow.prefab` - готовый prefab окна.

`PF_InteractionHintWindow.prefab` необязателен. В `Room_v3` он назначен в `SceneInstaller.interactionHintWindowPrefab`, поэтому его `Window Style` работает как общий дефолт для всех подсказок. Если prefab не назначен и окна нет в сцене, первое взаимодействие создаст runtime-окно автоматически.

### Как добавить подсказку объекту

1. Убедиться, что объект уже имеет компонент с `IInteractable`.
2. Добавить `InteractionHintOnInteract`.
3. Заполнить `Message`.
4. Настроить `Max Show Count`.
5. Настроить `Placement`.
6. Проверить позицию через `World Offset` и `Screen Offset`.
7. Если этому объекту нужен свой фон/цвет/размер текста, раскрыть `Visual Override` и включить нужные `Override ...` флаги.

Если нужен один текст на каждое взаимодействие, достаточно блока `Default Hint`. Если нужны разные тексты для разных состояний объекта, использовать массив `Rules`.

### Поля InteractionHintOnInteract

| Поле | Значение |
|---|---|
| `Message` | Текст подсказки |
| `Max Show Count` | `1` - один раз, `0` - бесконечно, другое число - ограниченное количество |
| `Duration` | Сколько секунд подсказка висит |
| `Placement` | Центр экрана, рядом с объектом, custom world anchor или custom screen position |
| `Custom World Anchor` | Точка в мире, если выбран custom world placement |
| `World Offset` | Смещение от объекта/anchor |
| `Screen Offset` | Смещение в UI-координатах |
| `Visual Override` | Необязательное переопределение визуала подсказки именно для этого объекта |

### Визуал подсказки

`InteractionHintWindow` или `PF_InteractionHintWindow.prefab` задает общий внешний вид подсказок по умолчанию. Это удобно для базового стиля всей игры. Чтобы настройки prefab точно использовались, назначить `PF_InteractionHintWindow` в `SceneInstaller > Interaction Hint Window Prefab` или положить instance `InteractionHintWindow` в сцену.

Если отдельному объекту нужен другой вид, использовать `InteractionHintOnInteract > Visual Override`:

1. Включить только те флаги `Override ...`, которые нужно заменить.
2. `Override Window Style` меняет фон: можно задать цвет, sprite и `Image Type`.
3. `Override Window Size` меняет размер панели.
4. `Override Text Color`, `Override Font Size`, `Override Text Padding` меняют текст.

Если у объекта есть `Rules`, у каждого правила тоже есть `Visual Override`. Оно имеет приоритет над визуалом объекта. Это позволяет, например, показывать красный фон для "нет нужного предмета", зеленый фон для "активировано" и обычный фон для остальных подсказок.

Если ни один override не включен, используется стиль из `InteractionHintWindow`/prefab.

### Условные подсказки

`Rules` проверяются сверху вниз. Показывается первое правило, условие которого подходит. Если массив `Rules` не пустой и ни одно правило не подошло, подсказка не показывается.

Поля правила:

| Поле | Значение |
|---|---|
| `Condition` | Условие показа |
| `Message` | Текст именно для этого условия |
| `Max Show Count` | Сколько раз это правило может показаться |
| `Duration` | Сколько секунд висит этот текст |
| `Visual Override` | Необязательный стиль именно для этого правила; имеет приоритет над стилем объекта |

Условия для объектов с `IInteractionHintStateProvider`:

| Condition | Когда подходит |
|---|---|
| `InactiveMissingRequiredItem` | Объект неактивен, нужного элемента нет в инвентаре |
| `ActivatedByThisInteraction` | Это взаимодействие только что активировало объект |
| `AlreadyActive` | Объект был активен еще до взаимодействия |
| `InactiveHasRequiredItem` | Объект был неактивен, нужный элемент был в инвентаре |
| `Inactive` | Объект был неактивен, без проверки инвентаря |
| `Always` | Любое взаимодействие |

Сейчас `IInteractionHintStateProvider` реализует `CraftTableInteractable`. Если позже другому интерактивному объекту нужны такие же условия, ему нужно отдать подсказкам состояние активности и требуемый элемент через этот интерфейс.

Пример для котла во втором уровне:

1. Добавить `InteractionHintOnInteract` на объект котла/стола.
2. В `Rules` добавить 3 записи.
3. Первое правило: `ActivatedByThisInteraction` - текст вроде "Котел разгорелся".
4. Второе правило: `InactiveMissingRequiredItem` - текст вроде "Нужен факел".
5. Третье правило: `AlreadyActive` - текст вроде "Котел готов к работе".
6. Не использовать `Default Hint`, если все нужные состояния покрыты правилами.

## Game Over

`GameOverController` использует `GameOverController.IsGameOver`. Пока game over активен, движение и взаимодействие должны останавливаться.

`CraftGameOverTrigger` слушает событие `CraftingPanelUI.ElementCrafted` и вызывает game over, если создан заданный опасный элемент.

Как добавить game over на новый рецепт:

1. Создать элемент-результат.
2. Добавить рецепт в `MainCraftRecipeDatabase`.
3. На объекте с `CraftGameOverTrigger` назначить этот элемент в `gameOverElement`.
4. Проверить наличие `GameOverController` в сцене.

## Room transitions

`RoomTransitionTrigger` переносит игрока к `RoomSpawnPoint` и переключает камеру через `RoomCameraController`.

Как добавить переход:

1. Создать trigger collider.
2. Добавить `RoomTransitionTrigger`.
3. Назначить `targetSpawnPoint`.
4. Назначить `targetCameraPoint`.
5. Проверить, что spawn point не внутри стены/пола.

## Частые проблемы

| Симптом | Что проверить |
|---|---|
| Элемент не подбирается | Collider, дистанция `PlayerInteractor`, `ElementSourceInteractable.element`, свободный слот |
| Стол не открывается | `SceneInstaller.craftTables`, ссылка на panel, `startsActive`, activation element |
| Рецепт не работает | `roomId`, input A/B, result, дубликаты рецептов |
| Предмет не возвращается при закрытии стола | Свободные слоты, `discardOnTableClose`, предметы еще лежат на table UI |
| Drag/drop не работает | `EventSystem`, Canvas, raycast target Image у drop area |
| Offering visual не виден | `worldPrefab`, anchors, scale, Console errors |
| FBX-wrapper не виден | `sourceModel`, `sourceChildName`, `includeSourceNameVariants`, `sourceModelFitSize` |
| Звук не заменился | Профиль назначен, `Sound Id` совпадает, `AudioClip` не пустой |
| Подсказка не появляется | Компонент на объекте с `IInteractable`, `Message` не пустой, `Max Show Count` не исчерпан |

## Как тестировать игру в Unity

Даже если в финальной версии игроку не нужно вручную перезагружать сцену, reload и повторный запуск Play Mode полезны как проверка, что глобальные состояния и UI не остаются в сломанном виде после предыдущей игровой сессии.

### Базовый smoke-test

1. Открыть `Assets/Scenes/Room_v3.unity`.
2. Сохранить сцену, очистить Console и запустить Play Mode.
3. Проверить, что нет красных ошибок и `SceneInstaller` установил ссылки.
4. Проверить движение и взаимодействие через `E`.
5. Подобрать элемент, открыть craft table, перетащить элемент из инвентаря на стол.
6. Перетащить элемент между обычными слотами инвентаря и проверить перенос/обмен местами.
7. Проверить удачный рецепт и неудачную пару.
8. Закрыть стол и убедиться, что обычные предметы вернулись в инвентарь.
9. Проверить постоянную глину: она должна оставаться доступной и не занимать обычный слот.
10. Проверить offering table: предмет появляется на нужном anchor, дверь/прогрессия срабатывает.
11. Проверить звуки pickup/drop/craft/table open/table close.
12. Проверить подсказки на объектах с `InteractionHintOnInteract`, если они есть в сцене.
13. Для UI-фона элемента временно включить `Ui Background Mode` у одного `ElementDefinition` и проверить инвентарь, craft table и drag-preview.

### Проверка перезапуска и reload

1. Запустить Play Mode, подобрать предметы, открыть/закрыть стол, затем остановить Play Mode и запустить его снова.
2. Проверить, что инвентарь стартует чисто, движение работает, столы открываются, подсказки и звуки не дублируются.
3. Довести игру до game over и нажать restart, если этот путь используется в текущей сборке.
4. После restart проверить, что `Time.timeScale` снова `1`, `GameOverController.IsGameOver` сброшен, movement/interact работают.
5. Если в Project Settings включены нестандартные `Enter Play Mode Options`, отдельно проверить запуск с domain reload и без него. Без domain reload static-поля могут переживать остановку Play Mode.

### Что смотреть во время теста

- В Console не должно быть `NullReferenceException`, ошибок загрузки prefab/sprite/audio и повторяющихся ошибок каждый кадр.
- В Hierarchy не должны накапливаться лишние `GameAudio`, `InteractionHintWindow`, drag-объекты или предметы на столе после закрытия панели.
- На craft table в режиме `Table Item Background Mode = None` у предметов не должно быть видимого квадратного фона.
- Если у элемента включен личный `Ui Background Mode`, его фон должен быть одинаковым в инвентаре, на craft table и в drag-preview.
- UI должен читаться в Game view на `1920x1080`, а также на более узком размере окна.

## Что проверять перед коммитом

1. `git status --short` - понять, какие файлы изменены.
2. Не коммитить `Library/`, `Temp/`, `.dotnet/`, `.netfx-reference-assemblies/`.
3. Запустить compile-check через локальный `.dotnet`, если менялся C#.
4. Запустить Unity Play Mode, если менялась логика взаимодействий, UI, сцена, prefab или ScriptableObject.
5. Если менялись данные рецептов/элементов/прогрессии, обновить `DESIGN.md`, `DESIGNRU.md`, `TECHDOC.md` и при необходимости `AGENTS.md`.
