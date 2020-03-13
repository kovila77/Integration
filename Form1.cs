using Knapsack;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SYAP_prac_27._02._2020
{
    public partial class fKnapsack : Form
    {
        int takeColumnIndex;
        int massColumnIndex;
        int costsColumnIndex;
        int nameColumnIndex;
        int numberColumnIndex;

        public fKnapsack()
        {
            InitializeComponent();
            InitializeDgvItems();
            button1.Enabled = false;
            dgvItems.Tag = false;
            tbMaxMass.Tag = false;
            comboBox1.Tag = false;
            epMain.SetError(tbMaxMass, "Обязательное поле!");
        }
        private void InitializeDgvItems()
        {
            numberColumnIndex = dgvItems.Columns.Add("number", "№");
            dgvItems.Columns[numberColumnIndex].Width = 20;
            nameColumnIndex = dgvItems.Columns.Add("name", "Название предмета");
            massColumnIndex = dgvItems.Columns.Add("mass", "Вес");
            costsColumnIndex = dgvItems.Columns.Add("costs", "Стоимость");
            takeColumnIndex = dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "take",
                HeaderText = "Брать ли?",
                ReadOnly = true,
                Visible = false
            });
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var solver = ((ListBoxItem)comboBox1.SelectedItem).solver;
            int[] m, c;
            string answerText = "";
            getColumnsDGV(out m, out c);
            bool[] answer;
            try
            {
                answer = solver.Solve(Convert.ToInt32(tbMaxMass.Text), m, c);
            }
            catch (Exception f)
            {
                MessageBox.Show("Непредвиденная ошибка библиотеки: " + f.Message);
                return;
            }
            for (int i = 0; i < answer.Length; i++)
            {
                if (answer[i])
                {
                    dgvItems.Rows[i].Cells[takeColumnIndex].Value = "Берём";
                    answerText += $"{i + 1},";
                }
                else
                {
                    dgvItems.Rows[i].Cells[takeColumnIndex].Value = "";
                }
            }
            dgvItems.Columns[takeColumnIndex].Visible = true;
            if (answerText.Length > 1)
                answerText = answerText.Substring(0, answerText.Length - 1);
            MessageBox.Show("Нужно взять следующие предметы: " + answerText);
        }

        private void getColumnsDGV(out int[] mass, out int[] costs)
        {
            List<int> _mass = new List<int>();
            List<int> _costs = new List<int>();
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Cells[massColumnIndex].Value as string)
                       && string.IsNullOrWhiteSpace(row.Cells[costsColumnIndex].Value as string))
                { continue; }
                _mass.Add(Convert.ToInt32(row.Cells[massColumnIndex].Value));
                _costs.Add(Convert.ToInt32(row.Cells[costsColumnIndex].Value));
            }
            mass = _mass.ToArray();
            costs = _costs.ToArray();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) { return; }
            Type[] types = null;
            try
            {
                types = Assembly.LoadFrom(dlg.FileName).GetTypes();
            }
            catch (ReflectionTypeLoadException el)
            {
                string res = "Выбрана некорректная библиотека\n";

                Exception[] errors = el.LoaderExceptions;
                foreach (var er in errors)
                {
                    res += er.Message + '\n';
                }
                MessageBox.Show(res);
                return;
            }
            foreach (var t in types)
            {
                if (t.GetInterface("ISolver") == null)
                {
                    continue;
                }
                var constr = t.GetConstructor(new Type[0]);
                if (constr != null)
                {
                    var solver = constr.Invoke(new object[0]) as ISolver;
                    if (solver != null) comboBox1.Items.Add(new ListBoxItem(solver));
                    return;
                }
            }
            MessageBox.Show("В библиотеке нед подходящего класса!");
        }
        private void dgvItems_Validating(object sender, CancelEventArgs e) { Checkdgv(); }
        private void dgvItems_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
        }

        private void tbMaxMass_TextChanged(object sender, EventArgs e)
        {
            if (!tbMaxMass.Text.All(char.IsDigit))
            {
                epMain.SetError(tbMaxMass, "Должен содержать только цифры!");
                tbMaxMass.Tag = false;
            }
            else
            {
                if (string.IsNullOrEmpty(tbMaxMass.Text.Trim()))
                {
                    epMain.SetError(tbMaxMass, "Обязательное поле!");
                    tbMaxMass.Tag = false;
                }
                else
                {
                    epMain.SetError(tbMaxMass, "");
                    tbMaxMass.Tag = true;
                }
            }
            CheckButton();
        }

        private void tbMaxMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !(e.KeyChar == (char)Keys.Delete) && !(e.KeyChar == (char)Keys.Back)) e.Handled = true;
        }

        private void tbMaxMass_Validating(object sender, CancelEventArgs e) { }

        private void CheckButton()
        {
            button1.Enabled = (bool)dgvItems.Tag && (bool)tbMaxMass.Tag && (bool)comboBox1.Tag;
        }

        private void Checkdgv()
        {
            bool res = true;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Cells[massColumnIndex].Value as string)
                    && string.IsNullOrWhiteSpace(row.Cells[costsColumnIndex].Value as string)
                    && string.IsNullOrWhiteSpace(row.Cells[nameColumnIndex].Value as string))
                { continue; }
                if (string.IsNullOrWhiteSpace(row.Cells[massColumnIndex].Value as string))
                {
                    row.Cells[massColumnIndex].ErrorText = "Пустое значение!";
                    res = false;
                }
                if (string.IsNullOrWhiteSpace(row.Cells[costsColumnIndex].Value as string))
                {
                    row.Cells[costsColumnIndex].ErrorText = "Пустое значение!";
                    res = false;
                }
                if (!(row.Cells[massColumnIndex].ErrorText == "")
                    && !(row.Cells[costsColumnIndex].ErrorText == ""))
                {
                    res = false;
                }
            }
            if (dgvItems.Rows.Count <= 1)
            {
                dgvItems.Tag = false;
            }
            else
            {
                dgvItems.Tag = res;
            }
            dgvItems.Tag = res;
        }

        private void comboBox1_Validating(object sender, CancelEventArgs e)
        {
            comboBox1.Tag = !(comboBox1.SelectedItem == null);
            CheckButton();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Tag = !(comboBox1.SelectedItem == null);
            CheckButton();
        }

        private void dgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgvItems.Columns[takeColumnIndex].Visible = false;
            if (!(e.ColumnIndex == massColumnIndex || e.ColumnIndex == costsColumnIndex))
            {
                Checkdgv(); return;
            }
            DataGridViewCell cell = dgvItems[e.ColumnIndex, e.RowIndex];
            if (string.IsNullOrWhiteSpace((string)cell.Value))
            {
                cell.ErrorText = "Пустое значение!";
            }
            else
            {
                if ((cell.ColumnIndex == massColumnIndex || cell.ColumnIndex == costsColumnIndex) && !(((string)cell.Value).All(char.IsDigit)))
                {
                    cell.ErrorText = "Только числа!";
                }
                else
                {
                    cell.ErrorText = "";
                }
            }
            Checkdgv();
            CheckButton();
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            int badCount = 0;
            var dlg = new OpenFileDialog();
            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() != DialogResult.OK) { return; }
            try
            {
                using (var sr = new StreamReader(dlg.FileName))
                {
                    string str;
                    string[] data;
                    string name;
                    string mass;
                    string costs;
                    int trash;
                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            str = sr.ReadLine();
                            data = str.Split(' ');
                            name = data[0];
                            for (int i = 0; i < data.Length - 2; i++)
                            {
                                name += " " + data[i];
                            }
                            if (!int.TryParse(data[data.Length - 1], out trash) || !int.TryParse(data[data.Length - 2], out trash))
                            {
                                throw new Exception("Не парсится число");
                            }
                            mass = data[data.Length - 1];
                            costs = data[data.Length - 2];
                        }
                        catch (Exception el)
                        {
                            badCount++;
                            continue;
                        }
                        int indexLastRow = dgvItems.Rows.Add();
                        dgvItems.Rows[indexLastRow].Cells[massColumnIndex].Value = mass;
                        dgvItems.Rows[indexLastRow].Cells[costsColumnIndex].Value = costs;
                        dgvItems.Rows[indexLastRow].Cells[nameColumnIndex].Value = name;
                    }
                }
            }
            catch (Exception el)
            {
                MessageBox.Show($"Ошибка: {el.Message}");
                return;
            }
            MessageBox.Show($"Не прочитано строчек: {badCount}");
        }

        private void dgvItems_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < dgvItems.Rows.Count; i++)
            {
                dgvItems.Rows[i].Cells[numberColumnIndex].Value = i + 1;
            }
        }
    }

    class ListBoxItem
    {
        public ISolver solver;

        public ListBoxItem(ISolver solver)
        {
            this.solver = solver;
        }

        public override string ToString()
        {
            return solver.GetName();
        }
    }
}
