using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using GemBox.Spreadsheet;

namespace Crawler
{
    public class TfIdfHandler
    {
        public Dictionary<string, List<int>> Index = new Dictionary<string, List<int>>();
        public List<FileInfo> FileInfos = new List<FileInfo>();
        public string path;
        public List<WordFreqInfo> TftdfInfo;
        public List<List<TFIDFFileInfoDto>> MainVector = new List<List<TFIDFFileInfoDto>>();
        
        public TfIdfHandler(string path)
        {
            var filePath = Directory.GetCurrentDirectory() + "/" + path + "/index/invertedlist.txt";
            this.path = Directory.GetCurrentDirectory() + "/" + path + "/index/tftdflist.csv";
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
            var directoryPath = Directory.GetCurrentDirectory() + "/" + path;
            DirectoryInfo di = Directory.CreateDirectory(directoryPath+"/lemmas");
            var counter = 1;
            foreach (string file in Directory.EnumerateFiles(directoryPath+"/lemmas", "*.txt"))
            {
                string contents = File.ReadAllText(file);
                var filewords = GetWords(contents);
                FileInfos.Add(new FileInfo()
                {
                    words = filewords.ToList(),
                    wordsCount = filewords.Length,
                    id = counter
                });
                counter += 1;
            }
            
            TftdfInfo = Index.Select(x =>
                new WordFreqInfo
                {
                    Word = x.Key,
                    IDF = Math.Log(200 / (double) x.Value.Count()),
                    TF = x.Value.ToDictionary(q => q, q =>
                    {
                        var file = FileInfos.FirstOrDefault(f => f.id == q);
                        var wordsCount =
                            (double) file.words.Count(z => z == x.Key);
            
                        return wordsCount / file.wordsCount;
                    })
                }).ToList();
            for (var i = 0; i < 200; i++)
            {
                var vector = TftdfInfo
                    .Select(x => new TFIDFFileInfoDto
                    {
                        DocNumber = i,
                        Word = x.Word,
                        TFIDF = x.TFIDF.ToList().Where(y => y.Key == i).Select(o => o.Value).FirstOrDefault()
                    }).ToList();
                
                MainVector.Add(vector);
            }
        }

        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\w(?<!\d)[\w'-]*");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select m.Value;

            return words.ToArray();
        }
        public void GetTfTdf()
        {
            var tfIdfInfo = Index.Select(x =>
                    new WordFreqInfo
                    {
                        Word = x.Key,
                        IDF = Math.Log(200 / (double) x.Value.Count()),
                        TF = x.Value.ToDictionary(q => q, q =>
                        {
                            var file = FileInfos.FirstOrDefault(f => f.id == q);
                            var wordsCount =
                                (double) file.words.Count(z => z == x.Key);
            
                            return wordsCount / file.wordsCount;
                        })
                    });
            WriteTfIdf(tfIdfInfo);
        }

        public void Search(string[] words)
        {
            var queryVector = new List<List<TFIDFFileInfoDto>>(MainVector);
            
            foreach (var temp in queryVector.SelectMany(item => item.Where(temp => words.Contains(temp.Word) && temp.TFIDF > 0)))
            {
                temp.TFIDF = 1 / (double) queryVector.Count;
            }
            
            foreach (var temp in queryVector.SelectMany(item => item.Where(temp => !words.Contains(temp.Word))))
            {
                temp.TFIDF = 0;
            }
            
            var result = new List<TFIDFFileInfoDto>();

            for (var i = 0; i < MainVector.Count; i++)
            {
                for (var j = 0; j < MainVector[i].Count; j++)
                {
                    var temp = MainVector[i][j].Word == queryVector[i][j].Word ? MainVector[i][j].TFIDF * queryVector[i][j].TFIDF : 0;
                    result.Add(new TFIDFFileInfoDto
                    {
                        DocNumber = MainVector[i][j].DocNumber,
                        Word = MainVector[i][j].Word,
                        TFIDF = temp
                    });
                }
            }

            result = result.OrderByDescending(x => x.TFIDF).GroupBy(p => p.DocNumber)
                .Select(g => g.First())
                .Where(x => x.TFIDF > 0)
                .Take(10)
                .ToList();

            foreach (var item in result)
            {
                Console.Write(item.DocNumber + " ");
            }
        }
        public void WriteTfIdf(IEnumerable<WordFreqInfo> tfIdfInfo)
        {
            File.Create(path).Dispose();
            using (var writer = new StreamWriter(this.path))
            using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
            {
                csv.WriteField("слово");

                csv.WriteField("IDF");
                csv.WriteField("TF:");
                for (int f = 1; f < 200; f++)
                {
                    csv.WriteField(f);
                }
                csv.WriteField("TF-IDF:");
                for (int f = 1; f < 200; f++)
                {
                    csv.WriteField(f);
                }
                csv.NextRecord();
                foreach (var item in tfIdfInfo)
                {
                    csv.WriteField(item.Word);

                    csv.WriteField(Math.Round(item.IDF, 6));

                    csv.WriteField("TF:");
                    for (int f = 1; f < 200; f++)
                    {
                        var value = item.TF.FirstOrDefault(x => x.Key == f).Value;
                        csv.WriteField(Math.Round(value, 6));
                    }
                    csv.WriteField("TF-IDF:");
                    for (int f = 1; f < 200; f++)
                    {
                        var value = item.TFIDF.FirstOrDefault(x => x.Key == f).Value;
                        csv.WriteField(Math.Round(value, 6));
                    }
                    csv.NextRecord();
                }
            }
        }
    }
    public class WordFreqInfo
    {
        /// <summary>
        /// Слово
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// TF
        /// </summary>
        public Dictionary<int, double> TF { get; set; }

        /// <summary>
        /// IDF
        /// </summary>
        public double IDF { get; set; }

        /// <summary>
        /// TF-IDF
        /// </summary>
        public Dictionary<int, double> TFIDF => TF.ToDictionary(x => x.Key, x => TF[x.Key] * IDF);
    }

    public class FileInfo
    {
        public List<string> words { get; set; }
        public int wordsCount { get; set; }
        public int id { get; set; }
    }
    public class TFIDFFileInfoDto
    {
        public int DocNumber { get; set; }
        public string Word { get; set; }
        public double TFIDF { get; set; }
    }
}