using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class EigensolutionsShow
    {
        private readonly FeModel model;

        public EigensolutionsShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
        }

        private void EigenvaluesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eigenfrequencies = new Dictionary<int, double>();
            var nStates = model.Eigenstate.NumberOfStates;
            for (var k = 0; k < nStates; k++)
            {
                var value = Math.Sqrt(model.Eigenstate.Eigenvalues[k]) / 2 / Math.PI;
                eigenfrequencies.Add(k, value);
            }

            EigenvaluesGrid = sender as DataGrid;
            if (EigenvaluesGrid != null) EigenvaluesGrid.ItemsSource = eigenfrequencies;
        }

        private void EigenvectorsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eienvectorsGrid = new Dictionary<string, string>();
            var dimension = model.Eigenstate.Eigenvectors[0].Length;
            var i = 0;
            for (var j = 0; j < dimension; j++)
            {
                var line = model.Eigenstate.Eigenvectors[0][i].ToString("N5");
                for (var k = 1; k < model.Eigenstate.NumberOfStates; k++)
                    line += "\t" + model.Eigenstate.Eigenvectors[k][i].ToString("N5");
                eienvectorsGrid.Add(j.ToString(), line);
                i++;
            }

            EigenvectorsGrid = sender as DataGrid;
            if (EigenvectorsGrid != null) EigenvectorsGrid.ItemsSource = eienvectorsGrid;
        }
    }
}