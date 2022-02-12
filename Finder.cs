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
        public bool InAllDirectories { get; set; }
        public bool IncludeFilesWithoutMatches { get; set; }
        public bool IsSilent { get; set; }

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

                    resultItem.NumMatches += _fiound.Split(new string[] { _find }, StringSplitOptions.None).Length - 1;

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
	}
}
