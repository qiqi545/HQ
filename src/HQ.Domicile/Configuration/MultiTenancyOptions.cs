using HQ.Common;
using HQ.Common.Configuration;

namespace HQ.Domicile.Configuration
{
    public class MultiTenancyOptions : FeatureToggle<PublicApiOptions>
    {
        public bool RequireTenant { get; set; } = false;
        public string DefaultTenantId { get; set; } = "0";
        public string DefaultTenantName { get; set; } = Constants.MultiTenancy.DefaultTenantName;
        public string TenantHeader { get; set; } = null;
        public int? TenantLifetimeSeconds { get; set; } = null;
        public TenantPartitionStrategy PartitionStrategy { get; set; } = TenantPartitionStrategy.Shared;
    }
}
