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
using System.Reactive;
using System.Reactive.Linq;

namespace HQ.Platform.Api.Notifications.Models
{
	internal class FileSystemObservable
	{
		private readonly IObservable<EventPattern<FileSystemEventArgs>> _observable;

		public FileSystemObservable(string directory, string filter, bool includeSubdirectories)
		{
			var watcher = new FileSystemWatcher(directory, filter)
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = includeSubdirectories,
				InternalBufferSize = 50000
			};

			_observable =
				Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Created += h,
					h => watcher.Created -= h);
		}

		public void Subscribe(Action<EventPattern<FileSystemEventArgs>> onNext)
		{
			_observable.Subscribe(onNext);
		}
	}
}