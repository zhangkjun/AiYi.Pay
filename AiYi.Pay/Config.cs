namespace AiYi.Pay
{
    public class Config
    {
        private long id;
        private long merchantid;
        private string configvalues;
        private string configure;
        /// <summary>
        /// 进件相关json配置
        /// </summary>
        public string Configure
        {
            get
            {
                return configure;
            }

            set
            {
                configure = value;
            }
        }
        /// <summary>
        /// 支付配置参数
        /// </summary>
        public string Configvalues
        {
            get
            {
                return configvalues;
            }

            set
            {
                configvalues = value;
            }
        }
        /// <summary>
        /// 支付的编号
        /// </summary>
        public long Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }
        /// <summary>
        /// 商家编号
        /// </summary>
        public long MerchantId
        {
            get
            {
                return merchantid;
            }

            set
            {
                merchantid = value;
            }
        }
    }
}

