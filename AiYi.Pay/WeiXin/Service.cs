using AiYi.Core;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using System.IO.Compression;

namespace AiYi.Pay.WeiXin
{
    /// <summary>
    /// 支付实现方法
    /// </summary>
    public class Service : AiYi.Pay.IPayment.IPay
    {
      
        #region 反序列话对象
        /// <summary>
        /// 反序列话对象
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        private static WechatTenpayClientOptions Getmodel(string txt)
        {
            var model = AiYi.Core.Utils.Deserialize<Settings>(txt);
            var manager = new InRedisCertificateManager();
            var options = new WechatTenpayClientOptions()
            {

                MerchantId = model.MerchantId,//商户号
                MerchantV3Secret =model.SecretV3,//商户API v3密钥
                MerchantCertificateSerialNumber = model.CertificateSerialNumber,//商户API证书序列号
                MerchantCertificatePrivateKey = model.CertificatePrivateKey,//商户API证书私钥
                PlatformCertificateManager = manager // 证书管理器的具体用法请参阅下文的高级技巧与加密、验签有关的章节
            };
            return options;// Utils.Deserialize<Config>(txt);
        }

        public override AiYi.Pay.Data AccountQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #endregion
        public override AiYi.Pay.Data BatchQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #region 下载对账单
        public override async Task<AiYi.Pay.Data> Billdownload(IHttpClientFactory clientFactory, AiYi.Pay.Data dt, AiYi.Pay.Config payment)
        {

            string bill_date = string.Format("{0:yyyyMMdd}", dt.GetValue("bill_date").ToDateTime());

            var res = new AiYi.Pay.Data();
            try
            {
                var options = Getmodel(payment.Configvalues);
                var client = new WechatTenpayClient(options);
                     var request = new GetBillTradeBillRequest()
                     {
                         BillType= "ALL",
                         TarType= "GZIP",
                         BillDateString = bill_date
                     };
                
                var response = await client.ExecuteGetBillTradeBillAsync(request);
                if (response.IsSuccessful())
                {
                    var httpClient = clientFactory.CreateClient("openpay");
                    var stream = await httpClient.GetStreamAsync(response.DownloadUrl);//获取流
                    string extractPath = Directory.GetCurrentDirectory() + "/file/weixin";//需要解压到的文件夹路径
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    var list = new List<DataBill>();
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
                                        if (dr.Length == 27)
                                        {
                                            if (counter > 0)
                                            {
                                                var model = new DataBill();
                                                model.Id = dr[6].Replace("`", "");
                                                model.Tradetime = dr[0].Replace("`", "").ToDateTime();
                                                model.Ghid = dr[1].Replace("`", "");
                                                model.Mchid = dr[2].Replace("`", "");
                                                model.Submch = dr[3].Replace("`", "");

                                                model.Deviceid = dr[4].Replace("`", "");
                                                model.Wxorder = dr[5].Replace("`", "");
                                                model.Bzorder = dr[6].Replace("`", "");
                                                model.Openid = dr[7].Replace("`", "");
                                                model.Tradetype = dr[8].Replace("`", "");
                                                model.Tradestatus = dr[9].Replace("`", "");//SUCCESS，支付成功，说明该行数据为一笔支付成功的订单
                                                                                           // REFUND，转入退款，说明该行数据为一笔发起退款成功的退款单
                                                                                           //REVOKED，已撤销，说明该行数据为一笔在用户支付成功后发起撤销的退款单
                                                model.Bank = dr[10].Replace("`", "");
                                                model.Currency = dr[11].Replace("`", "");
                                                model.Totalmoney = dr[12].Replace("`", "").ToDecimal();
                                                model.Redpacketmoney = dr[13].Replace("`", "");
                                                model.Paymentid = 2;
                                                model.Wxrefund = dr[14].Replace("`", "");
                                                model.Bzrefund = dr[15].Replace("`", "");
                                                model.Refundmoney = dr[16].Replace("`", "");
                                                model.Redpacketrefund = dr[17].Replace("`", "");
                                                model.Refundtype = dr[18].Replace("`", "");
                                                model.Refundstatus = dr[19].Replace("`", "");
                                                model.Productname = dr[20].Replace("`", "");
                                                model.Bzdatapacket = dr[21].Replace("`", "");
                                                model.Fee = dr[22].Replace("`", "");
                                                model.Rate = dr[23].Replace("`", "");
                                                list.Add(model);
                                            }
                                            counter++;
                                        }

                                    }

                                }
                            }

                        }
                    }

                  
                       res.SetValue("status", "1");
                        res.SetValue("message", list.Serialize());
                    
                }
                else
                {
                    res.SetValue("status", "0");//状态不明确
                    res.SetValue("message", response.ErrorMessage.Tostring());
                }
            }
            catch
            {
                res.SetValue("status", "0");//状态不明确
                res.SetValue("message", "系统异常");
            }
            return res;

        }

        #endregion
        #region 绑定分账账号
        /// <summary>
        /// 绑定分账账号
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Bind(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            if (model.IsSet("appid"))
            {
                var appid = model.GetValue("appid").Tostring();
                var options = Getmodel(payment.Configvalues);
                var client = new WechatTenpayClient(options);
                //client.
               
                var request = new AddProfitSharingReceiverRequest()
                {
                    AppId = appid,
                    // 获取或设置接收方类型。
                    Type = model.GetValue("type").Tostring(),
                    //获取或设置接收方账户。
                    Account = model.GetValue("account").Tostring(),
                    // 获取或设置接收方名称（需使用平台公钥/证书加密）。
                    Name = model.GetValue("name").Tostring(),
                    RelationType= model.GetValue("relationtype").Tostring(),
                };
                client.EncryptRequestSensitiveProperty(request);
               var response = await client.ExecuteAddProfitSharingReceiverAsync(request);
                if (response.IsSuccessful())
                {
                    res.SetValue("status", "1");//添加成功
                    res.SetValue("message", "ok");
                }
                else
                {
                    res.SetValue("status", "2");//状态不明确
                    res.SetValue("message", response.ErrorMessage.Tostring());
                }
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", "appid不能为空");
            }
            return res;

        }

        #endregion

        public override AiYi.Pay.Data CallbackUrl(AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #region 关闭订单
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Close(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();
            var options = Getmodel(payment.Configvalues);
            var client = new WechatTenpayClient(options);

            var request = new ClosePayTransactionRequest()
            {
                OutTradeNumber = out_trade_no,
                MerchantId = options.MerchantId
            };
            var response = await client.ExecuteClosePayTransactionAsync(request);

            if (response.IsSuccessful())
            {
                res.SetValue("status", "1");//操作成功
                res.SetValue("message", "ok");
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", response.ErrorMessage.Tostring());
            }
            return res;

        }

        #endregion
        #region 创建订单
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Create(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            try
            {
                //支付方式
                var tradeType = model.GetValue("tradetype").ToInt();
                var options = Getmodel(payment.Configvalues);
                string out_trade_no = model.GetValue("out_trade_no").Tostring();
                var appid = model.GetValue("appid").Tostring();
                var NotifyUrl = model.GetValue("notifyurl").Tostring();
                var body = model.GetValue("body").Tostring();
                var price = (int)(Convert.ToDecimal(model.GetValue("total_fee").Tostring()) * 100);//单位：分
                
                var ProfitSharing = true;
                //是否分账
                if (model.IsSet("profitsharing"))
                {
                    ProfitSharing = false;
                }
                var client = new WechatTenpayClient(options);
                //App微信支付
                if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.APP)
                {
                    var request = new CreatePayTransactionAppRequest()
                    {
                        OutTradeNumber = out_trade_no,
                        AppId = appid,
                        Settlement = new CreatePayTransactionJsapiRequest.Types.Settlement() { IsProfitSharing = ProfitSharing },
                        Description = body,
                        NotifyUrl = NotifyUrl,
                        Amount = new CreatePayTransactionJsapiRequest.Types.Amount() { Total = price }
                    };
                    var response = await client.ExecuteCreatePayTransactionAppAsync(request);
                    if (response.IsSuccessful())
                    {
                        var paramMap = client.GenerateParametersForAppPayRequest(appid, response.PrepayId);
                        var dt = new AiYi.Pay.Data();
                        dt.SetValue("appId", request.AppId);
                        dt.SetValue("timeStamp", paramMap["timeStamp"]);
                        dt.SetValue("nonceStr", paramMap["nonceStr"]);
                        dt.SetValue("package", paramMap["package"]);
                        dt.SetValue("paySign", paramMap["paySign"]);
                        dt.SetValue("signType", paramMap["signType"]);
                        res.SetValue("status", "1");//成功
                        res.SetValue("message", dt.ToJson());
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", response.ErrorMessage.Tostring());
                    }
                }
                //JsApi通用微信支付
                else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.JSAPI)
                {
                    if (model.IsSet("openid"))
                    {
                        var openId = model.GetValue("openid").Tostring();
                        var request = new CreatePayTransactionJsapiRequest()
                        {
                            OutTradeNumber = out_trade_no,
                            AppId = appid,
                            Settlement = new CreatePayTransactionJsapiRequest.Types.Settlement() { IsProfitSharing = ProfitSharing },
                            Description = body,
                            NotifyUrl = NotifyUrl,
                            Amount = new CreatePayTransactionJsapiRequest.Types.Amount() { Total = price },
                            Payer = new CreatePayTransactionJsapiRequest.Types.Payer() { OpenId = openId }
                        };
                        var response = await client.ExecuteCreatePayTransactionJsapiAsync(request);
                        if (response.IsSuccessful())
                        {
                            var paramMap = client.GenerateParametersForJsapiPayRequest(appid, response.PrepayId);

                            var dt = new AiYi.Pay.Data();
                            dt.SetValue("appId", request.AppId);
                            dt.SetValue("timeStamp", paramMap["timeStamp"]);
                            dt.SetValue("nonceStr", paramMap["nonceStr"]);
                            dt.SetValue("package", paramMap["package"]);
                            dt.SetValue("paySign", paramMap["paySign"]);
                            dt.SetValue("signType", paramMap["signType"]);
                            res.SetValue("status", "1");//成功
                            res.SetValue("message", dt.ToJson());
                        }
                        else
                        {
                            res.SetValue("status", "0");//失败
                            res.SetValue("message", response.ErrorMessage.Tostring());
                        }
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", "OpenId不能为空");
                    }
                }
                //JsApi通用微信支付（和上一样，但是为了区分，单独编写。）
                else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.JSAPI_OFFICIAL)
                {
                    if (model.IsSet("openid"))
                    {
                        var openId = model.GetValue("openid").Tostring();
                        var request = new CreatePayTransactionJsapiRequest()
                        {
                            OutTradeNumber = out_trade_no,
                            AppId = appid,
                            Settlement = new CreatePayTransactionJsapiRequest.Types.Settlement() { IsProfitSharing = ProfitSharing },
                            Description = body,
                            NotifyUrl = NotifyUrl,
                            Amount = new CreatePayTransactionJsapiRequest.Types.Amount() { Total = price },
                            Payer = new CreatePayTransactionJsapiRequest.Types.Payer() { OpenId = openId }
                        };
                        var response = await client.ExecuteCreatePayTransactionJsapiAsync(request);
                        if (response.IsSuccessful())
                        {
                            var paramMap = client.GenerateParametersForJsapiPayRequest(appid, response.PrepayId);

                            var dt = new AiYi.Pay.Data();
                            dt.SetValue("appId", request.AppId);
                            dt.SetValue("timeStamp", paramMap["timeStamp"]);
                            dt.SetValue("nonceStr", paramMap["nonceStr"]);
                            dt.SetValue("package", paramMap["package"]);
                            dt.SetValue("paySign", paramMap["paySign"]);
                            dt.SetValue("signType", paramMap["signType"]);
                            res.SetValue("status", "1");//成功
                            res.SetValue("message", dt.ToJson());
                        }
                        else
                        {
                            res.SetValue("status", "0");//失败
                            res.SetValue("message", response.ErrorMessage.Tostring());
                        }
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", "OpenId不能为空");
                    }
                }
                //扫码支付
                else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.NATIVE)
                {
                    var request = new CreatePayTransactionNativeRequest()
                    {
                        OutTradeNumber = out_trade_no,
                        AppId = appid,
                        Settlement = new CreatePayTransactionNativeRequest.Types.Settlement() { IsProfitSharing = ProfitSharing },
                        Description = body,
                        NotifyUrl = NotifyUrl,
                        Amount = new CreatePayTransactionNativeRequest.Types.Amount { Total = price }
                    };
                    var response = await client.ExecuteCreatePayTransactionNativeAsync(request);
                    if (response.IsSuccessful())
                    {
                        res.SetValue("status", "1");//成功
                        res.SetValue("message", response.QrcodeUrl);
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", response.ErrorMessage.Tostring());
                    }
                }
                //H5支付
                else if (tradeType == (int)GlobalEnumVars.WeiChatPayTradeType.MWEB)
                {
                    var request = new CreatePayTransactionH5Request()
                    {
                        OutTradeNumber = out_trade_no,
                        AppId = appid,
                        Settlement = new CreatePayTransactionH5Request.Types.Settlement { IsProfitSharing = ProfitSharing },
                        Description = body,
                        NotifyUrl = NotifyUrl,
                        Amount = new CreatePayTransactionH5Request.Types.Amount { Total = price }

                    };
                    var response = await client.ExecuteCreatePayTransactionH5Async(request);
                    if (response.IsSuccessful())
                    {
                        res.SetValue("status", "1");//成功
                        res.SetValue("message", response.H5Url);
                    }
                    else
                    {
                        res.SetValue("status", "0");//失败
                        res.SetValue("message", response.ErrorMessage.Tostring());
                    }

                }
                else
                {
                    res.SetValue("status", "0");//失败
                    res.SetValue("message", "未知的支付方式");
                }

            }
            catch (Exception ex)
            {
                res.SetValue("status", "0");//失败
                res.SetValue("message", ex.ToString());
            }
            return res;
        }

        #endregion

        public override AiYi.Pay.Data DownloadurlQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override AiYi.Pay.Data EreceiptApply(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override AiYi.Pay.Data EreceiptQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #region 完结分账
        /// <summary>
        /// 完结分账
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Finish(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_order_no = model.GetValue("out_trade_no").Tostring();
            var transaction_id = model.GetValue("transaction_id").Tostring();
            var options = Getmodel(payment.Configvalues);
            var client = new WechatTenpayClient(options);
            var request = new SetProfitSharingOrderUnfrozenRequest()
            {
                TransactionId = transaction_id,
                // 获取或设置接收方类型。
                OutOrderNumber = out_order_no
            };
            var response = await client.ExecuteSetProfitSharingOrderUnfrozenAsync(request);
           
            if (response.IsSuccessful())
            {
                res.SetValue("out_order_no", out_order_no);
                res.SetValue("transaction_id", transaction_id);
                res.SetValue("status", "1");//分账
                res.SetValue("message",AiYi.Core.Utils.Serialize(response.ReceiverList));
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", response.ErrorMessage.Tostring());
            }
            return res;

        }

        #endregion

        public override AiYi.Pay.Data Notify(AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override AiYi.Pay.Data NotifyRefunds(AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #region 订单查询
        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Query(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {

            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();

            var options = Getmodel(payment.Configvalues);
            
            var client = new WechatTenpayClient(options);

            var request = new GetPayTransactionByOutTradeNumberRequest()
            {
                OutTradeNumber = out_trade_no,
                MerchantId = options.MerchantId,//商户号
                WechatpayCertificateSerialNumber = options.MerchantCertificateSerialNumber//商户API证书序列号
            };
            var response = await client.ExecuteGetPayTransactionByOutTradeNumberAsync(request);
           
            if (response.IsSuccessful() && response.TradeState == "SUCCESS")
            {
                int payTotal = response.Amount.Total;

                if (payTotal > 0)
                {
                    res.SetValue("status", "1");//支付成功
                    res.SetValue("time_end", string.Format("{0:yyyy-MM-dd HH:mm:ss}", response.SuccessTime));//成功时间time_end
                    res.SetValue("transaction_id", response.TransactionId.Tostring());
                    res.SetValue("total_fee", response.Amount.PayerTotal.ToDecimal() / 100M);
                    res.SetValue("orderid", response.OutTradeNumber.Tostring());
                    res.SetValue("message", "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>");
                }
                else
                {
                    res.SetValue("status", "2");//状态不明确
                    res.SetValue("message", "交易金额为零");
                }
            }
            else
            {
                    res.SetValue("status", "2");//状态不明确
                   res.SetValue("message", response.ErrorMessage.Tostring());
            }
            return res;
        }

        #endregion
        /// <summary>
        /// 退款查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> QueryRefunds(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var out_trade_no = model.GetValue("out_trade_no").Tostring();

            var options = Getmodel(payment.Configvalues);

            var client = new WechatTenpayClient(options);

            var request = new GetRefundDomesticRefundByOutRefundNumberRequest()
            {
                OutRefundNumber = out_trade_no
            };
            var response = await client.ExecuteGetRefundDomesticRefundByOutRefundNumberAsync(request);

            if (response.IsSuccessful())
            {
                if (response.Status == "SUCCESS")
                {
                    res.SetValue("status", "1");//支付成功
                    res.SetValue("time_end", string.Format("{0:yyyy-MM-dd HH:mm:ss}", response.SuccessTime));//成功时间time_end
                    res.SetValue("transaction_id", response.TransactionId.Tostring());
                    res.SetValue("total_fee", response.Amount.SettlementRefund.ToDecimal() / 100M);
                    res.SetValue("orderid", response.OutTradeNumber.Tostring());
                    res.SetValue("message", "");
                }
                //处理中
                else if (response.Status == "PROCESSING")
                {
                    res.SetValue("status", "2");//处理中
                    res.SetValue("message", "");
                }
                //退款异常
                else if (response.Status == "ABNORMAL")
                {
                    res.SetValue("status", "3");//支付成功

                    res.SetValue("message", "");
                }
                /// 退款关闭
                else if (response.Status == "CLOSED")
                {
                    res.SetValue("status", "4");//支付成功

                    res.SetValue("message", "");
                }
                else
                {
                    res.SetValue("status", "5");

                    res.SetValue("message", response.ErrorMessage.Tostring());
                }

            }
            else
            {
                res.SetValue("status", "5");//状态不明确
                res.SetValue("message", response.ErrorMessage.Tostring());
            }
            return res;
        }
        #region 退款
        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Refunds(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {


            var res = new AiYi.Pay.Data();
            
            var outTradeNo = model.GetValue("out_trade_no").ToString();
            var Reason= model.GetValue("reason").Tostring();
            var totalFee = (int)(Convert.ToDecimal(model.GetValue("total_fee").Tostring()) * 100);//单位：分
            string outRefundNo = model.GetValue("out_refund_no").Tostring(); //退款单号
            int refundFee = (int)(Convert.ToDecimal(model.GetValue("refundFee").ToString()) * 100);//单位：分;//
           
            var options = Getmodel(payment.Configvalues);
            var client = new WechatTenpayClient(options);
            var request = new CreateRefundDomesticRefundRequest()
            {
                OutTradeNumber = outTradeNo,
                OutRefundNumber = outRefundNo,
                Reason= Reason,
                Amount=new CreateRefundDomesticRefundRequest.Types.Amount() { Total = totalFee, Refund= refundFee }
            };
            var response = await client.ExecuteCreateRefundDomesticRefundAsync(request);
         
            if (response.IsSuccessful())
            {
                res.SetValue("status", "1");//退款成功
                res.SetValue("message", "ok");
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", response.ErrorMessage.Tostring());
            }
            return res;

        }

        #endregion
        
        /// <summary>
        /// 分账接口 30天内调分账接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override AiYi.Pay.Data Settle( AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();

        }



        public override AiYi.Pay.Data SettleQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override AiYi.Pay.Data SettleRefund(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        public override AiYi.Pay.Data Transfer(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }
        #region 分账
        /// <summary>
        /// 分账
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="rs"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> TransferAccounts(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var OrderId = model.GetValue("OrderId").Tostring();
            var appid = model.GetValue("appid").Tostring();
            var outTradeNo = model.GetValue("out_trade_no").Tostring();
            var transaction_id = model.GetValue("transaction_id").Tostring();
            var IsUnsplitAmountUnfrozen = false;
            //是否解冻剩余金额
            if (model.IsSet("unfrozen"))
            {
                IsUnsplitAmountUnfrozen = true;
            }
            var options = Getmodel(payment.Configvalues);
           
            var client = new WechatTenpayClient(options);
            var list=new List<SKIT.FlurlHttpClient.Wechat.TenpayV3.Models.CreateProfitSharingOrderRequest.Types.Receiver> ();
            var row = AiYi.Core.Utils.Deserialize<List<Setdetails>>(model.GetValue("model").Tostring());//详情
            if (row is not null)
            {
                foreach (var dr in row)
                {
                  
                    var dt = new SKIT.FlurlHttpClient.Wechat.TenpayV3.Models.CreateProfitSharingOrderRequest.Types.Receiver();
                    dt.Account = dr.Identity;
                    dt.Amount = (int)(dr.ProfitAmount * 100);//分账金额
                    dt.Name = dr.Name;
                    dt.Description = outTradeNo + "分账";
                    list.Add(dt);
                }
                var request = new CreateProfitSharingOrderRequest()
                {
                    TransactionId = transaction_id,
                    OutOrderNumber = OrderId,
                    IsUnsplitAmountUnfrozen = IsUnsplitAmountUnfrozen,
                    ReceiverList = list
                };
                client.EncryptRequestSensitiveProperty(request);
                var response = await client.ExecuteCreateProfitSharingOrderAsync(request);

                
                if (response.IsSuccessful())
                {
                    res.SetValue("status", "1");//分账
                    res.SetValue("order_id", response.OrderId);//分账微信单号
                    res.SetValue("message", "分账成功");
                }
                else
                {
                    res.SetValue("status", "2");//状态不明确
                    res.SetValue("message", "状态不明确");
                }
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", "接收方序列换失败");
            }
            return res;


        }

        #endregion
        #region 分账查询订单查询
        /// <summary>
        /// 转账订单查询
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="rs"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> TransferAccountsQuery( AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var res = new AiYi.Pay.Data();
            var OrderId = model.GetValue("OrderId").Tostring();

            var out_trade_no = model.GetValue("out_trade_no").Tostring();
            var transaction_id = model.GetValue("transaction_id").Tostring();
            var options = Getmodel(payment.Configvalues);
          
            var client = new WechatTenpayClient(options);
            var request = new GetProfitSharingOrderByOutOrderNumberRequest()
            {
                TransactionId = transaction_id,
                OutOrderNumber = OrderId
            };
            var response = await client.ExecuteGetProfitSharingOrderByOutOrderNumberAsync(request);
            if (response.IsSuccessful())
            {
                if (response.State == "PROCESSING")
                {
                    res.SetValue("status", "0");//错误
                    res.SetValue("message", "分账处理中");
                }
                else if (response.State == "FINISHED")
                {
                    if (response.ReceiverList is not null)
                    {
                        var list = new List<ShareingQuery>();
                        foreach (var rs in response.ReceiverList)
                        {
                            var dt = new ShareingQuery();
                            dt.account = rs.Account;
                            dt.amount = rs.Amount.ToDecimal() / 100M;
                            dt.description = rs.Description;
                            dt.result = rs.Result;
                            dt.finish_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", rs.FinishTime);
                            dt.failreason = rs.FailReason.Tostring();
                            list.Add(dt);
                        }
                        res.SetValue("status", "1");//分账
                        res.SetValue("message", list.Serialize());
                    }
                    else
                    {
                        res.SetValue("status", "3");//状态不明确
                        res.SetValue("message", "未返回分账数据");
                    }

                }
                else
                {
                    res.SetValue("status", "2");//状态不明确
                    res.SetValue("message", "未知状态");
                }
            }
            return res;

        }

        #endregion

        public override AiYi.Pay.Data TransferQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 解绑分账账号
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public override async Task<AiYi.Pay.Data> Unbind(AiYi.Pay.Data model, AiYi.Pay.Config payment)
        {
            var options = Getmodel(payment.Configvalues);
           
            var res = new AiYi.Pay.Data();
            if (model.IsSet("appid"))
            {
                var appid = model.GetValue("appid").Tostring();
                var client = new WechatTenpayClient(options);
                var request = new DeleteProfitSharingReceiverRequest()
                {
                    AppId = appid,
                    // 获取或设置接收方类型。
                    Type = model.GetValue("type").Tostring(),
                    //获取或设置接收方账户。
                    Account = model.GetValue("account").Tostring()
                };
                var response = await client.ExecuteDeleteProfitSharingReceiverAsync(request);
              
                if (response.IsSuccessful())
                {
                    res.SetValue("status", "1");//添加成功
                    res.SetValue("message", "ok");
                }
                else
                {
                    res.SetValue("status", "2");//状态不明确
                    res.SetValue("message", response.ErrorMessage.Tostring());
                }
            }
            else
            {
                res.SetValue("status", "2");//状态不明确
                res.SetValue("message", "appid不能为空");
            }
            return res;
        }

    }
}