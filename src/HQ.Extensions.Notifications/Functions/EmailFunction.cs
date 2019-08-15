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

namespace HQ.Extensions.Notifications.Functions
{
	public class EmailFunction
	{
		//private void SaveBacklog()
		//{
		//	EmailMessage message;
		//	var toSave = new List<EmailMessage>();
		//	while (_backlog.TryDequeue(out message))
		//	{
		//		toSave.Add(message);
		//	}
		//	Parallel.ForEach(toSave, Backlog);
		//}

		//private void Backlog(EmailMessage message)
		//{
		//	var folder = _backlogFolder;
		//	var json = _serializer.SerializeToString(message);
		//	var path = Path.Combine(folder, string.Concat(message.Id, ".json"));
		//	File.WriteAllText(path, json);
		//}

		//private void Undeliverable(EmailMessage message)
		//{
		//	var folder = _undeliverableFolder;
		//	var json = _serializer.SerializeToString(message);
		//	var path = Path.Combine(folder, string.Concat(message.Id, ".json"));
		//	File.WriteAllText(path, json);
		//}
	}
}