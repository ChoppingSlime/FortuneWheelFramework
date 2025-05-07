using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FortuneWheelFramework
{
    /// <summary>
    /// The main entry point for the Fortune Wheel Framework mod.
    /// </summary>
    public class ModEntry : Mod
    {
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Initialize the wheel API
            WheelAPI.Initialize(Monitor);

            // Register for events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Log startup message
            Monitor.Log("Fortune Wheel Framework initialized", LogLevel.Info);
        }

        /// <summary>
        /// Raised after the game is launched, right before the first update tick.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Register mod configuration
            RegisterModConfig();

            // Register API for other mods to use
            RegisterModApi();

            Monitor.Log("Fortune Wheel Framework API registered and ready for use by other mods", LogLevel.Info);
        }

        /// <summary>
        /// Registers the mod configuration.
        /// </summary>
        private void RegisterModConfig()
        {
            // No configuration needed for this framework mod yet.
            // Could add general settings like default wheel size, animation speed, etc. in the future.
        }

        /// <summary>
        /// Registers the API for other mods to use.
        /// </summary>
        private void RegisterModApi()
        {
            // Create an API instance for other mods to access
            Helper.ModRegistry.SetApi(this, new FortuneWheelApi(Monitor));
        }
    }

    /// <summary>
    /// API implementation that other mods can access.
    /// </summary>
    public class FortuneWheelApi
    {
        private readonly IMonitor Monitor;

        /// <summary>
        /// Constructs a new API instance.
        /// </summary>
        /// <param name="monitor">The monitor for logging.</param>
        public FortuneWheelApi(IMonitor monitor)
        {
            Monitor = monitor;
        }

        /// <summary>
        /// Shows a fortune wheel with the provided entries.
        /// </summary>
        /// <param name="entries">List of entries to display on the wheel.</param>
        /// <param name="title">Title to display above the wheel.</param>
        /// <param name="onResult">Callback that is invoked with the selected entry.</param>
        public void ShowWheel(System.Collections.Generic.List<WheelEntry> entries, string title, Action<WheelEntry> onResult)
        {
            // Forward the call to the static API
            WheelAPI.ShowWheel(entries, title, onResult);
        }

        /// <summary>
        /// Closes any active wheel menu.
        /// </summary>
        public void CloseWheel()
        {
            WheelAPI.CloseWheel();
        }

        /// <summary>
        /// Checks if a wheel menu is currently active.
        /// </summary>
        /// <returns>True if a wheel menu is currently displayed.</returns>
        public bool IsWheelActive()
        {
            return WheelAPI.IsWheelActive();
        }
    }
}