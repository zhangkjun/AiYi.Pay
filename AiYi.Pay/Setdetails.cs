using Newtonsoft.Json;

namespace AiYi.Pay
{
    public class Setdetails
    {
       
        /// <summary>
        /// 用户转账的账号
        /// </summary>
        [JsonProperty]
        public string Identity { get; set; } = string.Empty;
        [JsonProperty]
        public string IdentityType { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户转账的姓名
        /// </summary>
        [JsonProperty]
        public string Name { get; set; } = "";

       
      
        public decimal ProfitAmount { get; set; } = 0M;

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remarks { get; set; } = "";


    }
}
