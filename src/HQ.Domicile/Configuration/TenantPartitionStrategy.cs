namespace HQ.Domicile.Configuration
{
    public enum TenantPartitionStrategy
    {
        /// <summary>
        /// The tenant shares the application pipeline. Data is disambiguated by Tenant ID.
        /// </summary>
        Shared,
        /// <summary>
        /// The tenant has its own application pipeline. Data is disambiguated by Tenant ID.
        /// </summary>
        Forked,
        /// <summary>
        /// The tenant has its own application pipeline and data.
        /// </summary>
        Isolated
    }
}