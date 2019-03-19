// Copyright (c) Tavis Software Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See THIRD_PARTY_NOTICES.txt in the project root for license information.

namespace Blowdart.UI.Internal.UriTemplates
{
    public class OperatorInfo
    {
        public bool Default { get; set; }
        public string First { get; set; }
        public char Separator { get; set; }
        public bool Named { get; set; }
        public string IfEmpty { get; set; }
        public bool AllowReserved { get; set; }

    }
}