// Copyright (c) Tavis Software Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See THIRD_PARTY_NOTICES.txt in the project root for license information.

using System.Text;

namespace Blowdart.UI.Internal.UriTemplates
{
    public class VarSpec
    {
        public StringBuilder VarName = new StringBuilder();
        public bool Explode = false;
        public int PrefixLength = 0;
        public bool First = true;

        public VarSpec(OperatorInfo operatorInfo)
        {
            OperatorInfo = operatorInfo;
        }

        public OperatorInfo OperatorInfo { get; }

        public override string ToString()
        {
            return (First ? OperatorInfo.First : "") +
                   VarName
                   + (Explode ? "*" : "")
                   + (PrefixLength > 0 ? ":" + PrefixLength : "");

        }
    }
}