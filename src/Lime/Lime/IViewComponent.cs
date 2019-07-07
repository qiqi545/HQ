// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen;

namespace Lime
{
	public interface IViewComponent { }

	public interface IViewComponent<in TValue> : IViewComponent
	{
		void BeforeView(Ui ui);
		void Render(Ui ui, AccessorMember field, TValue value);
		void AfterView(Ui ui);
	}
}