namespace tophat
{
    public enum ConnectionScope
    {
        /// <summary>
        /// One connection is opened on every request
        /// </summary>
        AlwaysNew,
        /// <summary>
        /// One connection is opened for a single HTTP request
        /// </summary>
        ByRequest,
        /// <summary>
        /// One connection is opened per thread
        /// </summary>
        ByThread,
        /// <summary>
        /// One connection is opened on first use
        /// </summary>
        KeepAlive
    }
}