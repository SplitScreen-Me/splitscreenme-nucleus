using System;
using System.Collections.Generic;
using System.Drawing;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>Pure helpers of the snapshot mapping.</summary>
    internal static class SnapshotMath
    {
        public static Rect ToRect(Rectangle r) => new Rect { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height };

        /// <summary>Contract layout of a Nucleus screen type (same members, same order).</summary>
        public static ScreenLayout ToLayout(UserScreenType type) => (ScreenLayout)Enum.Parse(typeof(ScreenLayout), type.ToString());

        /// <summary>Nucleus screen type of a contract layout.</summary>
        public static UserScreenType ToScreenType(ScreenLayout layout) => (UserScreenType)Enum.Parse(typeof(UserScreenType), layout.ToString());

        /// <summary>
        /// Rows and columns of a layout (UserScreen.UpdateSubBounds): custom uses the custom layout lines + 1, manual is a
        /// square grid whose size comes from the number of slots.
        /// </summary>
        public static (int rows, int columns) Grid(UserScreenType type, int customHorizontalLines, int customVerticalLines, int slotCount)
        {
            switch (type)
            {
                case UserScreenType.DualHorizontal: return (2, 1);
                case UserScreenType.DualVertical: return (1, 2);
                case UserScreenType.FourPlayers: return (2, 2);
                case UserScreenType.SixPlayers: return (2, 3);
                case UserScreenType.EightPlayers: return (2, 4);
                case UserScreenType.SixteenPlayers: return (4, 4);
                case UserScreenType.Custom: return (customHorizontalLines + 1, customVerticalLines + 1);
                case UserScreenType.Manual:
                    int n = Math.Max(1, (int)Math.Round(Math.Sqrt(slotCount)));
                    return (n, n);
                default: return (1, 1);
            }
        }

        /// <summary>Whether a placed player can be expanded/shrunk (layouts of 4 cells or more, not manual, no profile).</summary>
        public static bool CanExpand(UserScreenType type, bool profileLoaded) => !profileLoaded && (int)type >= (int)UserScreenType.FourPlayers && type != UserScreenType.Manual;

        /// <summary>Whether a placed player can be resized with edge steps (screens with 4 slots or more, no profile).</summary>
        public static bool CanResize(int slotCount, bool profileLoaded) => !profileLoaded && slotCount >= 4;

        /// <summary>Index of the first player area intersecting <paramref name="slot"/>, or -1.</summary>
        public static int OccupantOf(Rectangle slot, IReadOnlyList<Rectangle> playerAreas)
        {
            for (int i = 0; i < playerAreas.Count; i++)
            {
                if (!playerAreas[i].IsEmpty && playerAreas[i].IntersectsWith(slot))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
