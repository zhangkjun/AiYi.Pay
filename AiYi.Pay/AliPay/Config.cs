using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay.AliPay
{
    public class Config
    {
        [Description("{\"id\":\"types\",\"name\":\"分账方类型\"}")]
        public string types { get; set; }

        [Description("{\"id\":\"account\",\"name\":\"分账方帐号\"}")]
        public string account { get; set; }

        [Description("{\"id\":\"name\",\"name\":\"分账方全称\"}")]
        public string name { get; set; }

        [Description("{\"id\":\"memo\",\"name\":\"分账关系描述\"}")]
        public string memo { get; set; }
    }
}
