using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AiYi.Pay
{
    public static class PayServiceCollectionExtensions
    {

        /// <summary>
        /// 支付注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">    private readonly Func<string, Chinahoo.IPayment.IService> serviceAccessor;</exception>
        public static IServiceCollection AddPaySetup(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.TryAddSingleton<AiYi.Pay.AliPay.Service>();
            services.TryAddSingleton<AiYi.Pay.WeiXin.Service>();
            services.TryAddSingleton(provider =>
            {
                Func<string, AiYi.Pay.IPayment.IPay?> accesor = key =>
                {
                    switch (key)
                    {
                        case "AiYi.Pay.AliPay":
                            return provider.GetService<AiYi.Pay.AliPay.Service>();
                        case "AiYi.Pay.WeiXin":
                            return provider.GetService<AiYi.Pay.WeiXin.Service>();
                        default:
                            return provider.GetService<AiYi.Pay.WeiXin.Service>();
                    }
                };
                return accesor;
            });
            // 注入后台任务
            services.AddHostedService<AiYi.Pay.TenpayCertificateRefreshingBackgroundService>();
            return services;
        }
    }
}
