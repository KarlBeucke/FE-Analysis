using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Linq;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class HeatloadKeys
{
    public HeatloadKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;

        var loads = new List<AbstractLoad>();
        var nodeloads = model.Loads.Where(item => item.Value is NodeLoad).
            Select(item => item.Value).ToList();
        loads.AddRange(nodeloads);
        var lineloads = model.LineLoads.Where(item => item.Value is LineLoad).
            Select(item => (AbstractLoad)item.Value).ToList();
        loads.AddRange(lineloads);
        var elementloads = model.ElementLoads.Where(item => item.Value is ElementLoad3).
            Select(item => (AbstractLoad)item.Value).ToList();
        loads.AddRange(elementloads);
        elementloads = model.ElementLoads.Where(item => item.Value is ElementLoad4).
            Select(item => (AbstractLoad)item.Value).ToList();
        loads.AddRange(elementloads);
        HeatloadKey.ItemsSource = loads;
    }
}