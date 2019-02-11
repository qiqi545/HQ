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
using System.Text;

namespace HQ.Data.Streaming.Internal
{
    internal static class Constants
    {
        public const byte CarriageReturn = (byte) '\r';
        public const byte LineFeed = (byte) '\n';
        public const string Comma = ",";
        public const string Tab = "\t";
        public const string Pipe = "|";

        public const int ReadAheadSize = 128;
        public const int PadSize = 4;
        public const int BlockSize = 4096;
        public const int WorkingBufferLength = ReadAheadSize + BlockSize + PadSize;

        [ThreadStatic] private static byte[] _working;

        public static readonly UTF32Encoding BigEndianUtf32 = new UTF32Encoding(true, true);
        public static byte[] WorkingBuffer => _working ?? (_working = new byte[WorkingBufferLength]);
    }
}
