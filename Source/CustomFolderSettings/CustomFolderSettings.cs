using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FoldersMonitor
{
    /// <summary>
    /// 此類定義文件系統觀察程序要監視的文件類型及其資料夾
    /// </summary>
    public class CustomFolderSettings
    {
        /// <summary>
        /// 文件類型/資料夾/001|002|003
        /// </summary>
        [XmlAttribute]
        public string FolderID { get; set; }

        /// <summary>
        /// 資料夾監視是否開啟
        /// </summary>
        [XmlElement]
        public bool FolderEnabled { get; set; }

        /// <summary>
        /// 資料夾文件描述
        /// </summary>
        [XmlElement]
        public string FolderDescription { get; set; }

        /// <summary>
        /// 欲監視的文件類型(ex: *.shp, *.*, Project00*.zip)
        /// </summary>
        [XmlElement]
        public string FolderFilter { get; set; }

        /// <summary>
        /// 資料夾路徑(i.e.: C:\Temp\Test )
        /// </summary>
        [XmlElement]
        public string FolderPath { get; set; }

        /// <summary>
        /// 子資料夾是否也被監視
        /// </summary>
        [XmlElement]
        public bool FolderIncludeSub { get; set; }

        /// <summary>
        /// 建構子
        /// </summary>       
        public CustomFolderSettings()
        {
        }
    }
}
