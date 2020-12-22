using Anilist.Client.GraphQl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Anilist.Client.Schema
{
    public class Page<TItem> : List<TItem>
    {
        public int Total { get; private set; }
        public int CurrentPage { get; private set; }
        public int PerPage { get; private set; }

        public Page(ICollection<TItem> items, int? total, int? current, int? per) : base(items)
        {
            Total = total ?? this.Count();
            CurrentPage = current ?? 1;
            PerPage = per ?? this.Count();
        }
    }

    internal static class PageExtensions
    {
        public static Page<TItem> FromQuery<TItem>(Page queryPage, Expression<Func<Page, ICollection<TItem>>> accessor) =>
            new Page<TItem>(accessor.Compile().Invoke(queryPage), queryPage.PageInfo.Total, queryPage.PageInfo.CurrentPage, queryPage.PageInfo.PerPage);

        public static Page<TItem> ToPage<TItem>(this Page queryPage, Expression<Func<Page, ICollection<TItem>>> accessor) => FromQuery<TItem>(queryPage, accessor);
    }
}
