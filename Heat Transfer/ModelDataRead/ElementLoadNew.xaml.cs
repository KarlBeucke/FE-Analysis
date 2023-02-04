using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class ElementLoadNew
{
    private readonly FeModel model;
    private AbstractElementLoad existingElementload;
    private readonly HeatloadKeys loadKeys;

    public ElementLoadNew(FeModel model)
    {
        this.model = model;
        InitializeComponent();
        ElementLoadId.Text = string.Empty;
        loadKeys = new HeatloadKeys(model);
        loadKeys.Show(); Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementlastId = ElementLoadId.Text;
        if (elementlastId == "")
        {
            _ = MessageBox.Show("Elementlast Id muss definiert sein", "neue Elementlast");
            return;
        }

        // existing Elementload
        if (model.ElementLoads.Keys.Contains(elementlastId))
        {
            model.ElementLoads.TryGetValue(elementlastId, out existingElementload);
            Debug.Assert(existingElementload != null, nameof(existingElementload) + " != null");

            existingElementload.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            model.Elements.TryGetValue(existingElementload.ElementId, out var element);
            switch (element)
            {
                case Element2D3:
                    {
                        if (Node1.Text.Length > 0) existingElementload.Loadvalues[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) existingElementload.Loadvalues[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) existingElementload.Loadvalues[2] = double.Parse(Node3.Text);
                        break;
                    }
                case Element2D4:
                    {
                        if (Node1.Text.Length > 0) existingElementload.Loadvalues[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) existingElementload.Loadvalues[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) existingElementload.Loadvalues[2] = double.Parse(Node3.Text);
                        if (Node4.Text.Length > 0) existingElementload.Loadvalues[3] = double.Parse(Node4.Text);
                        break;
                    }
                case Element3D8:
                    {
                        if (Node1.Text.Length > 0) existingElementload.Loadvalues[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) existingElementload.Loadvalues[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) existingElementload.Loadvalues[2] = double.Parse(Node3.Text);
                        if (Node4.Text.Length > 0) existingElementload.Loadvalues[3] = double.Parse(Node4.Text);
                        if (Node5.Text.Length > 0) existingElementload.Loadvalues[4] = double.Parse(Node5.Text);
                        if (Node6.Text.Length > 0) existingElementload.Loadvalues[5] = double.Parse(Node6.Text);
                        if (Node7.Text.Length > 0) existingElementload.Loadvalues[6] = double.Parse(Node7.Text);
                        if (Node8.Text.Length > 0) existingElementload.Loadvalues[7] = double.Parse(Node8.Text);
                        break;
                    }
            }
        }
        // new Elementload
        else
        {
            var elementId = "";
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            model.Elements.TryGetValue(existingElementload.ElementId, out var element);
            switch (element)
            {
                case Element2D3:
                    {
                        var T = new double[3];
                        if (Node1.Text.Length > 0) T[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) T[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) T[2] = double.Parse(Node3.Text);
                        var elementload = new ElementLoad3(elementlastId, elementId, T);
                        model.ElementLoads.Add(elementlastId, elementload);
                        break;
                    }
                case Element2D4:
                    {
                        var T = new double[4];
                        if (Node1.Text.Length > 0) T[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) T[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) T[2] = double.Parse(Node3.Text);
                        if (Node4.Text.Length > 0) T[3] = double.Parse(Node4.Text);
                        var elementload = new ElementLoad4(elementlastId, elementId, T);
                        model.ElementLoads.Add(elementlastId, elementload);
                        break;
                    }
                case Element3D8:
                    {
                        var T = new double[8];
                        if (Node1.Text.Length > 0) T[0] = double.Parse(Node1.Text);
                        if (Node2.Text.Length > 0) T[1] = double.Parse(Node2.Text);
                        if (Node3.Text.Length > 0) T[2] = double.Parse(Node3.Text);
                        if (Node4.Text.Length > 0) T[3] = double.Parse(Node4.Text);
                        if (Node5.Text.Length > 0) T[4] = double.Parse(Node5.Text);
                        if (Node6.Text.Length > 0) T[5] = double.Parse(Node6.Text);
                        if (Node7.Text.Length > 0) T[6] = double.Parse(Node7.Text);
                        if (Node8.Text.Length > 0) T[7] = double.Parse(Node8.Text);
                        break;
                    }
            }

            loadKeys?.Close();
            Close();
            MainWindow.heatModelVisual.Close();
        }
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        loadKeys?.Close();
        Close();
    }

    private void ElementloadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.ElementLoads.ContainsKey(ElementLoadId.Text))
        {
            ElementId.Text = "";
            Node1.Text = "";
            Node2.Text = "";
            Node3.Text = "";
            Node4.Text = "";
            Node5.Text = "";
            Node6.Text = "";
            Node7.Text = "";
            Node8.Text = "";
            return;
        }

        // existing Elementload definition
        model.ElementLoads.TryGetValue(ElementLoadId.Text, out existingElementload);
        Debug.Assert(existingElementload != null, nameof(existingElementload) + " != null");
        ElementLoadId.Text = existingElementload.LoadId;
        ElementId.Text = existingElementload.ElementId;
        Node1.Text = existingElementload.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        Node2.Text = existingElementload.Loadvalues[1].ToString("G3", CultureInfo.CurrentCulture);
        switch (existingElementload)
        {
            case ElementLoad3:
                Node3.Text = existingElementload.Loadvalues[2].ToString("G3", CultureInfo.CurrentCulture);
                break;
            case ElementLoad4:
                Node3.Text = existingElementload.Loadvalues[2].ToString("G3", CultureInfo.CurrentCulture);
                Node4.Text = existingElementload.Loadvalues[3].ToString("G3", CultureInfo.CurrentCulture);
                break;
                //case ElementLast8:
                //    Node3.Text = existingElementload.Loadvalues[2].ToString("G3", CultureInfo.CurrentCulture);
                //    Node4.Text = existingElementload.Loadvalues[3].ToString("G3", CultureInfo.CurrentCulture);
                //    Node5.Text = existingElementload.Loadvalues[4].ToString("G3", CultureInfo.CurrentCulture);
                //    Node6.Text = existingElementload.Loadvalues[5].ToString("G3", CultureInfo.CurrentCulture);
                //    Node7.Text = existingElementload.Loadvalues[6].ToString("G3", CultureInfo.CurrentCulture);
                //    Node8.Text = existingElementload.Loadvalues[7].ToString("G3", CultureInfo.CurrentCulture);
                //    break;
        }
    }
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.ElementLoads.Keys.Contains(ElementLoadId.Text)) return;
        model.ElementLoads.Remove(ElementLoadId.Text);
        loadKeys?.Close();
        Close();
        MainWindow.heatModelVisual.Close();
    }
}