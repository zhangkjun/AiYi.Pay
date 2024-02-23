using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay
{
    public class GlobalEnumVars
    {
        /// <summary>
        /// 微信支付交易类型
        /// </summary>
        public enum WeiChatPayTradeType
        {
            [Description("小程序支付")]
            JSAPI = 1,
            [Description("公众号支付")]
            JSAPI_OFFICIAL = 2,
            [Description("扫码支付")]
            NATIVE = 3,
            [Description("APP支付")]
            APP = 4,
            [Description("H5支付")]
            MWEB = 5
        }
    }
}
