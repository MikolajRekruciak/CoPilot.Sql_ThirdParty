using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.Server;

namespace ConsoleAppTester
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //var sheet = Sheet();

            //string filePath = @"C:\Users\DELL\Downloads\Rezerwa_Fiber#1_202411_Capex v2_z.xlsx";

            //var bytes = File.ReadAllBytes(filePath);

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

            //string fileContent = File.ReadAllText(@"C:\Users\DELL\Downloads\Rezerwa_Fiber#1_202411_Capex v2_z.xlsx");

            //fileContent = fileContent + fileContent + fileContent + fileContent + fileContent;

            //Sql_ThirdParty.SqlHelper.LogToTable(filePath, bytes);

            string commandText = $@"SELECT Binaries FROM CDN._IM_MR_SqlLogs WHERE Binaries IS NOT NULL";
            byte[] fileContent = (byte[])Sql_ThirdParty.SqlHelper.ExecuteScalar(commandText);
            File.WriteAllBytes(@"C:\Users\DELL\Downloads\Success.xlsx", fileContent);
        }
    }
}
