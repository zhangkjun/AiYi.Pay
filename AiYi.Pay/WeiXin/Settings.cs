using System.ComponentModel;

namespace AiYi.Pay.WeiXin
{
    public class Settings
    {
        [Description("{\"id\":\"MerchantId\",\"name\":\"商户号\"}")]
        public string MerchantId { get; set; } = "";
        [Description("{\"id\":\"SecretV3\",\"name\":\"V3密钥\"}")]
        public string SecretV3 { get; set; } = "";
        [Description("{\"id\":\"CertificateSerialNumber\",\"name\":\"证书序列号\"}")]
        public string CertificateSerialNumber { get; set; } = "";
        [Description("{\"id\":\"CertificatePrivateKey\",\"name\":\"证书内容\"}")]
        public string CertificatePrivateKey { get; set; } = "";
    }
}
