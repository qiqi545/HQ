using System;

namespace Lime.Web
{
	[Flags]
	public enum InputAttributes
	{
		ReadOnly = 0 << 1,
		Disabled = 0 << 2,
		NoValidate = 0 << 3,
		AutoFocus = 0 << 4,
		FormNoValidate = 0 << 5,
		Multiple = 0 << 6,
		Required = 0 << 7,
		None = 0,
		All = 0xFFFFFF
	}
}