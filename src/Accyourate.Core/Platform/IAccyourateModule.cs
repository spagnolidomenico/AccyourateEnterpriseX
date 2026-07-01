namespace Accyourate.Core.Platform;

public interface IAccyourateModule
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
}
