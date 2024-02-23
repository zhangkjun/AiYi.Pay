using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay.AliPay
{
    public class Settings
    {
        [Description("{\"id\":\"app_id\",\"name\":\"app_id\"}")]
        public string app_id { get; set; } = "";
        [Description("{\"id\":\"PID\",\"name\":\"PID\"}")]
        public string PID { get; set; } = "";
        [Description("{\"id\":\"merchant_private_key\",\"name\":\"商户私钥\"}")]
        public string merchant_private_key { get; set; } = "";
        [Description("{\"id\":\"EncryptKey\",\"name\":\"EncryptKey\"}")]
        public string EncryptKey { get; set; } = "";
        [Description("{\"id\":\"AlipayPublicCertPath\",\"name\":\"AlipayPublicCertPath\"}")]
        public string AlipayPublicCertPath { get; set; } = "";
        [Description("{\"id\":\"AppCertPath\",\"name\":\"AppCertPath\"}")]
        public string AppCertPath { get; set; } = "";
        [Description("{\"id\":\"RootCertPath\",\"name\":\"RootCertPath\"}")]
        public string RootCertPath { get; set; } = "";
        // public static string AlipayPublicCertPath = Directory.GetCurrentDirectory() + "/cert/alipayCertPublicKey_RSA2.crt";
        // public static string AppCertPath = Directory.GetCurrentDirectory() + "/cert/appCertPublicKey_2021004132655032.crt";
        // public static string RootCertPath = Directory.GetCurrentDirectory() + "/cert/alipayRootCert.crt";
    }
}
