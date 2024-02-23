
using AiYi.Core;
using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using Newtonsoft.Json;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static SKIT.FlurlHttpClient.Wechat.TenpayV3.Models.CreateApplyForSubMerchantApplymentRequest.Types.Business.Types.SaleScene.Types;

namespace AiYi.Pay.AliPay
{
    public class Service : AiYi.Pay.IPayment.IPay
    {
        private static (AlipayConfig config,string PId ) Getmodel(string txt)
        {
            var model = AiYi.Core.Utils.Deserialize<Settings>(txt);
            AlipayConfig alipayConfig = new AlipayConfig();
            alipayConfig.ServerUrl = "https://openapi.alipay.com/gateway.do";
            alipayConfig.AppId = model.app_id;
            alipayConfig.PrivateKey = model.merchant_private_key;
            alipayConfig.Charset = "utf-8";
            alipayConfig.SignType = "RSA2";
            alipayConfig.Format = "json";
            alipayConfig.AlipayPublicCertPath = model.AlipayPublicCertPath;
            alipayConfig.AppCertPath = model.AppCertPath;
            alipayConfig.RootCertPath = model.RootCertPath;
            alipayConfig.EncryptKey = model.EncryptKey;
            return (alipayConfig, model.PID);
        }
        /// <summary>
        /// 查询账户
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override Data AccountQuery(Data model, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            try
            {
                var(alipayConfig,  PId) = Getmodel(payment.Configvalues);
                IAopClient client = new DefaultAopClient(alipayConfig);
                AlipayFundAccountQueryRequest request = new AlipayFundAccountQueryRequest();
                AlipayFundAccountQueryModel requestmodel = new AlipayFundAccountQueryModel();
                requestmodel.AlipayUserId = PId;
                requestmodel.AccountType = "ACCTRANS_ACCOUNT";
                request.SetBizModel(requestmodel);
                AlipayFundAccountQueryResponse response = client.CertificateExecute(request);
                if (!response.IsError)
                {
                    res.SetValue("amount", response.AvailableAmount);//可用金额
                    res.SetValue("freezeamount", response.FreezeAmount);
                    res.SetValue("message", "ok");
                    res.SetValue("status", "1");
                }
                else
                {
                    res.SetValue("amount", "0");//错误
                    res.SetValue("message", response.Msg);
                    res.SetValue("status", "0");//失败
                }

            }
            catch
            {
                res.SetValue("amount", "0");//错误
                res.SetValue("message", "系统异常");
                res.SetValue("status", "0");//失败
            }
            return res;
        }

