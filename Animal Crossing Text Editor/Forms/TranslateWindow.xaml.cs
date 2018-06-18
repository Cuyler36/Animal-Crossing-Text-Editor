using System.Windows;

namespace Animal_Crossing_Text_Editor.Forms
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TranslateWindow : Window
    {
        public string TranslationText
        {
            get => translationTextBox.Text;
            set
            {
                translationTextBox.Text = value;
            }
        }

        public TranslateWindow()
        {
            InitializeComponent();
        }
    }
}
