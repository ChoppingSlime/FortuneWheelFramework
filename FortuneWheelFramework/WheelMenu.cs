using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FortuneWheelFramework
{
    /// <summary>
    /// Menu that displays and handles the fortune wheel UI.
    /// </summary>
    public class WheelMenu : IClickableMenu
    {
        private readonly List<WheelEntry> entries;
        private readonly List<WheelSection> sections;
        private readonly string title;
        private readonly Action<WheelEntry> onResultSelected;
        private readonly IMonitor monitor;

        private bool spinning = false;
        private float spinSpeed = 0f;
        private float arrowRotation = 0f;
        private const float InitialSpinSpeed = 0.2f;
        private const float SpinDeceleration = 0.0015f;
        private WheelEntry selectedEntry = null;

        private Vector2 wheelCenter;
        private float wheelRadius;
        private readonly List<Color> sectionColors;

        private const int CloseButtonID = 101;

        /// <summary>
        /// Class representing a section of the wheel.
        /// </summary>
        private class WheelSection
        {
            public WheelEntry Entry { get; }
            public float StartAngle { get; }
            public float EndAngle { get; }
            public Color Color { get; }

            public WheelSection(WheelEntry entry, float startAngle, float endAngle, Color color)
            {
                Entry = entry;
                StartAngle = startAngle;
                EndAngle = endAngle;
                Color = color;
            }
        }

        /// <summary>
        /// Creates a new wheel menu with the specified entries.
        /// </summary>
        /// <param name="entries">List of entries to display on the wheel</param>
        /// <param name="title">Title to show above the wheel</param>
        /// <param name="onResult">Callback invoked when a result is selected</param>
        /// <param name="monitor">SMAPI monitor for logging</param>
        public WheelMenu(List<WheelEntry> entries, string title, Action<WheelEntry> onResult, IMonitor monitor)
            : base(0, 0, 0, 0)
        {
            this.entries = new List<WheelEntry>(entries);
            this.title = title;
            this.onResultSelected = onResult;
            this.monitor = monitor;

            // Initialize wheel dimensions
            wheelRadius = Math.Min(Game1.uiViewport.Width, Game1.uiViewport.Height) * 0.3f;

            // Center the wheel in the game window
            wheelCenter = new Vector2(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2f);

            // Set menu dimensions
            width = Game1.uiViewport.Width;
            height = Game1.uiViewport.Height;
            xPositionOnScreen = 0;
            yPositionOnScreen = 0;

            // Generate random colors for sections
            Random rand = new Random();
            sectionColors = new List<Color>();

            for (int i = 0; i < entries.Count; i++)
            {
                Color randomColor = new Color(
                    rand.Next(50, 230),  // R - avoid too dark or too bright
                    rand.Next(50, 230),  // G
                    rand.Next(50, 230)   // B
                );
                sectionColors.Add(randomColor);
            }

            // Create wheel sections
            sections = CreateWheelSections();

            // Add close button
            upperRightCloseButton = new ClickableTextureComponent(
                new Rectangle(
                    Game1.uiViewport.Width - 48,
                    Game1.tileSize / 4,
                    36, 36
                ),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                3f
            );
        }

        /// <summary>
        /// Creates wheel sections based on entry weights.
        /// </summary>
        private List<WheelSection> CreateWheelSections()
        {
            List<WheelSection> result = new List<WheelSection>();

            // Calculate total weight
            int totalWeight = entries.Sum(e => e.Weight);

            // Create wheel sections
            float currentAngle = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                // Calculate section size based on weight
                float sectionSize = (entries[i].Weight / (float)totalWeight) * MathHelper.TwoPi;

                // Create section
                result.Add(new WheelSection(
                    entries[i],
                    currentAngle,
                    currentAngle + sectionSize,
                    sectionColors[i]
                ));

                // Update current angle
                currentAngle += sectionSize;
            }

            return result;
        }

        /// <summary>
        /// Shows the wheel menu.
        /// </summary>
        public void Show()
        {
            Game1.activeClickableMenu = this;
        }

        /// <summary>
        /// Handles left-click events.
        /// </summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Close button
            if (upperRightCloseButton.containsPoint(x, y))
            {
                exitThisMenu();
                Game1.playSound("bigDeSelect");
                return;
            }

            // Check if click is within wheel
            float distance = Vector2.Distance(new Vector2(x, y), wheelCenter);
            if (distance <= wheelRadius && !spinning && selectedEntry == null)
            {
                // Start spinning
                spinning = true;
                spinSpeed = InitialSpinSpeed;
                Game1.playSound("powerup");
            }
        }

        /// <summary>
        /// Updates the menu each frame.
        /// </summary>
        public override void update(GameTime time)
        {
            base.update(time);

            if (spinning)
            {
                // Update arrow rotation
                arrowRotation += spinSpeed;

                // Slow down the spin
                spinSpeed -= SpinDeceleration;

                // Check if spin is complete
                if (spinSpeed <= 0)
                {
                    spinning = false;
                    spinSpeed = 0;

                    // Determine which section was selected
                    float angle = arrowRotation % MathHelper.TwoPi;
                    if (angle < 0) angle += MathHelper.TwoPi;

                    // Find the section containing this angle
                    selectedEntry = GetEntryAtAngle(angle);

                    // Report the selected section
                    monitor.Log($"Wheel landed on: {selectedEntry.Label}", LogLevel.Info);

                    // Play success sound
                    Game1.playSound("newRecord");

                    // Call the result callback if provided
                    onResultSelected?.Invoke(selectedEntry);
                }
            }
        }

        /// <summary>
        /// Finds the entry at a specific angle.
        /// </summary>
        private WheelEntry GetEntryAtAngle(float angle)
        {
            foreach (var section in sections)
            {
                if (angle >= section.StartAngle && angle < section.EndAngle)
                {
                    return section.Entry;
                }
            }

            // Fallback to first entry if no match found (shouldn't happen)
            return sections[0].Entry;
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void draw(SpriteBatch b)
        {
            // Draw background dimming
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);

            // Draw menu background
            drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 0, 96, 96),
                xPositionOnScreen + width / 2 - (int)wheelRadius - 64,
                yPositionOnScreen + height / 2 - (int)wheelRadius - 100,
                (int)wheelRadius * 2 + 128,
                (int)wheelRadius * 2 + 200,
                Color.White
            );

            // Draw title
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            b.DrawString(
                Game1.dialogueFont,
                title,
                new Vector2(
                    wheelCenter.X - titleSize.X / 2,
                    wheelCenter.Y - wheelRadius - 60
                ),
                Game1.textColor
            );

            // Draw wheel sections
            DrawWheel(b);

            // Draw the arrow
            DrawArrow(b);

            // Draw selected entry info
            if (selectedEntry != null && !spinning)
            {
                DrawSelectedEntryInfo(b);
            }

            // Draw close button
            upperRightCloseButton.draw(b);

            // Draw cursor
            drawMouse(b);
        }

        /// <summary>
        /// Draws the wheel with all its sections.
        /// </summary>
        private void DrawWheel(SpriteBatch b)
        {
            // Draw wheel background circle
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            b.Draw(
                pixel,
                new Rectangle(
                    (int)(wheelCenter.X - wheelRadius),
                    (int)(wheelCenter.Y - wheelRadius),
                    (int)(wheelRadius * 2),
                    (int)(wheelRadius * 2)
                ),
                null,
                Color.DarkGray,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0.9f
            );

            // Draw each section
            for (int i = 0; i < sections.Count; i++)
            {
                WheelSection section = sections[i];

                // Draw section
                DrawSectionTriangles(b, section);

                // Draw section label
                DrawSectionLabel(b, section);
            }

            // Draw wheel outline
            DrawCircleOutline(b, wheelCenter, wheelRadius, Color.Black, 4f);
        }

        /// <summary>
        /// Draws a single wheel section using triangles.
        /// </summary>
        private void DrawSectionTriangles(SpriteBatch b, WheelSection section)
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Number of triangles to approximate the section
            int triangleCount = 20;

            float angleStep = (section.EndAngle - section.StartAngle) / triangleCount;

            for (int i = 0; i < triangleCount; i++)
            {
                float startAngle = section.StartAngle + i * angleStep;
                float endAngle = startAngle + angleStep;

                // Triangle vertices
                Vector2 v1 = wheelCenter;
                Vector2 v2 = new Vector2(
                    wheelCenter.X + wheelRadius * (float)Math.Cos(startAngle),
                    wheelCenter.Y + wheelRadius * (float)Math.Sin(startAngle)
                );
                Vector2 v3 = new Vector2(
                    wheelCenter.X + wheelRadius * (float)Math.Cos(endAngle),
                    wheelCenter.Y + wheelRadius * (float)Math.Sin(endAngle)
                );

                // Draw triangle
                DrawTriangle(b, pixel, v1, v2, v3, section.Color);
            }
        }

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        private void DrawTriangle(SpriteBatch b, Texture2D texture, Vector2 v1, Vector2 v2, Vector2 v3, Color color)
        {
            // Draw lines between vertices
            DrawLine(b, texture, v1, v2, color, 1f);
            DrawLine(b, texture, v2, v3, color, 1f);
            DrawLine(b, texture, v3, v1, color, 1f);

            // Fill the triangle (approximate)
            Vector2 min = new Vector2(
                Math.Min(Math.Min(v1.X, v2.X), v3.X),
                Math.Min(Math.Min(v1.Y, v2.Y), v3.Y)
            );

            Vector2 max = new Vector2(
                Math.Max(Math.Max(v1.X, v2.X), v3.X),
                Math.Max(Math.Max(v1.Y, v2.Y), v3.Y)
            );

            for (int x = (int)min.X; x <= max.X; x++)
            {
                for (int y = (int)min.Y; y <= max.Y; y++)
                {
                    Vector2 point = new Vector2(x, y);
                    if (IsPointInTriangle(point, v1, v2, v3))
                    {
                        b.Draw(texture, new Rectangle(x, y, 1, 1), color);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a point is inside a triangle.
        /// </summary>
        private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // Barycentric coordinate method
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        private void DrawLine(SpriteBatch b, Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            Vector2 delta = end - start;
            float angle = (float)Math.Atan2(delta.Y, delta.X);
            float length = delta.Length();

            b.Draw(
                texture,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0
            );
        }

        /// <summary>
        /// Draws a circle outline.
        /// </summary>
        private void DrawCircleOutline(SpriteBatch b, Vector2 center, float radius, Color color, float thickness = 1f)
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            int segments = 64;
            float angleStep = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;

                Vector2 p1 = new Vector2(
                    center.X + radius * (float)Math.Cos(angle1),
                    center.Y + radius * (float)Math.Sin(angle1)
                );

                Vector2 p2 = new Vector2(
                    center.X + radius * (float)Math.Cos(angle2),
                    center.Y + radius * (float)Math.Sin(angle2)
                );

                DrawLine(b, pixel, p1, p2, color, thickness);
            }
        }

        /// <summary>
        /// Draws the label for a wheel section.
        /// </summary>
        private void DrawSectionLabel(SpriteBatch b, WheelSection section)
        {
            // Calculate middle angle of the section
            float middleAngle = (section.StartAngle + section.EndAngle) / 2;

            // Calculate position for the text, at about 75% of the radius from center
            Vector2 position = new Vector2(
                wheelCenter.X + wheelRadius * 0.75f * (float)Math.Cos(middleAngle),
                wheelCenter.Y + wheelRadius * 0.75f * (float)Math.Sin(middleAngle)
            );

            // Measure text size
            Vector2 textSize = Game1.smallFont.MeasureString(section.Entry.Label);

            // Draw text
            b.DrawString(
                Game1.smallFont,
                section.Entry.Label,
                position,
                Color.Black,
                middleAngle + MathHelper.PiOver2,  // Rotate text to align with segment
                textSize / 2,  // Center text on position
                0.8f,  // Scale
                SpriteEffects.None,
                1f
            );
        }

        /// <summary>
        /// Draws the spinning arrow.
        /// </summary>
        private void DrawArrow(SpriteBatch b)
        {
            // Draw the arrow (using a better in-game asset that looks like an arrow)
            // Position the arrow with its tip on the edge of the wheel, pointing inward
            float arrowLength = 50f; // Length of the arrow from center
            Vector2 arrowPosition = new Vector2(
                wheelCenter.X + (wheelRadius) * (float)Math.Cos(arrowRotation),
                wheelCenter.Y + (wheelRadius) * (float)Math.Sin(arrowRotation)
            );

            b.Draw(
                Game1.mouseCursors,
                arrowPosition,
                new Rectangle(421, 459, 11, 13),  // Arrow pointer from mouseCursors
                Color.White,
                arrowRotation - MathHelper.PiOver2, // Rotate to point inward toward center
                new Vector2(5.5f, 1), // Position pivot at the bottom of the arrow
                4f,  // Scale up the arrow
                SpriteEffects.None,
                1f
            );
        }

        /// <summary>
        /// Draws information about the selected entry.
        /// </summary>
        private void DrawSelectedEntryInfo(SpriteBatch b)
        {
            // Draw result title
            string resultText = $"Result: {selectedEntry.Label}";
            Vector2 resultSize = Game1.dialogueFont.MeasureString(resultText);

            b.DrawString(
                Game1.dialogueFont,
                resultText,
                new Vector2(
                    wheelCenter.X - resultSize.X / 2,
                    wheelCenter.Y + wheelRadius + 40
                ),
                Game1.textColor
            );

            // Draw description
            if (!string.IsNullOrEmpty(selectedEntry.Description))
            {
                string wrappedDescription = WrapText(selectedEntry.Description, (int)(wheelRadius * 2), Game1.smallFont);
                Vector2 descSize = Game1.smallFont.MeasureString(wrappedDescription);

                b.DrawString(
                    Game1.smallFont,
                    wrappedDescription,
                    new Vector2(
                        wheelCenter.X - descSize.X / 2,
                        wheelCenter.Y + wheelRadius + 80
                    ),
                    Game1.textColor
                );
            }
        }

        /// <summary>
        /// Wraps text to fit within a maximum width.
        /// </summary>
        private string WrapText(string text, int maxWidth, SpriteFont font)
        {
            string[] words = text.Split(' ');
            string wrappedText = "";
            string line = "";

            foreach (string word in words)
            {
                string testLine = line.Length > 0 ? line + " " + word : word;
                Vector2 size = font.MeasureString(testLine);

                if (size.X > maxWidth)
                {
                    wrappedText += line + "\n";
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }

            wrappedText += line;
            return wrappedText;
        }
    }
}