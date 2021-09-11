using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Data.SqlClient;
using igra80syncDB.Models;
using System.Data;

namespace igra80syncDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("refnum: {A}", args[0]);

            Program p = new Program();

            CaModel cam = p.GetCa(Int32.Parse(args[0]));

            p.SyncIgra80(cam);

            //Console.ReadLine();
        }

        public CaModel GetCa(int refnum)
        {
            string connString = @"Data Source=" + Settings.Default.serverName + ";Initial Catalog="
                        + Settings.Default.dbName + ";Persist Security Info=True;User ID=" + Settings.Default.userName + ";Password=" + Settings.Default.password;

            SqlConnection conn = new SqlConnection(connString);


            try
            {
                conn.Open();

                CaModel res = new CaModel();

                String sql = "SELECT Id, Msg FROM CaDummy WHERE Id=" + refnum;
                using (SqlCommand command = new SqlCommand(sql, conn))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0} {1}", reader.GetInt32(0), reader.GetString(1));
                            res.Id = reader.GetInt32(0);
                            res.Msg = reader.GetString(1);
                        }
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                Log.Debug("Error: {A}", e.Message);
                return null;
            }
        }

        public void SyncIgra80(CaModel cam)
        {
            string connString = @"Data Source=" + Settings.Default.serverName + ";Initial Catalog="
                        + Settings.Default.dbName + ";Persist Security Info=True;User ID=" + Settings.Default.userName + ";Password=" + Settings.Default.password;

            SqlConnection conn = new SqlConnection(connString);


            try
            {
                conn.Open();

                // Create a command object with parameters for stored procedure
                SqlCommand sqlCmd = new SqlCommand("[dbo].[SyncIgra80]", conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@Msg", SqlDbType.NVarChar).Value = cam.Msg;
                sqlCmd.Parameters.AddWithValue("@CaId", SqlDbType.Int).Value = cam.Id;

                // Execute the command and get the data in a data reader.
                SqlDataReader sqlDr = sqlCmd.ExecuteReader();

                while (sqlDr.Read())
                {
                    Console.WriteLine(sqlDr.ToString());
                }

                conn.Close();
                sqlDr.Close();

            }
            catch (Exception e)
            {
                Log.Debug("Error: {A}", e.Message);
            }
        }
    }
}
