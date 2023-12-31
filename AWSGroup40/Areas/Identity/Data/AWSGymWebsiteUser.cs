﻿using System;
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

    public DateTime RegDate { get; set; }

    public string role { get; set; }

    public string BusinessContactNumber { get; set; }

    public string BusinessEmail { get; set; }

    public string BusinessSSM { get; set; }//New: ABCD Sdn. Bhd. – Company No. 202201234565

    public AWSGymWebsiteUser()
    {
        this.Gender = " ";
        this.ContactNumber = " ";
        this.BusinessEmail = " ";
        this.BusinessContactNumber = " ";
        this.BusinessSSM = " ";
    }

}


