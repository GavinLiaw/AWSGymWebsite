using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;


namespace AWSGymWebsite.Models
{
    public class Subscriber
    {
        [Key]//primary key below
        public int id { get; set; }

        public string UserID { get; set; }

        public int GymID { get; set; }

        public string SubARN { get; set; }

        public DateTime SubDate { get; set; }

    }
}
