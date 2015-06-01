using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Hzexe.Air;

namespace fetchtest
{
    /// <summary>
    /// 本类是获取空气基本数据的演示
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //获取当前空气基本数据，并填充或更新到数据库
            //如果当前是发行版本，会按时检测数据变化，详细看CheckBaseData类的实现
            //数据库填充使用entity framework的code-first，请更改app.config中的数据连接字符串

            com.Hzexe.Air.CheckBaseData.go(null );  
            Console.WriteLine("工作完成");

            //////导出空气站历史数据，当前已禁用，数据量很大
            ////getareahistory work = new getareahistory();
            ////work.workok += (a, b) => { Console.WriteLine("工作完全完成"); };
            ////work.start();


            while (true)
                System.Threading.Thread.Sleep(2000);
        }

    }
}
