using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Env.CnemcPublish.DAL;
using com.Hzexe.Air.Env.ContractLocal;
using OpenRiaServices.DomainServices.Client;
using System.IO;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading;

namespace com.Hzexe.Air
{
    /// <summary>
    /// 导出所有检测点历史空气数据
    /// </summary>
    /// <remarks>数据量十分大，把数据临时写入某目录，以json格式，而不是直接写入数据库</remarks>
    public class getareahistory
    {
        const int pertime = 1000;
        static int alltime = 0;
        static int sccess = 0;
        static object locked;

        public event EventHandler workok;


        public void start()
        {
            locked = new object();
            ClientContext client = new ClientContext();
            var t1 = client.GetAQIDataPublishHistoriesQuery().OrderByDescending(c => c.TimePoint).Take(1);
            t1.IncludeTotalCount = true;

            var t2 = client.Load(t1);
            var t3 = t2.waitLoaded();
            double allcount = t2.TotalEntityCount; client = null;
            Console.Write("共有{0}条数据\r\n", allcount);
            

            alltime=(int)Math.Ceiling(allcount / Convert.ToDouble(pertime));
            Console.Write("共需要处理{0}次数据\r\n", alltime);
            ThreadPool.SetMinThreads(6, 4);
            ThreadPool.SetMaxThreads(200, 2);
            System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, alltime - 1), i=>go(i));

            Console.Write("共{0}   个工作任务，完成{1} 百分比:{2}\r\n", alltime, sccess, 0.ToString("0.00%"));
        }

        protected void ok()
        {
            lock (locked)
            {
                ++sccess;
                double v=Convert.ToDouble( sccess)/alltime;
                Console.Write("\r共{0}   个工作任务，完成{1} 百分比:{2}", alltime, sccess,v.ToString("0.00%"));
                if (sccess == alltime)
                    workok.Invoke(this,new EventArgs ());
            }
        }

        protected void go(object o, int times)
        {
            if (times > 10)
            {
                ok();
            }
            int step = (int)o;
            ClientContext client = new ClientContext();
            try
            {
                var eee = client.Load(client.GetAQIDataPublishHistoriesQuery().Skip(pertime * step).Take(pertime), LoadBehavior.RefreshCurrent, true).waitLoaded();
                if (eee.Count() > 0)
                {
                    string path=System.IO.Path.Combine(System.Environment.CurrentDirectory, "data", step + ".json");
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(eee);
                    File.WriteAllText(path, json, System.Text.Encoding.UTF8);
                    //写文件
                    ok();
                }
            }
            catch (Exception ex)
            {
                go(o, ++times);
            }
        
        }
        public void go(object o)
        {
            go(o, 0);

        }

    }
}
