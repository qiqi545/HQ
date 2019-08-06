namespace HQ.Data.Contracts.Migration
{
	public interface IMigration
	{
		void Up(MigrationContext context);
	}
}
