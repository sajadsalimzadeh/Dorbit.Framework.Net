using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class MeliPayamakProvider : IMessageProvider<MessageSmsRequest>
{
    public string Name => "MeliPayamak";
    private string _username;
    private string _password;
    private string _apiKey;

    private class SendResponse
    {
        public string RecId { get; set; }
        public string Status { get; set; }
    }

    public void Configure(AppSettingMessageProvider configuration)
    {
        _username = configuration.Username;
        _password = configuration.Password.GetDecryptedValue();
        _apiKey = configuration.ApiKey.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> Send(MessageSmsRequest request)
    {
        var client = new HttpHelper($"https://console.melipayamak.com/api/send/shared/{_apiKey}");
        var response = await client.PostAsync<SendResponse>("", new
        {
            bodyId = Convert.ToInt32(request.TemplateId),
            to = request.To,
            args = request.Args
        });
        return response.Result.RecId switch
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
            null => throw new Exception(response.Result.Status),
            _ => new QueryResult<string>(response.Result.RecId)
        };
    }
}