namespace HQ.Flow.Tests.Messages
{
	public class ErrorMessage : BaseMessage
	{
		public bool Error { get; set; }

		public ErrorMessage()
		{
			Error = true;
		}
	}
}