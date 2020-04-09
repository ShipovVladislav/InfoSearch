using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crawler
{
    public class IndexHandler
    {
        public IndexHandler()
        {
            
        }
        
        public void GetIndex(string path)
        {
            var directoryPath = Directory.GetCurrentDirectory() + "/" + path + "/lemmas";
            var counter = 1;
            var index = new List<WordModel>();
            foreach (string file in Directory.EnumerateFiles(directoryPath, "*.txt"))
            {
                string contents = File.ReadAllText(file);
                var words = GetWords(contents);
                foreach (var word in words)
                {
                    if (index.Any(x => x.Word == word))
                    {
                        index.FirstOrDefault(x => x.Word == word).Files.Add(counter);
                    }
                    else
                    {
                        index.Add(new WordModel()
                        {
                            Word = word,
                            Files = new List<int>{ counter}
                        });
                    }
                }

                counter += 1;
            }
            WriteWords(index, Directory.GetCurrentDirectory() + "/" + path + "/index/invertedlist.txt");
            
        }
        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\w(?<!\d)[\w'-]*");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select m.Value;

            return words.ToArray();
        }
        public void WriteWords(List<WordModel> words, string path)
        {
            File.Create(path).Dispose();
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var word in words)
                {
                    sw.Write(word.Word);
                    sw.Write(";;");
                    foreach (var file in word.Files.Distinct())
                    {
                        sw.Write(","+file);
                    }
                    sw.WriteLine();
                }
            }
        }
    }

    public class WordModel
    {
        public string Word { get; set; }
        public List<int> Files { get; set; }
    }
}