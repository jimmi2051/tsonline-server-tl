using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace TS_Server
{
    class TSMysqlConnection
    {
        public MySqlConnection connection;
        public static string server = "";
        public static string database = "";
        public static string uid = "";
        public static string password = "";
        public static string port = "";


        public TSMysqlConnection()
        {
            loadTxt("dbconfig.txt");
            connection = new MySqlConnection("SERVER=" + server + ";PORT="+port+";DATABASE=" + database + ";UID=" + uid + ";PASSWORD=" + password + ";");
        }

        public static void loadTxt(string input)
        {
            FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read);
            StreamReader s = new StreamReader(fs);
            s.ReadLine();
            while (!s.EndOfStream)
            {
                string str = s.ReadLine();
                string[] data = str.Split('\t');
                switch (data[0])
                {
                    case "server":
                        server = data[1];
                        break;
                    case "database":
                        database = data[1];
                        break;
                    case "uid":
                        uid = data[1];
                        break;
                    case "password":
                        password = data[1];
                        break;
                    case "port":
                        port = data[1];
                        break;
                }
            }
            s.Close(); fs.Close();
        }

        public bool updateQuery(string query)
        {
            try
            {
                connection.Open();
                new MySqlCommand(query, connection).ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public MySqlDataReader selectQuery(string query)
        {
            MySqlDataReader data;
            try
            {
                connection.Open();
                data = new MySqlCommand(query, connection).ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            return data;
        }

        public int getLastId(string table)
        {
            int ret;
            string query = "SELECT LAST_INSERT_ID()";
            try
            {
                connection.Open();
                ret = Convert.ToInt32(new MySqlCommand(query, connection).ExecuteScalar());
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
            return ret;

        }

    }
}
