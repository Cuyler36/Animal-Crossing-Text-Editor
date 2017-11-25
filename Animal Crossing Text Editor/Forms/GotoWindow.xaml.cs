using System.Windows;

namespace Animal_Crossing_Text_Editor
{
    /// <summary>
    /// Interaction logic for GotoWindow.xaml
    /// </summary>
    public partial class GotoWindow : Window
    {
        public int GotoValue = 0;

        public GotoWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(GotoTextBox.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out int Hex))
            {
                GotoValue = Hex;
                DialogResult = true;
                Close();
            }
        }
    }
}
