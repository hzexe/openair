using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using Tako.CRC;
using System.ComponentModel;
using System.Diagnostics;

namespace OpenRiaServices.DomainServices.Client
{
    public interface IEntityDatahash
    {
        int datahash { get; set; }
        void RaiseDataMemberChanging(string x);

    }




    static class EntityQueryExt
    {

        //IEnumerable<Province>
        public static IEnumerable<T> fixEntityCRC32<T>(this IEnumerable<T> ents) where T : Entity, IEntityDatahash
        {
            foreach (var item in ents)
            {
                //var status = item.EntityState;
                item.fixEntityCRC32();
                //var status2 = item.EntityState;

            }
            return ents;
        }


        ///
        /// <summary>
        /// 填充数据的datahash值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static T fixEntityCRC32<T>(this T ent) where T : Entity, IEntityDatahash
        {
            ent.datahash = (int)ent.getEntityCRC32();
            return ent;
        }

        /// <summary>
        /// 获取数据的crc32值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static uint getEntityCRC32<T>(this T ent) where T : Entity, IEntityDatahash
        {
            uint crc32;

            var jsonSerializer = new DataContractJsonSerializer(ent.GetType());
            using (var memoryStream = new MemoryStream())
            {
                jsonSerializer.WriteObject(memoryStream, ent);
                byte[] bytes = memoryStream.ToArray();
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                JObject jt = JObject.Parse(json);
                jt["datahash"] = 0;
                var s = jt.ToString();
                bytes = System.Text.Encoding.UTF8.GetBytes(s);

                CRCManager manager = new CRCManager() { DataFormat = EnumOriginalDataFormat.HEX };
                var provider = manager.CreateCRCProvider(EnumCRCProvider.CRC32);

                var status = provider.GetCRC(bytes);
                crc32 = status.CrcDecimal;
            }
            return crc32;
        }



        public static T Decompress<T>(byte[] compressedData) where T : class
        {
            T local = default(T);
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
        /// 同步反序列化成指定的类型
        /// </summary>
        /// <typeparam name="T">要反序列化的类型</typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static T waitInvoke<T>(this InvokeOperation<byte[]> q) where T : class
        {
            System.Threading.AutoResetEvent auto = new System.Threading.AutoResetEvent(false);
            q.Completed += (a, b) => auto.Set();
            auto.WaitOne();
            auto.Dispose();
            byte[] result = Decompress(q.Value);
#if(DEBUG)
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
        }



        /// <summary>
        /// 等待数据载入，提供一个同步方法的扩展
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> waitLoaded<TEntity>(this LoadOperation<TEntity> q) where TEntity : Entity
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
