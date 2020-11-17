using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Hotel_Web.Models
{
    public class CustomerMetadata
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
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer { }


    public class LoginModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}