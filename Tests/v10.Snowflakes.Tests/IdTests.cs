using Xunit.Abstractions;

namespace v10.Snowflakes.Tests;

public class IdTests
{
    private readonly ITestOutputHelper _output;

    public IdTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void Id_Create()
    {
        Id id = Id.Create();

        Assert.True(id > 1);
    }

    [Fact]
    public void Id_CreateManyFast()
    {
        Id[] ids = Enumerable.Range(0, 1000).Select(_ => Id.Create()).ToArray();

        Assert.All(ids, id => ids.Count(i => i == id).Equals(1));
    }

    [Fact]
    public async Task Id_CreateManyDelayed()
    {
        List<Id> ids = [];

        for (int i = 0; i < 100; i++)
        {
            ids.Add(Id.Create());
            await Task.Delay(TimeSpan.FromMilliseconds(5));
        }

        Assert.All(ids, id => ids.Count(i => i == id).Equals(1));
    }

    [Fact]
    public void Id_Equality()
    {
        // This test should never fail so long as Id is a struct.
        Id left = (Id)5956206959003041793;
        Id right = (Id)5956206959003041793;

        Assert.Equal(left, right);
    }

    [Fact]
    public void Id_Sortable()
    {
        // The sequence in which Ids are generated should be equal to a set of sorted Ids.
        Id[] ids = Enumerable.Range(0, 1000).Select(_ => Id.Create()).ToArray();
        Id[] sorted = [.. ids.OrderBy(i => i)];

        Assert.True(ids.SequenceEqual(sorted));
    }

    [Fact]
    public void Id_ToString()
    {
        long id = Id.Create();

        Assert.Equal(id.ToString(), id.ToString());
    }
}
