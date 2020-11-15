using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Hotel_Web.Models
{
    public class CustomerMedata
    {
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number!")]
        public int PhoneNo { get; set; }
        [Required]
        [RegularExpression(@"[FM]", ErrorMessage = "Invalid {0}.")]
        public string Gender { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

    }
    [MetadataType(typeof(CustomerMedata))]
    public partial class Customer { }
}