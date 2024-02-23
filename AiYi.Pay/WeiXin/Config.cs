using System.ComponentModel;

namespace AiYi.Pay.WeiXin
{
    public class Config
    {
        [Description("{\"id\":\"receiveType\",\"name\":\"分账接收方类型\"}")]
        public string receiveType { get; set; } = string.Empty;
        [Description("{\"id\":\"account\",\"name\":\"分账接收方账号\"}")]
        public string account { get; set; } = string.Empty;

        [Description("{\"id\":\"name\",\"name\":\"分账接收方全称\"}")]
        public string name { get; set; } = string.Empty;

        [Description("{\"id\":\"receiverRelationType\",\"name\":\"关系类型\"}")]
        public string receiverRelationType { get; set; } = string.Empty;

        [Description("{\"id\":\"custom_relation\",\"name\":\"自定义的分账关系\"}")]
        public string custom_relation { get; set; } = string.Empty;
    }
}
