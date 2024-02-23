using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay
{
    public class ShareingQuery
    {
        public string account { get; set; } = "";
        public decimal amount { get; set; } = 0M;
        public string description { get; set; } = "";
        /// <summary>
        /// PENDING:待分账,SUCCESS:分账成功,CLOSED: 分账失败已关闭
        /// </summary>
        public string result { get; set; } = "";
        public string finish_time { get; set; } = "";
        public string failreason { get; set; } = "";
        
    }
}
