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
using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit.Extensions
{
    public class SkippedDataRowTestCase :TestCase
    {
        private string skipReason;

        /// <summary />
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public SkippedDataRowTestCase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Xunit.Sdk.XunitSkippedDataRowTestCase" /> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="skipReason">The reason that this test case will be skipped</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        [Obsolete("Please call the constructor which takes TestMethodDisplayOptions")]
        public SkippedDataRowTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            string skipReason,
            object[] testMethodArguments = null)
            : this(diagnosticMessageSink, defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, skipReason, testMethodArguments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Xunit.Sdk.XunitSkippedDataRowTestCase" /> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="skipReason">The reason that this test case will be skipped</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public SkippedDataRowTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            string skipReason,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
            this.skipReason = skipReason;
        }

        /// <inheritdoc />
        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            this.skipReason = data.GetValue<string>("SkipReason");
        }

        /// <inheritdoc />
        protected override string GetSkipReason(IAttributeInfo factAttribute)
        {
            return this.skipReason;
        }

        /// <inheritdoc />
        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue("SkipReason", (object)this.skipReason, (Type)null);
        }
    }
}
