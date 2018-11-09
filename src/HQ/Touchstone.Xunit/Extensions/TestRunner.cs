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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HQ.Touchstone.Extensions
{
    public class TestRunner : XunitTestRunner
    {
        public TestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments,
            MethodInfo testMethod, object[] testMethodArguments, string skipReason,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator exceptionAggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason,
                beforeAfterAttributes, exceptionAggregator, cancellationTokenSource)
        {
        }

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var xunitOutputProvider = new XunitOutputProvider();
            AmbientContext.OutputProvider = xunitOutputProvider;

            var testOutputHelper = xunitOutputProvider.inner;
            testOutputHelper.Initialize(MessageBus, Test);

            var time = await InvokeTestMethodAsync(aggregator);

            var buffer = testOutputHelper.Output;
            testOutputHelper.Uninitialize();
            xunitOutputProvider.inner = null;

            return Tuple.Create(time, buffer);
        }

        protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        {
            return new XunitTestInvoker(Test, MessageBus, TestClass, ConstructorArguments, TestMethod,
                TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource).RunAsync();
        }
    }
}
