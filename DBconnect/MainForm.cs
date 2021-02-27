﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DBconnect
{
    public partial class MainForm : Form
    {
        MyDB db;
        Form parent;
        bool stateEdit;
        string[] original;

        public MainForm(string login, string password, Form parent)
        {
            InitializeComponent();
            this.parent = parent;

            try 
            {
                db = new MyDB(login, password);
                List<string> databases = db.GetDatabases();
                if (databases.Count > 0)
                {
                    comboBoxDatabases.Items.AddRange(databases.ToArray());
                    comboBoxDatabases.SelectedItem = comboBoxDatabases.Items[0];
                }
                this.Show();
                //parent.Close();
                //parent.Dispose();
                parent.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неверный логин или пароль\n"+ex.Message);
                this.Close();
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!stateEdit)
            {
                if (textBox.Text.Contains("use "))
                {
                    string[] query = textBox.Text.Split(' ');
                    for (int i = 0; i < comboBoxDatabases.Items.Count; i++)
                    {
                        if (comboBoxDatabases.Items[i].ToString() == query[1])
                        {
                            comboBoxDatabases.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else if (textBox.Text.Contains("select "))
                {
                    string[] query = textBox.Text.Split(' ');
                    for (int i = 0; i < query.Length; i++)
                    {
                        if (query[i] == "from" && query.Length > i + 1)
                        {
                            for (int j = 0; j < comboBoxTables.Items.Count; j++)
                            {
                                if (comboBoxTables.Items[j].ToString() == query[i + 1])
                                {
                                    comboBoxTables.SelectedIndex = j;
                                    db.SelectTable(textBox.Text, dataGridView);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    db.NonQuery(textBox.Text);
                    db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView);
                }
                textBox.Text = "";
            }
            else
            {
                try
                {
                    if (original == null)
                    {
                        string query = $"insert into {comboBoxTables.SelectedItem}(";
                        for (int i = 1; i < dataGridView.ColumnCount; i++)
                        {
                            query += $"{dataGridView.Columns[i].HeaderText}, ";
                        }
                        query = query.Remove(query.Length - 2);
                        query += ") values (";

                        for (int i = 1; i < dataGridView.ColumnCount; i++)
                        {
                            if (dataGridView[i, 0].Value.GetType() == typeof(string))
                            {
                                string value = dataGridView[i, 0].Value.ToString();
                                query += $"'{value}',";
                            }
                            else
                            {
                                double value = Convert.ToDouble(dataGridView[i, 0].Value.ToString());
                                query += $"{value},";
                            }
                        }
                        query = query.Remove(query.Length - 1);
                        query += ")";
                        db.NonQuery(query);
                    }
                    else
                    {
                        string query = $"update {comboBoxTables.SelectedItem} set ";
                        for (int i = 1; i < dataGridView.ColumnCount; i++)
                        {
                            query += $"{dataGridView.Columns[i].HeaderText} = ";
                            if (dataGridView[i, 0].Value.GetType() == typeof(string))
                            {
                                string value = dataGridView[i, 0].Value.ToString();
                                query += $"'{value}',";
                            }
                            else
                            {
                                double value = Convert.ToDouble(dataGridView[i, 0].Value.ToString());
                                query += $"{value},";
                            }
                        }
                        query = query.Remove(query.Length - 1);
                        string colName = dataGridView.Columns[0].HeaderText;
                        query += $" where {colName} = {Convert.ToInt32(original[0])}";
                        db.NonQuery(query);
                        Array.Clear(original, 0, original.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    EditState(false);
                }
                db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView);
            }
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            if (stateEdit)
            {
                EditState(false);
            }
            db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView);
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить строку?",
                "Удалить строку", MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Yes)
            {
                string query = $"delete from {comboBoxTables.SelectedItem} where ";
                int j = dataGridView.CurrentCell.RowIndex;
                for (int i = 0; i < dataGridView.ColumnCount; i++)
                {
                    string colName = dataGridView.Columns[i].HeaderText;
                    string value = dataGridView[i, j].Value.ToString();
                    query += $" {colName} = '{value}' and ";
                }
                query = query.Remove(query.Length - 5);
                db.NonQuery(query);
                db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            dataGridView.Rows.Add();
            stateEdit = true;
            EditState(true);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            original = new string[dataGridView.ColumnCount];
            int j = dataGridView.CurrentCell.RowIndex;
            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                original[i] = dataGridView[i, j].Value.ToString();
            }
            dataGridView.Rows.Clear();
            dataGridView.Rows.Add();
            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                dataGridView[i, 0].Value = original[i];
            }
            stateEdit = true;
            EditState(true);
        }

        private void EditState(bool state) //true - редактирование
        {
            buttonAdd.Visible = !state;
            buttonDelete.Visible = !state;
            buttonEdit.Visible = !state;
            textBox.Visible = !state;
            dataGridView.ReadOnly = !state;
            if (state == true)
            {
                buttonReload.Text = "Отмена";
            }
            else
            {
                buttonReload.Text = "Обновить";
            }
        }

        private void comboBoxDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            db.NonQuery($"use {comboBoxDatabases.SelectedItem}");
            comboBoxTables.Items.Clear();
            List<string> tables = db.GetTables();
            if (tables.Count > 0)
            {
                comboBoxTables.Items.AddRange(tables.ToArray());
                comboBoxTables.SelectedItem = comboBoxTables.Items[0];
            }
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();            
        }
    }
}