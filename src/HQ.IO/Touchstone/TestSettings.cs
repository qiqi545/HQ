namespace HQ.Touchstone
{
	public class TestSettings
	{
		public string[] SystemsUnderTest { get; set; }
		public string EnvironmentName { get; set; } = "InteractionTests";
		public string AppSettingsPath { get; set; } = null;
	}
}
