using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class TranslationService(TranslationRepository translationRepository, OpenAiService openAiService)
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();

    public async Task LoadAllLocaleAsync()
    {
        var localeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Locales");
        if (Directory.Exists(localeDir))
        {
            foreach (var localeFile in Directory.GetFiles(localeDir))
            {
                var locale = Path.GetFileNameWithoutExtension(localeFile);
                var content = await File.ReadAllTextAsync(localeFile);
                _translations[locale] = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }
        }
    }

    public string TranslateLocale(string str, string locale)
    {
        if (locale is not null && _translations.TryGetValue(locale, out var dict))
        {
            if (dict.TryGetValue(str, out var result)) return result;
        }

        return str;
    }

    public Task<Translation> GetTranslationAsync(string key, string locale)
    {
        return translationRepository.Set().FirstOrDefaultAsync(x => x.Key == key && x.Locale == locale);
    }

    public Task<string> TranslateByOpenAiAsync(string value, string language)
    {
        return openAiService.ChatAsync($"translate text below to '{language}':\n\n{value}");
    }

    public Task<Translation> AddTranslationAsync(string key, string locale, string value)
    {
        return translationRepository.InsertAsync(new Translation()
        {
            Key = key,
            Locale = locale,
            Value = value
        });
    }
}