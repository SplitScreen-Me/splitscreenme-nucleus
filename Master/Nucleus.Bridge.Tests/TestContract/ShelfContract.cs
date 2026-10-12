using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Tests.TestContract;

/// <summary>Items on a shelf. See <see cref="ShelfChanged"/>.</summary>
[BridgeService("shelf")]
public interface IShelfService
{
    /// <summary>Every item.</summary>
    Task<List<ShelfItem>> List();

    /// <summary>The item at <see cref="FindItemRequest.Index"/>, or null.</summary>
    ShelfItem? Find(FindItemRequest request);

    /// <summary>Name of an item, or null.</summary>
    Task<string?> NameOf(FindItemRequest request);

    /// <summary>Renames an item. Throws <c>notFound</c>.</summary>
    void Rename(RenameRequest request);

    /// <summary>Labels by item id.</summary>
    Dictionary<string, string> Labels();

    /// <summary>Returns its argument.</summary>
    Shapes Echo(Shapes shapes);

    /// <summary>Waits until cancelled.</summary>
    Task Wait(CancellationToken cancellationToken);

    /// <summary>Throws an unexpected exception.</summary>
    void Fail();
}

/// <summary>A service the host tests never register.</summary>
[BridgeService("clock")]
public interface IClockService
{
    /// <summary>Current time.</summary>
    DateTime Now();
}

/// <summary>Colour of an item.</summary>
public enum Color
{
    /// <summary>Like an apple.</summary>
    Red,

    /// <summary>Like the sky.</summary>
    SkyBlue,
}

/// <summary>Something on the shelf.</summary>
public sealed class ShelfItem
{
    /// <summary>Id.</summary>
    public string Id { get; set; } = "";

    /// <summary>Name, shown as is.</summary>
    public string Name { get; set; } = "";

    /// <summary>Colour, when known.</summary>
    public Color? Color { get; set; }
}

/// <summary>Parameters of <see cref="IShelfService.Find"/>.</summary>
public sealed class FindItemRequest
{
    /// <summary>Position on the shelf, 0-based.</summary>
    public int Index { get; set; }
}

/// <summary>Parameters of <see cref="IShelfService.Rename"/>.</summary>
public sealed class RenameRequest
{
    /// <summary>Item id.</summary>
    public string ItemId { get; set; } = "";

    /// <summary>New name.</summary>
    public string Name { get; set; } = "";
}

/// <summary>Every supported shape. <para>Second paragraph with <c>code</c>.</para></summary>
public sealed class Shapes
{
    /// <summary>Text.</summary>
    public string Text { get; set; } = "";

    /// <summary>Optional text.</summary>
    public string? MaybeText { get; set; }

    /// <summary>Integer.</summary>
    public int Count { get; set; }

    /// <summary>Optional integer.</summary>
    public int? MaybeCount { get; set; }

    /// <summary>Big integer.</summary>
    public long Big { get; set; }

    /// <summary>Optional decimal.</summary>
    public decimal? Price { get; set; }

    /// <summary>Flag.</summary>
    public bool Flag { get; set; }

    /// <summary>Id.</summary>
    public Guid Id { get; set; }

    /// <summary>UTC time.</summary>
    public DateTime At { get; set; }

    /// <summary>Optional time.</summary>
    public DateTime? MaybeAt { get; set; }

    /// <summary>Time with offset.</summary>
    public DateTimeOffset Offset { get; set; }

    /// <summary>Colour.</summary>
    public Color Color { get; set; }

    /// <summary>Optional colour.</summary>
    public Color? MaybeColor { get; set; }

    /// <summary>Tags.</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Tags with holes.</summary>
    public List<string?> SparseTags { get; set; } = new();

    /// <summary>Array.</summary>
    public string[] Names { get; set; } = Array.Empty<string>();

    /// <summary>Read-only list of DTOs.</summary>
    public IReadOnlyList<ShelfItem> Items { get; set; } = new List<ShelfItem>();

    /// <summary>Nested lists.</summary>
    public List<List<int>> Grid { get; set; } = new();

    /// <summary>Map of strings.</summary>
    public Dictionary<string, string> Labels { get; set; } = new();

    /// <summary>Map of optional DTOs.</summary>
    public IReadOnlyDictionary<string, ShelfItem?> ItemsById { get; set; } = new Dictionary<string, ShelfItem?>();

    /// <summary>Optional list.</summary>
    public List<int>? MaybeList { get; set; }

    /// <summary>Set of enums.</summary>
    public HashSet<Color> Palette { get; set; } = new();

    /// <summary>Sequence of optional numbers.</summary>
    public IEnumerable<double?> Samples { get; set; } = new List<double?>();

    /// <summary>Nested DTO.</summary>
    public ShelfItem Main { get; set; } = new();

    /// <summary>Optional nested DTO, see <see cref="ShelfItem.Name"/> and <see cref="Nucleus.Bridge.Tests.TestContract.Color.SkyBlue"/>.</summary>
    public ShelfItem? Spare { get; set; }
}

/// <summary>Items changed. Refetch <see cref="IShelfService.List"/>.</summary>
[BridgeEvent("shelf.changed", Invalidates = new[] { "shelf.list", "shelf.find" })]
public sealed class ShelfChanged
{
    /// <summary>Changed items.</summary>
    public List<string> ItemIds { get; set; } = new();
}

/// <summary>Somebody looked at the shelf.</summary>
[BridgeEvent("shelf.seen")]
public sealed class ShelfSeen
{
    /// <summary>When.</summary>
    public DateTime At { get; set; }
}

/// <summary>The test contract: the types to build the model from.</summary>
public static class ShelfContract
{
    /// <summary>Service interfaces and event classes of the test contract.</summary>
    public static readonly Type[] Types = { typeof(IShelfService), typeof(IClockService), typeof(ShelfChanged), typeof(ShelfSeen) };
}
