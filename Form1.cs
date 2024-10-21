using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace winforms_Laba2
{
    public partial class Form1 : Form
    {
        public BindingSource Data { get; set; }
        public Dictionary<string, BindingSource> Tables { get; set; }

        public Form1()
        {
            InitializeComponent();

            Data = new BindingSource();
            Data.Add(new Data() { X = 0, Y = 0 });
            Data.Add(new Data() { X = 1, Y = 1 });
            Data.Add(new Data() { X = 2, Y = 4 });

            Tables = new Dictionary<string, BindingSource>();
            Tables.Add("Default Table", Data);

            dataGridView1.DataSource = Data;
            tableComboBox.Items.Add("Default Table");
            tableComboBox.SelectedIndex = 0;

            Data.ListChanged += Data_ListChanged;
            UpdateChart();
        }

        private void Data_ListChanged(object sender, ListChangedEventArgs e)
        {
            UpdateChart();
        }

        private void TableComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tableComboBox.SelectedItem != null && Tables.ContainsKey(tableComboBox.SelectedItem.ToString()))
            {
                Data = Tables[tableComboBox.SelectedItem.ToString()];
                dataGridView1.DataSource = Data;
                Data.ListChanged += Data_ListChanged;
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            if (graphPanel != null && Data != null)
            {
                var graphData = Data.Cast<Data>().ToList();
                string tableName = tableComboBox.SelectedItem.ToString();
                graphPanel.AddDataset(tableName, graphData);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            Data.Add(new Data() { X = 4, Y = 16 });
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    foreach (Data data in Data)
                    {
                        writer.WriteLine($"{data.X},{data.Y}");
                    }
                }
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = Path.GetFileName(openFileDialog.FileName);
                if (!Tables.ContainsKey(fileName))
                {
                    var newData = new BindingSource();
                    List<Data> graphData = new List<Data>();

                    using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var values = line.Split(',');
                            if (values.Length == 2 && double.TryParse(values[0], out double x) && double.TryParse(values[1], out double y))
                            {
                                newData.Add(new Data() { X = x, Y = y });
                                graphData.Add(new Data() { X = x, Y = y });
                            }
                        }
                    }

                    Tables.Add(fileName, newData);
                    tableComboBox.Items.Add(fileName);
                    graphPanel.AddDataset(fileName, graphData);
                }
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                graphPanel.SetGraphType(GraphPanel.GraphType.Line);
            }
            else
            {
                graphPanel.SetGraphType(GraphPanel.GraphType.Spline);
            }

            UpdateChart();
        }
    }

    public class Data
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
