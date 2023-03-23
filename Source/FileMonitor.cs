using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;

namespace FoldersMonitor
{
    public class FileMonitor
    {
        //FTP連線基本資訊集合
        private FTPTarget FTPs = (ConfigurationManager.GetSection("FTP") as FTP).FTPTarget;

        //資料夾監控器集合
        private List<FileSystemWatcher> FileSystemWatcherList;

        //FTP伺服器集合
        private List<FTPObject> FTPObjectList;

        /// <summary>
        /// 物件初始化
        /// </summary>
        public FileMonitor()
        {
            try
            {
                //取得所有想要監聽的資料夾
                var listFolders = GetWatcherDirList();
                // Creates a new instance of the list
                FileSystemWatcherList = new List<FileSystemWatcher>();
                FTPObjectList = new List<FTPObject>();

                #region 設定資料夾如何監控 FileSystemWatcherList                
                foreach (CustomFolderSettings customFolder in listFolders)
                {
                    DirectoryInfo dir = new DirectoryInfo(customFolder.FolderPath);
                    // 確認資料夾是否存在&&監聽是否開啟
                    if (customFolder.FolderEnabled && dir.Exists)
                    {
                        FileSystemWatcher fileWatch = new FileSystemWatcher();

                        // 欲監視的文件類型
                        fileWatch.Filter = customFolder.FolderFilter;

                        // 資料夾路徑(根路徑)
                        fileWatch.Path = customFolder.FolderPath;

                        // 子資料夾是否也被監視
                        fileWatch.IncludeSubdirectories = customFolder.FolderIncludeSub;

                        // 設定所要監控的變更類型
                        fileWatch.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                        //設定觸發事件                 
                        fileWatch.Created += (senderObj, fileSysArgs) => fileWatch_Created(senderObj, fileSysArgs, customFolder.FolderPath);
                        fileWatch.Deleted += (senderObj, fileSysArgs) => fileWatch_Deleted(senderObj, fileSysArgs, customFolder.FolderPath);
                        fileWatch.Renamed += (senderObj, fileSysArgs) => fileWatch_Renamed(senderObj, fileSysArgs, customFolder.FolderPath);
                        //fileWatch.Changed += (senderObj, fileSysArgs) => fileWatch_Changed(senderObj, fileSysArgs, customFolder.FolderPath);

                        // Begin watching
                        fileWatch.EnableRaisingEvents = customFolder.FolderEnabled;
                        // Add the systemWatcher to the list
                        FileSystemWatcherList.Add(fileWatch);
                    }
                }
                #endregion

                #region FTP連線設定FTP伺服器集合 FTPObjectList
                for (int i = 0; i < FTPs.Count; i++)
                {
                    //FTP連線物件
                    FTPObject FTPObject = new FTPObject(FTPs[i].Host, FTPs[i].Port, FTPs[i].UserName, FTPs[i].Password, FTPs[i].EncryptionMode);
                    FTPObjectList.Add(FTPObject);
                }
                #endregion               
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("資料夾監控失敗，原因：{0}", ex.ToString()));
            }
        }

        public void StopMonitor()
        {
            if (FileSystemWatcherList != null)
            {
                foreach (FileSystemWatcher fsw in FileSystemWatcherList)
                {
                    // 停止監控
                    fsw.EnableRaisingEvents = false;
                    fsw.Dispose();
                }
                FileSystemWatcherList.Clear();
            }
        }

