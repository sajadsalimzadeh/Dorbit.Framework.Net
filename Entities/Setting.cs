﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(Key), IsUnique = true)]
public class Setting : Entity
{
    [MaxLength(32), Required]
    public string Key { get; set; }
    [MaxLength(10240)]
    public string Value { get; set; }

    [MaxLength(64)]
    public string Access { get; set; }

    public Setting()
    {
    }

    public Setting(Enum key, object value) : this(key.ToString(), value)
    {
    }

    public Setting(string key, object value)
    {
        Key = key;
        SetValue(value);
    }

    public void SetValue(object value)
    {
        Value = JsonSerializer.Serialize(value, JsonSerializerOptions.Web);
    }

    public T GetValue<T>(T defaultValue = default)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Value, JsonSerializerOptions.Web);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }
}