        public override Data BatchQuery(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override async Task<Data> Billdownload(IHttpClientFactory clientFactory, Data drr, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            try
            {
                string bill_date = string.Format("{0:yyyy-MM-dd}", drr.GetValue("bill_date").ToDateTime());
                var (alipayConfig, PId) = Getmodel(payment.Configvalues);
                IAopClient client = new DefaultAopClient(alipayConfig);
                AlipayDataDataserviceBillDownloadurlQueryRequest request = new AlipayDataDataserviceBillDownloadurlQueryRequest();
                request.BizContent = "{" +
                "  \"bill_type\":\"trade\"," +
                "  \"bill_date\":\"" + bill_date + "\"" +
                "}";
                AlipayDataDataserviceBillDownloadurlQueryResponse response = client.CertificateExecute(request);
                if (response.Code == "10000")
                {
                    var list = new List<DataBill>();
                    res.SetValue("status", "1");//正确
                    var httpClient = clientFactory.CreateClient("openpay");
                    var stream =await httpClient.GetStreamAsync(response.BillDownloadUrl);//获取流
                    string extractPath = Directory.GetCurrentDirectory() + "/file/alipay";//需要解压到的文件夹路径
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        extractPath += Path.DirectorySeparatorChar;
                    //获取zip内容。GB2312需要在 Program Main方法中注册: System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, true, System.Text.Encoding.GetEncoding("GB2312")))
                    {
                        foreach (var item in archive.Entries)
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(extractPath, item.FullName));
                            item.ExtractToFile(destinationPath, true);//解压到指定文件夹，true代表覆盖原有的文件

                            if (item.FullName.IndexOf("业务明细.csv") > 10)
                            {
                                using (System.IO.StreamReader file = new System.IO.StreamReader(item.Open(), System.Text.Encoding.GetEncoding("GB2312")))
                                {
                                    //这里直接读取zip文件中的流item.Open()，亦可以读取文件。具体用法可参考CsvHelper官方文档
                                    int counter = 0;
                                    string line;
                                   
                                        while ((line = file.ReadLine()) != null)
                                        {
                                            Console.WriteLine(line);
                                            var dr = line.Split(',');
                                            if (dr.Length == 25)
                                            {
                                                if (counter > 0)
                                                {
                                                    var model = new DataBill();
                                                    model.Id = dr[1].Replace("\t", "");
                                                    model.Tradetime = (dr[5].Replace("\t", "").ToDateTime());
                                                    model.Ghid = "";
                                                    model.Mchid = "";
                                                    model.Submch = "";
                                                    model.Deviceid = "";
                                                    model.Wxorder = dr[0].Replace("\t", "");
                                                    model.Bzorder = dr[1].Replace("\t", "");
                                                    model.Openid = dr[10].Replace("\t", "");
                                                    model.Tradetype = dr[2].Replace("\t", "");
                                                    model.Tradestatus = "SUCCESS";//SUCCESS，支付成功，说明该行数据为一笔支付成功的订单
                                                                                  // REFUND，转入退款，说明该行数据为一笔发起退款成功的退款单
                                                                                  //REVOKED，已撤销，说明该行数据为一笔在用户支付成功后发起撤销的退款单
                                                    model.Bank = "";
                                                    model.Currency = "CNY";
                                                    model.Paymentid = 1;
                                                    model.Totalmoney = dr[11].Replace("\t", "").ToDecimal();
                                                    model.Redpacketmoney = dr[13].Replace("\t", "");
                                                    model.Wxrefund = dr[14].Replace("\t", "");
                                                    model.Bzrefund = dr[15].Replace("\t", "");
                                                    model.Refundmoney = "";
                                                    model.Redpacketrefund = "";
                                                    model.Refundtype = "";
                                                    model.Refundstatus = "";
                                                    model.Productname = dr[3].Replace("\t", "");
                                                    model.Bzdatapacket = "";
                                                    model.Fee = dr[22].Replace("\t", "").Replace("-", "");
                                                    model.Rate = "";
                                                    list.Add(model);
                                                }
                                                counter++;
                                            }

                                        }
                                   
                                }
                            }

                        }
                    }

                    res.SetValue("message", list.Serialize());
                }
                else
                {
                    res.SetValue("status", "0");//状态不明确
                    res.SetValue("message", response.SubMsg);
                }
            }
            catch
            {
                res.SetValue("status", "0");//状态不明确
                res.SetValue("message", "系统异常");
            }
            return res;
        }

