using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Formats.Asn1;

namespace WordSearchApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the word to search: ");
            string searchWord = Console.ReadLine();

            Console.Write("Enter the path of the solution folder: ");
            string solutionFolderPath = Console.ReadLine();

            if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(solutionFolderPath))
            {
                Console.WriteLine("Word or path cannot be empty.");
                return;
            }

            if (!Directory.Exists(solutionFolderPath))
            {
                Console.WriteLine("The specified folder does not exist.");
                return;
            }

            var results = SearchWordInFiles(searchWord, solutionFolderPath);
            SaveResultsToCsv(results, solutionFolderPath);
            Console.WriteLine("Search completed and results saved to CSV file.");
        }

        static List<SearchResult> SearchWordInFiles(string word, string folderPath)
        {
            var results = new List<SearchResult>();

            var csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            foreach (var file in csFiles)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new SearchResult
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file),
                            FolderPath = Path.GetDirectoryName(file),
                            LineNumber = i + 1,
                            LineText = lines[i].Trim()
                        });
                    }
                }
            }

            return results;
        }

        static void SaveResultsToCsv(List<SearchResult> results, string folderPath)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string csvFilePath = Path.Combine(folderPath, $"SearchResults_{timestamp}.csv");

            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(results);

                writer.WriteLine(); // Add a blank line for separation

                // Write summary table header
                writer.WriteLine("Summary by Folder");
                writer.WriteLine("FolderPath,Count");
                var folderCounts = results.GroupBy(r => r.FolderPath)
                                          .Select(g => new { FolderPath = g.Key, Count = g.Count() });
                foreach (var folderCount in folderCounts)
                {
                    writer.WriteLine($"{folderCount.FolderPath},{folderCount.Count}");
                }

                writer.WriteLine(); // Add another blank line

                // Write summary table header
                writer.WriteLine("Summary by File");
                writer.WriteLine("FilePath,Count");
                var fileCounts = results.GroupBy(r => r.FilePath)
                                        .Select(g => new { FilePath = g.Key, Count = g.Count() });
                foreach (var fileCount in fileCounts)
                {
                    writer.WriteLine($"{fileCount.FilePath},{fileCount.Count}");
                }
            }
        }
    }

    public class SearchResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public int LineNumber { get; set; }
        public string LineText { get; set; }
    }
}
