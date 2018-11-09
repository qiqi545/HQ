// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Bus.Messages;

namespace HQ.Flow.Tests.Bus.Handlers
{
    public class ErrorHandler : IMessageHandler<ErrorMessage>
    {
        public int Handled { get; private set; }

        public bool Handle(ErrorMessage message)
        {
            if (message.Error)
                throw new Exception("The message made me do it!");
            Handled++;
            return true;
        }
    }
}
