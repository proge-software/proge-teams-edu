using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proge.Teams.Edu.DAL
{
    public static class Extensions
    {
        public static IQueryable<T> NotDeleted<T>(this IQueryable<T> queryable) where T : BaseEntity
        {
            return queryable.Where(t => t.DeletedOn == null);
        }

        public static IEnumerable<T> DistinctBy<T>(this IQueryable<T> list, Func<T, object> propertySelector)
        {
            return list.GroupBy(propertySelector).Select(x => x.First());
        }

        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> list, Func<T, object> propertySelector)
        {
            return list.GroupBy(propertySelector).Select(x => x.First());
        }
    }
}
