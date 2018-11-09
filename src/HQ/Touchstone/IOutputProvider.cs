namespace HQ.Touchstone
{
    public interface IOutputProvider
    {
        bool IsAvailable { get; }
        void WriteLine(string message);
        void WriteLine(string format, params object[] args);
    }
}
