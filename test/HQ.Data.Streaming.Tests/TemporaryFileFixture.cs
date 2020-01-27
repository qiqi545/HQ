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
using System.IO;

namespace HQ.Data.Streaming.Benchmarks
{
    public class TemporaryFileFixture : IDisposable
    {
        private const int BufferSize = 4096;

        private readonly bool _persistent;

        private bool _disposed;

        public TemporaryFileFixture(bool persistent = false)
        {
            _persistent = persistent;
            FilePath = Path.GetTempFileName();

            // keep the file contents close to memory
            var attributes = File.GetAttributes(FilePath);
            File.SetAttributes(FilePath, attributes | FileAttributes.Temporary);

            // hint to the OS to purge the file on cleanup
            FileStream = File.Create(FilePath, BufferSize, !persistent ? FileOptions.DeleteOnClose : 0);
        }

        public string FilePath { get; }

        public FileStream FileStream { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                FileStream.Dispose();

            if (!_persistent)
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch
                {
                    // best effort
                }
            }

            _disposed = true;
        }

        ~TemporaryFileFixture()
        {
            Dispose(false);
        }
    }
}
