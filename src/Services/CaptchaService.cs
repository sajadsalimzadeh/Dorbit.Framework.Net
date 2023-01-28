using System;
using System.Collections.Generic;
using System.Linq;
using Devor.Framework.Exceptions;
using Devor.Framework.Attributes;
using Devor.Framework.Models;
using Devor.Framework.Services.Abstractions;
using Devor.Framework.Utils.Captcha;
using Microsoft.Extensions.DependencyInjection;

namespace Devor.Framework.Services
{
    [ServiceRegisterar]
    public class CaptchaService
    {
        private static Dictionary<string, string> Captchas = new();
        private readonly AppSetting appSetting;

        public CaptchaService(IServiceProvider serviceProvider)
        {
            this.appSetting = serviceProvider.GetService<AppSetting>();
        }

        public KeyValuePair<string, string> Generate(CaptchaGenerateModel dto)
        {
            if (dto.Width > 500 || dto.Height > 500) throw new OperationException(Errors.CaptchaSizeIsTooLarg);

            if (dto.Dificulty == Enums.CaptchaDificulty.None) dto.Dificulty = appSetting.Captcha.Dificulty;
            if (dto.Length == 0) dto.Length = appSetting.Captcha.Length;
            if (string.IsNullOrEmpty(dto.Pattern)) dto.Pattern = appSetting.Captcha.Pattern;

            var generator = new CaptchaGenerator
            {
                Width = dto.Width,
                Height = dto.Height,
                Dificulty = dto.Dificulty,
                Length = dto.Length,
                Pattern = dto.Pattern
            };

            var key = Guid.NewGuid().ToString();
            var value = generator.NewText();
            lock (Captchas)
            {
                if (Captchas.Count > 1000) Captchas = Captchas.Skip(500).ToDictionary(x => x.Key, x => x.Value);
                Captchas.Add(key, value);
            }
            return new KeyValuePair<string, string>(key, generator.GenerateBase64(value));
        }

        public bool Verify(string key, string value)
        {
            if (Captchas.ContainsKey(key))
            {
                var result = value.ToLower() == Captchas[key].ToLower();
                lock (Captchas) Captchas.Remove(key);
                return result;
            }
            return false;
        }

        public bool Verify(KeyValuePair<string, string> obj)
        {
            return Verify(obj.Key, obj.Value);
        }
    }
}
