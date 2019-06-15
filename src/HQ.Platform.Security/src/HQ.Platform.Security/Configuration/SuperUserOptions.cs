#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.ComponentModel.DataAnnotations;
using HQ.Common;
using HQ.Data.Contracts.Attributes;

namespace HQ.Platform.Security.Configuration
{
    public class SuperUserOptions : FeatureToggle
    {
        private string _email;
        private string _password;
        private string _phoneNumber;
        private string _username;

        public SuperUserOptions()
        {
            Enabled = false;
        }

        [Required]
        [DataType(DataType.Text)]
        [SensitiveData(SensitiveDataCategory.OperationalSecurity)]
        public string Username
        {
            get => Enabled ? _username : null;
            set => _username = value;
        }

        [Required]
        [DataType(DataType.Password)]
        [SensitiveData(SensitiveDataCategory.OperationalSecurity)]
        public string Password
        {
            get => Enabled ? _password : null;
            set => _password = value;
        }

        [DataType(DataType.PhoneNumber)]
        [SensitiveData(SensitiveDataCategory.OperationalSecurity)]
        public string PhoneNumber
        {
            get => Enabled ? _phoneNumber : null;
            set => _phoneNumber = value;
        }

        [DataType(DataType.EmailAddress)]
        [SensitiveData(SensitiveDataCategory.OperationalSecurity)]
        public string Email
        {
            get => Enabled ? _email : null;
            set => _email = value;
        }
    }
}
