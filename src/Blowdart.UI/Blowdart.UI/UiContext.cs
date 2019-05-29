// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;

namespace Blowdart.UI
{
    public class UiContext
    {
		public IServiceProvider UiServices { get; internal set; }
		public string TraceIdentifier { get; internal set; }
	    public ClaimsPrincipal User { get; internal set; }

	    public virtual void Clear()
	    {
		    UiServices = null;
		    User = null;
		    TraceIdentifier = default;
	    }
    }
}