using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Hzexe.Air.Env.CnemcPublish
{
    [Table("DataChangedHistory")]
    public class DataChangedHistory
    {

        [Key]
        public Int64 id { get; set; }
        [StringLength(60000)]
        public string provinceChanged
        {
            get
            {
                return provinceChangeditem.ToString();
            }
            set { }
        }
        [StringLength(60000)]
        public string cityChanged
        {
            get
            {
                return cityChangeditem.ToString();
            }
            set { }
        }
        [StringLength(60000)]
        public string cityAirChanged
        {
            get
            {
                return cityAirChangeditem.ToString();
            }
            set { }
        }
        [StringLength(60000)]
        public string statianChanged
        {
            get
            {
                return statianChangeditem.ToString();
            }
            set { }
        }

        [Required]
        public DateTime changedTime { get; set; }
         [Required]
        public bool issend { get; set; }

        public DateTime? sendtime { get; set; }

        public DataChangedHistory()
        {
            TimeZoneInfo china = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            TimeZoneInfo utc = TimeZoneInfo.Utc;

            this.changedTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, china);
            issend = false;

            provinceChangeditem = new ChangedItem<int>();
            cityChangeditem = new ChangedItem<int>();
            cityAirChangeditem = new ChangedItem<int>();
            statianChangeditem = new ChangedItem<string>();
        }

        [NotMapped]
        public ChangedItem<int> provinceChangeditem { get; set; }

        [NotMapped]
        public ChangedItem<int> cityChangeditem { get; set; }

        [NotMapped]
        public ChangedItem<int> cityAirChangeditem { get; set; }

        [NotMapped]
        public ChangedItem<string> statianChangeditem { get; set; }

        public bool hasData
        {
            get
            {
                return (provinceChangeditem.hasData ||
                    cityChangeditem.hasData ||
                    cityAirChangeditem.hasData ||
                    statianChangeditem.hasData
                    );
            }
        }

    }

    public class ChangedItem<T>
    {
        public List<T> added { get; set; }
        public List<T> deleted { get; set; }

        public List<T> updated { get; set; }

        public ChangedItem()
        {
            added = new List<T>(10);
            deleted = new List<T>(10);
            updated = new List<T>(10);
        }

        public void Add(T item)
        {
            added.Add(item);
            hasData = true;
        }

        public void Delete(T item)
        {
            deleted.Add(item);
            hasData = true;
        }

        public void Delete(IEnumerable<T> item)
        {
            deleted.AddRange(item);
            hasData = true;
        }

        public void Update(T item)
        {
            updated.Add(item);
            hasData = true;
        }


        [Newtonsoft.Json.JsonIgnore]
        public bool hasData { get; set; }

        public override string ToString()
        {
            if (!hasData)
                return null;
            else
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);

            }



        }

    }


}
