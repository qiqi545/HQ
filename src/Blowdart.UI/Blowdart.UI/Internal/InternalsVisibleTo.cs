﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Blowdart.UI.Tests")]
[assembly: InternalsVisibleTo("Blowdart.UI.Web")]
[assembly: InternalsVisibleTo("Blowdart.UI.Web.SemanticUI")]
[assembly: InternalsVisibleTo("Blowdart.UI.iOS")]
[assembly: InternalsVisibleTo("Blowgun")]
[assembly: InternalsVisibleTo("Blowgun.Web")]
[assembly: InternalsVisibleTo("Blowgun.Web.SemanticUI")]

namespace Blowdart.UI.Internal
{
    internal sealed class InternalsVisibleTo
    {
    }
}