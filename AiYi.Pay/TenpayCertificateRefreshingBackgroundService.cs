using AiYi.Pay.WeiXin;
using Microsoft.Extensions.Hosting;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;

namespace AiYi.Pay
{
    /// <summary>
    /// 更新证书
    /// </summary>
    public class TenpayCertificateRefreshingBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //foreach (var tenpayMerchantOptions in _tenpayOptions.Merchants)
                //{
                try
                {
                    const string ALGORITHM_TYPE = "RSA";
                    var model = AiYi.Core.Utils.Deserialize<Settings>("");
                    if (model is not null)
                    {
                        var manager = new InRedisCertificateManager();
                        var options = new WechatTenpayClientOptions()
                        {

                            MerchantId = model.MerchantId,//商户号
                            MerchantV3Secret = model.SecretV3,//商户API v3密钥
                            MerchantCertificateSerialNumber = model.CertificateSerialNumber,//商户API证书序列号
                            MerchantCertificatePrivateKey = model.CertificatePrivateKey,//商户API证书私钥
                            PlatformCertificateManager = manager, // 证书管理器的具体用法请参阅下文的高级技巧与加密、验签有关的章节
                            AutoEncryptRequestSensitiveProperty = false,
                            AutoDecryptResponseSensitiveProperty = false
                        };
                        var client = new WechatTenpayClient(options);

                        var request = new QueryCertificatesRequest() { AlgorithmType = ALGORITHM_TYPE };
                        var response = await client.ExecuteQueryCertificatesAsync(request, cancellationToken: stoppingToken);
                        if (response.IsSuccessful())
                        {
                            // NOTICE:
                            //   如果构造 Client 时启用了 `AutoDecryptResponseSensitiveProperty` 配置项，则无需再执行下面一行的手动解密方法：
                            response = client.DecryptResponseSensitiveProperty(response);

                            foreach (var certificate in response.CertificateList)
                            {
                                client.PlatformCertificateManager.AddEntry(CertificateEntry.Parse(ALGORITHM_TYPE, certificate));
                            }
                            // _logger.LogInformation("刷新微信商户平台证书成功。");
                        }
                        else
                        {
                            // _logger.LogWarning(
                            //"刷新微信商户平台证书失败（状态码：{0}，错误代码：{1}，错误描述：{2}）。",
                            // response.RawStatus, response.ErrorCode, response.ErrorMessage
                            // );
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "刷新微信商户平台证书遇到异常。");
                }
                //}
                await Task.Delay(TimeSpan.FromDays(1)); // 每隔 1 天轮询刷新
            }
        }
    }
}
