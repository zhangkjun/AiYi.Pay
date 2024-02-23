using AiYi.Core;
using FreeRedis;
using Newtonsoft.Json;

namespace AiYi.Pay
{
    public class Coordinate
    {

        //经度
        private decimal longitude;

        //纬度
        private decimal latitude;

        //用户id
        private string key="";

        public decimal getLatitude()
        {
            return latitude;
        }
        public void setLatitude(decimal latitude)
        {
            this.latitude = latitude;
        }
        public decimal getLongitude()
        {
            return longitude;
        }
        public void setLongitude(decimal longitude)
        {
            this.longitude = longitude;
        }
        public string getKey()
        {
            return key;
        }
        public void setKey(string key)
        {
            this.key = key;
        }

    }
    public class RedisFactory
    {
        static Lazy<RedisClient> _cliLazy = new Lazy<RedisClient>(() =>
        {
            var r = new RedisClient(AiYi.Core.Caching.ConfigSettings.Setting.ConnectionString);
            r.Serialize = obj => JsonConvert.SerializeObject(obj);
            r.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
            return r;
        });
        public static RedisClient jedis => _cliLazy.Value;
      
        /// <summary>
        /// 锁数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public static async Task<T> CreateLock<T>(string key, Func<Task<T>> acquire, int timeoutSeconds = 1)
        {
			try
			{
				var lockObj = jedis.Lock(key, timeoutSeconds);
				if (lockObj is not null)
				{
					try
					{
						var result =await acquire();
						if (result is not null)
						{
							return result;
						}
						else
						{
							return default(T);
						}
					}
					catch
					{
						return default(T);
					}
					finally
					{
						lockObj.Unlock(); // 解锁
					}
				}
				else
				{
					return default(T);
				}

			}
			catch
			{
				return default(T);
			}
        }
        /// <summary>
        ///分布式锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="expirationTime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task<bool> PerformActionWithLockAsync(string resource, int expirationTime, Func<Task> action)
        {
            try
            {
                var lockObj = jedis.Lock(resource, expirationTime);
                if (lockObj is not null)
                {
                    try
                    {
                        await action();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        lockObj.Unlock(); // 解锁
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 插入任务队列
        /// </summary>
        /// <param name="QueueName">队列名称</param>
        /// <param name="jobId">任务名称</param>
        /// <param name="delaySeconds">多少秒后过期</param>
        /// <returns></returns>
        public static async Task<long> addJobId(string QueueName, string jobId, long delaySeconds)
        {
            try
            {

                return await jedis.ZAddAsync(QueueName, delaySeconds, jobId);
            }
            catch (Exception e)
            {
                //Chinahoo.Core.Utils.WriteTxt("reids", e.ToString());
                return 0;
            }
        }
        /// <summary>
        /// 获取队列列表
        /// </summary>
        /// <param name="QueueName"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static async Task<ZMember[]> GZAdd(string QueueName, DateTime now)
        {
            //ZRangeByScoreWithScores
            return await jedis.ZRangeByScoreWithScoresAsync(QueueName, 0, now.ToTimeUnix());
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="QueueName"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public static async Task<long> ZRem(string QueueName, params string[] members)
        {
            //ZRem
            return await jedis.ZRemAsync(QueueName, members);
        }

        /// <summary>
        /// 添加进入队列
        /// </summary>
        /// <param name="QueueName"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static async Task<long> addLPush(string QueueName, string str)
        {
            try
            {

                return await jedis.LPushAsync(QueueName, str);
            }
            catch (Exception e)
            {
                //Chinahoo.Core.Utils.WriteTxt("reids", e.ToString());
                return 0;
            }
        }
        /// <summary>
        ///   添加坐标
        /// </summary>
        /// <param name="coordinate">key 经度 维度  距离</param>
        /// <param name="table"></param>
        /// <returns>m 表示单位为米</returns>
        private static async Task<long> addReo(Coordinate coordinate, string table)
        {
            //第一个参数可以理解为表名
            var model = new GeoMember(coordinate.getLongitude(), coordinate.getLatitude(), coordinate.getKey());
            await jedis.ZRemAsync(table, model.member);
            return await jedis.GeoAddAsync(table, model);

        }
        /// <summary>
        /// 添加数据到redis中
        /// </summary>
        /// <param name="Latitude">维度</param>
        /// <param name="Longitude">经度</param>
        /// <param name="Id">用户的唯一id</param>
        /// <param name="table">表名</param>
        public static async Task<bool> AddGeo(decimal Latitude, decimal Longitude, string Id, string table)
        {
            //添加经纬度
            Coordinate coordinate = new Coordinate();
            coordinate.setLatitude(Latitude);  //维度
            coordinate.setLongitude(Longitude); //经度
            coordinate.setKey(Id);  //可以作为用户表的id
            return (await addReo(coordinate, table) > 0);

        }
        /// <summary>
        /// 查询附近人
        /// </summary>
        /// <param name="Longitude">经度</param>
        /// <param name="Latitude">维度</param>
        /// <param name="unit"> m = 0(表示单位为米), km = 1(表示单位为千米。), mi = 2(表示单位为英里),ft = 3(表示单位为英尺)</param>
        /// <param name="count">最多返回的数量</param>
        /// <param name="radius">多少范围的数据和unit对应</param>
        /// <param name="table">表名称</param>
        /// <returns></returns>
        public static async Task<GeoRadiusResult[]> geoQuery(decimal Longitude, decimal Latitude, int unit, long count, decimal radius, string table)
        {
           
                // 命令：GEORADIUS key longitude latitude radius m| km | ft | mi[WITHCOORD][WITHDIST][WITHHASH][COUNT count]
                return await jedis.GeoRadiusAsync(table, Longitude, Latitude, radius, (GeoUnit)unit, true, true, false, count, Collation.asc);
            
        }
        /// <summary>
        /// 添加验证码数据到缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        public static async Task AddCode(string key, string Code)
        {
            await jedis.SetAsync<string>("lock." + key, Code, 1800);//加入锁定缓存30分钟此手机号码不能被其他人使用
            await jedis.SetAsync<string>(key, Code, 300);
        }
       /// <summary>
       /// 检查验证码是否正确
       /// </summary>
       /// <param name="key"></param>
       /// <param name="Code"></param>
       /// <returns></returns>
        public static async Task<(bool isSet, string item)> CheckCode(string key,string Code)
        {
            
                if (!string.IsNullOrEmpty(Code))
                {
                    //是否包含key
                    var exit = await jedis.ExistsAsync(key);
                    if (exit)
                    {
                        //次数
                        var number = await jedis.GetAsync<string>(key);
                        if (number==Code)
                        {
                            return (true, "验证通过");
                        }
                        else
                        {
                        //删除验证码缓存
                         await jedis.DelAsync(key);
                          return (false, "验证码错误");
                        }
                    
                    }
                    else//不存在
                    {
                    return (false, "验证码已过期请重新获取");
                }
                }
                else
                {
                    return (false, "验证码不存在");
                }


        }

    }
}
