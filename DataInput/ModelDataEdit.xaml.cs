using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace FE_Analysis.DataInput
{
    public partial class ModelDataEdit
    {
        public ModelDataEdit()
        {
            InitializeComponent();
            var openFileDialog = new OpenFileDialog { Filter = "Input Files (*.inp)|*.inp" };
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
        }

        public ModelDataEdit(string path)
        {
            InitializeComponent();
            txtEditor.Text = File.ReadAllText(path);
        }

        private void BtnOpenFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Input Files (*.inp)|*.inp" };
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "Input Files (*.inp)|*.inp" };
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
        }
    }
}