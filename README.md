# Fortune Wheel Framework

A Stardew Valley mod framework that allows other mods to display and operate a spinning wheel for random selection.

## Overview

Fortune Wheel Framework is a utility mod designed to be used by other Stardew Valley mods. It provides a simple API for displaying an interactive spinning wheel that randomly selects from a provided list of entries. This can be used for daily rewards, random challenges, quest rewards, or any other scenario where you want to add an element of chance with visual feedback.

## Features

- Simple API for other mods to use
- Visually appealing spinning wheel interface
- Weighted entries (more likely items get larger wheel segments)
- Customizable wheel entries with labels and descriptions
- Animation with realistic deceleration
- Callback support for handling selected results

## For Mod Developers

### Installation

1. Add a dependency to your `manifest.json`:
```json
"Dependencies": [
  {
    "UniqueID": "YourName.FortuneWheelFramework",
    "MinimumVersion": "1.0.0"
  }
]
```

2. Get the API in your mod's entry method:
```csharp
private FortuneWheelApi wheelApi;

public override void Entry(IModHelper helper)
{
    helper.Events.GameLoop.GameLaunched += (s, e) => {
        wheelApi = helper.ModRegistry.GetApi<FortuneWheelApi>("YourName.FortuneWheelFramework");
    };
}
```

### Usage

Show a wheel with entries:

```csharp
// Create wheel entries (ID, Label, Description, Weight)
List<WheelEntry> entries = new List<WheelEntry>
{
    new WheelEntry("item_1", "Gold Coin", "Receive 100 gold", 5),
    new WheelEntry("item_2", "Diamond", "A precious gem", 2),
    new WheelEntry("item_3", "Energy Potion", "Restores 50 energy", 3)
};

// Show the wheel with a title and callback for handling results
wheelApi.ShowWheel(entries, "Daily Rewards", result => {
    // Handle the selected result
    if (result.ID == "item_1") {
        Game1.player.Money += 100;
    }
});
```

### API Methods

- `ShowWheel(List<WheelEntry> entries, string title, Action<WheelEntry> onResult)`: Displays the wheel with the specified entries
- `CloseWheel()`: Closes any active wheel menu
- `IsWheelActive()`: Checks if a wheel menu is currently displayed

### Understanding Wheel Entries

Each `WheelEntry` represents an item on the wheel and has these properties:

- `ID`: Used by your mod to identify what was selected (not shown to player)
- `Label`: Short text displayed on the wheel segment
- `Description`: Longer text shown after selection
- `Weight`: An integer from 1-9 determining segment size and selection probability (higher = more likely)

## Project Structure

- `WheelEntry.cs`: Defines the class for wheel entries
- `WheelAPI.cs`: Contains the static API used by other mods
- `WheelMenu.cs`: Handles UI rendering and wheel animation
- `ModEntry.cs`: SMAPI entry point and API registration

## Example Implementation

See `SampleUsage.cs` for a complete example of how another mod could implement the Fortune Wheel.

## Compatibility

- Stardew Valley 1.5.6 or later
- SMAPI 3.0.0 or later

## License

This mod is released under the MIT License.