using System;
using System.IO;
using System.Linq;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Crawler crawler = new Crawler();
            //Lemmatizer lemmatizer = new Lemmatizer();
            IndexHandler indexHandler = new IndexHandler();
            
            //crawler.GetPagesFromUrl("https://www.kp.ru/");
            //lemmatizer.GetAllWords("2020-08-4--14-16-40");
            //indexHandler.GetIndex("2020-08-4--14-16-40");
            QueryHandler queryHandler = new QueryHandler("2020-08-4--14-16-40");
            //var query1 = "вспышка AND мозг OR выглядеть";
            //Console.WriteLine(query1);
            //var result1 = queryHandler.HandleQuery(query1);
            //Console.WriteLine(string.Join(";", result1));
            //var query2 = "вспышка OR мозг OR выглядеть";
            //Console.WriteLine(query2);
            //var result2 = queryHandler.HandleQuery(query2);
            //Console.WriteLine(string.Join(";", result2));
            //var query3 = "вспышка AND NOT мозг OR NOT выглядеть";
            //Console.WriteLine(query3);
            //var result3 = queryHandler.HandleQuery(query3);
            //Console.WriteLine(string.Join(";", result3));
            //var query4 = "вспышка OR NOT мозг OR NOT выглядеть";
            //Console.WriteLine(query4);
            //var result4 = queryHandler.HandleQuery(query4);
            //Console.WriteLine(string.Join(";", result4));
            
            TfIdfHandler tfIdfHandler = new TfIdfHandler("2020-08-4--14-16-40");
            //tfIdfHandler.GetTfTdf();
            var q1 = new[] {"чемпионат", "футбол"};
            Console.WriteLine(string.Join(" ", q1));
            tfIdfHandler.Search(q1);
            Console.WriteLine();
            var q2 = new[] {"чемпионат"};
            Console.WriteLine(string.Join(" ", q2));
            tfIdfHandler.Search(q2);
            
            Console.ReadLine();
        }
    }
}