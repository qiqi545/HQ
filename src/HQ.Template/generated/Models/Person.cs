/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
    Generated For: Demo
    Generated On: Sunday, May 26, 2019 5:54:55 PM
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts;
using HQ.Extensions.CodeGeneration.Scripting;

[assembly:HQ.Data.Contracts.Attributes.Fingerprint(16911708163220264388)]

namespace HQ.Template
{

    [Fingerprint(10167278396239781350)]
    [Description("A user of our application.")]
    [DataContract]
    public partial class PersonMetadata
    {
        #region Attributes
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        [Browsable(false)]
        [Display(Name = nameof(Id), Order = 0)]
        [Description("The identifier for this object, unique within the object type, across all versions.")]
        #endregion
        public long Id;

        #region Attributes
        [Description("")]
        [Display(Name = "Name", Description = "Name", Order = 1, ShortName = null)]
        [Column(nameof(Name), TypeName = "string", Order = 1)]
        [ProtectedPersonalData]
        [Required]
        [ReadOnly(false)]
        [DataMember]
        #endregion
        public string Name;

        #region Attributes
        [Description("")]
        [Display(Name = "Welcome", Description = "Welcome", Order = 2, ShortName = null)]
        [Column(nameof(Welcome), TypeName = "string", Order = 2)]
        [Editable(false)]
        [ReadOnly(true)]
        [DataMember]
        #endregion
        public string Welcome;

        [Description("The time the object with this type and ID was first stored")]
        [DataMember]
        [Browsable(false)]
        [Editable(false)]
        [Display(Name = nameof(CreatedAt), Order = 3)]
        public DateTimeOffset CreatedAt;

        #region Attributes
        [DataMember]
        [Browsable(false)]
        [Editable(false)]
        [Display(Name = nameof(DeletedAt), Order = 4)]
        [Description("The time the object with this type and ID was marked as deleted")]
        #endregion
        public DateTimeOffset? DeletedAt;
    }

    /// <summary>A user of our application.</summary>
    [MetadataType(typeof(PersonMetadata))]
    [DataContract]
    public partial class Person : IObject
    {
        /// <summary>The identifier for this object, unique within the object type, across all versions</summary>
        public virtual long Id { get; set; }

        /// <summary></summary>
        [DataMember]
        public virtual string Name { get; set; } 

        /// <summary></summary>
        [DataMember]
        public virtual string Welcome { get => ComputedString.Compute(this, "Hello, {{ Name }}!"); set { } }

        /// <summary>The time the object with this type and ID was first stored</summary>
        public virtual DateTimeOffset CreatedAt { get; set; }

        /// <summary>The time the object with this type and ID was marked as deleted</summary>
        public virtual DateTimeOffset? DeletedAt { get; set; }
    }
}
