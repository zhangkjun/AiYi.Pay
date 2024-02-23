namespace AiYi.Pay
{
    /// <summary>
    /// 支付方式编辑显示的model
    /// </summary>
    public class PayInputModel
    {
        private string _Id;
        private string _Name;
        private string _Content;

        /// <summary>
        /// 显示的名称
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }
        public string Id
        {
            get
            {
                return _Id;
            }

            set
            {
                _Id = value;
            }
        }
        public string Content
        {
            get
            {
                return _Content;
            }

            set
            {
                _Content = value;
            }
        }
    }
}
