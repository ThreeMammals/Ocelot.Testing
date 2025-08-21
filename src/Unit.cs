namespace Ocelot.Testing;

public class Unit
{
    private readonly Guid _testId = Guid.NewGuid();
    protected string TestID { get => _testId.ToString("N"); }
}
