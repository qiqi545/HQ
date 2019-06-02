namespace HQ.Data.Contracts
{
    public interface IStreamHeader
    {
        long Start { get; }
        long End { get; }
        int Count { get; }

        long TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }

        string BeforePage { get; }
        string AfterPage { get; }
    }
}