namespace MgKit.Interface
{
    public interface IPackageAction
    {
        IPackage Package { get; }
        string Type { get; }
        string Status { get; }
        int Progress { get; }
    }
}