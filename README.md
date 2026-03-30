# Card Sorting — Unity Card Game

A card game built with Unity3D and C# for the Zynga Card Sorting assignment. Implements Run, Set, and Smart sorting algorithms with a full game loop, animations, drag-and-drop, and a bot opponent.

---

###  Gameplay
[![Gameplay](https://img.youtube.com/vi/vJeW4MSbY0s/0.jpg)](https://youtu.be/vJeW4MSbY0s)

###  Dev Hands — Manual Test Scenarios
[![Dev Hands](https://img.youtube.com/vi/xqJOLp4MLS4/0.jpg)](https://www.youtube.com/watch?v=xqJOLp4MLS4)

Dev Hands lets you inject a specific hand directly into the game without dealing from the deck.  
Used to test exact sorting scenarios without waiting for a lucky deal.

###  Test Runner — Unit Tests
[![Test Runner](https://img.youtube.com/vi/0MMVBmiO7es/0.jpg)](https://www.youtube.com/watch?v=0MMVBmiO7es)

## Sorting Algorithms

All three algorithms live in `CardSorter.cs` — a static class with no state and no Unity dependencies. Input is a `List<CardData>`, output is a `SortResult`.

```csharp
public class SortResult
{
    public List<List<CardData>> Groups { get; set; }   // matched groups
    public List<CardData> UnmatchedCards { get; set; } // leftover cards
    public int UnmatchedPoints { get; set; }           // the score we minimize
}
```

Being stateless keeps unit tests straightforward — no scene, no setup, just call the method.

---

### Run Sort (1-2-3)

Finds groups of 3 or more consecutive cards of the same suit. Cards are sorted by rank, then scanned linearly — each card either extends the current run or closes it and starts a new one.

---

### Set Sort (7-7-7)

Finds groups of 3–4 same-rank cards, each from a different suit. Cards are grouped by rank; any group with 3 or more distinct suits forms a valid Set.

---

### Smart Sort

Runs all viable strategies and picks the result with the lowest unmatched score:

- **Run only / Set only** — single-pass baselines
- **Run → Set / Set → Run** — apply one, then run the other on the remainder
- **TrySplitRuns** — tries cutting a long run into two shorter ones to free a card for a Set
- **TryTrimSets** — tries dropping one card from a 4-card Set if it scores better in a Run

---

### Manual Sorting

You can arrange cards by hand without using the sort buttons. Drag a card and drop it between any two cards — the hand is re-evaluated on the spot and valid Run or Set groups are highlighted with colors.

Two linear passes: first consecutive same-suit sequences, then same-rank groups (skipping positions already claimed by a Run). Each card is counted in at most one group.

---

## Performance

The main GC optimization is the `CardPool` — avoiding `Instantiate`/`Destroy` on Card MonoBehaviours, where Unity's per-frame overhead accumulates.

### Sprite Atlas

All 52 card face sprites per theme are packed into a single `SpriteAtlas`. One atlas per theme (`Atlas_0`, `Atlas_1`, `Atlas_2`).

Without an atlas, each card face is a separate texture draw call. With atlases, all cards in a theme share one texture.

Textures are compressed with **ASTC 6x6** on Android and iOS, cutting memory roughly in half with no visible quality difference at card sizes.

---

### Addressables

Card themes are loaded via **Unity Addressables** rather than a serialized array on `ThemeManager`.

The original approach kept all three themes in memory the entire session — 52 × 3 = 156 sprites loaded at startup whether the player ever switched themes or not. With Addressables, only the active theme is resident. When the player switches, the new theme loads async and the old one is released. Uses `AssetReferenceT<CardTheme>` instead of a string key, so references are set via drag-and-drop in the Inspector.

## Architecture

Two namespaces, one physical folder (`Assets/Scripts/Core/`):

- **`CardGame.Core`** — Pure C#. No Unity imports. Deck, CardData, CardSorter, PlayerHand, GameEvents. Runs in unit tests without a scene.
- **`CardGame.UI`** — MonoBehaviours. Visuals, animations, input.
- **`CardGame`** — GameManager and GameUIManager sit here as top-level orchestrators.

The hand system is split into four focused classes:

```
HandController   <- orchestrator, owns the game logic decisions
PlayerHand       <- data: the card list, score, worst card
HandLayout       <- visuals: arc positions, group colors, animations
DragHandler      <- input: drag, hover, discard zone
```

HandLayout has no knowledge of game state. DragHandler has no knowledge of scoring.

---

## Scripts

### Core

| Script | Responsibility |
|---|---|
| `CardData.cs` | Immutable card value object. Suit, Rank, Id, Points. |
| `Deck.cs` | 52-card deck. Shuffle, Deal, Reset. No UI dependency. |
| `CardSorter.cs` | Run, Set, Smart sorting + manual group evaluation. |
| `PlayerHand.cs` | Card list, score calculation, worst card detection. |
| `GameEvents.cs` | Static event bus. Decouples all systems. |
| `SortType.cs` | Enum: Run, Set, Smart. |
| `CardTheme.cs` | ScriptableObject: 52 face sprites, back sprite, theme name. |
| `SaveManager.cs` | JSON persistence for theme and sound settings. |
| `LocalizationManager.cs` | Loads UI strings from a JSON file. Singleton. |

### UI — Game Flow

| Script | Responsibility |
|---|---|
| `GameManager.cs` | State machine. Turn flow, timer, win/lose conditions. |
| `GameUIManager.cs` | Sort buttons, smart sort toggle, score display, panel management. |
| `BotController.cs` | Bot turn: draw from deck, discard a random card. |

### UI — Hand System

| Script | Responsibility |
|---|---|
| `HandController.cs` | Orchestrator. Deal, add card, discard, apply sort. |
| `HandLayout.cs` | Arc positions, group colors, sort animations. |
| `DragHandler.cs` | Drag, hover, preview insert, discard zone detection. |
| `CardPool.cs` | Object pool for Card MonoBehaviours. |

### UI — Cards & Table

| Script | Responsibility |
|---|---|
| `Card.cs` | Card view. Flip, theme apply, group color, drag callbacks. |
| `DeckView.cs` | Deck visual. Reuses a single Card instance for deal animation — no Instantiate per deal. |
| `DiscardArea.cs` | Discard pile with random offset/rotation per card. |

### UI — Panels & Navigation

| Script | Responsibility |
|---|---|
| `BasePanel.cs` | Abstract base with shared show/hide DOTween animations. |
| `StartPanel.cs` | Start screen before each game. |
| `ResultPanel.cs` | Win/Lose/Draw screen with play again. |
| `SettingsPanel.cs` | Theme picker and sound toggles. |
| `HowToPlayPanel.cs` | Rules screen. |
| `GameMenuPanel.cs` | In-game hamburger menu. |
| `MainMenuManager.cs` | Main menu scene. |
| `SceneTransition.cs` | Fade transition between scenes. Singleton, DontDestroyOnLoad. |
| `WarningPopup.cs` | Slides in from top to show contextual game warnings. |

### UI — Misc

| Script | Responsibility |
|---|---|
| `ThemeManager.cs` | Loads CardTheme via Addressables, releases previous theme on switch. Singleton. |
| `CardThemeButton.cs` | Theme selector button in Settings. |
| `SoundManager.cs` | SFX pool + music loop. Singleton, DontDestroyOnLoad. |
| `FloatingCards.cs` | Idle float animation on main menu. |

---

## Game Flow

```
Main Menu
    ↓ Play
Game Scene
    ↓ Deal (11 cards each + 1 to discard pile)
FirstDiscardOffer — pick the top discard or pass
    ↓
PlayerTurn — draw from deck or discard pile, then discard one card
    ↓
BotTurn — draws from deck, discards randomly
    ↓ (repeat until win, lose, or draw)
GameOver
```

Win condition: player score ≤ 10 after discarding worst card.
Draw: deck runs out.
Timer: 30 seconds per turn. On timeout, auto-draws and discards the worst card.

---

## Event System

`GameEvents.cs` is a static class that acts as a shared message board. Instead of systems calling each other directly, they post and listen to events — so `HandController` never needs to know that `GameUIManager` exists, and vice versa.

```csharp
GameEvents.OnDeckClicked += OnDeckClicked;   // Awake   — start listening
GameEvents.DeckClicked();                     //         — post the message
GameEvents.OnDeckClicked -= OnDeckClicked;   // OnDestroy — stop listening
```

Subscribe in `Awake`, unsubscribe in `OnDestroy`. This matters because a card sitting in the discard pile with its MonoBehaviour disabled still needs to receive theme updates — Awake/OnDestroy pairing handles that correctly.

---

## Theme System

Themes are `ScriptableObject` assets loaded via Addressables (`AssetReferenceT<CardTheme>`). Each holds 52 face sprites and a card back. Switching themes fires `ThemeManager.OnThemeChanged`, which every active `Card` picks up and re-applies immediately — including cards in the discard pile.

---

## Warning System

Invalid actions — drawing when a card is already drawn, discarding before drawing, dragging to the discard zone out of turn — show a contextual warning popup rather than failing silently.

`WarningPopup` listens to `GameEvents.OnWarningShown` and slides in from the top of the screen, holds briefly, then slides back out. If a new warning fires while one is already showing, the current sequence is killed and the popup resets immediately.

Warning strings come from `LocalizationManager` via `Resources/Localization/en.json`:

```json
{ "key": "warning_draw_first", "value": "Draw a card first" }
```

---

## Testing

Unity Test Runner, Edit Mode. Tests call `CardSorter` directly — no scene needed.

The assignment's example hand is one of the test cases:

```
Hand: Ace of Hearts, 2 of Spades, 5 of Diamonds, 4 of Hearts, Ace of Spades,
      3 of Diamonds, 4 of Clubs, 4 of Spades, Ace of Diamonds, 3 of Spades, 4 of Diamonds

Expected Smart Sort result:
  Group 1: Ace of Spades, 2 of Spades, 3 of Spades    (Run)
  Group 2: 4 of Spades, 4 of Hearts, 4 of Clubs        (Set)
  Group 3: 3 of Diamonds, 4 of Diamonds, 5 of Diamonds (Run)
  Unmatched: Ace of Hearts, Ace of Diamonds             (2 points)
```

---

## Built With

- Unity 6 (6000.3.11f1)
- DOTween
- TextMeshPro
- Unity Test Runner
- Unity Addressables
- Unity Sprite Atlas

---