        public override Task<Data> Bind(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data CallbackUrl(Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Task<Data> Close(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override async Task<Data> Create(Data model, Pay.Config payment)
        {
            string sp_billno = model.GetValue("out_trade_no").Tostring();
            var total_fee = model.GetValue("total_fee").ToDecimal();
            var body = model.GetValue("body").ToString();
            var tradeType = model.GetValue("tradetype").ToInt();
            var res = new AiYi.Pay.Data();
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);
            IAopClient client = new DefaultAopClient(alipayConfig);
            //App微信支付
            if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.APP)
            {
               
            }
            //JsApi通用支付
            else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.JSAPI)
            {
                if (model.IsSet("openid"))
                {
                    // 构造请求参数以调用接口
                    AlipayTradeCreateRequest request = new AlipayTradeCreateRequest();
                    AlipayTradeCreateModel requestmodel = new AlipayTradeCreateModel();

                    // 设置商户订单号
                    requestmodel.OutTradeNo = sp_billno;

                    // 设置产品码
                    requestmodel.ProductCode = "JSAPI_PAY";

                    // 设置小程序支付中
                    requestmodel.OpAppId = model.GetValue("openid").Tostring();

                    // 设置订单总金额
                    requestmodel.TotalAmount = total_fee.ToString();

                    // 设置业务扩展参数
                    //ExtendParams extendParams = new ExtendParams();
                    //extendParams.TradeComponentOrderId = "2023060801502300000008810000005657";
                    // requestmodel.ExtendParams = extendParams;

                    // 设置可打折金额
                    // requestmodel.DiscountableAmount = "80.00";

                    // 设置订单标题
                    // requestmodel.Subject = "Iphone6 16G";

                    // 设置订单附加信息
                    requestmodel.Body = body;

                    // 设置买家支付宝用户ID
                    // requestmodel.BuyerId = "2088102146225135";

                    // 设置商户门店编号
                    // requestmodel.StoreId = "NJ_001";

                    request.SetBizModel(requestmodel);
                    AlipayTradeCreateResponse response = client.CertificateExecute(request);

                    if (!response.IsError)
                    {
                        res.SetValue("status", "1");//成功
                        //trade_no
                        res.SetValue("message", response.TradeNo);
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", response.Msg);
                    }
                }
                else
                {
                    res.SetValue("status", "0");//失败
                    res.SetValue("message", "openid不能为空");
                }
            }
            //JsApi通用微信支付（和上一样，但是为了区分，单独编写。）
            else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.JSAPI_OFFICIAL)
            {
               
            }
            //扫码支付
            else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.NATIVE)
            {
                
            }
            //H5支付
            else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.MWEB)
            {

                AlipayTradeWapPayRequest request = new AlipayTradeWapPayRequest();
                request.SetNotifyUrl(model.GetValue("Notify").ToString());
                request.SetReturnUrl(model.GetValue("ReturnUrl").ToString());
                request.BizContent = "{" +
                "\"body\":\"" + HttpUtility.UrlEncode(body, Encoding.UTF8) + "\"," +
                "\"subject\":\"" + HttpUtility.UrlEncode(body, Encoding.UTF8) + "\"," +
                "\"out_trade_no\":\"" + sp_billno + "\"," +
                  "\"product_code\":\"QUICK_WAP_PAY\"," +
                     "\"quit_url\":\"" + model.GetValue("ReturnUrl").ToString() + "\"," +
                "\"total_amount\":" + string.Format("{0:f2}", total_fee) + "" +
                "}";
                AlipayTradeWapPayResponse response = client.pageExecute(request);
                res.SetValue("status", "1");//成功
                res.SetValue("message", response.Body);
            }
            else
            {
                res.SetValue("status", "0");//失败
                res.SetValue("message", "未知的支付方式");
            }

            return await Task.FromResult(res);
        }

        public override Data DownloadurlQuery(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data EreceiptApply(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data EreceiptQuery(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Task<Data> Finish(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data Notify(Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data NotifyRefunds(Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<Data> Query(Data model, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);
            IAopClient client = new DefaultAopClient(alipayConfig);

            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.BizContent = "{" +
            "\"out_trade_no\":\"" + out_trade_no + "\"" +
            "  }";
            AlipayTradeQueryResponse response = client.CertificateExecute(request);
            if (response.Code == "10000")
            {
                if (response.TradeStatus == "TRADE_SUCCESS")
                {
                    res.SetValue("status", "1");//支付成功s
                    res.SetValue("time_end", response.SendPayDate);//成功时间end_pay_date
                    res.SetValue("transaction_id", response.TradeNo);//dr["trade_no"].ToString()
                    res.SetValue("total_fee", response.TotalAmount);//dr["total_amount"].ToString()
                    res.SetValue("orderid", out_trade_no);
                    res.SetValue("message", "success");
                }
                else
                {
                    res.SetValue("status", "0");//错误
                    res.SetValue("message", response.Msg);
                }

            }
            else
            {
                res.SetValue("status", "0");//状态不明确
                res.SetValue("message", response.SubMsg);
            }
            return await Task.FromResult(res);
           
        }
        /// <summary>
        /// 退款查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<Data> QueryRefunds(Data model, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);
            IAopClient client = new DefaultAopClient(alipayConfig);
            AlipayTradeFastpayRefundQueryRequest request = new AlipayTradeFastpayRefundQueryRequest();
            Dictionary<string, object> bizContent = new Dictionary<string, object>();
            //bizContent.Add("trade_no", "2021081722001419121412730660");
            bizContent.Add("out_request_no", out_trade_no);

            //// 返回参数选项，按需传入
            List<string> queryOptions = new List<string>();
            queryOptions.Add("refund_detail_item_list");
            bizContent.Add("query_options", queryOptions);

            string Contentjson = JsonConvert.SerializeObject(bizContent);
            request.BizContent = Contentjson;
            AlipayTradeFastpayRefundQueryResponse response = client.CertificateExecute(request);
            Console.WriteLine(response.Body);
            if (response.Code == "10000")
            {
                if (response.RefundStatus == "REFUND_SUCCESS")
                {
                    res.SetValue("status", "1");//支付成功
                    res.SetValue("time_end", response.GmtRefundPay);//成功时间time_end
                    res.SetValue("transaction_id", response.TradeNo.Tostring());
                    res.SetValue("total_fee", response.RefundAmount);
                    res.SetValue("orderid", response.OutRequestNo.Tostring());
                    res.SetValue("message", "");
                }
                ////处理中
                //else if (response.Status == "PROCESSING")
                //{
                //    res.SetValue("status", "2");//处理中
                //    res.SetValue("message", "");
                //}
                ////退款异常
                //else if (response.Status == "ABNORMAL")
                //{
                //    res.SetValue("status", "3");//支付成功

                //    res.SetValue("message", "");
                //}
                /// 退款关闭
                else if (response.RefundStatus == "REFUND_CLOSED")
                {
                    res.SetValue("status", "4");//支付成功

                    res.SetValue("message", "");
                }
                else
                {
                    res.SetValue("status", "5");

                    res.SetValue("message", response.Msg.Tostring());
                }

            }
            else
            {
                res.SetValue("status", "5");//状态不明确
                res.SetValue("message", response.SubMsg);
            }
            return await Task.FromResult(res);
        }
        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<Data> Refunds(Data model, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();

            var outTradeNo = model.GetValue("out_trade_no").Tostring();
            var totalFee = model.GetValue("refundFee").ToDecimal();
            var trade_no = model.GetValue("trade_no");
            string outRefundNo = model.GetValue("out_refund_no").Tostring(); //退款单号
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);
          
            IAopClient client = new DefaultAopClient(alipayConfig);
            //

            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
            Dictionary<string, object> bizContent = new Dictionary<string, object>();
            bizContent.Add("out_trade_no", outTradeNo);
            bizContent.Add("trade_no", trade_no);
            bizContent.Add("refund_amount", string.Format("{0:f2}", totalFee));
            bizContent.Add("out_request_no", outRefundNo);

            string Contentjson = JsonConvert.SerializeObject(bizContent);
            request.BizContent = Contentjson;
            AlipayTradeRefundResponse result = client.CertificateExecute(request);
            if (result.Code == "10000")
            {

                res.SetValue("status", "1");//退款成功
                res.SetValue("message", "ok");
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", result.SubMsg);
            }
            return await Task.FromResult(res);
        }

        public override Data Settle(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data SettleQuery(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data SettleRefund(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Data Transfer(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override async Task<Data> TransferAccounts(Data rs, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var outTradeNo = rs.GetValue("out_trade_no").ToString();
            var remarks = rs.GetValue("remarks").ToString();
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);

            IAopClient alipayClient = new DefaultAopClient(alipayConfig);
            AlipayFundTransUniTransferRequest request = new AlipayFundTransUniTransferRequest();
            AlipayFundTransUniTransferModel model = new AlipayFundTransUniTransferModel();
            var details = AiYi.Core.Utils.Deserialize<List<Setdetails>>(rs.GetValue("model").Tostring()).FirstOrDefault();//详情
            if (details is not null)
            {
                model.OutBizNo = outTradeNo;//订单号
                model.TransAmount = String.Format("{0:f2}", details.ProfitAmount);//订单总金额
                model.ProductCode = "TRANS_ACCOUNT_NO_PWD";
                model.BizScene = "DIRECT_TRANSFER";
                model.OrderTitle = remarks;
                model.BusinessParams = "{\"payer_show_name_use_alias\":\""+ remarks + "\"}";//转账业务请求的扩展参数，支持传入的扩展参数如下： payer_show_name_use_alias：使用别名支持付款方展示，可选，收款方在支付宝账单中可见。
                Participant payeeInfo = new Participant();
                payeeInfo.Identity = details.Identity;//标识
                payeeInfo.IdentityType = details.IdentityType;
                payeeInfo.Name = details.Name;
                model.PayeeInfo = payeeInfo;
                request.SetBizModel(model);
                AlipayFundTransUniTransferResponse response = alipayClient.CertificateExecute(request);
                if (!response.IsError)
                {
                    if (response.Code == "10000")
                    {
                        if (response.Status.ToUpper() == "SUCCESS")
                        {
                            res.SetValue("status", "1");//分账
                            res.SetValue("order_id", response.OrderId);//分账单号
                            res.SetValue("message", "ok");
                        }
                        else
                        {
                            res.SetValue("status", "0");//状态不明确
                            res.SetValue("message", response.SubMsg);
                        }
                    }
                    else
                    {
                        res.SetValue("status", "0");//状态不明确
                        res.SetValue("message", response.SubMsg);
                    }
                }
                else
                {
                    res.SetValue("status", "0");//状态不明确
                    res.SetValue("message", "调用失败");
                }
            }
            else
            {
                res.SetValue("status", "0");//状态不明确
                res.SetValue("message", "接收方序列换失败");
            }

            return await Task.FromResult(res);
        }

        public override async Task<Data> TransferAccountsQuery(Data model, Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();
            var (alipayConfig, PId) = Getmodel(payment.Configvalues);
            IAopClient alipayClient = new DefaultAopClient(alipayConfig);
            AlipayFundTransCommonQueryRequest request = new AlipayFundTransCommonQueryRequest();
            AlipayFundTransCommonQueryModel querymodel = new AlipayFundTransCommonQueryModel();
            querymodel.OutBizNo = out_trade_no;//订单号
            querymodel.BizScene = "DIRECT_TRANSFER";
            querymodel.ProductCode = "TRANS_ACCOUNT_NO_PWD";
            request.SetBizModel(querymodel);
            AlipayFundTransCommonQueryResponse response = alipayClient.CertificateExecute(request);
            if (response.Code == "10000")
            {
                if (response.Status.ToUpper() == "SUCCESS")
                {
                    res.SetValue("status", "1");//支付成功s
                    res.SetValue("time_end", response.PayDate);//成功时间end_pay_date
                    res.SetValue("transaction_id", response.OrderId);//dr["trade_no"].ToString()
                    res.SetValue("orderid", out_trade_no);
                    res.SetValue("message", "ok");
                }
                else
                {
                    res.SetValue("status", "0");//错误
                    res.SetValue("message", response.Msg);
                }

            }
            else
            {
                res.SetValue("status", "0");//状态不明确
                res.SetValue("message", response.SubMsg);
            }
            return await Task.FromResult(res);
        }

        public override Data TransferQuery(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override Task<Data> Unbind(Data model, Pay.Config payment)
        {
            throw new NotImplementedException();
        }
    }
}
