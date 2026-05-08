# DESIGNRU.md - Golem Craft

## Обзор

`SibGameJam_May_2026` - 3D-игра про исследование комнат и крафт алхимических элементов. Игрок управляет глиняным големом, собирает элементы, соединяет их парами на столах крафта и использует результаты для открытия комнат, активации столов и финальных подношений.

## Основной цикл

1. Исследовать связанные комнаты.
2. Собирать базовые элементы из бесконечных источников клавишей `E`.
3. Открывать стол крафта и перетаскивать два элемента друг на друга.
4. Использовать созданные элементы для активации, открытия проходов или подношений.
5. Возвращаться к прежним комнатам за нужными ингредиентами.

## Управление

| Ввод | Действие |
|---|---|
| WASD / стрелки | Движение относительно камеры |
| E | Взаимодействие: raycast 2.4 м, затем fallback-сфера 1.8 м |
| Escape | Сначала закрыть панель крафта, иначе открыть/закрыть меню паузы |

## Игрок и инвентарь

- Движение работает через `Rigidbody`.
- Направление движения берется относительно активной камеры с наибольшим `depth`.
- В инвентаре 4 обычных слота.
- Глина всегда доступна через отдельный UI-слот. Она не хранится в массиве `Inventory` и не занимает обычный слот.

## Комнаты

| Комната | ID | Описание | Стол | Источники |
|---|---|---|---|---|
| Уровень 1 | `room_01` | Кузница: верстак, наковальня, печь | Стол крафта | iron, flint, coal, wood, air |
| Уровень 2 | `room_02` | Влажная комната: котел, активируется факелом | Стол крафта | water, saltpeter, lime, vitriol |
| Уровень 3 | `room_03` | Кладовая: главный верстак, органика | Стол крафта | herbs, berries, horn |
| Уровень 4 | `room_04` | Кабинет алхимика, открывает финал | 1 стол подношений | cinnabar, chest, yeast |
| Уровень 5 | `room_05` | Магическая комната, финал | 4 стола подношений | нет |

Все комнаты находятся в одной сцене `Assets/Scenes/Room_v3.unity` и соединены триггерами `RoomTransitionTrigger`.

## Интерактивные объекты

| Тип | Поведение |
|---|---|
| Источник элемента | Бесконечный pickup. Добавляет элемент в первый свободный слот. |
| Стол крафта | Открывает UI крафта или сначала требует элемент активации. |
| Дверь | Расходует нужный элемент, отключает блокирующий collider и скрывается/открывается. |
| Стол подношений | Принимает конкретные элементы по одному, создает 3D-визуалы и вызывает completion events. |
| Мусорка | Очищает 4 обычных слота инвентаря. Глина не затрагивается. |
| Подсказка | Необязательный `InteractionHintOnInteract`, показывает текст после взаимодействия. Может использовать правила по состоянию интерактивного объекта. |

## Система крафта

- Каждый рецепт состоит ровно из 2 входов и 1 результата.
- Рецепты привязаны к `roomId`.
- Порядок входов не важен: `CraftKey` сортирует id элементов.
- Элементы можно переносить между комнатами.
- В UI крафта можно перетаскивать элементы из обычных слотов, постоянного слота глины и со стола.
- При закрытии панели обычные элементы со стола возвращаются в инвентарь, если хватает места.
- Элементы с `discardOnTableClose` исчезают при закрытии панели.

## Базовые элементы

| Элемент | Где находится | Примечание |
|---|---|---|
| clay | всегда | Постоянный UI-слот |
| iron | `room_01` | Базовый металл |
| flint | `room_01` | Камень для искры |
| coal | `room_01` | Топливо |
| wood | `room_01` | Базовый материал |
| air | `room_01` | Универсальный реагент |
| water | `room_02` | Растворитель |
| saltpeter | `room_02` | Реактивный минерал |
| lime | `room_02` | Строительный материал |
| vitriol | `room_02` | Кислый минерал |
| herbs | `room_03` | Травы |
| berries | `room_03` | Органика |
| horn | `room_03` | Органический материал |
| yeast | `room_04` | Ферментация |
| cinnabar | `room_04` | Красный минерал |
| chest | `room_04` | Загадочный элемент |

## 3D-визуалы элементов

`ElementDefinition.worldPrefab` задает модель, которую столы подношений показывают в мире. Базовые runtime-модели лежат в `Assets/Game/Prefabs/ElementWorldModels/`. Импортированные wrapper-prefab'ы лежат в `Assets/Game/Art/ElementModels/` и могут показывать выбранный дочерний mesh из FBX уровня.

## Таблица рецептов

### `room_01` - кузница

