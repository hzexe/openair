using Env.CnemcPublish.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using MySql.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

namespace openria.Env
{
        public class MyContext : DbContext
        {

            static MyContext()
            {
                //Database.SetInitializer<MyContext>(null);
                //CreateDatabaseIfNotExists<openria.Env.MyDbContext.MyContext>
                Database.SetInitializer(new DropCreateDatabaseAlways<MyContext>());
                //Database.SetInitializer(new CreateDatabaseIfNotExists<MyContext>());

            }


            public MyContext()
                : base("name=mysql1")
            {




               // Database.Configuration.ProxyCreationEnabled = false;
                //this.ChangeTracker.
                //Configuration.AutoDetectChangesEnabled = false;
               // Configuration.ValidateOnSaveEnabled = false;

                //var f = System.IO.File.CreateText("a.sql");
#if(DEBUG)
               Database.Log = s => Debug.WriteLine(s);
#endif
            }


            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                //modelBuilder.Conventions.Add()
                base.OnModelCreating(modelBuilder);
            }

            //public DbSet<Data> Datas { get; set; }
            /*
            public DbSet<AQIChartValue> AQIChartValues
            { get; set; }
            */
            public DbSet<AQIDataPublishHistory> AQIDataPublishHistories
            { get; set; }
            
            public DbSet<AQIDataPublishLive> AQIDataPublishLives { get; set; }
            /*
            public DbSet<BlackList> BlackLists
            { get; set; }
            */
            public DbSet<City> Cities
            { get; set; }
            /*
            public DbSet<CityAQIConfig> CityAQIConfigs
            { get; set; }

            public DbSet<CityAQIModel> CityAQIModels
            { get; set; }

            public DbSet<CityAQIPublishHistory> CityAQIPublishHistories
            { get; set; }
             * */
            public DbSet<CityAQIPublishLive> CityAQIPublishLives
            { get; set; }
            /*
            public DbSet<CityDayAQIPublishHistory> CityDayAQIPublishHistories
            { get; set; }
           
            public DbSet<CityDayAQIPublishLive> CityDayAQIPublishLives
            { get; set; }
          
            public DbSet<FuctionTable> FuctionTables
            { get; set; }

            public DbSet<IAQIDataPublishHistory> IAQIDataPublishHistories
            { get; set; }

            public DbSet<IAQIDataPublishLive> IAQIDataPublishLives
            { get; set; }

            public DbSet<ModelCityConfig> ModelCityConfigs
            { get; set; }
            */
            public DbSet<Province> Provinces
            { get; set; }
            /*
            public DbSet<PublishLog> PublishLogs
            { get; set; }

            public DbSet<StationConfig> StationConfigs
            { get; set; }

            public DbSet<SystemConfig> SystemConfigs
            { get; set; }

            public DbSet<WhiteList> WhiteLists
            { get; set; }

            */

            public DbSet<com.Hzexe.Air.API.Models.user> users { get; set; }

            public DbSet<com.Hzexe.Air.Env.CnemcPublish.DataChangedHistory> DataChangedHistory { get; set; }


        }

}
