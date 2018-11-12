using HQ.Common.Configuration;

namespace HQ.Tokens.Configuration
{
    public class TokenOptions : FeatureToggle<SecurityOptions>
    {
        public string Path { get; set; } = "tokens";
        public string Key { get; set; }
        public string Issuer { get; set; } = "https://mysite.com";
        public string Audience { get; set; } = "https://mysite.com";
        public int TimeToLiveSeconds { get; set; } = 180;
    }
}