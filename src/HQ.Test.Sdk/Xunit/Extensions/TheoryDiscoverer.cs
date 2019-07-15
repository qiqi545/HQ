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
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit.Extensions
{
    public class TheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public TheoryDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected IEnumerable<IXunitTestCase> CreateTestCasesForSkip(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            string skipReason)
        {
            yield return new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
        }

        protected IEnumerable<IXunitTestCase> CreateTestCasesForTheory(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute)
        {
            yield return new TheoryTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
        }

        protected virtual IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            object[] dataRow)
        {
            return new []
            {
                new TestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, dataRow)
            };
        }

        protected virtual IEnumerable<IXunitTestCase> CreateTestCasesForSkippedDataRow(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            object[] dataRow,
            string skipReason)
        {
            return new[]
            {
                new SkippedDataRowTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, skipReason, dataRow)
            };
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            var scopes = theoryAttribute.GetNamedArgument<string[]>(nameof(DataDrivenTestAttribute.Environments));
            if (scopes?.Length > 0)
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env != null && !scopes.Contains(env, StringComparer.OrdinalIgnoreCase))
                {
                    return new[] { new SkipTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                        discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                        $"This test does not apply in '{env}' environment, skipping.") };
                }
            }

            var skipReason = theoryAttribute.GetNamedArgument<string>(nameof(FactAttribute.Skip));
            if (skipReason != null)
            {
                return CreateTestCasesForSkip(discoveryOptions, testMethod, theoryAttribute, skipReason);
            }

            if (!discoveryOptions.PreEnumerateTheoriesOrDefault())
            {
                return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
            }

            try
            {
                var customAttributes = testMethod.Method.GetCustomAttributes(typeof(DataAttribute));
                var testCases = new List<IXunitTestCase>();
                foreach (var attributeInfo in customAttributes)
                {
                    var dataDiscovererAttribute = attributeInfo.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    IDataDiscoverer dataDiscoverer;
                    try
                    {
                        dataDiscoverer = ExtensibilityPointFactory.GetDataDiscoverer(_diagnosticMessageSink, dataDiscovererAttribute);
                    }
                    catch (InvalidCastException)
                    {
                        if (attributeInfo is IReflectionAttributeInfo reflectionAttributeInfo)
                        {
                            testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                                $"Data discoverer specified for {reflectionAttributeInfo.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not implement IDataDiscoverer."));
                            continue;
                        }
                        testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                            $"A data discoverer specified on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not implement IDataDiscoverer."));
                        continue;
                    }
                    if (dataDiscoverer == null)
                    {
                        if (attributeInfo is IReflectionAttributeInfo reflectionAttributeInfo)
                            testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                                $"Data discoverer specified for {reflectionAttributeInfo.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not exist."));
                        else
                            testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                                $"A data discoverer specified on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not exist."));
                    }
                    else
                    {
                        skipReason = attributeInfo.GetNamedArgument<string>(nameof(FactAttribute.Skip));
                        if (!dataDiscoverer.SupportsDiscoveryEnumeration(attributeInfo, testMethod.Method))
                        {
                            return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
                        }

                        var data = dataDiscoverer.GetData(attributeInfo, testMethod.Method);
                        if (data == null)
                        {
                            testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod,
                                $"Test data returned null for {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}. Make sure it is statically initialized before this test method is called."));
                        }
                        else
                        {
                            foreach (var dataRow in data)
                            {
                                if (!XunitSerializationInfo.CanSerializeObject(dataRow))
                                {
                                    _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Non-serializable data ('{dataRow.GetType().FullName}') found for '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'; falling back to single test case."));
                                    return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
                                }
                                var collection = skipReason != null ? CreateTestCasesForSkippedDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, skipReason) : CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow);
                                testCases.AddRange(collection);
                            }
                        }
                    }
                }
                if (testCases.Count == 0)
                    testCases.Add(new ExecutionErrorTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "No data found for " + testMethod.TestClass.Class.Name + "." + testMethod.Method.Name));

                return testCases;
            }
            catch (Exception ex)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception thrown during theory discovery on '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'; falling back to single test case.{Environment.NewLine}{ex}"));
            }

            return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
        }
    }
}
