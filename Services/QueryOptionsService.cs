﻿using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Queries;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class QueryOptionsService : IQueryOptionsService
{
    private readonly Dictionary<string, QueryOptions> dict = new();

    public QueryOptionsService()
    {

    }

    public IQueryOptionsService AddOptions(Type type, QueryOptions options)
    {
        dict[type.FullName] = options;
        return this;
    }

    public IQueryOptionsService AddOptions<T>(QueryOptions options)
    {
        return AddOptions(typeof(T), options);
    }

    public IQueryable<T> ApplyTo<T>(IQueryable<T> query)
    {
        var type = typeof(T);
        if (!dict.ContainsKey(type.FullName)) return query;
        var options = dict[type.FullName];
        return options.ApplyTo(query);
    }

    public IQueryable<T> ApplyCountTo<T>(IQueryable<T> query)
    {
        var type = typeof(T);
        if (!dict.ContainsKey(type.FullName)) return query;
        var options = dict[type.FullName];
        return options.ApplyCountTo(query);
    }
}