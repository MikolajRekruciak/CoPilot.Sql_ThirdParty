using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql_ThirdParty
{
    public class InitConfig
    {
        public string AppName { get; set; } = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public string Username { get; set; }
        public string Password { get; set; }
        public string CustomConnectionStringName { get; set; } = "SQL";
        public string ConnectionString { get; set; }
        public int LogRetentionDays { get; set; } = 30;
        public string LogTableName { get; set; } = "CDN._IM_MR_SqlLogs"; // Dodano parametr nazwy tabeli
    }

}
