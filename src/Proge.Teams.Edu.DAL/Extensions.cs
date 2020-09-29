using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        public static IServiceCollection EnsureMigrationOfContext<T>(this IServiceCollection services)
            where T : DbContext
        {
            var sp = services.BuildServiceProvider();
            var context = sp.GetService<T>();
            context.EnsureMigrationOfContext();
            return services;
        }

        public static void EnsureMigrationOfContext<T>(this T context)
           where T : DbContext
        {
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();

        }


    }
}
