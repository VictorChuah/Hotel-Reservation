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
        [Required]
        [StringLength(20)]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

    }

    public class RegisterModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(20)]
        [System.Web.Mvc.Remote("CheckUsername", "Account", ErrorMessage = "Duplicated {0}.")]
        public string Username { get; set; }
        [Required]
        [StringLength(20,MinimumLength = 5)]
        [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        [Compare("Password")]
        public string Confirm { get; set; }
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 11)]
        [Phone]
        [RegularExpression(@"^({01}[0-9]{1})\-([0-9]{7,8})){10,11}$", ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }

        public HttpPostedFileBase Photo { get; set; }
    }

    public class AccDetailModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 11)]
        [Phone]
        [RegularExpression(@"^({01}[0-9]{1})\-([0-9]{7,8})){10,11}$", ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }

        public string PhotoURL { get; set; }
        public HttpPostedFileBase Photo { get; set; }
    }

    public class ChangePassModel
    {
        [Display(Name = "Current Password")]
        [Required]
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        public string Current { get; set; }

        [Display(Name = "New Password")]
        [Required]
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        public string New { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        [Compare("New")]
        public string Confirm { get; set; }
    }

    public class AdminCSEditVM
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [RegularExpression(@"^(01)([0-9]{1})[\-]([0-9]{7,9})$", ErrorMessage = "Invalid Phone Number")]
        public string PhoneNo { get; set; }
        [Required]
        [RegularExpression(@"[FM]", ErrorMessage = "Invalid {0}.")]
        public string Gender { get; set; }
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }


        public HttpPostedFileBase Photo { get; set; }
        public string PhotoURL { get; set; }
    }
}