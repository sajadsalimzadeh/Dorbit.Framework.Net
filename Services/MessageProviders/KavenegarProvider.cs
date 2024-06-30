using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Kavenegar;
using Kavenegar.Models;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class KavenegarProvider : IMessageProviderSms
{
    public string Name => "Kavenegar";
    private string _apKey;
    private string _sender;
    private KavenegarApi _api;

    private class SendResponse
    {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Sender { get; set; }
        public long Date { get; set; }
        public int Cost { get; set; }
    }

    public void Configure(ConfigMessageSmsProvider configuration)
    {
        _sender = configuration.Sender;
        _apKey = configuration.ApiKey.GetDecryptedValue();
        _api = new Kavenegar.KavenegarApi(_apKey);
    }

    public Task<QueryResult<string>> SendAsync(MessageSmsRequest request)
    {
        SendResult result;
        if (string.IsNullOrEmpty(request.TemplateId))
        {
            result = _api.Send(_sender, request.To, request.Text);
        }
        else
        {
            result = _api.VerifyLookup(request.To, request.Args[0], request.TemplateId);
        }

        return Task.FromResult(result.Status switch
        {
            6 => throw new Exception("خطا در ارسال پیام که توسط سر شماره پیش می آید و به معنی عدم رسیدن پیامک می باشد"),
            11 => throw new Exception("نرسیده به گیرنده ، این وضعیت به دلایلی از جمله خاموش یا خارج از دسترس بودن گیرنده اتفاق می افتد "),
            13 => throw new Exception("ارسال پیام از سمت کاربر لغو شده یا در ارسال آن مشکلی پیش آمده که هزینه آن به حساب برگشت داده می شود"),
            14 => throw new Exception("بلاک شده است، عدم تمایل گیرنده به دریافت پیامک از خطوط تبلیغاتی که هزینه آن به حساب برگشت داده می شود"),
            100 => throw new Exception(
                "شناسه پیامک نامعتبر است ( به این معنی که شناسه پیام در پایگاه داده کاوه نگار ثبت نشده است یا متعلق به شما نمی باشد)"),
            _ => new QueryResult<string>(result.Messageid.ToString())
        });
    }

    public Task<long> GetCreditMessageCountAsync()
    {
        var accountInfo = _api.AccountInfo();
        return Task.FromResult(accountInfo.RemainCredit);
    }
}