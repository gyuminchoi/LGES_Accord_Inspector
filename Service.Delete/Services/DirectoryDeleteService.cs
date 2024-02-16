using Service.Logger.Services;
using Service.Pattern;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Delete.Services
{
    public class DirectoryDeleteService : LazySingleton<DirectoryDeleteService>
    {
        private ISettingManager _settingManager;
        private LogWrite _logWrite = LogWrite.Instance;

        private DirectoryDeleteService() { }

        public void Initialize(ISettingManager sm)
        {
            _settingManager = sm;
        }

        public void DeleteOldFolder()
        {
            string rootPath = _settingManager.AppSetting.ImageSetting.InspectionImageSavePath;
            string[] directories = Directory.GetDirectories(rootPath);
            DateTime dtNow = DateTime.Now;

            foreach (string directory in directories)
            {
                try
                {
                    string folderName = new DirectoryInfo(directory).Name;

                    if (!DateTime.TryParse(folderName, out DateTime dt))
                        continue;

                    bool flag = CalculateDateDifferences(dtNow, dt);
                    if (flag)
                        Directory.Delete(directory, true);
                }
                catch (Exception err)
                {
                    _logWrite.Error($"{directory} 경로의 폴더가 삭제되지 않았습니다.{Environment.NewLine}" + err.Message + Environment.NewLine + err.StackTrace, false, false);
                }
            }
        }

        private bool CalculateDateDifferences(DateTime dt1, DateTime dt2)
        {
            TimeSpan ts = dt1 - dt2;
            int intDay = Math.Abs((int)ts.TotalDays);
            return intDay > 7;
        }
    }
}
