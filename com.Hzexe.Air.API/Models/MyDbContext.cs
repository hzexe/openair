using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    public class MyContext : DbContext
    {
        public MyContext()
            : base("name=mysql1")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AQIDataPublishLive> AQIDataPublishLives
        { get; set; }

        public DbSet<City> Cities
        { get; set; }

        public DbSet<CityAQIPublishLive> CityAQIPublishLives
        { get; set; }

        public DbSet<Province> Provinces { get; set; }


        public DbSet<user> users { get; set; }
    }
}