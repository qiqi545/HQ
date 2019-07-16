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
using System.Collections.Generic;
using System.Linq;
using HQ.Test.Sdk.Internal;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit.Extensions
{
    public class TestCaseDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public TestCaseDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod, IAttributeInfo factAttribute)
        {
	        var scopes = ResolveScopes(testMethod, factAttribute);
	        if (scopes?.Count > 0)
            {
                var env = Environment.GetEnvironmentVariable(Constants.AspNetCoreEnvironment);
                if (env != null && !scopes.Contains(env, StringComparer.OrdinalIgnoreCase))
                {
                    yield return new SkipTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                        discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                        $"This test does not apply in '{env}' environment, skipping.");
                    yield break;
                }
            }

            yield return new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
        }

        private static List<string> ResolveScopes(ITestMethod testMethod, IAttributeInfo factAttribute)
        {
	        var scopes = new List<string>();

	        foreach (var attribute in testMethod.TestClass.Class.GetCustomAttributes(typeof(TestEnvironmentAttribute)))
	        {
		        scopes.AddRange(attribute.GetNamedArgument<string[]>(nameof(TestAttribute.Environments)));
	        }

	        var environments = factAttribute.GetNamedArgument<string[]>(nameof(TestAttribute.Environments));

			if(environments != null)
				scopes.AddRange(environments);

	        return scopes;
        }
    }
}
