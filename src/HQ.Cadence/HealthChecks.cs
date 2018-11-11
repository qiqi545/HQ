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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HQ.Cadence
{
    /// <summary>
    ///     A manager class for health checks
    /// </summary>
    public class HealthChecks
    {
        private static readonly ConcurrentDictionary<string, HealthCheck> _checks =
            new ConcurrentDictionary<string, HealthCheck>();

        private HealthChecks()
        {
        }

        /// <summary>
        ///     Returns <code>true</code>  <see cref="HealthCheck" />s have been registered, <code>false</code> otherwise
        /// </summary>
        public static bool HasHealthChecks
        {
            get { return _checks.IsEmpty; }
        }

        /// <summary>
        ///     Registers an application <see cref="HealthCheck" /> with a given name
        /// </summary>
        /// <param name="name">The named health check instance</param>
        /// <param name="check">The <see cref="HealthCheck" /> function</param>
        public static void Register(string name, Func<HealthCheck.Result> check)
        {
            var healthCheck = new HealthCheck(name, check);
            if (!_checks.ContainsKey(healthCheck.Name)) _checks.TryAdd(healthCheck.Name, healthCheck);
        }

        /// <summary>
        ///     Runs the registered health checks and returns a map of the results.
        /// </summary>
        public static IDictionary<string, HealthCheck.Result> RunHealthChecks()
        {
            var results = new SortedDictionary<string, HealthCheck.Result>();
            foreach (var entry in _checks)
            {
                var result = entry.Value.Execute();
                results.Add(entry.Key, result);
            }

            return results;
        }
    }
}
