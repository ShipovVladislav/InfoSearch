using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LemmaSharp.Classes;

namespace Crawler
{
    public class Lemmatizer
    {
        private readonly LemmaSharp.Classes.Lemmatizer lemmatizer;
        public Lemmatizer()
        {
            var stream = File.OpenRead(Directory.GetCurrentDirectory()+"/full7z-mlteast-ru.lem");
            this.lemmatizer = new LemmaSharp.Classes.Lemmatizer(stream);
        }
        public void GetAllWords(string path)
        {
            var directoryPath = Directory.GetCurrentDirectory() + "/" + path;
            DirectoryInfo di = Directory.CreateDirectory(directoryPath+"/lemmas");
            var counter = 1;
            foreach (string file in Directory.EnumerateFiles(directoryPath, "*.txt"))
            {
                string contents = File.ReadAllText(file);
                var words = GetWords(contents);
                var lemmas = new List<string>();
                foreach (var word in words)
                {
                    var lemma = GetLemma(word);
                    lemmas.Add(lemma);
                    Console.WriteLine(word+" - "+lemma);
                }
                WriteWords(lemmas, directoryPath + "/lemmas/lemmas"+counter+".txt");
                counter += 1;
            }
        }

        public string GetLemma(string word)
        {
            return lemmatizer.Lemmatize(word);
        }
        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\w(?<!\d)[\w'-]*");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select m.Value;

            return words.ToArray();
        }
        public void WriteWords(List<string> words, string path)
        {
            File.Create(path).Dispose();
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var word in words)
                {
                    sw.WriteLine(word);
                }
            }
        }
    }
}