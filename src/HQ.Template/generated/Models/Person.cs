/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
    Generated For: HQ.io
    Generated On: Sunday, March 10, 2019 5:13:38 AM
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HQ.Data.Contracts;
using HQ.Extensions.CodeGeneration.Scripting;

namespace HQ.Template
{
    [DataContract]
    public partial class Person : IObject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), DataMember]
        public virtual long Id { get; set; }

        /// <summary>Name</summary>
        [Required]
        [ReadOnly(false)]
        [DataMember]
        public virtual string Name { get; set; } 

        /// <summary>Welcome</summary>
        [ReadOnly(false)]
        [DataMember]
        public virtual string Welcome => ComputedString.Compute(this, "Hello, {{ Name }}!");

        [DataMember]
        public virtual DateTimeOffset CreatedAt { get; set; }

        [DataMember]
        public virtual DateTimeOffset? DeletedAt { get; set; }
    }
}
