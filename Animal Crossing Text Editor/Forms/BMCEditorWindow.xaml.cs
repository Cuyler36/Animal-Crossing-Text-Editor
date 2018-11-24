using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Animal_Crossing_Text_Editor
{
    /// <summary>
    /// Interaction logic for BMCEditorWindow.xaml
    /// </summary>
    public partial class BMCEditorWindow : Window
    {
        public BMC BMC_File { get; private set; }
        private int SelectedIndex = -1;
        private bool ChangingColor = false;
        private bool FinishedAddingColors = false;

        public BMCEditorWindow()
        {
            InitializeComponent();
        }

        private void AddListBoxItem(uint color, int i)
        {
            var Panel = new DockPanel
            {
                LastChildFill = false
            };

            var Index = new Label
            {
                Content = i,
                Width = 30
            };

            var ColorPreview = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb((byte)(color >> 24), (byte)color, (byte)(color >> 8), (byte)(color >> 16))),
                Tag = color,
                Width = 30,
                Height = 20,
                Margin = new Thickness(10, 0, 0, 0)
            };

            Panel.Children.Add(Index);
            Panel.Children.Add(ColorPreview);

            listBox.Items.Add(Panel);
        }

        public void Show(BMC bmcFile)
        {
            BMC_File = bmcFile;
            listBox.Items.Clear();

            int i = 0;
            foreach (uint color in BMC_File.CLT_Section.Items)
            {
                AddListBoxItem(color, i);
                i++;
            }

            base.Show();
            FinishedAddingColors = true;
        }

        private void colorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!ChangingColor && e.NewValue != null && SelectedIndex > -1 && SelectedIndex < listBox.Items.Count)
            {
                var Item = listBox.Items[SelectedIndex] as DockPanel;
                var ColorCanvas = Item.Children[1] as Canvas;
                ColorCanvas.Tag = (uint)((e.NewValue.Value.A << 24) | (e.NewValue.Value.B << 16) | (e.NewValue.Value.G << 8) | e.NewValue.Value.R);
                ColorCanvas.Background = new SolidColorBrush((Color)e.NewValue);
            }
            ChangingColor = false;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FinishedAddingColors && listBox.SelectedItem != null)
            {
                ChangingColor = true;
                var color = (uint)((Canvas)((DockPanel)listBox.SelectedItem).Children[1]).Tag;
                colorCanvas.SelectedColor = Color.FromArgb((byte)(color >> 24), (byte)color, (byte)(color >> 8), (byte)(color >> 16));
                SelectedIndex = listBox.Items.IndexOf(listBox.SelectedItem);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (BMC_File != null)
            {
                BMC_File.Length += 4;
                BMC_File.CLT_Section.Length += 4;
                BMC_File.CLT_Section.Entries++;
                Array.Resize(ref BMC_File.CLT_Section.Items, BMC_File.CLT_Section.Items.Length + 1);
                BMC_File.CLT_Section.Items[BMC_File.CLT_Section.Items.Length - 1] = 0xFFFFFFFF;
                AddListBoxItem(0xFFFFFFFF, BMC_File.CLT_Section.Items.Length - 1);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (BMC_File != null && BMC_File.CLT_Section.Entries > 0 && SelectedIndex > -1)
            {
                BMC_File.Length -= 4;
                BMC_File.CLT_Section.Length -= 4;
                BMC_File.CLT_Section.Entries--;
                for (int i = SelectedIndex + 1; i < BMC_File.CLT_Section.Items.Length; i++)
                {
                    BMC_File.CLT_Section.Items[i - 1] = BMC_File.CLT_Section.Items[i];
                    var uiElementCollection = (listBox.Items[i] as DockPanel)?.Children;
                    if (uiElementCollection != null && uiElementCollection.Count > 0)
                        ((Label) ((DockPanel) listBox.Items[i])?.Children[0]).Content = i - 1;
                }

                listBox.Items.RemoveAt(SelectedIndex);
                listBox.SelectedIndex = SelectedIndex;
                Array.Resize(ref BMC_File.CLT_Section.Items, BMC_File.CLT_Section.Items.Length - 1);
            }
        }
    }
}
