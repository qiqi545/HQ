using System;
using System.ComponentModel.DataAnnotations;

namespace HQ.Platform.Identity.Stores.Sql.Models
{
    public class AspNetPasswordHistory<TKey>
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        public TKey UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? EndedAt { get; set; }
    }
}
