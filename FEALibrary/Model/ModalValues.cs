namespace FEALibrary.Model
{
    public class ModalValues
    {
        public double Damping { get; set; }
        public string Text { get; set; }

        public ModalValues(double value)
        {
            Damping = value;
        }
        public ModalValues(double value, string text)
        {
            Damping = value;
            Text = text;
        }
    }
}
