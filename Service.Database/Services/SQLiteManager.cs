using Prism.Mvvm;
using Service.Database.Models;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Database.Services
{
    public class SQLiteManager : BindableBase, ISQLiteManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private SQLiteConnection _dbConnector;
        private DataBaseSetting _dbSetting;
        private bool _isConnection = false;
        
        public bool IsOpen { get => _isConnection; set => SetProperty(ref _isConnection, value); }

        public event EventHandler<string> ExecuteCommandComplete;

        public SQLiteManager()
        {
            
        }
        public void Initialize(ISettingManager sm)
        {
            try
            {
                _dbSetting = sm.AppSetting.DataBaseSetting;

                CreateDatabase();

                _dbConnector = new SQLiteConnection("Data Source=" + _dbSetting.DBPath + ";Version=3;");

                OpenDatabase();
                CreateTable();
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        public void OpenDatabase()
        {
            try
            {
                _dbConnector.Open();
                IsOpen = true;

                ExecuteCommandComplete?.Invoke(this, "Database open complete");
                _logWrite.Info("Database open complete");
            }
            catch (Exception err)
            {
                _dbConnector.Close();

                _logWrite.Error(err);
                IsOpen = false;

                ExecuteCommandComplete?.Invoke(this, "Database close complete");
                _logWrite.Info("Database close complete");
            }
        }

        public void CreateTable()
        {
            try
            {
                if (!IsOpen) return;

                bool isResult = TableExistence();
                if (isResult)
                {
                    ExecuteCommandComplete?.Invoke(this, "Already created table");
                    return;
                }

                StringBuilder sb = new StringBuilder(4096);
                sb.Append($"CREATE TABLE {_dbSetting.TableName} (");
                sb.Append("DATE_TIME TEXT");
                sb.Append(", PARCEL_BARCODE TEXT");
                sb.Append(", PRODUCT_BARCODE TEXT");
                sb.Append(", IMAGE_PATH TEXT);");

                int result = ExecuteQuery(sb.ToString());
                ExecuteCommandComplete?.Invoke(this, "Affected rows : " + result.ToString());
            }
            catch (Exception err)
            {
                ExecuteCommandComplete?.Invoke(this, "Create table error");
                _logWrite?.Error(err, false, false);
            }
            
        }

        public void DeleteTable()
        {
            try
            {
                if (!IsOpen) return;

                bool isResult = TableExistence();
                if (!isResult)
                {
                    ExecuteCommandComplete?.Invoke(this, "Have not table to delete");
                    return;
                }

                string query = $"DROP TABLE {_dbSetting.TableName}";
                ExecuteNonQuery(query);
                ExecuteCommandComplete?.Invoke(this, "Table drop complete");
            }
            catch (Exception err)
            {
                ExecuteCommandComplete?.Invoke(this, "Create table error");
                _logWrite?.Error(err, false, false);
            }
            
        }

        public void InsertData(RecordData data)
        {
            try
            {
                if (!IsOpen) return;

                bool isResult = TableExistence();
                if (!isResult)
                {
                    ExecuteCommandComplete?.Invoke(this, "Table not found");
                    return;
                }

                string strDateTime = data.DateTime.ToString("yyyy.MM.dd HH:mm:ss");

                StringBuilder sb = new StringBuilder(4096);
                sb.Append($"INSERT INTO {_dbSetting.TableName} ");
                sb.Append("(");
                sb.Append("DATE_TIME");
                sb.Append(", PARCEL_BARCODE");
                sb.Append(", PRODUCT_BARCODE");
                sb.Append(", IMAGE_PATH ");
                sb.Append(") ");
                sb.Append("VALUES ");
                sb.Append("(");
                sb.Append($"'{strDateTime}'");
                sb.Append($", '{data.ParcelBarcode}'");
                sb.Append($", '{data.ProductBarcode}'");
                sb.Append($", '{data.ImagePath}'");
                sb.Append(" );");

                ExecuteNonQuery(sb.ToString());
            }
            catch (Exception err)
            {
                ExecuteCommandComplete?.Invoke(this, "Data insert error");
                _logWrite?.Error(err, false, false);
            }
        }

        public DataTable Search(string parcelBarcode, string productBarcode, DateTime startTime, DateTime endTime)
        {
            try
            {
                if (!IsOpen) return null;

                bool isResult = TableExistence();
                if (!isResult)
                {
                    ExecuteCommandComplete?.Invoke(this, "Table not found");
                    return null;
                }

                string start = startTime.ToString("yyyy.MM.dd HH:mm:ss");
                string end = endTime.ToString("yyyy.MM.dd HH:mm:ss");

                StringBuilder sb = new StringBuilder(4096);
                sb.Append($"SELECT * FROM {_dbSetting.TableName} WHERE 1=1");

                if (!string.IsNullOrEmpty(parcelBarcode))
                    sb.Append($" AND PARCEL_BARCODE = {parcelBarcode}");

                if (!string.IsNullOrEmpty(productBarcode))
                    sb.Append($" AND PRODUCT_BARCODE = {productBarcode}");

                sb.Append(" AND DATE_TIME BETWEEN '" + start + "' AND '" + end + "' ");

                DataTable dt = ExecuteSearch(sb.ToString());
                
                ExecuteCommandComplete?.Invoke(this, "Data Search Complete");
                return dt;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public ConnectionState CheckDBState()
        {
            return _dbConnector.State;
        }

        public void Reconnect()
        {
            try
            {
                if (IsOpen)
                    Dispose();

                CreateDatabase();

                _dbConnector = new SQLiteConnection("Data Source=" + _dbSetting.DBPath + ";Version=3;");

                OpenDatabase();
                CreateTable();
            }
            catch (Exception err)
            {
                IsOpen = false;
                _logWrite.Error(err);
            }
            
        }

        public void ExportCSV(DataTable dt)
        {
            if (!IsOpen) return;

            Directory.CreateDirectory(_dbSetting.CSVPath);

            ThreadPool.QueueUserWorkItem(WriteCSV, dt);
        }

        private void WriteCSV(object data)
        {
            DataTable dt = (DataTable)data;

            string csvPath = Path.Combine(_dbSetting.CSVPath, $"History_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.csv");
            using (FileStream fs = new FileStream(csvPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    // 컬럼 표시
                    string line = string.Join(",", dt.Columns.Cast<object>());
                    sw.WriteLine(line);

                    // 데이터 쓰기
                    foreach (DataRow row in dt.Rows)
                    {
                        //TODO : 엑셀에서 0000 -> 0으로 표시되는 것을 방지
                        line = string.Join(",", row.ItemArray.Cast<object>().Select(r => $"=\"{r}\""));
                        sw.WriteLine(line);
                    }
                }
            }

            ExecuteCommandComplete?.Invoke(this, "Complete Export CSV");
        }

        public void Dispose()
        {
            if (IsOpen) _dbConnector.Close();
            _dbConnector.Dispose();
            IsOpen = false;
        }

        private void CreateDatabase()
        {
            if (!File.Exists(_dbSetting.DBPath))
            {
                SQLiteConnection.CreateFile(_dbSetting.DBPath);
                ExecuteCommandComplete?.Invoke(this, "Create database complete");
                _logWrite.Info("Create database complete");
            }
        }

        private bool TableExistence()
        {
            string query = $"SELECT COUNT(*) FROM sqlite_master WHERE name = '{_dbSetting.TableName}'";
            int result = ExecuteQuery(query);

            if (result == 0) return false;
            else return true;
        }

        private int ExecuteQuery(string query)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(query, _dbConnector))
            {
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        private void ExecuteNonQuery(string query)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(query, _dbConnector))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private DataTable ExecuteSearch(string query) 
        {
            using(SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, _dbConnector))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
