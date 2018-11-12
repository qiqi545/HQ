using HQ.Common.Configuration;

namespace HQ.Tokens.Configuration
{
    public class BlockListOptions : FeatureToggle<SecurityOptions>
    {
        private static readonly string[] Empty = new string[0];
        public string[] PasswordBlockList { get; set; } = Empty;
        public string[] UsernameBlockList { get; set; } = Empty;
    }
}