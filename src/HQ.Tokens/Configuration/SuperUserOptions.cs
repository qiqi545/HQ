using System.ComponentModel.DataAnnotations;
using HQ.Common.Configuration;

namespace HQ.Tokens.Configuration
{
    public class SuperUserOptions : FeatureToggle<SecurityOptions>
    {
        private string _username;
        private string _password;
        private string _phoneNumber;
        private string _email;

        public SuperUserOptions(bool enabled = false)
        {
            Enabled = enabled;
        }

        [Required]
        [DataType(DataType.Text)]
        public string Username
        {
            get => Enabled ? _username : null;
            set => _username = value;
        }

        [Required]
        [DataType(DataType.Password)]
        public string Password
        {
            get => Enabled ? _password : null;
            set => _password = value;
        }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber
        {
            get => Enabled ? _phoneNumber : null;
            set => _phoneNumber = value;
        }

        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get => Enabled ? _email : null;
            set => _email = value;
        }
    }
}