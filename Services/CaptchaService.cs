using System;
using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Utils.Captcha;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class CaptchaService
{
    private static Dictionary<string, string> _captchas = new();
    private readonly AppSetting _appSetting;

    public CaptchaService(IServiceProvider serviceProvider)
    {
        _appSetting = serviceProvider.GetService<AppSetting>();
    }

    public KeyValuePair<string, string> Generate(CaptchaGenerateModel dto)
    {
        if (dto.Width > 500 || dto.Height > 500) throw new OperationException(Errors.CaptchaSizeIsTooLarg);

        if (dto.Dificulty == CaptchaDificulty.None) dto.Dificulty = _appSetting.Captcha.Difficulty;
        if (dto.Length == 0) dto.Length = _appSetting.Captcha.Length;
        if (string.IsNullOrEmpty(dto.Pattern)) dto.Pattern = _appSetting.Captcha.Pattern;

        var generator = new CaptchaGenerator
        {
            Width = dto.Width,
            Height = dto.Height,
            Difficulty = dto.Dificulty,
            Length = dto.Length,
            Pattern = dto.Pattern
        };

        var key = Guid.NewGuid().ToString();
        var value = generator.NewText();
        lock (_captchas)
        {
            if (_captchas.Count > 1000) _captchas = _captchas.Skip(500).ToDictionary(x => x.Key, x => x.Value);
            _captchas.Add(key, value);
        }
        return new KeyValuePair<string, string>(key, generator.GenerateBase64(value));
    }

    public bool Verify(string key, string value)
    {
        if (_captchas.ContainsKey(key))
        {
            var result = value.ToLower() == _captchas[key].ToLower();
            lock (_captchas) _captchas.Remove(key);
            return result;
        }
        return false;
    }

    public bool Verify(KeyValuePair<string, string> obj)
    {
        return Verify(obj.Key, obj.Value);
    }
}