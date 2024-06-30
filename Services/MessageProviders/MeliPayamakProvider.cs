using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;
using mpNuget;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class MeliPayamakProvider : IMessageProviderSms
{
    public string Name => "MeliPayamak";
    private string _username;
    private string _password;
    private readonly HttpHelper _client;

    private class SendResponse
    {
        public string Value { get; set; }
        public int RetStatus { get; set; }
        public string StrRetStatus { get; set; }
    }

    public MeliPayamakProvider()
    {
        _client = new HttpHelper("https://rest.payamak-panel.com/api/");
    }

    public void Configure(ConfigMessageSmsProvider configuration)
    {
        _username = configuration.Username;
        _password = configuration.Password.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> SendAsync(MessageSmsRequest request)
    {
        var data = new
        {
            username = _username,
            password = _password,
            bodyId = Convert.ToInt32(request.TemplateId),
            to = request.To,
            text = string.Join(';', request.Args)
        };
        var response = (await _client.PostAsync<SendResponse>($"SendSMS/BaseServiceNumber", data)).Result;
        return response.Value switch
        {
            "-7" => throw new Exception(" خطایی در شماره فرستنده رخ داده است با پشتیبانی تماس بگیرید"),
            "-6" => throw new Exception(" خطای داخلی رخ داده است با پشتیبانی تماس بگیرید"),
            "-5" => throw new Exception(" متن ارسالی باتوجه به متغیرهای مشخص شده در متن پیشفرض همخوانی ندارد"),
            "-4" => throw new Exception(" کد متن ارسالی صحیح نمی‌باشد و یا توسط مدیر سامانه تأیید نشده است"),
            "-3" => throw new Exception(" خط ارسالی در سیستم تعریف نشده است، با پشتیبانی سامانه تماس بگیرید"),
            "-2" => throw new Exception(" محدودیت تعداد شماره، محدودیت هربار ارسال یک شماره موبایل می‌باشد"),
            "-1" => throw new Exception(" دسترسی برای استفاده از این وبسرویس غیرفعال است. با پشتیبانی تماس بگیرید"),
            "0" => throw new Exception("نام کاربری یا رمزعبور صحیح نمی‌باشد"),
            "2" => throw new Exception("اعتبار کافی نمی‌باشد"),
            "6" => throw new Exception("سامانه درحال بروزرسانی می‌باشد"),
            "7" => throw new Exception("متن حاوی کلمه فیلتر شده می‌باشد، با واحد اداری تماس بگیرید"),
            "10" => throw new Exception("کاربر موردنظر فعال نمی‌باشد"),
            "11" => throw new Exception("ارسال نشده"),
            "12" => throw new Exception("مدارک کاربر کامل نمی‌باشد"),
            _ => new QueryResult<string>(response.Value) { Success = true }
        };
    }

    public async Task<long> GetCreditMessageCountAsync()
    {
        var response = await _client.PostAsync<SendResponse>($"SendSMS/GetCredit", new
        {
            username = _username,
            password = _password,
        }).ToResultAsync();
        if (double.TryParse(response.Value, out var messageCount)) return (long)messageCount;
        return -1;
    }
}