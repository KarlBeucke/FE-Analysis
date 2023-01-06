﻿using System.Linq;
using FEALibrary.Model;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class ElementKeys
{
    public ElementKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var elements = model.Elements.Select(item => item.Value).ToList();
        ElementKey.ItemsSource = elements;
    }
}