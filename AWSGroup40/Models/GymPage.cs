using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AWSGymWebsite.Models
{
    public class GymPage
    {

        [Key]//primary key below
        public int ID { get; set; }

        public string? OwnerID { get; set; }

        [Required(ErrorMessage = "Gym Name is Required")]
        [Display(Name = "Gym Name")]
        public string GymName { get; set; }

        [Required(ErrorMessage = "Gym Location is Required")]
        [Display(Name = "Gym Location")]
        public string GymLocation { get; set; }

        [Required(ErrorMessage = "Closing Time is Required")]
        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public DateTime ClosingTime { get; set; }

        [Required(ErrorMessage = "Opening Time is Required")]
        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public DateTime OpeningTime { get; set; }

        [Required(ErrorMessage = "Contact Number Time is Required")]
        [Display(Name = "Contact Number")]
        public String ContactNumber { get; set; }

        [Required(ErrorMessage = "Gym Details is Required")]
        [Display(Name = "Gym Details")]
        public String Details { get; set; }
        public String? ImgURL { get; set; }
        public String? S3Key { get; set; }

        public int viewer { get; set; }

    }
}
