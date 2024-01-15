using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class DataBaseSetting : BindableBase
    {
        private string _dbPath;
        private string _tableName;
        private string _csvPath;

        public string DBPath { get => _dbPath; set => SetProperty(ref _dbPath, value); }
        public string TableName { get => _tableName; set => SetProperty(ref _tableName, value); }
        public string CSVPath { get => _csvPath; set => SetProperty(ref _csvPath, value); }
        public DataBaseSetting(string path, string tableName, string csvPath) 
        {
            DBPath = path;
            TableName = tableName;
            CSVPath = csvPath;
        }
    }
}
