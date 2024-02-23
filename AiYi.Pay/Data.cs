using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace AiYi.Pay
{
    public class WxPayException : Exception
    {
        public WxPayException(string msg) : base(msg)
        {

        }
    }
    public class Data
    {
        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();

        public void SetValue(string key, object value)
        {
            m_values[key] = value;
        }

        public object GetValue(string key)
        {
            object o = null;
            try
            {
                m_values.TryGetValue(key, out o);
            }
            catch
            {
            }
            return o;
        }



        public string ToJsonNo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            string[] arrKeys2 = m_values.Keys.ToArray();
            Array.Sort(arrKeys2, new Comparison<string>(string.CompareOrdinal));
            string[] array = arrKeys2;
            foreach (string key in array)
            {
                sb.Append("\"" + key + "\":\"" + m_values[key].ToString() + "\",");
            }
            return sb.ToString().Trim(',') + "}";
        }

        public bool IsSet(string key)
        {
            object? o;
            m_values.TryGetValue(key, out o);
            if (o != null)
            {
                return true;
            }
            return false;
        }

        public string ToXml()
        {
            if (m_values.Count == 0)
            {
                throw new WxPayException("WxPayData数据为空!");
            }
            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value is not null)
                {
                    xml = xml + "<" + pair.Key + "><![CDATA[" + pair.Value?.ToString() + "]]></" + pair.Key + ">";
                }
            }
            return xml + "</xml>";
        }

        public SortedDictionary<string, object> FromXml(string xml, string KEY)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new WxPayException("将空的xml串转换为WxPayData不合法!");
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;
            }
            try
            {
                CheckSign(KEY);
            }
            catch (WxPayException ex)
            {
                throw new WxPayException(ex.Message);
            }
            return m_values;
        }

        public SortedDictionary<string, object> FromXmlNatvie(string xml, string KEY)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new WxPayException("将空的xml串转换为WxPayData不合法!");
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;
            }
            try
            {
                CheckSign(KEY);
            }
            catch (WxPayException ex)
            {
                throw new WxPayException(ex.Message);
            }
            return m_values;
        }

        public List<KeyValuePair<string, string>> ToDictionary()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (!string.IsNullOrEmpty(pair.Value.ToString()))
                {
                    KeyValuePair<string, string> dt = new KeyValuePair<string, string>(pair.Key, pair.Value.ToString());
                    list.Add(dt);
                }
            }
            return list;
        }

        public string ToUrl()
        {
            string buff = "";
            string[] arrKeys2 = m_values.Keys.ToArray();
            Array.Sort(arrKeys2, new Comparison<string>(string.CompareOrdinal));
            string[] array = arrKeys2;
            foreach (string key in array)
            {
                if (key.ToLower() != "sign" && !string.IsNullOrEmpty(m_values[key].ToString()))
                {
                    buff = buff + key + "=" + m_values[key].ToString() + "&";
                }
            }
            return buff.TrimEnd('&');
        }

        public string ToUrlNo()
        {
            string buff = "";
            string[] arrKeys2 = m_values.Keys.ToArray();
            Array.Sort(arrKeys2, new Comparison<string>(string.CompareOrdinal));
            string[] array = arrKeys2;
            foreach (string key in array)
            {
                if (key.ToLower() != "sign" && !string.IsNullOrEmpty(m_values[key].ToString()))
                {
                    buff = buff + key + "=" + m_values[key].ToString() + "&";
                }
            }
            return buff.Trim('&');
        }

        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString()).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public string ToUrlEncode()
        {
            string buff = "";
            string[] arrKeys2 = m_values.Keys.ToArray();
            Array.Sort(arrKeys2, new Comparison<string>(string.CompareOrdinal));
            string[] array = arrKeys2;
            foreach (string key in array)
            {
                if (key.ToLower() != "sign" && !string.IsNullOrEmpty(m_values[key].ToString()))
                {
                    buff = buff + key + "=" + UrlEncode(m_values[key].ToString()) + "&";
                }
            }
            return buff.Trim('&');
        }

        public string ToGetUrl()
        {
            string buff = "";
            string[] arrKeys2 = m_values.Keys.ToArray();
            Array.Sort(arrKeys2, new Comparison<string>(string.CompareOrdinal));
            string[] array = arrKeys2;
            foreach (string key in array)
            {
                buff = buff + key + "=" + m_values[key].ToString() + "&";
            }
            return buff.Trim('&');
        }

        public string ToPrintStr()
        {
            string str = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    throw new WxPayException("WxPayData内部含有值为null的字段!");
                }
                str += $"{pair.Key}={pair.Value.ToString()}&";
            }
            return str.TrimEnd('&');
        }

        public string MakeSign(string KEY)
        {
            string str = ToUrl();
            str = str + "&key=" + KEY;
            MD5 md5 = MD5.Create();
            byte[] bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            byte[] array = bs;
            foreach (byte b in array)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value.ToString() != "")
                {
                    sb.Append("\"" + pair.Key + "\":\"" + pair.Value.ToString() + "\",");
                }
            }
            return sb.ToString().Trim(',') + "}";
        }

        public SortedDictionary<string, object> FromJson(string xml)
        {
            try
            {
                if (string.IsNullOrEmpty(xml))
                {
                    throw new WxPayException("将空的Json串转换为WxPayData不合法!");
                }
                JObject ob = new JObject();
                ob = JObject.Parse(xml);
                foreach (var o in ob)
                {
                    m_values[o.Key] = o.Value.ToString() ?? "";
                }
                return m_values;
            }
            catch
            {
                return null;
            }
        }

        public SortedDictionary<string, object> FromXmlNo(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new WxPayException("将空的xml串转换为WxPayData不合法!");
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;
            }
            return m_values;
        }

        public bool CheckSign(string KEY)
        {
            if (!IsSet("sign"))
            {
                return false;
            }
            if (GetValue("sign") == null || GetValue("sign").ToString() == "")
            {
                throw new WxPayException("WxPayData签名存在但不合法!");
            }
            string return_sign = GetValue("sign").ToString();
            string cal_sign = MakeSign(KEY);
            if (cal_sign.ToLower() == return_sign.ToLower())
            {
                return true;
            }
            return false;
        }

        public SortedDictionary<string, object> GetValues()
        {
            return m_values;
        }
    }
}