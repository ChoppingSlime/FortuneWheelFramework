using System;

namespace FortuneWheelFramework
{
    /// <summary>
    /// Represents an entry on the fortune wheel.
    /// </summary>
    public class WheelEntry
    {
        /// <summary>
        /// Unique identifier for the entry, used by the calling mod to recognize what was selected.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Text displayed on the wheel segment.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Detailed description shown below the wheel after selection.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Weight determining the size of the segment and probability (1-9, default 5).
        /// Higher weight = bigger segment = higher chance of selection.
        /// </summary>
        public int Weight { get; set; } = 5;

        /// <summary>
        /// Creates a new wheel entry with the specified parameters.
        /// </summary>
        /// <param name="id">Unique identifier for the entry</param>
        /// <param name="label">Text displayed on the wheel segment</param>
        /// <param name="description">Description shown after selection</param>
        /// <param name="weight">Weight value between 1-9 (default: 5)</param>
        public WheelEntry(string id, string label, string description, int weight = 5)
        {
            ID = id;
            Label = label;
            Description = description;
            Weight = Math.Clamp(weight, 1, 9); // Ensure weight is between 1 and 9
        }
    }
}