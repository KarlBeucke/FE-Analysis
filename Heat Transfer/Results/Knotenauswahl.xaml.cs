using FEALibrary.Model;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.WärmeErgebnisse
{
    public partial class Knotenauswahl : Window
    {
        FEModel modell;
        string knotenId;
        public Node knoten;

        public Knotenauswahl(FEModel _modell)
        {
            modell = _modell;
            InitializeComponent();
            Auswahl.ItemsSource = modell.Knoten.Keys;
            Show();
        }
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            knotenId = (string)Auswahl.SelectedItem;
            if (modell.Knoten.TryGetValue(knotenId, out knoten)) { };
            Close();
        }
        private void KnotenErgebnisse(object sender, RoutedEventArgs e)
        {
            Wärmeberechnung.WärmeErgebnisse.InstationäreErgebnisse wärme
                            = new Wärmeberechnung.WärmeErgebnisse.InstationäreErgebnisse(modell, knoten, knotenId);
            wärme.Show();
        }
    }
}
