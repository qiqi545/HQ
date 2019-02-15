namespace HQ.Data.Streaming.Memory
{
    public struct SegmentStats
    {
        public long RecordCount { get; set; }
        public int RecordLength { get; set; }
        public int SegmentCount { get; set; }
    }
}