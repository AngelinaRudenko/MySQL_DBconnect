using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DBconnect
{
    public partial class MainForm : Form
    {
        MyDB db;
        bool stateEdit;
        string[] original;

        public MainForm(string login, string password, Form parent)
        {
            InitializeComponent();

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
                MessageBox.Show("Неверный логин или пароль\n" + ex.Message);
                this.Close();
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (buttonOk.Text == "Ок") //добавление или редактирование
            {
                try
                {
                    if (original == null) //добавить
                    {
                        string query = $"insert into {comboBoxTables.SelectedItem}(";
                        for (int i = 0; i < dataGridView.ColumnCount; i++)
                        {
                            query += $"{dataGridView.Columns[i].HeaderText}, ";
                        }

                        query = query.Remove(query.Length - 2) + ") values (";

                        for (int i = 0; i < dataGridView.ColumnCount; i++)
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

                        query = query.Remove(query.Length - 1) + ")";
                        db.NonQuery(query);
                    }
                    else //редактировать
                    {
                        string query = $"update {comboBoxTables.SelectedItem} set ";

                        for (int i = 0; i < dataGridView.ColumnCount; i++)
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
                    db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView, comboBoxSearch);
                }
            }
            else //поиск
            {
                string query = $"select * from {comboBoxTables.SelectedItem} where {comboBoxSearch.SelectedItem} = ";
                if (textBox.Text.GetType() == typeof(string))
                {
                    db.SelectTable(query + $"'{textBox.Text}'", dataGridView, comboBoxSearch);
                }
                else
                {
                    db.SelectTable(query + $"{textBox.Text}", dataGridView, comboBoxSearch);
                }
                comboBoxSearch.Text = "";
            }
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            if (stateEdit)
            {
                EditState(false);
            }
            db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView, comboBoxSearch);
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
                db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView, comboBoxSearch);
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
            dataGridView.ReadOnly = !state;
            if (state == true)
            {
                buttonOk.Text = "Ок";
                buttonReload.Text = "Отмена";
            }
            else
            {
                buttonOk.Text = "Поиск";
                buttonReload.Text = "Обновить";
            }
        }

        private void comboBoxDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выбрать БД\n" + ex.Message);
            }
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            db.SelectTable($"select * from {comboBoxTables.SelectedItem}", dataGridView, comboBoxSearch);
            string s = db.GetPrimaryKeyName(comboBoxDatabases.SelectedItem.ToString(), comboBoxTables.SelectedItem.ToString());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}

