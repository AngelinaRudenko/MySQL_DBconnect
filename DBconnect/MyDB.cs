﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace DBconnect
{
    class MyDB
    {
        private MySqlConnection connection;

        public MyDB(string serverName, string port, string username, string password, string databaseName)
        { 
            connection = new MySqlConnection($"server={serverName}; " +
                $"port={port}; username={username}; password={password}; database={databaseName}");
            OpenConnection();
        }

        public MyDB(string username, string password, string databaseName)
        {
            connection = new MySqlConnection($"server=localhost; port=3306; username={username}; password={password}; database={databaseName}");
            OpenConnection();
        }

        public MyDB(string username, string password)
        {
            connection = new MySqlConnection($"server=localhost; port=3306; username={username}; password={password}");
            OpenConnection();
        }

        ~MyDB()
        {
            CloseConnection();
        }

        private void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        private void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
        public List<string> GetDatabases()
        {   
            return Select("show databases"); 
        }

        public List<string> GetTables()
        {     
            return Select("show tables");
        }

        public void SelectTable(string query, DataGridView table, ComboBox comboBox)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                using (MySqlDataReader dataReader = command.ExecuteReader())
                {                  
                    table.Rows.Clear();
                    table.Refresh();
                    comboBox.Items.Clear();
                    comboBox.Refresh();
                    table.ColumnCount = dataReader.FieldCount;
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        table.Columns[i].HeaderText = dataReader.GetName(i);
                        comboBox.Items.Add(dataReader.GetName(i));
                    }
                    comboBox.SelectedIndex = 0;              

                    while (dataReader.Read())
                    {
                        int j = table.RowCount;
                        table.Rows.Add();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            table[i, j].Value = dataReader[i].ToString();
                        }           
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить запрос\n//"+ ex.Message);
            }
        }

        public List<string> Select(string query)
        {
            List<string> returned = new List<string>();
            try
            {              
                MySqlCommand command = new MySqlCommand(query, connection);
                using (MySqlDataReader dataReader = command.ExecuteReader())
                {                   
                    while (dataReader.Read())
                    {
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            returned.Add(dataReader[i].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить запрос\n//" + ex.Message);
            }
            return returned;
        }

        public void NonQuery(string query)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить запрос\n//" + ex.Message);
            }
        }

        public string GetPrimaryKeyName(string db_name, string table_name)
        {
            string query = $"SELECT `COLUMN_NAME` FROM `information_schema`.`COLUMNS` WHERE (`TABLE_SCHEMA` = '{db_name}') AND (`TABLE_NAME` = '{table_name}') AND (`COLUMN_KEY` = 'PRI')";
            string result = "";
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                using (MySqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            result += dataReader[i].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить запрос\n//" + ex.Message);
            }
            return result;
        }
    }
}
