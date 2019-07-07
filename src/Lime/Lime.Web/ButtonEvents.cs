using System;

namespace Lime.Web
{
	public class ButtonEvents
	{
		public Action<MouseEventData> click;
		public Action<MouseEventData> mouseover;
		public Action<MouseEventData> mouseout;
	}
}