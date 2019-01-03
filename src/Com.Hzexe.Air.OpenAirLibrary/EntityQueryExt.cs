using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Com.Hzexe.Air.OpenAirLibrary
{
    using System.Linq;
    /// <summary>
    /// 请求返回数据处理扩展
    /// </summary>
    public static class EntityQueryExt
    {
        public static T Decompress<T>(byte[] compressedData) where T : class, new()
        {
            //T local = default(T);
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                byte[] buffer = Decompress(compressedData);
                stream.Write(buffer, 0, buffer.Length);
                stream.Position = 0;
                return (serializer.ReadObject(stream) as T);
            }
        }

        /// <summary>
        /// 转换成Task异步,并且把压缩数据反序列化成指定的类型
        /// </summary>
        /// <typeparam name="T">要反序列化的类型</typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Task<T> ResultAsync<T>(this InvokeOperation<byte[]> q) where T : class
        {
            return Task.Run(() =>
            {
                System.Threading.AutoResetEvent auto = new System.Threading.AutoResetEvent(false);
                q.Completed += (a, b) => auto.Set();
                auto.WaitOne();
                auto.Dispose();
                byte[] result = Decompress(q.Value);
#if (DEBUG)
                string xml = System.Text.Encoding.UTF8.GetString(result);
                //System.IO.File.WriteAllBytes("bigfile.txt", result);
                //var s = xml.Substring(0, 100);
                //Debug.WriteLine(s);

#endif
                using (MemoryStream stream = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    stream.Write(result, 0, result.Length);
                    stream.Position = 0;// (0L);
                    return (serializer.ReadObject(stream) as T);
                }
            });
        }

        /// <summary>
        /// 转换成Task异步
        /// </summary>
        /// <typeparam name="U">返回类型</typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Task<U> ResultAsync<U>(this InvokeOperation<U> q)
        {
            return Task.Run(() =>
            {
                System.Threading.AutoResetEvent auto = new System.Threading.AutoResetEvent(false);
                q.Completed += (a, b) => auto.Set();
                auto.WaitOne();
                auto.Dispose();
                return q.Value;
            });
        }



        /// <summary>
        /// 转换成Task异步
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TEntity>> ResultAsync<TEntity>(this LoadOperation<TEntity> q) where TEntity : Entity
        {
            return Task.Run(() =>
             {
                 System.Threading.AutoResetEvent auto = new System.Threading.AutoResetEvent(false);
                 q.Completed += (a, b) =>
                 {
                     auto.Set();
                 };

                 auto.WaitOne();
                 auto.Dispose();
                 if (null != q.Error)
                     throw q.Error;
                 return q.Entities;
             });
        }

        /// <summary>
        /// 转换成Task异步
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static async  Task<TEntity> ResultOneAsync<TEntity>(this LoadOperation<TEntity> q) where TEntity : Entity
        {
            var p = await q.ResultAsync< TEntity>();
            return p.FirstOrDefault();
        }


        /// <summary>
        /// Inflater解压
        /// </summary>
        /// <param name="baseBytes"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] baseBytes)
        {
            string resultStr = string.Empty;
            using (MemoryStream memoryStream = new MemoryStream(baseBytes))
            {
                using (InflaterInputStream inf = new InflaterInputStream(memoryStream))
                {
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        byte[] result = new byte[1024];
                        int resLen;
                        while ((resLen = inf.Read(result, 0, result.Length)) > 0)
                        {
                            buffer.Write(result, 0, resLen);
                        }
                        //resultStr = Encoding.UTF8.GetString(result);
                        byte[] bytes = buffer.ToArray();
                        //resultStr = Encoding.UTF8.GetString(bytes);
                        return bytes;
                    }
                }
            }

        }
    }
}
