// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Flow.Bus;
using HQ.Flow.Tests.Bus.Messages;

namespace HQ.Flow.Tests.Bus.Handlers
{
    public class StringMessageHandler : IMessageHandler<StringMessage>
    {
        public int Handled { get; private set; }

        public bool Handle(StringMessage message)
        {
            Handled++;
            return true;
        }
    }
}
