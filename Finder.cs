using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace findandreplace
{
    class Finder
    {
        public string Dir { get; set; }
        public string FileMask { get; set; }
        public string ExcludeFileMask { get; set; }
        public string FindText { get; set; }
        public string ReplaceText { get; set; }
        public bool InAllDirectories { get; set; }
        public bool IncludeFilesWithoutMatches { get; set; }
        public bool IsSilent { get; set; }
        public bool IsReplace { get; set; }

        public bool IsCancelRequested { get; set; }
        public class FindResultItem : ResultItem
        {
        }
        public class FindResult
        {
            public List<FindResultItem> Items { get; set; }
            
        }

		public Finder()
        {
        }

        private bool checkExlсudes(string name)
        {
            if (string.IsNullOrWhiteSpace(ExcludeFileMask))
                return true;
            string[] excludeMask = ExcludeFileMask.Split(',');
            var excludeFlag = excludeMask.Length;
            foreach (var mask in excludeMask)
            {
                if (mask[0] == ' ') mask.TrimStart(' ');
                Regex maskExp = new Regex('.'+mask.Replace(".", "\\."));

                if (!maskExp.IsMatch(name))
                {
                    excludeFlag--;
                }
            }

            return excludeFlag == 0;
        }

        public FindResult Find()
        {

            string[] filesInDirectory = Directory.GetFiles(Dir, 
                                                     FileMask, 
                                                                InAllDirectories?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly)
                                                                                .Where(checkExlсudes).ToArray();

            var resultItems = new List<FindResultItem>();


            foreach (string filePath in filesInDirectory)
            {
                
                var resultItem = FindInFile(filePath);

                //Skip files that don't have matches
                if (resultItem.IncludeInResultsList)
                    resultItems.Add(resultItem);

            }


            return new FindResult { Items = resultItems };
        }

		private FindResultItem FindInFile(string filePath)
		{
			var resultItem = new FindResultItem();

            resultItem.IsSuccess = true;

			resultItem.FileName = Path.GetFileName(filePath);
			resultItem.FilePath = filePath;
			resultItem.FileRelativePath = "." + filePath.Substring(Dir.Length);

            List<string> findTextLines = FindText.Split("\r\n").ToList();
            var linesLength = findTextLines.Count;
            List<string> fileLines = new List<string>(linesLength);
            foreach (string line in File.ReadLines(resultItem.FilePath))
            {
                if (fileLines.Count == linesLength)
                {
                    string _find = string.Join("\r\n", findTextLines.ToArray());
                    string _fiound = string.Join("\r\n", fileLines.ToArray());

                    resultItem.NumMatches += _fiound.Contains( _find)?1:0;

                    fileLines.RemoveAt(0);
                    fileLines.Add(line);
                }
                else
                {
                    fileLines.Add(line);
                }
            }
            
            return resultItem;
		}

        private FindResultItem FindAndReplaceInFile(string filePath)
        {
            var resultItem = new FindResultItem();

            resultItem.IsSuccess = true;

            resultItem.FileName = Path.GetFileName(filePath);
            resultItem.FilePath = filePath;
            resultItem.FileRelativePath = "." + filePath.Substring(Dir.Length);

            List<string> findTextLines = FindText.Split("\r\n").ToList();
            var linesLength = findTextLines.Count;
            List<string> fileLines = new List<string>(linesLength);
            using (StreamWriter sw = File.CreateText(filePath + ".FNRTEMP"))
            {
                foreach (string line in File.ReadLines(resultItem.FilePath))
                {
                    if (fileLines.Count == linesLength)
                    {
                        string _find = string.Join("\r\n", findTextLines.ToArray());
                        string _found = string.Join("\r\n", fileLines.ToArray());


                        if (_found.Contains(_find))
                        {
                            resultItem.NumMatches += 1;
                            var changed = _found.Replace(_find, ReplaceText);
                            var changedSplit = changed.Split("\r\n");
                            for (int i = 0; i < linesLength - 1; i++)
                            {
                                fileLines[i] = changedSplit[i];
                            }
                        }

                        sw.WriteLine(fileLines[0]);
                        fileLines.RemoveAt(0);
                        fileLines.Add(line);
                    }
                    else
                    {
                        fileLines.Add(line);
                    }
                }
            }

            File.Delete(filePath);
            File.Move(filePath + ".FNRTEMP", filePath);

            return resultItem;
        }
    }
}
