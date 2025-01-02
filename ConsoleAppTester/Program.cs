using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SqlConnectionStringBuilder builder =
                        new SqlConnectionStringBuilder();

            builder.Password = "KindybalApokalipsy123";
            builder.UserID = "user";
            builder.DataSource = "DESKTOP-MED7G5Q";
            builder.InitialCatalog = "XL_Demo";

            Sql_ThirdParty.Initialize.Init(new Sql_ThirdParty.InitConfig
            {
                AppName = "TEST",
                ConnectionString = builder.ConnectionString,
            });


            Sql_ThirdParty.Initialize.Init(new Sql_ThirdParty.InitConfig
            {
                AppName = "TEST",
                ConnectionString = builder.ConnectionString,
            });
        }
    }
}
