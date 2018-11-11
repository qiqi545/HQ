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
using HQ.Flow.Bus;
using HQ.Flow.Tests.Bus.Messages;

namespace HQ.Flow.Tests.Bus.Handlers
{
    public class ManifoldHierarchicalMessageHandler :
        IMessageHandler<BaseMessage>,
        IMessageHandler<InheritedMessage>,
        IMessageHandler<ErrorMessage>,
        IMessageHandler<IMessage>
    {
        public int HandledInterface { get; set; }
        public int HandledBase { get; set; }
        public int HandledInherited { get; set; }

        public bool Handle(BaseMessage message)
        {
            HandledBase++;
            return true;
        }

        public bool Handle(ErrorMessage message)
        {
            if (message.Error)
                throw new Exception("the message made me do it!");
            return true;
        }

        public bool Handle(IMessage message)
        {
            HandledInterface++;
            return true;
        }

        public bool Handle(InheritedMessage message)
        {
            HandledInherited++;
            return true;
        }
    }
}
