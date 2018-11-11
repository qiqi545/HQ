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

using Xunit.Abstractions;
using Xunit.Sdk;

namespace HQ.Touchstone.Xunit.Extensions
{
    public class TestFrameworkDiscoverer : global::Xunit.Sdk.TestFrameworkDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly CollectionPerClassTestCollectionFactory _testCollectionFactory;

        public TestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider,
            IMessageSink diagnosticMessageSink) : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
            _testCollectionFactory =
                new CollectionPerClassTestCollectionFactory(new TestAssembly(assemblyInfo), diagnosticMessageSink);
        }

        protected override ITestClass CreateTestClass(ITypeInfo @class)
        {
            return new TestClass(_testCollectionFactory.Get(@class), @class);
        }

        protected override bool FindTestsForType(ITestClass testClass,
            bool includeSourceInformation,
            IMessageBus messageBus,
            ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            foreach (var method in testClass.Class.GetMethods(true))
            {
                var testMethod = new TestMethod(testClass, method);
                var testCase = new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
                if (!ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus))
                    return false;
            }

            return true;
        }
    }
}
