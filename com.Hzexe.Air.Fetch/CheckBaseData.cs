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
    /// 检测或填充空气基础数据
    /// </summary>
    /// <remarks>数据包括，省，市，检测点，城市基础数据</remarks>
    public class CheckBaseData
    {
        //日志对象
        private static readonly log4net.ILog log = null;
        private static System.Timers.Timer tmr = null;
        static CheckBaseData()
        {
            log = log4net.LogManager.GetLogger(typeof(CheckBaseData));
#if !DEBUG
            tmr = new System.Timers.Timer(1000 * 20) { AutoReset = false };
            tmr.Elapsed += tmr_Elapsed;
            tmr.Start();
            log.Debug("开始");
#endif

        }
        /// <summary>
        /// 如果本程序集是控制台入口，需要堵塞，以后台线程做轮询
        /// </summary>
        static void Main()
        {
            while (true)
                System.Threading.Thread.Sleep(3000);
        }


        static void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                go(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ssssss" + ex.Message);
                log.Warn("tmr_Elapsed捕获异常", ex);
            }
            finally
            {
                tmr.Start();
            }
        }


        public static void go(object o)
        {
            #region MyRegion
            log.Info("本次开始");
            //Database.SetInitializer(new DropCreateDatabaseAlways<openria.Env.MyDbContext.MyContext>());
            ClientContext silverlightContext = new ClientContext();
            com.Hzexe.Air.Env.CnemcPublish.DataChangedHistory dch = new Env.CnemcPublish.DataChangedHistory();

            //获取所有省份
            {
                //同步方法获取省的集合
                IEnumerable<Province> provinces = silverlightContext.Load(silverlightContext.GetProvincesQuery()).waitLoaded().fixEntityCRC32();
                #region "对省的增删改查"
                openria.Env.MyContext db = new openria.Env.MyContext();
                var dbcxtransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                try
                {
                    var pids = provinces.Select(p => p.Id).ToList();
                    var dbprovince = db.Provinces.ToList();
                    //删除不包含的
                    var obs = dbprovince.Where(p => !pids.Contains(p.Id)).ToList();
                    if (obs.Count > 0)
                    {
                        db.Provinces.RemoveRange(obs);
                        dch.provinceChangeditem.Delete(obs.Select(a => a.Id));
                    }

                    //更新或添加
                    provinces.ToList().ForEach(p =>
                    {
                        var dbp = dbprovince.FirstOrDefault(pp => p.Id == pp.Id);
                        if (null == dbp)
                        {
                            db.Provinces.Add(p);
                            dch.provinceChangeditem.Add(p.Id);
                        }
                        else if (dbp.datahash != p.datahash)
                        {
                            db.Provinces.Remove(dbp);
                            db.Provinces.Add(p);
                            dch.provinceChangeditem.Update(p.Id);
                        }
                    });
                    int rows = db.SaveChanges();
                    dbcxtransaction.Commit();
                    log.Info("GetProvincesQuery成功:影响行数" + rows);
                }
                catch (Exception ex)
                {
                    dbcxtransaction.Rollback();
                    log.Warn("获取所有省异常", ex);
                }
                finally
                {
                    dbcxtransaction.Dispose();
                    db.Dispose();
                }
                #endregion

                ////回调的方式获取
                //client.Load(client.GetProvincesQuery(), LoadBehavior.RefreshCurrent, lo =>
                //{
                //    var cs = lo.Entities;   //省的集合
                //}, true
                //);
            }

            //获取所有城市
            {
                //同步方法获取所有城市
                IEnumerable<City> citys = silverlightContext.Load(silverlightContext.GetCitiesQuery()).waitLoaded().fixEntityCRC32();
                #region 城市处理
                openria.Env.MyContext db = new openria.Env.MyContext();
                var dbcxtransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                try
                {
                    var pids = citys.Select(p => p.Id).ToList();
                    var dbcitys = db.Cities.ToList();
                    //删除不包含的
                    var obs = dbcitys.Where(c => !pids.Contains(c.Id)).ToList();
                    if (obs.Count > 0)
                    {
                        db.Cities.RemoveRange(obs);
                        dch.cityChangeditem.Delete(obs.Select(a => a.Id));
                    }

                    //更新或添加
                    //var dbdic = dbcitys.ToDictionary(c => p.Id, p =>c.datahash);
                    citys.ToList().ForEach(c =>
                    {
                        var dbp = dbcitys.FirstOrDefault(ct => c.Id == ct.Id);

                        if (null == dbp)
                        {
                            db.Cities.Add(c);
                            dch.cityChangeditem.Add(c.Id);
                        }
                        else if (dbp.datahash != c.datahash)
                        {
                            db.Cities.Remove(dbp);
                            db.Cities.Add(c);
                            dch.cityChangeditem.Update(c.Id);
                        }
                    });
                    int rows = db.SaveChanges();
                    dbcxtransaction.Commit();
                    log.Info("GetCitiesQuery成功,影响行数:" + rows);
                }
                catch (Exception ex)
                {
                    dbcxtransaction.Rollback();
                    log.Warn("获取所有城市异常", ex);
                }
                finally
                {
                    dbcxtransaction.Dispose();
                    db.Dispose();
                    citys = null;
                }
                #endregion
            }

            //获取指定省的城市
            {
                //int pid = 11;       //省的编号，这里用浙江做例子

                ////同步方法获取当前省的所有城市
                //IEnumerable<City> citys = client.Load(client.GetCitiesByPidQuery(pid)).waitLoaded();
                ////回调的方式
                //client.Load(client.GetCitiesByPidQuery(pid), LoadBehavior.RefreshCurrent, lo =>
                //{
                //    var cs = lo.Entities;   //所有当前省份城市集合
                //}, true
                //);
            }

            //获取所有城市的实时空气质量
            {
                //同步方法获取所有城市的实时空气质量
                log.Info("开始CityAQIPublishLive");
                IEnumerable<CityAQIPublishLive> citylives = silverlightContext.Load(silverlightContext.GetCityAQIPublishLivesQuery()).waitLoaded().fixEntityCRC32();
                #region 城市空气处理
                openria.Env.MyContext db = new openria.Env.MyContext();
                var dbcxtransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                try
                {
                    var pids = citylives.Select(p => p.CityCode).ToList();
                    var dbcitys = db.CityAQIPublishLives.ToList();
                    //删除不包含的
                    var obs = dbcitys.Where(c => !pids.Contains(c.CityCode)).ToList();
                    if (obs.Count > 0)
                    {
                        db.CityAQIPublishLives.RemoveRange(obs);
                        dch.cityAirChangeditem.Delete(obs.Select(a => a.Id));
                    }

                    //更新或添加
                    //var dbdic = dbcitys.ToDictionary(c => p.Id, p =>c.datahash);
                    //int changed = 0;//更改数，如果一次更新很大，就分多次更新，来减少CPU占用
                    foreach (var c in citylives)
                    {
                        //if (++changed > 100) break;
                        var dbp = dbcitys.FirstOrDefault(ct => c.CityCode == ct.CityCode);
                        if (null == dbp)
                        {
                            db.CityAQIPublishLives.Add(c);
                            dch.cityAirChangeditem.Add(c.Id);
                        }
                        else if (dbp.datahash != c.datahash)
                        {
                            dbp.AQI = c.AQI;
                            dbp.Area = c.Area;
                            dbp.CityCode = c.CityCode;
                            dbp.CO = c.CO;
                            dbp.datahash = c.datahash;
                            dbp.Measure = c.Measure;
                            dbp.NO2 = c.NO2;
                            dbp.O3 = c.O3;
                            dbp.PM10 = c.PM10;
                            dbp.PM2_5 = c.PM2_5;
                            dbp.PrimaryPollutant = c.PrimaryPollutant;
                            dbp.Quality = c.Quality;
                            dbp.SO2 = dbp.SO2;
                            dbp.TimePoint = c.TimePoint;
                            dbp.Unheathful = dbp.Unheathful;

                            dbp.RaiseDataMemberChanging("Unheathful");
                            dch.cityAirChangeditem.Update(c.Id);


                            //db.CityAQIPublishLives.Remove(dbp);
                            //db.CityAQIPublishLives.Add(c);
                        }
                    }
                    int count = db.SaveChanges();
                    dbcxtransaction.Commit();
                    log.Info("CityAQIPublishLive成功,影响行数" + count);
                }
                catch (Exception ex)
                {
                    dbcxtransaction.Rollback();
                    log.Warn("保存城市的实时空气质量异常", ex);
                }
                finally
                {
                    dbcxtransaction.Dispose();
                    db.Dispose();
                    citylives = null;
                }
                #endregion
            }

            log.Info("AQIDataPublishLive开始");
            //获取所有城市的实时实时空气质量（包括AQI日报）
            {
                //同步方法获取当前省的所有城市
                IEnumerable<AQIDataPublishLive> aqiLive = silverlightContext.Load(silverlightContext.GetAQIDataPublishLivesQuery()).waitLoaded().fixEntityCRC32();
                openria.Env.MyContext db = new openria.Env.MyContext();
                var dbcxtransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                try
                {
                    var pids = aqiLive.Select(p => p.StationCode).ToList();
                    var dbcitys = db.AQIDataPublishLives.ToList();
                    //删除不包含的
                    var obs = dbcitys.Where(c => !pids.Contains(c.StationCode)).ToList();
                    if (obs.Count > 0)
                    {
                        db.AQIDataPublishLives.RemoveRange(obs);
                        dch.statianChangeditem.Delete(obs.Select(a => a.StationCode));
                    }

                    //更新或添加
                    //var dbdic = dbcitys.ToDictionary(c => p.Id, p =>c.datahash);
                    //int changed = 0;//更改数，如果一次更新很大，就分多次更新，来减少CPU占用
                    foreach (var c in aqiLive)
                    {
                        //if (++changed > 100) break;
                        var dbp = dbcitys.FirstOrDefault(ct => c.StationCode.Equals(ct.StationCode));
                        if (null == dbp)
                        {
                            db.AQIDataPublishLives.Add(c);
                            dch.statianChangeditem.Add(c.StationCode);
                        }
                        else if (dbp.datahash != c.datahash)
                        {
                            dbp.AQI = c.AQI;
                            dbp.Area = c.Area;
                            dbp.CityCode = c.CityCode;
                            dbp.CO = c.CO;
                            dbp.datahash = c.datahash;
                            dbp.Measure = c.Measure;
                            dbp.NO2 = c.NO2;
                            dbp.O3 = c.O3;
                            dbp.PM10 = c.PM10;
                            dbp.PM2_5 = c.PM2_5;
                            dbp.PrimaryPollutant = c.PrimaryPollutant;
                            dbp.Quality = c.Quality;
                            dbp.SO2 = dbp.SO2;
                            dbp.TimePoint = c.TimePoint;
                            dbp.Unheathful = dbp.Unheathful;
                            dbp.RaiseDataMemberChanging("Unheathful");
                            dbp.StationCode = c.StationCode;
                            dbp.SO2_24h = c.SO2_24h;
                            dbp.ProvinceId = c.ProvinceId;
                            dbp.CO_24h = c.CO_24h;
                            dbp.IsPublish = c.IsPublish;
                            dbp.Latitude = c.Latitude;
                            dbp.Longitude = c.Longitude;
                            dbp.NO2_24h = c.NO2_24h;
                            dbp.O3_24h = c.O3_24h;
                            dbp.O3_8h = c.O3_8h;
                            dbp.O3_8h_24h = c.O3_8h_24h;
                            dbp.OrderId = c.OrderId;
                            dbp.PM10_24h = c.PM10_24h;
                            dbp.PM2_5_24h = c.PM2_5_24h;
                            dbp.PositionName = c.PositionName;
                            dbp.ProvinceId = c.ProvinceId;
                            dbp.SO2_24h = c.SO2_24h;
                            dbp.RaiseDataMemberChanging("AQI");
                            dch.statianChangeditem.Update(c.StationCode);
                            //db.AQIDataPublishLives.Remove(dbp);
                            //db.AQIDataPublishLives.Add(c);
                        }
                    }
                    int count = db.SaveChanges();
                    dbcxtransaction.Commit();
                    log.Info("AQIDataPublishLive成功,影响行数" + count);
                }
                catch (Exception ex)
                {
                    dbcxtransaction.Rollback();
                    log.Warn("保存检测点的异常", ex);
                }
                finally
                {
                    dbcxtransaction.Dispose();
                    db.Dispose();
                    aqiLive = null;
                }
            }

            //检测是否存在更新的数据，写入数据库，以用来通知其它客户端数据变化，做更新缓存等操作
            if (dch.hasData)
            {
                openria.Env.MyContext db = new openria.Env.MyContext();
                db.DataChangedHistory.Add(dch);
                db.SaveChanges();
                //这里面是否要添加事件的触发?
            }


            log.Info("本次结束");

            #endregion
        }

    }
}
