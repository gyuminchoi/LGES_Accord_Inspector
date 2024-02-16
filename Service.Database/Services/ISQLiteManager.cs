using Service.Database.Models;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Database.Services
{
    public interface ISQLiteManager : IDisposable
    {
        bool IsOpen { get; set; }
        event EventHandler<string> ExecuteCommandComplete;
        ConnectionState CheckDBState();
        void Reconnect();
        void Initialize(ISettingManager sm);
        void OpenDatabase();
        void CreateTable();
        void DeleteTable();
        void InsertData(RecordData data);
        DataTable Search(string parcelBarcode, string productBarcode, DateTime startTime, DateTime endTime);
        int SearchCount(string parcelBarcode, string productBarcode, DateTime startTime, DateTime endTime);
        void ExportCSV(DataTable dt);
    }
}
