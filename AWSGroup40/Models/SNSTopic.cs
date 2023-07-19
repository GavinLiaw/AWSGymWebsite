using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AWSGymWebsite.Models
{
    public class SNSTopic
    {
        [Key]//primary key below
        public int id { get; set; }

        public int GymID { get; set; }

        public string TopicARN { get; set; }

    }
}
