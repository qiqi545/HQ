using System;
using Microsoft.AspNetCore.Identity;

namespace HQ.Extensions.Identity.Models
{
    public class IdentityRoleExtended<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
    {
        public int TenantId { get; set; }
    }

    public class IdentityRoleExtended : IdentityRoleExtended<string> {  }
}