| Вход A | Вход B | Результат |
|---|---|---|
| iron | flint | firesteel |
| firesteel | coal | fire |
| fire | wood | torch |
| clay | iron | ore |
| ore | fire | copper |
| copper | wood | mechanism |
| clay | wood | paper |
| air | clay | dust |

### `room_02` - влажная комната

| Вход A | Вход B | Результат |
|---|---|---|
| water | saltpeter | acid |
| water | berries | must |
| air | saltpeter | explosion |
| water | clay | dust |
| water | vitriol | poison |
| water | lime | gypsum |
| iron | vitriol | copper |

`explosion` запускает game over.

### `room_03` - кладовая

| Вход A | Вход B | Результат |
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

## Пути ключевых артефактов

### Артефакты для `room_04`

1. `ancient_scroll`: clay+wood=paper, paper+wood=blank_scroll, clay+iron=ore, ore+fire=copper, copper+wood=mechanism, mechanism+blank_scroll=ancient_scroll.
2. `philosopher_stone`: water+saltpeter=acid, water+lime=gypsum, acid+gypsum=base_stone, water+herbs=elixir, base_stone+elixir=philosopher_stone.
3. `magic_wand`: iron+flint=firesteel, firesteel+coal=fire, fire+wood=torch, clay+iron=ore, ore+fire=copper, torch+copper=magic_wand.

### Элементы финала для `room_05`

1. `spirit`: clay+iron=ore, ore+fire=copper, copper+wood=mechanism, mechanism+air=vessel, vessel+fire=spirit.
2. `mind`: water+berries=must, must+horn=wine, fire+water=alcohol, alcohol+herbs=tincture, tincture+wine=mind.
3. `body`: clay+iron=ore, ore+fire=copper, clay+copper=body.
4. `soul`: fire+water=alcohol, alcohol+herbs=tincture, water+berries=must, must+horn=wine, tincture+wine=soul_essence, soul_essence+elixir=soul.

## Game Over

Создание `explosion` в `room_02` запускает `GameOverController`. На экране появляется overlay с кнопкой рестарта, а геймплей ставится на паузу через `Time.timeScale = 0`.

## UI

Основной UI генерируется во время игры:

- `InventoryUI` строит нижнюю панель инвентаря.
- `InventorySlotUI` отвечает за drag/drop слотов.
- `CraftingPanelUI` строит overlay стола крафта.
- `TableDropArea` принимает перетаскиваемые элементы.
- `TableItemUI` отвечает за предметы на столе и крафт пары.
- `PauseMenuController` строит Esc-меню паузы по настройкам `PF_PauseMenu` или по scene-instance, назначенному в `SceneInstaller`.
- `InteractionHintWindow` показывает временные текстовые подсказки.

`InventoryUI`, `CraftingPanelUI` и `PauseMenuController` имеют поля Inspector для настройки визуала: спрайты панелей, кнопок и слайдеров, цвета, размеры слотов, размеры иконок и размеры текста. `ElementDefinition` также может задавать личный UI-фон элемента, который будет виден в инвентаре, на столе крафта и в drag-preview. Если личный фон не настроен, элементы на столе используют fallback панели через `Table Item Background Mode`.

Меню паузы использует `Assets/Game/Prefabs/PF_PauseMenu.prefab`. В нем есть `Resume`, `Restart`, `Quit` и отдельные ползунки `Music` / `SFX`. `Music` управляет эмбиентом комнат через `AmbientMusicSwitcher`, а `SFX` управляет интерфейсными и игровыми звуками через `GameAudio`.

## Аудио

- `AmbientMusicSwitcher` переключает фоновую музыку по комнатам/зонам.
- Звуки взаимодействий идут через `GameAudio.Play(GameSoundId.X)`.
- `GameAudio` по умолчанию использует сгенерированные fallback-звуки.
- `GameAudioProfile` может заменить любой `GameSoundId` настоящим `AudioClip`.

## Архитектура

- `SceneInstaller` связывает ссылки сцены на `Start()`.
- `SceneInstaller.pauseMenuController` - необязательный override для scene-instance. Если он пустой, `SceneInstaller` ищет меню под root Canvas, затем создает instance из `pauseMenuPrefab`, затем использует простой runtime fallback.
- Статическое singleton-состояние используется в `Inventory`, `CraftingSystem`, `GameOverController` и `GameAudio`.
- Данные лежат в ScriptableObject: `ElementDefinition`, `CraftRecipeDatabase`, `GameAudioProfile`.
- Автоматических gameplay-тестов нет. Проверка проводится в Unity Editor Play Mode.
- Пакет Unity Input System установлен, но gameplay пока использует legacy `Input`.
