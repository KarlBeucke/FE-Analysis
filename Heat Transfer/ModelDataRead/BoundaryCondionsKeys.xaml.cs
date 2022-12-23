using System.Collections.Generic;
using System.Linq;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class BoundaryCondionsKeys
{
    public BoundaryCondionsKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;

        var boundaryConditions = new List<AbstractBoundaryCondition>();
        var temperatures = model.BoundaryConditions.
            Select(item => item.Value).ToList();
        boundaryConditions.AddRange(temperatures);
        var timeDependentTemperatures = model.TimeDependentBoundaryConditions.
            Select(item => item.Value).ToList();
        boundaryConditions.AddRange(timeDependentTemperatures);
        RandbedingungKey.ItemsSource = boundaryConditions;
    }
}