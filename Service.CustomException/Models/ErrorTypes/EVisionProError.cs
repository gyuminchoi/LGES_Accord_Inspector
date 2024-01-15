using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EVisionProError
    {
        [Description("File Doesn`t Exist")]
        FileDoesNotExist
    }
}
