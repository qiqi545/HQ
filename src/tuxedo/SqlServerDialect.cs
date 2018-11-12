namespace tuxedo
{
    public class SqlServerDialect : IDialect
    {
        public char StartIdentifier => '[';
        public char EndIdentifier => ']';
        public char Separator => '.';
        public int ParametersPerQuery => 500;
        public string Identity => "SELECT CAST(SCOPE_IDENTITY() AS INT) AS [Id]";
    }
}