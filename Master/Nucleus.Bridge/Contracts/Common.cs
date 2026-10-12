namespace Nucleus.Bridge.Contracts
{
    /// <summary>A rectangle in desktop pixels (virtual screen coordinates, may be negative on secondary monitors).</summary>
    public sealed class Rect
    {
        /// <summary>Left edge.</summary>
        public int X { get; set; }

        /// <summary>Top edge.</summary>
        public int Y { get; set; }

        /// <summary>Width.</summary>
        public int Width { get; set; }

        /// <summary>Height.</summary>
        public int Height { get; set; }
    }

    /// <summary>Identifies a game in the user's library.</summary>
    public sealed class GameRequest
    {
        /// <summary>Game id (the handler GUID).</summary>
        public string GameId { get; set; } = "";
    }

    /// <summary>Identifies an input device in the current setup.</summary>
    public sealed class DeviceRequest
    {
        /// <summary>Device id, see <see cref="SetupDevice.Id"/>.</summary>
        public string DeviceId { get; set; } = "";
    }

    /// <summary>Identifies a player in the current setup.</summary>
    public sealed class PlayerRequest
    {
        /// <summary>Player id, see <see cref="SetupPlayer.Id"/>.</summary>
        public string PlayerId { get; set; } = "";
    }

    /// <summary>Turns a flag on or off.</summary>
    public sealed class SetEnabledRequest
    {
        /// <summary>New value.</summary>
        public bool Enabled { get; set; }
    }

    /// <summary>One invalid field returned by a save operation.</summary>
    public sealed class ValidationError
    {
        /// <summary>Name of the field, as in the request DTO (camelCase). For a field of a list item, <c>list.field</c> (for example <c>slots.steamId</c>) with <see cref="Index"/> set.</summary>
        public string Field { get; set; } = "";

        /// <summary>Index of the list item, for a field of a list item.</summary>
        public int? Index { get; set; }

        /// <summary>What is wrong, in plain English, ready to show.</summary>
        public string Message { get; set; } = "";
    }
}
