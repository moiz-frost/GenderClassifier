using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Accord;
using Accord.Controls;
using Accord.IO;
using Accord.MachineLearning.Bayes;
using Accord.Math;
using Accord.Math.Distances;
using Accord.Statistics.Filters;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Visualizations;
using nullpointer.Metaphone;

namespace ML
{   
    class Program
    {
        private static string GetHash(string word)
        {
            word = word.ToLower();
            Match singleMatch = Regex.Match(word, @"");
            MatchCollection matches = Regex.Matches(word, @"");
            word = Regex.Replace(word, @"ain$", "ein");
            word = Regex.Replace(word, @"ai", "ae");
            word = Regex.Replace(word, @"ay$", "e");
            word = Regex.Replace(word, @"ey$", "e");
            word = Regex.Replace(word, @"ie$", "y");
            word = Regex.Replace(word, @"^es", "is");
            word = Regex.Replace(word, @"a+", "a");
            word = Regex.Replace(word, @"j+", "j");
            word = Regex.Replace(word, @"d+", "d");
            word = Regex.Replace(word, @"u", "o");
            word = Regex.Replace(word, @"o+", "o");
            word = Regex.Replace(word, @"ee+", "i");
            word = Regex.Replace(word, @"yi+", "i");
            singleMatch = Regex.Match(word, @"(ar)");
            if (singleMatch.Success)
            {
                word = Regex.Replace(word, @"ar", "r");
            }
            word = Regex.Replace(word, @"iy+", "i");
            word = Regex.Replace(word, @"ih+", "eh");
            word = Regex.Replace(word, @"s+", "s");
            singleMatch = Regex.Match(word, @"[rst]y");
            if (singleMatch.Success && (!word.EndsWith("y") || !word.EndsWith("Y")))
            {
                word = Regex.Replace(word, @"y", "i");
            }
            word = Regex.Replace(word, @"ya+", "ia");
            singleMatch = Regex.Match(word, @"[bcdefghijklmnopqrtuvwxyz]i");
            if (singleMatch.Success)
            {
                word = Regex.Replace(word, @"i$", "y");
            }
            singleMatch = Regex.Match(word, @"[acefghijlmnoqrstuvwxyz]h");
            if (singleMatch.Success)
            {
                word = Regex.Replace(word, @"h", "");
            }
            word = Regex.Replace(word, @"k+", "k");
            word = Regex.Replace(word, @"k", "q");
            word = Regex.Replace(word, @"q+", "q");

            return word;
        }

        private static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        private static List<string[]> DoubleMetaphoneGenerateLists(string name, string path, string worksheet)
        {
            DoubleMetaphone metaphone = new DoubleMetaphone(name);
            string code = metaphone.PrimaryKey;
            List<string[]> namesWithSameSound = new List<string[]>();
            DataTable table = new ExcelReader(path).GetWorksheet(worksheet);

            var tableValues = table.ToJagged<string>("NAMES", "GENDER");
            string[] names = new string[tableValues.Length];
            string[] gender = new string[tableValues.Length];
            for (int i = 0; i < tableValues.Length; i++)
            {
                names[i] = tableValues[i][0].ToLower();
                gender[i] = tableValues[i][1];
            }

            for (int i = 0; i < names.Length; i++)
            {
                metaphone.computeKeys(names[i]);
                if (metaphone.PrimaryKey == code)
                {
                    namesWithSameSound.Add(new string[] {names[i], gender[i]});
                }
            }

            return namesWithSameSound;
        }

        private static List<string[]> GetRomanUrduHashes(string name, string path, string worksheet)
        {
            string code = GetHash(name);
            List<string[]> namesWithSameSound = new List<string[]>();
            DataTable table = new ExcelReader(path).GetWorksheet(worksheet);
            var tableValues = table.ToJagged<string>("NAMES", "GENDER");
            string[] names = new string[tableValues.Length];
            string[] gender = new string[tableValues.Length];

            for (int i = 0; i < tableValues.Length; i++)
            {
                names[i] = tableValues[i][0].ToLower();
                gender[i] = tableValues[i][1];
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (object.Equals(GetHash(names[i]), code))
                {
                    namesWithSameSound.Add(new string[] { names[i], gender[i] });
                }
            }
            return namesWithSameSound;
        }

        public static string[] GetGender(string name, string path, string worksheet)
        {
            name = name.ToLower();
            char firstCharacterFromName = name[0];
            char firstCharacterFromIndexSpecifiedListValue;
            List<string[]> namesWithSameSoundWithDoubleMetaphoneAlgorithm = DoubleMetaphoneGenerateLists(name, path, worksheet);
            List<string[]> namesWithSameSoundWithRomanUrduHashes = GetRomanUrduHashes(name, path, worksheet);
            List<string[]> concatenatedNamesList = namesWithSameSoundWithDoubleMetaphoneAlgorithm.Concat(namesWithSameSoundWithRomanUrduHashes).ToList();


            for (int i = 0; i < concatenatedNamesList.Count; i++)
            {
                firstCharacterFromIndexSpecifiedListValue = concatenatedNamesList.ElementAt(i)[0][0];
                if (firstCharacterFromIndexSpecifiedListValue != firstCharacterFromName)
                {
                    concatenatedNamesList.RemoveAt(i);
                }
            }


            if (concatenatedNamesList.ElementAtOrDefault(0) == null)
            {
                return new string[] { "Name Not Found", "M"};
            }

            int lowestLevenshteinDistance = ComputeLevenshteinDistance(concatenatedNamesList.ElementAt(0)[0], name.ToLower());
            Console.WriteLine("lowest levenshtein is " + lowestLevenshteinDistance);
            int lowestLevenshteinDistanceIndex = 0;
            int levenshteinDistanceAtSpecifiedIndex = 0;


            /*for (int i = 0; i < concatenatedNamesList.Count; i++)
            {
                Console.WriteLine(concatenatedNamesList.ElementAt(i)[0]);
            }*/



            for (int i = 0; i < concatenatedNamesList.Count; i++)
            {
                levenshteinDistanceAtSpecifiedIndex = ComputeLevenshteinDistance(concatenatedNamesList.ElementAt(i)[0], name);

                if (levenshteinDistanceAtSpecifiedIndex < lowestLevenshteinDistance)
                {
                    lowestLevenshteinDistance = levenshteinDistanceAtSpecifiedIndex;
                    lowestLevenshteinDistanceIndex = i;
                }
            }


            return concatenatedNamesList.ElementAt(lowestLevenshteinDistanceIndex);
        }


        static void Main(string[] args)
        {

            string[] output = GetGender("pappu", @"D:\Projects\VS 15\ML\ML\MuslimNames.xls", "Muslim Names");
            Console.WriteLine(String.Format("{0} {1}", output[0], output[1]));

        }
    }
}