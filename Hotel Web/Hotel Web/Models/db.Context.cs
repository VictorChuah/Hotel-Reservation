﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbEntities1 : DbContext
    {
        public dbEntities1()
            : base("name=dbEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Admin> Admins { get; set; }

        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<ServiceType> ServiceTypes { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<RoomType> RoomTypes { get; set; }


        //public virtual DbSet<Reservation> Reservations { get; set; }
        // public virtual DbSet<Room> Rooms { get; set; }


    }
}
