using Oceana.Contracts;

namespace Oceana.Agent.Windows.Playback;

public class RoutingStoreTests
{
    [Fact]
    public void Current_IsEmptyByDefault()
    {
        new RoutingStore().Current.Should().BeEmpty();
    }

    [Fact]
    public void Set_ReplacesTheCurrentRouting()
    {
        var store = new RoutingStore();
        var outputs = new[] { new AudioOutput("A", [0, 1]), new AudioOutput("B", [2, 3]) };

        store.Set(outputs);

        store.Current.Should().BeSameAs(outputs);
        store.Current.Should().HaveCount(2);
    }
}
