using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataShow;

public partial class DialogDeleteStructuralObjects
{
    public bool deleteFlag;
    public DialogDeleteStructuralObjects(bool delete)
    {
        InitializeComponent();
        this.deleteFlag = delete;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        deleteFlag = false;
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        deleteFlag = false;
        Close();
    }
}