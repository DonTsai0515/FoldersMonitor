# FoldersMonitor

* 目的:配合內容維護系統(CMS)開發的小工具，可以偵測本機資料夾內的檔案異動(新、刪、修)並藉由FTP檔案傳輸方式更新至伺服器  
以防檔案傳輸過程的檔案遺漏，附帶計時器定時比對本機及伺服器以確保兩邊檔案的一致性，如有相異會以Log形式紀錄

## 設定檔介紹

CustomFolderSettings.xml
---
|標籤名稱|	值|	標籤用途
|---|---|---|
|FolderEnabled|	true|	是否開啟監測功能|
|FolderFilter|	*.*|	設定監測的檔案類型 (*.txt* , *.jpg* , *.csv*) |
|FolderPath|	C:\FolderPath|	設定需要監測的資料夾路徑|
|FolderIncludeSub	|true|	路徑底下檔案及子資料夾是否都要監測|

## app.config
---
* 支援傳送至多台FTP Server

|屬性名稱	|屬性設定|
|---|---|
|Host|	FTP伺服器IP位置|
|Port|	埠號|
|UserName|	帳號|
|Password|	密碼|
|EncryptionMode|	FTP伺服器檔案傳輸加密協定|

### appSettings
|屬性名稱	|屬性設定|
|---|---|
|XMLFileFolderSettings|	CustomFolderSettings.xml 的檔案位置|
|CheckFileInterval(minute)|	比對檔案時間間距(分鐘)|

## NLog.config
---
* 設定錯誤紀錄存放位置

## 安裝 Windows Service
以系統管理員使用命令提示字元執行以下命令  
`cd C:\Windows\Microsoft.NET\Framework\v4.0.30319`  
`InstallUtil.exe **C:\FoldersMonitor\FoldersMonitor.exe**`  
請自行調整成執行檔的實體路徑

## 啟動 Windows Service
開始 -> 搜尋 “服務” -> “FoldersMonitor” 啟動服務

## 停止 Windows Service
開始 -> 搜尋 “服務” -> “FoldersMonitor” 停止服務

## 卸載 Windows Service
以系統管理員使用命令提示字元執行以下命令  
`cd C:\Windows\Microsoft.NET\Framework\v4.0.30319`  
`InstallUtil.exe -u **C:\FoldersMonitor\FoldersMonitor.exe**`  
請自行調整成執行檔的實體路徑

---
* Log 檔案分類介紹  

|類型|	內容|
|---|---|
|Trace|	日常操作紀錄|
|Warn|	定時檔案比對，圖檔數量異常紀錄|
|Error|	連線FTPS、檔案傳輸失敗等嚴重錯誤|




