using Blowdart.UI;
using HQ.Installer.UI.Models;

namespace HQ.Installer.UI.Controllers
{
    public class DashboardController
    {
        public DashboardModel Index(Ui ui)
        {
            return new DashboardModel
            {
                TenantCount = 0
            };
        }
    }
}
