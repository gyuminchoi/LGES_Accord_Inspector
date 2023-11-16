using Service.Setting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Services
{
    public interface ISettingManager
    {
        /// <summary>
        /// SettingManager 클래스를 초기화 진행합니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Json파일을 읽어 값을 받아옵니다.
        /// </summary>
        void Deserialize();

        /// <summary>
        /// Json파일에 값을 저장합니다.
        /// </summary>
        void Serialize();

        AppSetting AppSetting { get; set; }
    }
}
