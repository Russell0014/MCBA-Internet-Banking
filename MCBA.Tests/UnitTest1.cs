namespace MCBA.Tests;

public class UnitTest1
{
    private static int Add(int x, int y) =>
        x + y;

    [Fact]
    public void Good() =>
        Assert.Equal(4, Add(2, 2));

}