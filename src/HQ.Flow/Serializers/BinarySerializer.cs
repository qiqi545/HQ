// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HQ.Flow.Serializers
{
	public class BinarySerializer : ISerializer
	{
		private readonly BinaryFormatter _formatter;

		public BinarySerializer()
		{
			_formatter = new BinaryFormatter();
		}

		public Stream SerializeToStream<T>(T message)
		{
			var ms = new MemoryStream();
			_formatter.Serialize(ms, message);
			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}

		public T DeserializeFromStream<T>(Stream stream)
		{
			return (T) _formatter.Deserialize(stream);
		}
	}
}