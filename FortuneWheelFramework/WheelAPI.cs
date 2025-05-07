using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace FortuneWheelFramework
{
    /// <summary>
    /// Static API class for the Fortune Wheel Framework mod.
    /// Provides methods for other mods to display and interact with the fortune wheel.
    /// </summary>
    public static class WheelAPI
    {
        private static IMonitor Monitor;
        private static WheelMenu ActiveMenu;

        /// <summary>
        /// Initializes the API with required SMAPI components.
        /// </summary>
        /// <param name="monitor">The SMAPI monitor for logging</param>
        internal static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        /// <summary>
        /// Shows a fortune wheel with the specified entries.
        /// </summary>
        /// <param name="entries">List of entries to display on the wheel</param>
        /// <param name="title">Title to show above the wheel</param>
        /// <param name="onResult">Callback invoked with the selected entry after spin completes</param>
        public static void ShowWheel(List<WheelEntry> entries, string title, Action<WheelEntry> onResult)
        {
            // Validate input
            if (entries == null || entries.Count == 0)
            {
                Monitor.Log("Cannot show wheel with empty entries list", LogLevel.Error);
                return;
            }

            // Close any existing wheel menu
            if (ActiveMenu != null && ActiveMenu.IsActive)
            {
                ActiveMenu.exitThisMenu();
                ActiveMenu = null;
            }

            // Create and show the new wheel menu
            ActiveMenu = new WheelMenu(entries, title, onResult, Monitor);
            ActiveMenu.Show();

            Monitor.Log($"Showing fortune wheel with title: {title} and {entries.Count} entries", LogLevel.Trace);
        }

        /// <summary>
        /// Closes the active wheel menu if one exists.
        /// </summary>
        public static void CloseWheel()
        {
            if (ActiveMenu != null && ActiveMenu.IsActive)
            {
                ActiveMenu.exitThisMenu();
                ActiveMenu = null;
                Monitor.Log("Closed fortune wheel menu", LogLevel.Trace);
            }
        }

        /// <summary>
        /// Checks if a wheel menu is currently active.
        /// </summary>
        /// <returns>True if a wheel menu is currently displayed</returns>
        public static bool IsWheelActive()
        {
            return ActiveMenu != null && ActiveMenu.IsActive;
        }
    }
}