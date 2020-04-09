using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Crawler
{
    public class QueryHandler
    {
        public Dictionary<string, List<int>> Index = new Dictionary<string, List<int>>();
        public QueryHandler(string path)
        {
            ReadIndex(path);
        }

        public List<int> HandleQuery(string query)
        {
            var splittedQuery = query.Split(' ').ToList();

            var words = new List<string>();

            foreach (var item in splittedQuery)
            {
                if (item != "AND" || item != "OR" || item != "NOT")
                {
                    words.Add(item);
                } 
            }
            var predicate = PredicateBuilder.False<KeyValuePair<string, List<int>>>();

            foreach (var keyword in words)
            {
                var temp = keyword;
                predicate = predicate.Or(p => p.Key == temp);
            }
            var indexrows = Index.Where(predicate.Compile()).ToList();
            var fullList = Enumerable.Range(1,200).ToList();
            var resultList = new List<int>();
            var lastOperation = 0;
            var isNOT = false;
            foreach (var word in splittedQuery)
            {
                var item = indexrows.FirstOrDefault(x => x.Key == word);
                
                switch (word)
                {
                    case "AND":
                        lastOperation = 1;
                        break;
                    case "OR":
                        lastOperation = 2;
                        break;
                    case "NOT":
                        isNOT = true;
                        break;
                }

                if (item.Key != null && resultList.Count > 0)
                {
                    var indexes = item.Value;
                    if (isNOT)
                    {
                        isNOT = false;
                        indexes = fullList.Except(indexes).ToList();
                    }
                    if (lastOperation == 1)
                        resultList = resultList.Intersect(indexes).ToList();
                    
                    if (lastOperation == 2)
                        resultList = resultList.Union(indexes).ToList();
                    
                }
                else if (item.Value != null)
                {
                    var indexes = item.Value;
                    if (isNOT)
                    {
                        isNOT = false;
                        indexes = fullList.Except(indexes).ToList();
                    }
                    resultList = indexes;
                }
            }
            return resultList.OrderBy(x => x).ToList();
        }

        private void ReadIndex(string path)
        {
            var filePath = Directory.GetCurrentDirectory() + "/" + path + "/index/invertedlist.txt";
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var splitted = line.Split(";;,");
                    var keyWord = splitted[0].Trim();
                    var docNumbers = splitted[1].Split(',').Select(x => Convert.ToInt32(x)).ToList();
                    Index.Add(keyWord, docNumbers);
                }
            }
        }
        

    }
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T> ()  { return f => true;  }
        public static Expression<Func<T, bool>> False<T> () { return f => false; }
 
        public static Expression<Func<T, bool>> Or<T> (this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse (expr1.Body, invokedExpr), expr1.Parameters);
        }
 
        public static Expression<Func<T, bool>> And<T> (this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso (expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}