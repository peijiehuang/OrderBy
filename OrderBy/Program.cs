namespace OrderBy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class Location
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Layer { get; set; }
        public int Depth { get; set; }

        public override string ToString()
        {
            return $"Row: {Row}, Column: {Column}, Layer: {Layer}, Depth: {Depth}";
        }
    }

    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool ascending)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.PropertyOrField(param, propertyName);
            var lambda = Expression.Lambda(body, param);

            var methodName = ascending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                                           .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                                           .MakeGenericMethod(typeof(T), body.Type);

            return (IOrderedQueryable<T>)method.Invoke(null, new object[] { source, lambda });
        }

        public static IOrderedQueryable<T> ThenByDynamic<T>(this IOrderedQueryable<T> source, string propertyName, bool ascending)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.PropertyOrField(param, propertyName);
            var lambda = Expression.Lambda(body, param);

            var methodName = ascending ? "ThenBy" : "ThenByDescending";
            var method = typeof(Queryable).GetMethods()
                                           .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                                           .MakeGenericMethod(typeof(T), body.Type);

            return (IOrderedQueryable<T>)method.Invoke(null, new object[] { source, lambda });
        }
    }

    class Program
    {
        static void Main()
        {
            // 示例数据
            var locations = new List<Location>
        {
            new Location { Row = 2, Column = 3, Layer = 1, Depth = 4 },
            new Location { Row = 1, Column = 2, Layer = 2, Depth = 3 },
            new Location { Row = 3, Column = 1, Layer = 3, Depth = 2 },
            new Location { Row = 1, Column = 3, Layer = 1, Depth = 5 },
            new Location { Row = 2, Column = 2, Layer = 2, Depth = 1 }
        };

            // 用户自定义排序规则（行升序、层降序、列升序）
            var sortConditions = new List<(string property, bool ascending)>
        {
            ("Row", true),     // Row 升序
            ("Layer", false),  // Layer 降序
            ("Column", true)   // Column 升序
        };

            // 应用动态排序
            var sortedLocations = ApplyDynamicSort(locations.AsQueryable(), sortConditions);

            // 打印排序结果
            Console.WriteLine("Sorted Locations:");
            foreach (var location in sortedLocations)
            {
                Console.WriteLine(location);
            }
        }

        // 动态排序应用
        static IOrderedQueryable<Location> ApplyDynamicSort(IQueryable<Location> query, List<(string property, bool ascending)> sortConditions)
        {
            IOrderedQueryable<Location> orderedQuery = null;

            foreach (var (property, ascending) in sortConditions)
            {
                if (orderedQuery == null)
                {
                    orderedQuery = ascending
                        ? query.OrderByDynamic(property, ascending)
                        : query.OrderByDynamic(property, ascending);
                }
                else
                {
                    orderedQuery = ascending
                        ? orderedQuery.ThenByDynamic(property, ascending)
                        : orderedQuery.ThenByDynamic(property, ascending);
                }
            }

            return orderedQuery;
        }
    }

}
