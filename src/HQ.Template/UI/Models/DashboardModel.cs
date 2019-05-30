namespace HQ.Template.UI.Models
{
    public class DashboardModel
    {
        public bool HasApplication { get; set; }
        public bool HasAccessToken { get; set; }
        public bool HasSettings { get; set; }

        public bool IsFirstTimeExperience => !HasApplication || !HasAccessToken || !HasSettings;
    }
}
