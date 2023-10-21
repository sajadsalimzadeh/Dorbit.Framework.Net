namespace Dorbit.Utils.Queries
{
    public class OrderByQueryOption
    {
        public List<KeyValuePair<string, bool>> Items { get; set; }

        public string ToSql()
        {
            if (Items is null) return null;
            return string.Join(",",
                Items.ConvertAll(x => $"{x.Key} {(x.Value ? "DESC" : "ASC")}"));
        }

        public OrderByQueryOption Clone()
        {
            return new OrderByQueryOption()
            {
                Items = Items?.ToList()
            };
        }
    }
}