using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Utils.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class TranslationService(TranslationRepository translationRepository, IOptions<ConfigTranslation> configTranslationOptions, OpenAiService openAiService)
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new();

    public async Task LoadAllLocaleAsync()
    {
        var localeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Locales");
        if (Directory.Exists(localeDir))
        {
            foreach (var localeFile in Directory.GetFiles(localeDir))
            {
                var locale = Path.GetFileNameWithoutExtension(localeFile);
                var content = await File.ReadAllTextAsync(localeFile);
                Translations[locale] = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }
        }
    }

    public string TranslateLocale(string str, string locale, Dictionary<string, string> args = null)
    {
        locale ??= "en";
        if (Translations.TryGetValue(locale, out var dict))
        {
            if (dict.TryGetValue(str, out var result))
            {
                if (args is not null)
                {
                    foreach (var keyValuePair in args)
                    {
                        result = result.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value);
                    }
                }

                return result;
            }
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

    public async Task<Dictionary<string, string>> TranslateByOpenAiAsync(Dictionary<string, string> dict, string language)
    {
        var result = await openAiService.ChatAsync($"translate json below to '{language} and return json in correct format.just translate value and not change keys':\n\n{JsonSerializer.Serialize(dict, JsonSerializerOptions.Web)}");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(result, JsonSerializerOptions.Web);
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

    public async Task AddRangeAsync(List<string> values)
    {
        var keys = await translationRepository.Set().Select(x => new { x.Key, x.Locale }).ToDictionaryAsync(x => x.Key + "-" + x.Locale, x => x.Key);
        values = values.Where(x => x.IsNotNullOrEmpty()).Distinct().ToList();
        foreach (var locale in configTranslationOptions.Value.Locales)
        {
            var chunks = values.Where(x => !keys.ContainsKey(HashUtil.Md5(x) + "-" + locale.Key)).Distinct().Chunk(50);
            foreach (var translationItems in chunks)
            {
                var dict = translationItems.ToDictionary(x => HashUtil.Md5(x), x => x);

                var translationResult = await TranslateByOpenAiAsync(dict, $"{locale.Value} ({locale.Key})");
                if (translationResult is null) continue;

                await translationRepository.BulkInsertAsync(translationResult.Select(x => new Translation()
                {
                    Key = x.Key,
                    Locale = locale.Key,
                    Value = x.Value
                }).ToList());
            }
        }
    }
}