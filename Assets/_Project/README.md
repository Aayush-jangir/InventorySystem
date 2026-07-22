# Inventory System
**Playmatrix Studio** — Plug-in-ready Unity system

---

## What This System Does

A complete, production-ready inventory system built for Unity 6.3 LTS and URP.
Drop it into any project and get a fully functional 29-slot inventory (24 grid + 5 hotbar),
drag-and-drop between slots, stack splitting, item tooltips, world drop and auto-pickup —
all driven by a static C# event hub and configured entirely through ScriptableObjects.

---

## Features

| Feature | Details |
|---|---|
| Grid Inventory | Configurable rows × columns (default 4×6 = 24 slots) |
| Hotbar | Dedicated slots (default 5), always visible, fills before grid |
| Total Capacity | Grid + Hotbar = 29 unique slots (fully configurable) |
| Item Stacking | Per-item max stack size, auto-stack on pickup |
| Stack Splitting | Right-click any stack → slider popup → split into two slots |
| Drag and Drop | Inventory ↔ Inventory, Inventory ↔ Hotbar, drag to world to drop |
| World Drop | Drag item outside inventory panel → item appears in world with arc animation |
| Auto Pickup | Player walks over world item → auto-collects into hotbar first, then grid |
| Tooltips | Hover any inventory or hotbar slot → item name, rarity, type, description |
| Rarity System | 5 tiers (Common → Legendary), color-coded borders, backgrounds, world items |
| Hotbar Selection | Click slot or press 1–5 keys to select, gold outline on active slot |
| ScriptableObject Data | All item data and config lives in SOs — zero hardcoded values |
| Object Pooling | WorldItem pool of 16 — no Instantiate/Destroy at runtime |
| Event Architecture | Static C# event hub — all systems decoupled and independently reusable |
| Mobile Controls | On-screen joystick for movement, BAG button to toggle inventory. Auto-shown on Android, hidden on PC and WebGL |
| Long Press to Split | Hold finger on any stack for 0.5s on mobile to open the split popup — same as right-click on PC |

---

## How to Install

### Step 1 — Required Packages

Open **Window → Package Manager** and confirm these packages are installed:

| Package | Version | Package ID |
|---|---|---|
| Universal Render Pipeline | 17.x | `com.unity.render-pipelines.universal` |
| Input System | 1.x | `com.unity.inputsystem` |
| TextMeshPro | Built into Unity 6.3 | `com.unity.ugui` |
| 2D Tilemap Editor | Built into Unity 6.3 | `com.unity.2d.tilemap` |

> ⚠️ Unity 6.3 LTS (6000.3.x) is required. Earlier versions are not supported.

### Step 2 — Copy the System Folders

Copy the following folders from this package into your project's `Assets/` folder:

```
Assets/
├── _Project/
│   ├── Prefabs/InventorySystem/     ← PFB_InventorySlot, PFB_HotbarSlot, PFB_WorldItem
│   ├── ScriptableObjects/InventorySystem/
│   │   ├── InventoryConfig_Default.asset
│   │   ├── ItemDatabase_Default.asset
│   │   └── (your ItemDataSO assets)
│   ├── Scripts/
│   │   ├── Systems/InventorySystem/  ← Core, Data, World folders
│   │   ├── UI/InventorySystem/       ← All UI scripts
│   │   └── Input/                    ← InventoryInputReader
│   ├── Input/                        ← InventoryInputActions asset
│   └── Scenes/
│       └── Demo_InventorySystem.unity
```

### Step 3 — Configure Input System

1. Go to **Edit → Project Settings → Player**.
2. Under **Other Settings → Active Input Handling**, set to **Input System Package (New)**.
3. Restart the editor when prompted.

### Step 4 — Confirm URP is Active

Go to **Edit → Project Settings → Graphics** and confirm a **URP Render Pipeline Asset** is assigned.

---

## How to Use

### Quick Start (5 minutes)

1. Open `Assets/_Project/Scenes/Demo_InventorySystem.unity`
2. Press **Play** — inventory is populated and fully functional immediately.
3. Press **I** to open and close the inventory panel.

### Add the System to Your Own Scene

**Step 1 — Create the GameManager:**

Create an empty GameObject named `GameManager`. Add these components:

| Component | Purpose |
|---|---|
| `InventorySystem` | Owns all 29 inventory slots |
| `HotbarSystem` | Manages hotbar slot selection |
| `InventoryInputReader` | Handles I key, hotkeys 1–5 |
| `InventoryDragController` | Manages drag-and-drop logic |
| `ItemDropper` | Spawns WorldItems from the pool |
| `DemoItemSpawner` *(optional)* | Populates inventory on Play for testing |

Wire references in the Inspector — each component has `[SerializeField]` fields clearly labelled.

**Step 2 — Create the Canvas:**

Create a **Canvas** (Screen Space — Overlay, 1920×1080 scale with screen size, match 0.5).

Add these children in order:

```
InventoryCanvas
├── InventoryUI_Root       ← InventoryUI script
│   └── InventoryPanel     ← toggled by InventoryUI
│       └── GridContainer  ← GridLayoutGroup
├── HotbarPanel            ← HotbarUI script, always visible
│   └── HotbarContainer    ← HorizontalLayoutGroup
├── Tooltip                ← TooltipUI script
├── SplitStackPopup_Root   ← SplitStackPopupUI script
│   └── PopupPanel
└── DragGhost              ← DragGhostUI script (MUST be last child)
```

**Step 3 — Assign the Player tag:**

Your player GameObject must have the **Player** tag. `WorldItem` uses `CompareTag("Player")` to detect pickup range.

**Step 4 — Wire Drop Spawn Point:**

On the `InventorySystem` component, drag your **Player transform** into the **Drop Spawn Point** field. Items dropped to world will appear near the player.

**Step 5 — Press Play.**

---

## How to Configure

All configuration lives in two ScriptableObjects. No code changes required.

---

### `InventoryConfigSO` — `InventoryConfig_Default.asset`

**Location:** `Assets/_Project/ScriptableObjects/InventorySystem/`

To create a new config: **Right-click → Create → PlayMatrix → Inventory System → Inventory Config**

#### Grid Settings

| Field | Default | What It Does |
|---|---|---|
| Rows | `4` | Number of grid rows in the inventory panel |
| Columns | `6` | Number of grid columns |
| Hotbar Slot Count | `5` | Number of dedicated hotbar slots |

> **Total slots = Rows × Columns + Hotbar Slot Count.** Change any of these and the system rebuilds automatically on Play.

#### Pickup Settings

| Field | Default | What It Does |
|---|---|---|
| Auto Pickup | `true` | Player auto-collects world items on contact (after cooldown) |
| Pickup Cooldown | `1.5s` | Seconds after dropping before an item can be picked up again. Prevents instant re-collection |

#### Rarity Colors

| Field | Default | What It Does |
|---|---|---|
| Common Color | `#C8C8C8` | Slot border and tint for Common items |
| Uncommon Color | `#1EAF45` | Slot border and tint for Uncommon items |
| Rare Color | `#4169E1` | Slot border and tint for Rare items |
| Epic Color | `#9B30FF` | Slot border and tint for Epic items |
| Legendary Color | `#FF8C00` | Slot border and tint for Legendary items |

---

### `ItemDataSO` — Individual Item Assets

**Location:** `Assets/_Project/ScriptableObjects/InventorySystem/`

To create a new item: **Right-click → Create → PlayMatrix → Inventory System → Item Data**

| Field | What It Does |
|---|---|
| Item Name | Display name shown in tooltips and split popup |
| Description | Tooltip description text |
| Icon | Sprite shown in inventory slots, hotbar, drag ghost, and world item |
| Item Type | `Weapon / Armor / Consumable / QuestItem / Misc` |
| Rarity | `Common / Uncommon / Rare / Epic / Legendary` — drives all color tinting |
| Max Stack Size | `1` = non-stackable. `>1` = stackable, right-click to split |
| World Prefab | Optional override prefab for this item's world appearance |

> If **Icon** is left empty, the system automatically tints the default sprite by rarity color. Assign a real sprite when your art is ready — no code changes needed.

---

### `ItemDatabaseSO` — Master Item Registry

**Location:** `Assets/_Project/ScriptableObjects/InventorySystem/`

To create: **Right-click → Create → PlayMatrix → Inventory System → Item Database**

Drag all your `ItemDataSO` assets into the **Items** array. Used by `DemoItemSpawner` and any future system (shop, quest rewards) that needs to look up items by name.

---

## Demo Scene

**Scene:** `Assets/_Project/Scenes/Demo_InventorySystem.unity`

Open and press **Play** — no setup required.

The demo scene demonstrates:

| Feature | How to Test |
|---|---|
| Inventory opens/closes | Press **I** |
| Hotbar selection | Press **1–5** or left-click a hotbar slot |
| Tooltips | Hover over any occupied slot |
| Drag and drop | Left-click drag any slot to another slot or hotbar |
| Swap items | Drag onto an occupied slot |
| Stack merging | Drag a partial stack onto another stack of the same item |
| Split stack | Right-click any stack of 2+ → adjust slider → Confirm |
| Drop to world | Drag any item outside the inventory panel |
| Auto pickup | Walk the player over a world item |
| Mobile controls | Enable Force Show In Editor on PFB_MobileControls to test joystick and BAG button in Editor |
| Split stack (mobile) | Hold finger on any stack for 0.5s to open the split popup |
| Rarity colors | All 5 rarities visible across the 6 demo items |

---

## Script Architecture Overview

```
InventoryEvents (static)         ← Central event hub — all systems talk through here
        │
        ├── InventorySystem      ← Owns all 29 slots. AddItem, RemoveFromSlot,
        │                           SwapSlots, SplitStack, DropSlot
        │
        ├── HotbarSystem         ← Maps hotbar slots to inventory slots 24-28
        │
        ├── ItemDropper          ← Object pool of WorldItems. Listens OnItemDroppedToWorld
        │
        ├── InventoryInputReader ← New Input System. Fires toggle + hotkeys
        │
        └── InventoryDragController ← Singleton. BeginDrag, UpdateDrag,
                                       EndDragOnSlot, EndDragOnHotbar, DropToWorld

UI Layer (reads events, never calls other UI directly):
        InventoryUI / InventorySlotUI
        HotbarUI / HotbarSlotUI
        TooltipUI / DragGhostUI / SplitStackPopupUI

Handlers on PFB_InventorySlot (4 components):
        InventorySlotUI           ← Displays item data
        InventorySlotDragHandler  ← Begin/Update/End drag
        InventorySlotTooltipHandler ← Hover tooltip
        InventorySlotContextHandler ← Right-click split popup

Handlers on PFB_HotbarSlot (5 components):
        HotbarSlotUI              ← Displays item + selection highlight
        HotbarSlotDropHandler     ← Accept drag from inventory
        HotbarSlotDragHandler     ← Begin/Update/End drag from hotbar
        HotbarSlotTooltipHandler  ← Hover tooltip
```

---

## Dependencies Summary

| Package | Version | Required |
|---|---|---|
| Unity Engine | 6.3 LTS (6000.3.x) | ✅ |
| Universal Render Pipeline | 17.x | ✅ |
| Input System | 1.x | ✅ |
| TextMeshPro | Built into Unity 6.3 | ✅ |
| 2D Tilemap Editor | Built into Unity 6.3 | ✅ (demo scene only) |

No paid Asset Store packages required.

---

## Platform Notes

| Platform | Status | Notes |
|---|---|---|
| PC (Windows) | ✅ Supported | Full keyboard + mouse input |
| Android | ✅ Supported | Touch controls via OnScreenStick and OnScreenButton. Set Default Orientation to Landscape Left in Player Settings before building. |
| WebGL | ✅ Supported | No threading used — fully WebGL-safe |

---

## Known Limitations

- **Icons are optional.** If `ItemDataSO.Icon` is null, the system tints a default white sprite by rarity color. Assign real sprite art when available — no code changes needed.
- **Stack split drops full stack.** Dragging a partial stack to the world drops all items in that slot. A shift-drag partial drop is a planned future feature.
- **No equipment slots.** Armor, weapons, and accessories are tracked by `ItemType` but there are no dedicated equip slots in this version. Add equip slots by extending `InventorySystem` with a separate `EquipmentSlot[]` array.
- **No save/load.** Inventory state is not persisted between sessions. Integrate with Project #3 (Save & Load System) when available.

---

## Namespace

All scripts use the namespace: `PlayMatrix.InventorySystem`

---

## Adding New Items (Future Workflow)

1. **Right-click → Create → PlayMatrix → Inventory System → Item Data**
2. Fill in name, description, type, rarity, max stack size
3. Assign an icon sprite (or leave null for rarity-color fallback)
4. Add the new asset to `ItemDatabase_Default.asset` Items array
5. Done — the item works in inventory, hotbar, tooltips, world drop, and auto-pickup automatically

---

*Built by Playmatrix Studio — Unity 6.3 LTS | URP | Input System | TextMeshPro*
