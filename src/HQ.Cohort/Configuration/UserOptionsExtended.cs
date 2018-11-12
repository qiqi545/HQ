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

using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Configuration
{
    public class UserOptionsExtended : UserOptions
    {
        public UserOptionsExtended()
        {
        }

        public UserOptionsExtended(UserOptions inner)
        {
            AllowedUserNameCharacters = inner.AllowedUserNameCharacters;
            RequireUniqueEmail = inner.RequireUniqueEmail;
        }

        public bool RequireUniqueUsername { get; set; } = true;
        public bool RequireUniquePhoneNumber { get; set; } = false;

        public string AllowedPhoneNumberCharacters { get; set; } = "()123456789-+#";

        public bool RequireEmail { get; set; } = true;
        public bool RequirePhoneNumber { get; set; } = false;
        public bool RequireUsername { get; set; } = true;
        public bool RequireEmailPhoneNumberOrUsername { get; set; } = false;
    }
}
