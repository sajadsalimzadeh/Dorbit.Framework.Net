using Devor.Framework.Attributes;
using Devor.Framework.Utils.Queries;
using Devor.Framework.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Services
{
    [ServiceRegisterar]
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
}
