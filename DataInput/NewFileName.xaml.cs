using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.DataInput
{
    public partial class NewFileName
    {
        public string fileName;

        public NewFileName()
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            fileName = FileName.Text;
            DialogResult = true;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}