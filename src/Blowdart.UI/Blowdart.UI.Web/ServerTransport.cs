// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Blowdart.UI.Web
{
    [Flags]
    public enum ServerTransport : byte
    {
        /// <summary>Duplex server-and client messaging.</summary>
        WebSockets = 1 << 0,

        /// <summary>Uni-directional server-to-client messaging. Each UI event is posted back to the server.</summary>
        ServerSentEvents = 1 << 1,

        /// <summary>Uni-directional client-to-server messaging.</summary>
        LongPolling = 1 << 2,

        All = 0xFF
    }
}