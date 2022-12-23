using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataShow;

public partial class DialogueDeleteHeatObjects
{
    public bool deleteFlag;

    public DialogueDeleteHeatObjects(bool delete)
    {
        deleteFlag = delete;
        InitializeComponent();
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