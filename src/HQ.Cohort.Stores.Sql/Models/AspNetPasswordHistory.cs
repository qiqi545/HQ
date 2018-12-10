using System;
using System.ComponentModel.DataAnnotations;

namespace HQ.Cohort.Stores.Sql.Models
{
    public class AspNetPasswordHistory<TKey>
    {
        [Required]
        public TKey UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? EndedAt { get; set; }
    }
}
