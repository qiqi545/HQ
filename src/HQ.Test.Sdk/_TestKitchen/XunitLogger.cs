﻿// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestKitchen
{
	public class XunitLogger : ILogger, IDisposable
	{
		private readonly ITestOutputHelper _output;
		public XunitLogger(ITestOutputHelper output)
		{
			_output = output;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => _output.WriteLine(state.ToString());
		public bool IsEnabled(LogLevel logLevel) => true;
		public IDisposable BeginScope<TState>(TState state) => this;
		public void Dispose() { }
	}

	public class XunitLogger<T> : XunitLogger, ILogger<T>
	{
		public XunitLogger(ITestOutputHelper output) : base(output) { }
	}
}