        /// <summary>
        /// 從Xml設定檔當中取得所有想要監聽的資料夾
        /// </summary>
        /// <returns></returns>
        private static List<CustomFolderSettings> GetWatcherDirList()
        {
            try
            {
                // Xml設定檔
                string fileNameXML = ConfigurationManager.AppSettings["XMLFileFolderSettings"];

                XmlSerializer deserializer = new XmlSerializer(typeof(List<CustomFolderSettings>));
                // Xml反序列化
                TextReader reader = new StreamReader(fileNameXML);
                object obj = deserializer.Deserialize(reader);

                reader.Close();

                //取得所有想要監聽的資料夾及特殊觀察屬性
                List<CustomFolderSettings> listFolders = obj as List<CustomFolderSettings>;
                return listFolders;
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("取得欲監聽的目標資料夾失敗，原因：{0}", ex.ToString()));
                return new List<CustomFolderSettings>();
            }
        }

        /// <summary>
        /// 比對檔案
        /// </summary>
        internal void CheckFile()
        {
            foreach (FileSystemWatcher fileSystemWatcher in FileSystemWatcherList)
            {
                var localPath = fileSystemWatcher.Path;

                List<string> localFilePaths = GetFilePaths(localPath);
                int localFilesNumber = localFilePaths.Count;

                foreach (FTPObject FTPObject in FTPObjectList)
                {
                    List<string> ftpFilePaths = FTPObject.GetFilePaths();
                    int ftpFilesNumber = ftpFilePaths.Count;

                    #region 比對及紀錄結果
                    if (localFilesNumber == ftpFilesNumber)
                    {
                        Record.Trace(string.Format("FTP伺服器 {0}:{1} 檔案比對相同", FTPObject.FTPClient.Host, FTPObject.FTPClient.Port));

                        //呼叫發送通知的 API (每日固定時間，發送檢查結果)
                    }
                    //FTP缺少檔案
                    else if (localFilesNumber > ftpFilesNumber)
                    {
                        foreach (string filePath in localFilePaths.Except(ftpFilePaths).ToList())
                        {
                            Record.Warn(string.Format("FTP伺服器 {0}:{1} 缺少檔案，檔案相對路徑：{2}", FTPObject.FTPClient.Host, FTPObject.FTPClient.Port, filePath));
                        }

                        //呼叫發送通知的 API (每日固定時間，發送檢查結果)
                    }
                    //FTP多出檔案
                    else if (localFilesNumber < ftpFilesNumber)
                    {
                        foreach (string filePath in ftpFilePaths.Except(localFilePaths).ToList())
                        {
                            Record.Warn(string.Format("FTP伺服器 {0}:{1} 多出檔案，檔案相對路徑：{2}", FTPObject.FTPClient.Host, FTPObject.FTPClient.Port, filePath));
                        }

                        //呼叫發送通知的 API (每日固定時間，發送檢查結果)
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 取得本機路徑下包含所有子資料夾下的檔案路徑清單
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        private List<string> GetFilePaths(string localPath)
        {
            var files = new List<string>();
            ExamineDirectory(localPath, ref files);

            for (int i = 0; i < files.Count; i++)
            {
                files[i] = GetRelativePath(files[i], localPath);
            }
            return files;
        }

        /// <summary>
        /// 遍歷搜尋路徑下包含所有子資料夾的檔案並把各檔案路徑記錄至files裡
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="files"></param>
        private void ExamineDirectory(string localPath, ref List<string> files)
        {
            foreach (string directorypath in Directory.GetDirectories(localPath))
            {
                ExamineDirectory(directorypath, ref files);
            }
            foreach (string filepath in Directory.GetFiles(localPath))
            {
                files.Add(filepath);
            }
        }

        /// <summary>
        /// 文件覆蓋過去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="basePath"></param>
        private void fileWatch_Changed(object sender, FileSystemEventArgs e, string basePath)
        {
            //Record.FileChangedWrite(e);
            //FileCreatedUpdateFTP(e, basePath);
        }

        /// <summary>
        /// 監控資料夾裡有文件被建立時觸發
        /// </summary>
        private void fileWatch_Created(object sender, FileSystemEventArgs e, string basePath)
        {
            Record.FileCreatedWrite(e);
            FileCreatedUpdateFTP(e, basePath);
        }

        /// <summary>
        /// 監控資料夾裡有文件被建立時觸發，更新FTP伺服器
        /// </summary>
        /// <param name="e"></param>
        private void FileCreatedUpdateFTP(FileSystemEventArgs e, string basePath)
        {
            try
            {
                foreach (FTPObject FTPObject in FTPObjectList)
                {
                    if (File.Exists(e.FullPath))
                    {
                        if (FTPObject.FTPClient != null)
                        {
                            FTPObject.UploadFile(e.FullPath, GetRelativePath(e.FullPath, basePath));
                        }
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        if (FTPObject.FTPClient != null)
                        {
                            FTPObject.CreateDirectory(GetRelativePath(e.FullPath, basePath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("檔案建立後更新至FTP伺服器失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 監控資料夾裡文件被改名時觸發
        /// </summary>
        private void fileWatch_Renamed(object sender, RenamedEventArgs e, string basePath)
        {
            Record.FileRenamedWrite(e);
            FileRenamedUpdateFTP(e, basePath);
        }

        /// <summary>
        /// 監控資料夾裡文件被改名時觸發，更新FTP伺服器
        /// </summary>
        /// <param name="e"></param>
        private void FileRenamedUpdateFTP(RenamedEventArgs e, string basePath)
        {
            try
            {
                foreach (FTPObject FTPObject in FTPObjectList)
                {
                    if (FTPObject.FTPClient != null)
                    {
                        FTPObject.Rename(GetRelativePath(e.OldFullPath, basePath), GetRelativePath(e.FullPath, basePath));
                    }
                }
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("檔案移動後更新至FTP伺服器失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 監控資料夾裡文件被刪除時觸發
        /// </summary>
        private void fileWatch_Deleted(object sender, FileSystemEventArgs e, string basePath)
        {
            Record.FileDeletedWrite(e);
            FileDeletedUpdateFTP(e, basePath);
        }

        /// <summary>
        /// 監控資料夾裡文件被刪除時，更新FTP伺服器
        /// </summary>
        /// <param name="e"></param>
        private void FileDeletedUpdateFTP(FileSystemEventArgs e, string basePath)
        {
            try
            {
                foreach (FTPObject FTPObject in FTPObjectList)
                {
                    if (FTPObject.FTPClient != null)
                    {
                        FTPObject.DeleteDocument(GetRelativePath(e.FullPath, basePath));
                    }
                }
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("檔案刪除後FTP伺服器更新失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 傳入完整路徑及根路徑取得相對路徑        
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="basePath"></param>        
        /// <returns></returns>
        private static string GetRelativePath(string fullPath, string basePath)
        {
            if (!basePath.EndsWith(@"\"))
            {
                basePath += @"\";
            }
            Uri fp = new Uri(fullPath);
            Uri bp = new Uri(basePath);

            var relPath = bp.MakeRelativeUri(fp).ToString().Replace("/", @"\");

            return relPath;
        }
    }
}
