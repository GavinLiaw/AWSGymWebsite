using AWSGymWebsite.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AWSGymWebsite.Models
{
    public class GymOwner : AWSGymWebsiteUser
    {

        [Required(ErrorMessage = "Business Contact Number is Required")]
        [Display(Name = "Business Contact Number")]
        public string BusinessContactNumber { get; set; }

        [Required(ErrorMessage = "Business Email is Required")]
        [Display(Name = "Business Email")]
        public string BusinessEmail { get; set; }

        [Required(ErrorMessage = "Business SSM Number is Required")]
        [Display(Name = "Business SSM Number")]
        public string BusinessSSM { get; set; }//New: ABCD Sdn. Bhd. – Company No. 202201234565


    }
}
