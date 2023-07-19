using AWSGymWebsite.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AWSGymWebsite.Models
{
    public class Viewer : AWSGymWebsiteUser
    {

        [DataType(DataType.Date)]
        public DateTime RegDate { get; set; }

    }
}
