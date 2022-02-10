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

        public FindResult Find()
        {

            string[] filesInDirectory = Directory.GetFiles(Dir, FileMask, SearchOption.AllDirectories).ToArray();

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
			//resultItem.IncludeFilesWithoutMatches = IncludeFilesWithoutMatches;

			resultItem.FileName = Path.GetFileName(filePath);
			resultItem.FilePath = filePath;
			resultItem.FileRelativePath = "." + filePath.Substring(Dir.Length);

            var fileText = File.ReadAllText(resultItem.FilePath);
            if (fileText.Contains(FindText))
            {
                resultItem.NumMatches = 1;
            }

            return resultItem;
		}
	}
}
