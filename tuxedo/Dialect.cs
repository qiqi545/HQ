namespace tuxedo
{
    public interface IDialect
    {
        char StartIdentifier { get; }
        char EndIdentifier { get; }
        char Separator { get; }
        int ParametersPerQuery { get; }
        string Identity { get; }
    }
}
