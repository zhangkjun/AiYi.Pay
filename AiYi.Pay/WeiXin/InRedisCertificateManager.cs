using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;

namespace AiYi.Pay.WeiXin
{

    public sealed class InRedisCertificateManager : ICertificateManager
    {

       
        private static string key = "AiYi.Pay.WeiXin";
        public InRedisCertificateManager()
        {
          
        }
        public  IEnumerable<CertificateEntry> AllEntries()
        {
            var list = new List<CertificateEntry>();
            foreach (var dr in RedisFactory.jedis.HKeys(key))
            {
                list.Add(AiYi.Core.Utils.Deserialize<CertificateEntry>(dr));
            }
            return list;
        }

        public  void AddEntry(CertificateEntry entry)
        {
            using (var tran = RedisFactory.jedis.Multi())
            {
                if (tran.HExists(key, entry.SerialNumber))
                {
                    tran.HDel(key, entry.SerialNumber);
                }
                tran.HMSet<CertificateEntry>(key, entry.SerialNumber, entry);
            }
        }

        public  CertificateEntry? GetEntry(string serialNumber)
        {
            return RedisFactory.jedis.HGet<CertificateEntry?>(key, serialNumber);

        }

        public  bool RemoveEntry(string serialNumber)
        {
            return RedisFactory.jedis.HDel(key, serialNumber) > 0;
        }


    }
}
