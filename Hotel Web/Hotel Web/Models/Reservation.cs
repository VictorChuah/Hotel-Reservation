//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hotel_Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Reservation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Reservation()
        {
            this.Services = new HashSet<Service>();
        }
    
        public string Id { get; set; }
        public string Username { get; set; }
        public string RoomId { get; set; }
        public System.DateTime CheckIn { get; set; }
        public System.DateTime CheckOut { get; set; }
        public decimal Price { get; set; }
        public int Person { get; set; }
        public int Day { get; set; }
        public decimal Total { get; set; }
        public bool Paid { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Room Room { get; set; }
        //public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Service> Services { get; set; }
    }
}
