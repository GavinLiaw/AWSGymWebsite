using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AWSGymWebsite.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AWSGymWebsiteUser class
public class AWSGymWebsiteUser : IdentityUser
{

    //Usage of changing userinfo table
    public string Userfname { get; set; }
    public string Userlname { get; set; }
    public string ContactNumber { get; set; }
    public string Gender { get; set; }
    public DateTime UserDob { get; set; }

    [DataType(DataType.Date)]
    public DateTime RegDate { get; set; }

    public string role { get; set; }

}

