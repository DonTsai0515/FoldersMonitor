using FluentFTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace FoldersMonitor
{
    public interface IFTPService
    {
        void UploadFile(string localFilePath, string remoteFilePath);
        void DeleteDocument(string remoteFilePath);
        void Rename(string oldRemotePath, string newRemotePath);
    }

    public class FTPObject : IFTPService
    {
        public FtpClient FTPClient;

        /// <summary>
        /// 連接狀態
        /// </summary>
        public bool Connected
        {
            get
            {
                return FTPClient.IsConnected;
            }
        }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="host">IP</param>
        /// <param name="port">通訊埠</param>
        /// <param name="username">使用者名稱</param>
        /// <param name="password">密碼</param>
        /// <param name="encryptionMode">加密協定</param>
		public FTPObject(string host, int port, string username, string password, int encryptionMode)
        {
            if (host != "" && username != "" && password != "")
            {
                FTPClient = new FtpClient()
                {
                    Host = host,
                    Port = port,
                    Credentials = new NetworkCredential(username, password),
                    Config = new FtpConfig
                    {
                        EncryptionMode = (FtpEncryptionMode)encryptionMode,
                        ValidateAnyCertificate = true
                    }
                };
            };
        }

        /// <summary>
        /// 連接FTP伺服器
        /// </summary>
		public void Connect()
        {
            try
            {
                if (!Connected)
                {
                    FTPClient.Connect();
                }
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("連接FTP伺服器失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 關閉連接FTP伺服器
        /// </summary>
		public void Disconnect()
        {
            try
            {
                if (FTPClient != null && Connected)
                {
                    FTPClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("關閉連接FTP伺服器失敗，原因：{0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 上傳文件
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="remoteFilePath"></param>
        public void UploadFile(string localFilePath, string remoteFilePath)
        {
            try
            {
                //資料夾位置
                string directoryPath = Path.GetDirectoryName(remoteFilePath);
                Connect();
                if (directoryPath != "" && !FTPClient.DirectoryExists(directoryPath))
                {
                    //如果檔案在資料夾內FTP伺服器需要先創建資料夾
                    CreateDirectory(directoryPath);
                }
                FTPClient.UploadFile(localFilePath, remoteFilePath);
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("FTP伺服器上傳文件失敗，原因：{0}", ex.ToString()));
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 創建FTP伺服器遠端目錄
        /// </summary>
        /// <param name="remotePath">遠端目錄</param>
        public void CreateDirectory(string remotePath)
        {
            try
            {
                string[] paths = remotePath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string curPath = @"\";
                Connect();
                for (int i = 0; i < paths.Length; i++)
                {
                    curPath += paths[i];
                    if (!FTPClient.DirectoryExists(curPath))
                    {
                        FTPClient.CreateDirectory(curPath);
                    }
                    if (i < paths.Length - 1)
                    {
                        curPath += @"\";
                    }
                }
            }
            catch (Exception ex)
            {
                if (FTPClient.IsConnected)
                {
                    Disconnect();
                }
                Record.Error(string.Format("FTP伺服器創建目錄失敗，原因：{0}", ex.ToString()));
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 刪除文件，需要分成資料夾跟檔案
        /// </summary>
        /// <param name="remoteFilePath"></param>
        public void DeleteDocument(string remoteFilePath)
        {
            try
            {
                Connect();
                string directoryPath = Path.GetDirectoryName(remoteFilePath);
                FtpListItem[] files = FTPClient.GetListing(directoryPath);

                //找到確切文件位置並利用FtpListItem的屬性確定該文件為檔案還是資料夾
                string comparisonPath = "/" + remoteFilePath.Replace(@"\", "/");
                foreach (FtpListItem file in files)
                {
                    if (file.FullName == comparisonPath && file.Type == FtpObjectType.File)
                    {
                        FTPClient.DeleteFile(remoteFilePath);
                    }
                    else if (file.FullName == comparisonPath && file.Type == FtpObjectType.Directory)
                    {
                        FTPClient.DeleteDirectory(remoteFilePath);
                    }
                }
            }
            catch (Exception exception)
            {
                Record.Error(string.Format("FTP伺服器刪除文件失敗，原因：{0}", exception.Message));
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// FTP伺服器文件改名
        /// </summary>
        /// <param name="oldRemotePath">舊遠端路徑</param>
        /// <param name="newRemotePath">新遠端路徑</param>
        public void Rename(string oldRemotePath, string newRemotePath)
        {
            try
            {
                Connect();
                FTPClient.Rename(oldRemotePath, newRemotePath);
            }
            catch (Exception ex)
            {
                Record.Error(string.Format("FTP伺服器文件改名失敗，原因：{0}", ex.ToString()));
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 取得FTP伺服器根路徑下包含所有子資料夾下的檔案路徑清單
        /// </summary>
        /// <returns></returns>
        public List<string> GetFilePaths()
        {
            List<string> files = new List<string>();
            Connect();
            ExamineDirectory("\\", ref files);
            Disconnect();
            return files;
        }

        /// <summary>
        /// 遍歷搜尋路徑下包含所有子資料夾的檔案並把各檔案路徑記錄至files裡
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="files"></param>
        private void ExamineDirectory(string rootPath, ref List<string> files)
        {
            foreach (FtpListItem FTPFile in FTPClient.GetListing(rootPath))
            {
                if (FTPFile.Type == FtpObjectType.Directory)
                {
                    ExamineDirectory(FTPFile.FullName, ref files);
                }
                else
                {
                    //  /A/A1/A11.txt -> A\A1\A11.txt
                    files.Add(FTPFile.FullName.Substring(1).Replace("/", "\\"));
                }
            }
        }
    }
}
