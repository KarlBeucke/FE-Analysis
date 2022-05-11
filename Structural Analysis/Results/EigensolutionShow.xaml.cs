using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class EigensolutionShow
    {
        private readonly FeModel model;

        public EigensolutionShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
        }

        private void EigenfrequenciesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eigenfrequencies = new Dictionary<int, string>();
            for (var k = 0; k < model.Eigenstate.NumberOfStates; k++)
            {
                var strFormat = $"{Math.Sqrt(model.Eigenstate.Eigenvalues[k]) / 2 / Math.PI,7:N3}";
                var sb = new StringBuilder(strFormat);
                eigenfrequencies.Add(k, sb.ToString());
            }

            EigenfrequenciesGrid = sender as DataGrid;
            if (EigenfrequenciesGrid != null) EigenfrequenciesGrid.ItemsSource = eigenfrequencies;
        }

        private void EigenvectorsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eigenvectors = new Dictionary<int, string>();
            for (var j = 0; j < model.Eigenstate.Eigenvectors[0].Length; j++)
            {
                var strFormat = $"{model.Eigenstate.Eigenvectors[0][j],15:N5}";
                var sb = new StringBuilder(strFormat);
                for (var k = 1; k < model.Eigenstate.NumberOfStates; k++)
                {
                    strFormat = $"{model.Eigenstate.Eigenvectors[k][j],15:N5}";
                    sb.Append(strFormat);
                }

                eigenvectors.Add(j, sb.ToString());
            }

            EigenvectorsGrid = sender as DataGrid;
            if (EigenvectorsGrid != null) EigenvectorsGrid.ItemsSource = eigenvectors;
        }
    }
}