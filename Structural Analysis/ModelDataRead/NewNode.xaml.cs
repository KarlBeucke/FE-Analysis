﻿using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewNode
    {
        private readonly FeModel model;

        public NewNode()
        {
            InitializeComponent();
        }

        public NewNode(FeModel model, int ndof)
        {
            InitializeComponent();
            this.model = model;
            NodeId.Text = "";
            NumberDof.Text = ndof.ToString("0");
            X.Text = "";
            Y.Text = "";
            Z.Text = "";
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var dimension = model.SpatialDimension;
            var knotenId = NodeId.Text;
            var numberNodalDof = int.Parse(NumberDof.Text);
            var crds = new double[dimension];
            if (X.Text.Length > 0) crds[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) crds[1] = double.Parse(Y.Text);
            if (NodeId.Text.Length > 0)
            {
                var knoten = new Node(knotenId, crds, numberNodalDof, dimension);
                model.Nodes.Add(knotenId, knoten);
            }

            Close();
        }
    }
}