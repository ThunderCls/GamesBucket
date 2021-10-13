using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GamesBucket.DataAccess.Models;

namespace GamesBucket.App.Models
{
    public class ProfileViewModel
    {
        public IList<Game> Games { get; set; }

        public ProfileDetailsViewModel ProfileDetails { get; set; }

        public ProfileSecurityViewModel ProfileSecurity { get; set; }
    }
}