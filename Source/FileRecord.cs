using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldersMonitor
{
    public class FileRecord
    {
        public string FilePath;
        public DateTime ChangeTime;

        public FileRecord(string filePath)
        {
            FilePath = filePath;
            ChangeTime = DateTime.Now;
        }
    }
}
