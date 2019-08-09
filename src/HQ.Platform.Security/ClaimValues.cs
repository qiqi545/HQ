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

namespace HQ.Platform.Security
{
    public static class ClaimValues
    {
        public const string SuperUser = "superuser";
        public const string ManageUsers = "manage_users";
        public const string ManageRoles = "manage_roles";
        public const string ManageTenants = "manage_tenants";
        public const string ManageApplications = "manage_applications";
        public const string ManageBackgroundTasks = "manage_background_tasks";
        public const string ManageConfiguration = "manage_configuration";
        public const string ManageObjects = "manage_objects";
        public const string ManageSchemas = "manage_schemas";

		public const string AccessMeta = "access_meta";
        public const string AccessOperations = "access_operations";
    }
}
