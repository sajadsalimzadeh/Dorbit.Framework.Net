using System;
using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Captcha;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class CaptchaService(IOptions<ConfigCaptcha> configCaptchaOptions)
{
    private static Dictionary<string, string> _captchas = new();
    private readonly ConfigCaptcha _configCaptcha = configCaptchaOptions.Value;

    public KeyValuePair<string, string> Generate(CaptchaGenerateModel dto)
    {
        if (dto.Width > 500 || dto.Height > 500) throw new OperationException(Errors.CaptchaSizeIsTooLarge);

        if (dto.Dificulty == CaptchaDificulty.None) dto.Dificulty = _configCaptcha.Difficulty;
        if (dto.Length == 0) dto.Length = _configCaptcha.Length;
        if (string.IsNullOrEmpty(dto.Pattern)) dto.Pattern = _configCaptcha.Pattern;

        var generator = new CaptchaGenerator
        {
            Width = dto.Width,
            Height = dto.Height,
            Difficulty = dto.Dificulty,
        };

        var key = Guid.CreateVersion7().ToString();
        var value = dto.Pattern.Random(dto.Length);
        lock (_captchas)
        {
            if (_captchas.Count > 1000) _captchas = _captchas.Skip(500).ToDictionary(x => x.Key, x => x.Value);
            _captchas.Add(key, value);
        }

        return new KeyValuePair<string, string>(key, generator.GenerateBase64(value));
    }

    public bool Validate(string key, string value)
    {
        lock (_captchas)
        {
            if (!_captchas.TryGetValue(key, out var captcha)) return false;
            var result = value.ToLower() == captcha.ToLower();
            lock (_captchas) _captchas.Remove(key);
            return result;
        }
    }

    public bool Validate(KeyValuePair<string, string> obj)
    {
        return Validate(obj.Key, obj.Value);
    }

    public bool Validate(HttpContext context)
    {
        var captchaKey = string.Empty;
        var captchaValue = string.Empty;
        var recaptcha = string.Empty;
        if (context.Request.Form.ContainsKey("CaptchaKey")) captchaKey = context.Request.Form["CaptchaKey"];
        if (context.Request.Form.ContainsKey("CaptchaValue")) captchaValue = context.Request.Form["CaptchaValue"];
        if (context.Request.Headers.ContainsKey("Captcha"))
        {
            var captchaHeader = context.Request.Headers["Captcha"];
            if (captchaHeader.Count == 1)
            {
                recaptcha = captchaHeader[0];
            }
            else if (captchaHeader.Count == 2)
            {
                captchaKey = captchaHeader[0];
                captchaValue = captchaHeader[1];
            }
        }

        if (recaptcha.IsNotNullOrEmpty())
        {
            var captchaValidator = context.RequestServices.GetRequiredService<ICaptchaValidator>();
            if (!captchaValidator.IsCaptchaPassedAsync(recaptcha).Result)
            {
                throw new OperationException(Errors.CaptchaNotCorrect);
            }
        }
        else if (captchaKey.IsNotNullOrEmpty() && captchaValue.IsNotNullOrEmpty())
        {
            if (!Validate(captchaKey, captchaValue))
            {
                throw new OperationException(Errors.CaptchaNotCorrect);
            }
        }
        else
        {
            throw new OperationException(Errors.CaptchaNotSet);
        }

        return true;
    }
}