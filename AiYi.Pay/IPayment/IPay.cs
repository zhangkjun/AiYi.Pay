using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay.IPayment
{
    /// <summary>
    /// 支付接口定义
    /// </summary>
    public abstract class IPay
    {
        /// <summary>
        /// 分账查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> TransferAccountsQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 分账接口
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> TransferAccounts( AiYi.Pay.Data rs, AiYi.Pay.Config payment);
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Create(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Query(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Close(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Refunds(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 查询单笔退款
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> QueryRefunds(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 退款结果通知
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data NotifyRefunds(AiYi.Pay.Config payment);
        /// <summary>
        /// 下载对账单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Billdownload(IHttpClientFactory clientFactory, AiYi.Pay.Data model, AiYi.Pay.Config payment);

        /// <summary>
        /// 支付结果通知
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data Notify(AiYi.Pay.Config payment);
        /// <summary>
        /// 前端支付返回结果
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data CallbackUrl(AiYi.Pay.Config payment);
        /// <summary>
        /// 分账管理绑定
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Bind(AiYi.Pay.Data model, AiYi.Pay.Config payment);

        public abstract Task<AiYi.Pay.Data> Finish(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 分账关系解绑
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract Task<AiYi.Pay.Data> Unbind(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 分账关系查询
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data BatchQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 统一收单交易结算接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data Settle(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 	统一收单线下交易查询接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data SettleQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 统一收单交易退款接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data SettleRefund(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 单笔付款到银行卡
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data Transfer(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 转账业务单据查询接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data TransferQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 支付宝资金账户资产查询接口
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data AccountQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 申请电子回单(incubating)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data EreceiptApply(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 查询电子回单状态(incubating)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data EreceiptQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);
        /// <summary>
        /// 查询对账单下载地址
        /// </summary>
        /// <param name="model"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public abstract AiYi.Pay.Data DownloadurlQuery(AiYi.Pay.Data model, AiYi.Pay.Config payment);





    }

}