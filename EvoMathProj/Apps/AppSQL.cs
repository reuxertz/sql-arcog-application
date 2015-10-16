using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace EvoMathProj
{
    public class AppSQL : AppForm.AppBase
    {
        public class ConnectionString
        {
            public string UserID;
            public string Password;
            public string Server;
            public string Database;

            public string GetConnectionString()
            {
                var connectionString = "server=" + Server + ";" + "database=" +
                    Database + ";" + "userid=" + UserID + ";" + "password=" + Password + ";" +
                    "Connection Timeout=3";

                return connectionString;
            }

            public ConnectionString(string id, string pwd, string serv, string db)
            {
                this.UserID = id;
                this.Password = pwd;
                this.Server = serv;
                this.Database = db;
            }

        }

        //Fields
        protected Dictionary<string, ConnectionString> _dbStringMap = new Dictionary<string, ConnectionString>(); 

        //SQL Getters
        public List<string> SQL_GetDatabasesFromServer(string connectionString, bool alertUser)
        {
            var r = new List<string>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SHOW DATABASES;";
            MySqlDataReader Reader;

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                if (alertUser)
                {
                    MessageBox.Show(ex.Message, this._parentForm.Text);
                }
                connection.Close();
                return null;
            }
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string row = "";
                for (int i = 0; i < Reader.FieldCount; i++)
                    row += Reader.GetValue(i).ToString();
                r.Add(row);
            }
            connection.Close();
            return r;
        }


        //Getters
        public string GetConnectionStringFromDBName(string dbName)
        {
            return this._dbStringMap[dbName].GetConnectionString();
        }
        public string[] GetServersFromConnectionStrings()
        {
            var r = new List<string>();

            for (int i = 0; i < this._dbStringMap.Count; i++)
            {
                var ri = (from ConnectionString cs in this._dbStringMap.Values
                          where !r.Contains(cs.Server)
                          select cs.Server).ToArray<string>();

                r.AddRange(ri);
            }
            return r.ToArray<string>();
        }
        public List<string> GetConnectionStrings()
        {
            var v = this._dbStringMap.Values.ToArray<ConnectionString>();
            var r = new List<string>();
            for (int i = 0; i < v.Length; i++)
                r.Add(v[i].GetConnectionString());

            return r;
        }
        public string[] GetDatabaseFromConnectionStrings()
        {
            return this._dbStringMap.Keys.ToArray<string>();
        }
        
        //SQL Getters
        public List<string> GetColumnsInfoFromTable(bool nameOnly, string dbName, string table)
        {
            //Get SQL Columns and verify a fasta and sequence columne exist for blasting
            string connection = this._dbStringMap[dbName].GetConnectionString();
            DataTable td = null;
            List<String> Columnnames = new List<String>();
            using (var con = new MySqlConnection(connection))
            {
                using (var schemaCommand = new MySqlCommand("SHOW COLUMNS FROM " + table + ";", con))
                {
                    con.Open();
                    using (var reader = schemaCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                Columnnames.Add(reader.GetValue(i).ToString());
                        }
                    }
                }
            }

            var r = new List<string>();
            if (nameOnly)
                for (int i = 0; i < Columnnames.Count; i += 6)
                    r.Add(Columnnames[i]);
            else
                for (int i = 0; i < Columnnames.Count; i ++)
                    r.Add(Columnnames[i]);

            return r;
        }
        public List<string> GetTables(string dbName)
        {
            if (!this._dbStringMap.Keys.Contains(dbName))
                return null;

            List<String> Tablenames = new List<String>();
            var connection = new MySqlConnection(this._dbStringMap[dbName].GetConnectionString());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SHOW TABLES;";
            MySqlDataReader Reader;

            try
            {
                connection.Open();
            }
            catch
            {
                return null;
            }
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                for (int i = 0; i < Reader.FieldCount; i++)
                    Tablenames.Add(Reader.GetValue(i).ToString());
            }

            connection.Close();
            return Tablenames;
        }
        public int GetTotalRowsInTable(string dbName, string tableName)
        {
            if (dbName == null || tableName == null)
                return -1;

            using (var con = new MySqlConnection(this._dbStringMap[dbName].GetConnectionString()))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM " + tableName, con))
                {
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

        }
        public List<List<string>> GetRowsInTable(string dbName, string tableName, AppWorking appW)
        {
            return this.GetRowsInTable(dbName, tableName, null, null, appW);
        }
        public List<List<string>> GetRowsInTable(string dbName, string tableName, string queryField, string queryValue, AppWorking appW)
        {
            //SELECT * FROM pet WHERE species = 'snake' OR species = 'bird';
            var tr = -1.0;
            if (appW != null)
                tr = this.GetTotalRowsInTable(dbName, tableName) - 1;

            //Get SQL Columns and verify a fasta and sequence columne exist for blasting
            string connection = this._dbStringMap[dbName].GetConnectionString();
            List<List<string>> t = new List<List<string>>();
            t.Add(new List<string>());
            using (var con = new MySqlConnection(connection))
            {
                var x = "";

                if (queryField != null && queryValue != null)
                    x = " WHERE \"" + queryField + "\" = " + queryValue;

                string com = "SELECT * FROM " + tableName + x + ";";
                using (var schemaCommand = new MySqlCommand(com, con))
                {
                    con.Open();
                    using (var reader = schemaCommand.ExecuteReader())
                    {
                        var tc = 0;
                        while (reader.Read())
                        {
                            tc++;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var ti = reader.GetValue(i);
                                var s = ti.ToString();

                                if (s.ToLower().Contains("system.byte[]"))
                                {
                                    var tb = (byte[])ti;
                                    s = "";
                                    for (int j = 0; j < tb.Length; j++)
                                        s += Convert.ToChar(tb[j]);

                                }

                                bool newLine = false;
                                if (s.Length > 0 && s.Substring(s.Length - 1, 1) == "\r")
                                {
                                    newLine = true;
                                    s = s.Substring(0, s.Length - 1);
                                }

                                t[t.Count - 1].Add(s);

                                if (newLine)
                                    t.Add(new List<string>());
                            }

                            if (appW != null)
                                appW.AddProgressPercent(null, 1.0 / tr);
                        }
                    }
                }
            }

            //var r = new List<string>();
            //if (nameOnly)
                //for (int i = 0; i < Columnnames.Count; i += 6)
                //    r.Add(Columnnames[i]);
            //else
            //    for (int i = 0; i < t.Count; i++)
            //        r.Add(t[i]);

            for (int i = 0; i < t.Count; i++)
            {
                if (t[i].Count == 0)
                {
                    t.RemoveAt(i);
                    i--;
                }
            }

            return t;

        }
        //Setters
        public void RemoveDBConnectionString(string db)
        {
            this._dbStringMap.Remove(db);
        }
        public void AddDBConnectionString(string db, ConnectionString cs)
        {
            this._dbStringMap.Add(db, cs);
        }
        public void AddDatabase(string userID, string password, string server, string db)
        {
            /*if (conString.ToLower().Contains("connection timeout"))
            {
                int startI = conString.ToLower().IndexOf("connection timeout");
                int endI = conString.IndexOf(";", startI);

                if (endI != -1)
                    conString = conString.Remove(startI, endI - startI);
                else
                    conString = conString.Substring(0, startI);
            }*/

            if (this._dbStringMap.ContainsKey(db))
                return;

            var cs = new ConnectionString(userID, password, server, db);
            this.AddDBConnectionString(db, cs);
        }
        public void ClearConnections()
        {
            this._dbStringMap.Clear();
        }
        public void RefreshServers()
        {
            var r = new List<string>();
            DataTable table = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            foreach (DataRow server in table.Rows)
            {
                r.Add(server[table.Columns["ServerName"]].ToString());
            }

            return;
        }
        
        //Constructor
        public AppSQL(RichTextBox screen, AppForm a)
            : base(screen, a)
        {

        }

        //Functions
        public string TestConnection(string connectionString, bool addToServers)
        {
            var r = new string[] { null, null, null, null };
            var scs = connectionString.Split(';');
            for (int i = 0; i < scs.Length; i++)
            {
                var spl = scs[i].Split('=');

                if (spl[0].ToLower() == "server")
                    r[0] = spl[1];
                if (spl[0].ToLower() == "database")
                    r[1] = spl[1];
                if (spl[0].ToLower() == "userid")
                    r[2] = spl[1];
                if (spl[0].ToLower() == "password")
                    r[3] = spl[1];
            }

            for (int i = 0; i < r.Length; i++)
                if (r[i] == null)
                    return null;

            return this.TestConnection(r[0], r[1], r[2], r[3], addToServers);
        }
        public string TestConnection(string server, string db, string uid, string pwd, bool addToServers)
        {
            var connectionString = "server=" + server + ";" + "database=" +
                db + ";" + "userid=" + uid + ";" + "password=" + pwd + ";" +
                "Connection Timeout=3";

            var sqlConnection = new MySqlConnection(connectionString);
            var os = "";
            try
            {
                using (sqlConnection)
                {
                    sqlConnection.Open();
                    os += "User:\t\t" + uid + "\n";
                    if (sqlConnection.Database == "")
                        db = "(none selected)";

                    os += "Database:\t\t" + db + "\n";

                    os += "Server:\t\t" + server + "\n";
                    os += "ServerVersion:\t" + sqlConnection.ServerVersion + "\n";

                    os = "User " + uid + " successfully connected to database " + db + " on server " + server + ".\n\n" + os;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            try
            {
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                return null;
            }

            if (os != "" && addToServers)
                this.AddDatabase(uid, pwd, server, db);

            return os;
        }
    }
}
