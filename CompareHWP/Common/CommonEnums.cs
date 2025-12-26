using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareHWP.Common
{
    public enum IOSMessageBoxIcon
    {
        None,
        Info,
        Warning,
        Success,
        Question   // ❓ 추가
    }

    public enum eScreenDiv
    {
        Main = 1,
        OCS = 2,
        DBSeparator = 3,
        DBBackupRestore = 4,
        LabelPrint,
    }
}
