using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Tests.TestContract;

internal sealed class FakeShelfService : IShelfService
{
    public List<ShelfItem> Items { get; } = new()
    {
        new ShelfItem { Id = "a", Name = "Apple", Color = Color.Red },
        new ShelfItem { Id = "b", Name = "Blueberry", Color = Color.SkyBlue },
        new ShelfItem { Id = "c", Name = "Cup" },
    };

    public Task<List<ShelfItem>> List() => Task.FromResult(Items);

    public ShelfItem? Find(FindItemRequest request) => request.Index < Items.Count ? Items[request.Index] : null;

    public Task<string?> NameOf(FindItemRequest request) => Task.FromResult(Find(request)?.Name);

    public void Rename(RenameRequest request)
    {
        var item = Items.FirstOrDefault(i => i.Id == request.ItemId)
            ?? throw new BridgeException(BridgeErrorKind.NotFound, "No such item.", new { itemId = request.ItemId });
        item.Name = request.Name;
    }

    public Dictionary<string, string> Labels() => Items.ToDictionary(i => i.Id, i => i.Name);

    public Shapes Echo(Shapes shapes) => shapes;

    public Task Wait(CancellationToken cancellationToken) => Task.Delay(Timeout.Infinite, cancellationToken);

    public void Fail() => throw new InvalidOperationException("secret detail");
}
