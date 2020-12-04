﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hotel_Web.Models
{

    public class RoomMetadata
    {
        [Required]
        public string RoomTypeId { get; set; }
    }
    [MetadataType(typeof(RoomMetadata))]
    public partial class Room { }


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
        [StringLength(20, MinimumLength = 8)]
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
        [StringLength(20,MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string Confirm { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [RegularExpression(@"^.+@.+mail.com$", ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }

        [Required]
        [StringLength(11)]
        [Phone]
        [RegularExpression(@"^(01)([0-9]{1})\-([0-9]{7})$", ErrorMessage = "Invalid Phone Number")]
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
        [RegularExpression(@"^.+@.+mail.com$", ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }

        [Required]
        [StringLength(11)]
        [Phone]
        [RegularExpression(@"^(01)([0-9]{1})\-([0-9]{7})$", ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }

        public string PhotoURL { get; set; }

        public HttpPostedFileBase Photo { get; set; }
    }

    public class ChangePassModel
    {
        [Display(Name = "Current Password")]
        [Required]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        public string Current { get; set; }

        [Display(Name = "New Password")]
        [Required]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        public string New { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.+[a-z])(?=.+[A-Z])(?=.+\d)(?=.+[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        [System.ComponentModel.DataAnnotations.Compare("New")]
        public string Confirm { get; set; }
    }

    public class forgetPassModel
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [RegularExpression(@"^.+@.+mail.com$", ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }
    }

    public class ResetPassModel
    {
        [Display(Name = "New Password")]
        [Required]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        public string New { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
        [System.ComponentModel.DataAnnotations.Compare("New")]
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

    public class EditAdminDetail
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

    public class MultipleClass
    {

        public Customer Cus { get; set; }
        public Reservation Re { get; set; }
        public Room room { get; set; }
        public RoomType roomtype { get; set; }
        public Service se { get; set; }
        public ServiceType Setype { get; set; }

    }

    public class joinRoom {
    
        public Room room { get; set; }
        public RoomType roomtype { get; set; }
    
    }

    public class InsertAdmin {

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Remote("CheckUsername", "Admin", ErrorMessage = "Duplicated {0}.")]
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

        [Required]
        [StringLength(20, MinimumLength = 5)]
       // [RegularExpression(@"^([A-Za-z]{1,}[0-9]{1,}[-!@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{1,}){5,20}$")]
        public string Password { get; set; }

        [Required]
        public HttpPostedFileBase Photo { get; set; }

    }

    public class addroom {

        public string id { get; set; }
        public string roomtype { get; set; }

        public string status { get; set; }
    
    }

    public class editRoomType {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string PhotoURL { get; set; }
        public int person { get; set; }
    }

    public class addRoomType {

        [Required]
        [StringLength(50)]
        public string name { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        [Required]
        [ RegularExpression("^[1-4]{1}$", ErrorMessage = "Only In digit between 1 to 4 person ")] 
        public int person { get; set; }

        [Required]
        public HttpPostedFileBase Photo { get; set; }
    }
}