#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using System;
using System.Threading.Tasks;
using HQ.Platform.Api.Models;
using Version = HQ.Platform.Api.Models.Version;

namespace HQ.Platform.Identity.Services
{
    public class IdentityVersionService : IVersionService
    {
        public Task<Version> FindByKeyAsync(string versionKey)
        {
            return Task.FromResult(new Version());
        }
    }
}
