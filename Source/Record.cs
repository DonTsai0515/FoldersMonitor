using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;

namespace FoldersMonitor
{
    public class Record
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary> 
        /// 寫入監聽結果到指定路徑
        /// </summary>
        public static void Trace(string Msg)
        {
            _logger.Trace(Msg);
        }

        public static void Warn(string Msg)
        {
            _logger.Warn(Msg);
        }

        public static void Error(string Msg)
        {
            _logger.Error(Msg);
        }

        /// <summary>
        /// 資料夾/檔案建立後記錄
        /// </summary>
        /// <param name="e"></param>
        public static void FileCreatedWrite(FileSystemEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo dirInfo = new DirectoryInfo(e.FullPath.ToString());

            sb.AppendLine();
            sb.AppendLine("新建於：" + dirInfo.FullName.Replace(dirInfo.Name, ""));

            if (File.Exists(e.FullPath))
            {
                sb.AppendLine("新建檔案名稱：" + dirInfo.Name);
            }
            else if (Directory.Exists(e.FullPath))
            {
                sb.AppendLine("新建資料夾名稱：" + dirInfo.Name);
            }
            sb.AppendLine("建立時間：" + dirInfo.CreationTime.ToString());
            sb.AppendLine("目錄下共有：" + dirInfo.Parent.GetFiles().Length + " 檔案");
            sb.AppendLine("目錄下共有：" + dirInfo.Parent.GetDirectories().Length + " 資料夾");
            sb.AppendLine("======================================");

            Trace(sb.ToString());
        }

        public static void FileChangedWrite(FileSystemEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo dirInfo = new DirectoryInfo(e.FullPath.ToString());

            sb.AppendLine();
            sb.AppendLine("被異動的檔名為：" + e.Name);
            sb.AppendLine("檔案所在位址為：" + e.FullPath.Replace(e.Name, ""));
            sb.AppendLine("異動內容時間為：" + dirInfo.LastWriteTime.ToString());
            sb.AppendLine("======================================");

            Trace(sb.ToString());
        }

        /// <summary>
        /// 資料夾/檔案刪除後記錄
        /// </summary>
        public static void FileDeletedWrite(FileSystemEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("刪除前路徑為：" + e.FullPath);
            sb.AppendLine("刪除時間：" + DateTime.Now.ToString());
            sb.AppendLine("======================================");

            Trace(sb.ToString());
        }

        /// <summary>
        /// 資料夾/檔案移動(改名)後紀錄
        /// </summary>
        public static void FileRenamedWrite(RenamedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo dirInfo = new DirectoryInfo(e.FullPath.ToString());

            if (File.Exists(e.FullPath))
            {
                sb.AppendLine();
                sb.AppendLine("檔案更改前的位置：" + e.OldFullPath.ToString());
                sb.AppendLine("檔案更改後的位置：" + e.FullPath.ToString());
            }
            else if (Directory.Exists(e.FullPath))
            {
                sb.AppendLine();
                sb.AppendLine("資料夾更改前的位置：" + e.OldFullPath.ToString());
                sb.AppendLine("資料夾更改後的位置：" + e.FullPath.ToString());
            }
            sb.AppendLine("更改時間：" + dirInfo.LastAccessTime.ToString());
            sb.AppendLine("======================================");

            Trace(sb.ToString());
        }
    }
}
