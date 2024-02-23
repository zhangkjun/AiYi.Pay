namespace AiYi.Pay
{
    public class DataBill
    {
        public string Id { get; set; } = "";


        public string Bank { get; set; } = "";


        public string Bzdatapacket { get; set; } = "";


        public string Bzorder { get; set; } = "";


        public string Bzrefund { get; set; } = "";


        public string Currency { get; set; } = "";


        public string Deviceid { get; set; } = "";


        public string Fee { get; set; } = "";


        public string Ghid { get; set; } = "";


        public string Mchid { get; set; } = "";


        public string Openid { get; set; } = "";


        public long Paymentid { get; set; } = 0;


        public string Productname { get; set; } = "";


        public string Rate { get; set; } = "";


        public string Redpacketmoney { get; set; } = "";


        public string Redpacketrefund { get; set; } = "";


        public string Refundmoney { get; set; } = "";


        public string Refundstatus { get; set; } = "";


        public string Refundtype { get; set; } = "";


        public string Submch { get; set; } = "";


        public decimal Totalmoney { get; set; } = 0;


        public string Tradestatus { get; set; } = "";


        public DateTime Tradetime { get; set; } = DateTime.Now;


        public string Tradetype { get; set; } = "";


        public string Wxorder { get; set; } = "";

        public string Wxrefund { get; set; } = "";
    }
}
