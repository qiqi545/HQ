using System.Net;

namespace HQ.Tokens.Configuration
{
    public class SecurityOptions
    {
        public SuperUserOptions SuperUser { get; set; } = new SuperUserOptions();
        public ClaimOptions Claims { get; set; } = new ClaimOptions();
        public TokenOptions Tokens { get; set; } = new TokenOptions();
        public BlockListOptions BlockLists { get; set; } = new BlockListOptions();

        public HttpStatusCode? ForbidStatusCode { get; set; } = HttpStatusCode.Forbidden;
    }
}
