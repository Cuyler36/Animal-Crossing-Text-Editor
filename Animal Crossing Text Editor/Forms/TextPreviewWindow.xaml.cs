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

namespace Animal_Crossing_Text_Editor.Forms
{
    /// <summary>
    /// Interaction logic for TextPreviewWindow.xaml
    /// </summary>
    public partial class TextPreviewWindow : Window
    {
        private BitmapSource[] textPreviews;
        private int currentPreview;

        public BitmapSource[] TextPreviews
        {
            get => textPreviews;
            set
            {
                textPreviews = value;
                if (textPreviews != null)
                {
                    if (currentPreview >= textPreviews.Length)
                    {
                        CurrentPreview = 0;
                    }

                    SetButtonVisiblity();

                    if (currentPreview < textPreviews.Length && textPreviews[currentPreview] != null)
                    {
                        textPreviewImage.Source = textPreviews[currentPreview];
                        textPreviewImage.Height = textPreviews[currentPreview].PixelHeight;
                    }
                    else
                    {
                        textPreviewImage.Source = null;
                    }
                }
            }
        }

        // TODO: Fix buttons showing/not showing on textPreviews set.
        public int CurrentPreview
        {
            get => currentPreview;
            set
            {
                if (textPreviews != null)
                {
                    if (value < textPreviews.Length && value > -1)
                    {
                        currentPreview = value;
                        SetButtonVisiblity();

                        if (currentPreview < textPreviews.Length && textPreviews[currentPreview] != null)
                        {
                            textPreviewImage.Source = textPreviews[currentPreview];
                            textPreviewImage.Height = textPreviews[currentPreview].PixelHeight;
                        }
                        else
                        {
                            textPreviewImage.Source = null;
                        }
                    }
                }
            }
        }

        private void SetButtonVisiblity()
        {
            if (currentPreview + 1 == textPreviews.Length)
            {
                nextButton.Visibility = Visibility.Hidden;
            }
            else
            {
                nextButton.Visibility = Visibility.Visible;
            }

            if (currentPreview <= 0)
            {
                prevButton.Visibility = Visibility.Hidden;
            }
            else
            {
                prevButton.Visibility = Visibility.Visible;
            }
        }

        public TextPreviewWindow()
        {
            InitializeComponent();
            CurrentPreview = 0;

            Closing += (sender, e) =>
            {
                e.Cancel = true;
                Hide();
            };
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextPreviews != null)
            {
                CurrentPreview++;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextPreviews != null)
            {
                CurrentPreview--;
            }
        }
    }
}
