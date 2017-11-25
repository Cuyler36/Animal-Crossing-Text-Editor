using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using FastColoredTextBoxNS;
using System.Windows.Media;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Animal_Crossing_Text_Editor
{
    public enum File_Type
    {
        Doubutsu_no_Mori_Plus = 0,
        Animal_Crossing = 1,
        Doubutsu_no_Mor_e_Plus = 2
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool IsBMG { get; private set; }
        private BMG BMG_Struct;
        private int Entry_Count = 0;
        private TextEntry[] Entries;
        private string File_Path;
        private string Table_Path;
        private System.Windows.Forms.OpenFileDialog SelectDialog = new System.Windows.Forms.OpenFileDialog();
        private int SelectedIndex = -1;
        private byte[] Buffer;
        private byte[] Table_Buffer; // Doesn't exist for BMG type
        private BackgroundWorker Parser_Worker = new BackgroundWorker();
        private TextStyle Cont_Style = new TextStyle(System.Drawing.Brushes.Orange, null, System.Drawing.FontStyle.Regular);
        private AutocompleteMenu AutoMenu;
        private string[] Keywords;
        private BMCEditorWindow BMCEditor = new BMCEditorWindow();

        public static File_Type Character_Set_Type = File_Type.Animal_Crossing;

        public MainWindow()
        {
            InitializeComponent();
            Parser_Worker.WorkerReportsProgress = true;
            Parser_Worker.DoWork += Parser_Worker_DoWork;
            Parser_Worker.ProgressChanged += Parser_Worker_Progress_Changed;
            Parser_Worker.RunWorkerCompleted += Parser_Worker_RunWorkerCompleted;

            // Setup autocomplete
            AutoMenu = new AutocompleteMenu(SyntaxBox);
            AutoMenu.SearchPattern = @"[\w\.:=!<>]";
            AutoMenu.AllowTabKey = true;
            AutoMenu.ShowItemToolTips = true;
            AutoMenu.ForeColor = System.Drawing.Color.White;
            AutoMenu.BackColor = System.Drawing.Color.Gray;

            Keywords = TextUtility.ContId_Map.Values.ToArray();
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (Keywords[i].Contains("{"))
                {
                    Keywords[i] = Regex.Replace(Keywords[i], @"\{\d\}", "");
                }
            }

            List<AutocompleteItem> Items = new List<AutocompleteItem>();
            foreach (var Item in Keywords)
            {
                Items.Add(new AutocompleteItem(Item) { ToolTipTitle = Item,
                    ToolTipText = ContDescriptions.Descriptions.ContainsKey(Item) ? ContDescriptions.Descriptions[Item] : "No Description" });
            }

            AutoMenu.Items.SetAutocompleteItems(Items);

            SyntaxBox.Font = new System.Drawing.Font("Consolas", 10);
            Closed += delegate (object sender, EventArgs e)
            {
                App.Current.Shutdown();
            };
        }

        private void Scroll_to_Index(int Index)
        {
            VirtualizingStackPanel vsp =
                (VirtualizingStackPanel)typeof(ItemsControl).InvokeMember("_itemsHost",
                BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic, null,
                TextListView, null);

            vsp.SetVerticalOffset(vsp.ScrollOwner.ScrollableHeight * Index / TextListView.Items.Count);
        }

        public void Goto(int Index)
        {
            if (Index > -1)
            {
                if (Entries != null && Index < Entries.Length)
                {
                    SyntaxBox.Enabled = true;
                    SyntaxBox.Text = Entries[Index].Text;
                    SelectedIndex = Index;
                    OffsetLabel.Content = "Entry: " + SelectedIndex.ToString("X") + " | Offset: 0x" + (Entries[SelectedIndex].Offset).ToString("X");
                    Scroll_to_Index(Index);
                    TextListView.SelectedIndex = Index;
                }
                else if (File_Path != null && Index < TextListView.Items.Count)
                {
                    SyntaxBox.Enabled = true;
                    SyntaxBox.Text = (string)(TextListView.Items[Index] as ListViewItem).Content;
                    SelectedIndex = Index;
                    OffsetLabel.Content = "Entry: " + SelectedIndex.ToString("X") + " | Offset: Unknown";
                    Scroll_to_Index(Index);
                    TextListView.SelectedIndex = Index;
                }
            }
        }

        private void Parser_Worker_Progress_Changed(object sender, ProgressChangedEventArgs e)
        {
            //Debug.WriteLine(e.ProgressPercentage);
            ProgressBar.Value = e.ProgressPercentage;
        }
        // TODO: Find a Unicode font (to properly support double width characters)
        private void Parser_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Entries = new TextEntry[Entry_Count];
            int Last_Offset = 0;

            int Interval = Entry_Count / 100;
            BinaryReader TableReader = new BinaryReader(new FileStream(Table_Path, FileMode.Open));
            BinaryReader ContentReader = new BinaryReader(new FileStream(File_Path, FileMode.Open));

            for (int i = 0; i < Entry_Count; i++)
            {
                //int This_Offset = BitConverter.ToInt32(Table_Buffer.Skip(i * 4).Take(4).Reverse().ToArray(), 0);
                int This_Offset = TableReader.ReadReversedInt32();

                //Entries[i] = new TextEntry(This_Offset, Buffer.Skip(Last_Offset).Take(This_Offset - Last_Offset).ToArray()); // NOTE: This is wrong. Should be (Next_Entry - This_Entry)
                ContentReader.BaseStream.Seek(Last_Offset, SeekOrigin.Begin);
                Entries[i] = new TextEntry(Last_Offset, ContentReader.ReadBytes(This_Offset - Last_Offset));
                Last_Offset = This_Offset;

                if (i % Interval == 0)
                {
                    double Percent = ((double)i / (double)Entry_Count) * 100;
                    Parser_Worker.ReportProgress((int)Percent);
                }
            }
        }

        private void Parser_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<ListViewItem> TextItems = (TextListView.ItemsSource as List<ListViewItem>) ?? new List<ListViewItem>();
                TextItems.Clear();

                for (int i = 0; i < Entries.Length; i++)
                {
                    if (Entries[i] != null)
                    {
                        if (!string.IsNullOrEmpty(Entries[i].Text))
                        {
                            ListViewItem TextEntry = new ListViewItem();
                            TextEntry.Content = Entries[i].Text;
                            TextEntry.HorizontalContentAlignment = HorizontalAlignment.Left;
                            TextEntry.VerticalContentAlignment = VerticalAlignment.Top;

                            TextItems.Add(TextEntry);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Entry was null! Entry #" + i);
                    }
                }

                TextListView.ItemsSource = TextItems;

                CollectionView View = (CollectionView)CollectionViewSource.GetDefaultView(TextListView.ItemsSource);
                View.Filter = Filter;

                SearchBox.IsEnabled = true;
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Ex2: " + ex2.Message + "\n\n" + ex2.StackTrace);
            }

            foreach (KeyValuePair<byte, int> Cont_Param_Rates in TextUtility.Cont_Id_Appearance)
                Debug.WriteLine(string.Format("Cont Param 0x{0} appeared {1} times!", Cont_Param_Rates.Key.ToString("X2"), Cont_Param_Rates.Value));
        }

        private bool Filter(object Item)
        {
            if (Item != null)
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    return true;
                }
                else
                {
                    ListViewItem ViewItem = Item as ListViewItem;
                    string Text = (string)ViewItem.Content;
                    if (string.IsNullOrEmpty(Text))
                        return false;
                    return Text.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            return true;
        }

        private void Resize(TextEntry Entry, int Entry_Index, string New_Text)
        {
            byte[] New_Data = TextUtility.Encode(New_Text, Character_Set_Type);
            string Decoded = TextUtility.Decode(New_Data); // TODO: Figure out how to not re-decode the bytes
            // TEST
            //MessageBox.Show("Arrays are equal size: " + (New_Data.Length == Entry.Data.Length).ToString());
            for (int i = 0; i < New_Data.Length; i++)
            {
                if (i < Entry.Data.Length)
                    Debug.WriteLine(string.Format("Index: {0} | Old Data: {1} | New Data: {2}", i, Entry.Data[i].ToString("X2"), New_Data[i].ToString("X2")));
                else
                    Debug.WriteLine("New Data is too long!");
            }
            int Size_Delta = New_Data.Length - Entry.Length;
            if (Size_Delta != 0)
            {
                for (int i = Entry_Index + 1; i < Entries.Length; i++)
                {
                    Entries[i].Offset += Size_Delta;
                }
            }

            Entry.Text = Decoded;
            Entry.Length = New_Data.Length;
            Entry.Data = New_Data;

            // Update Editor Text to reflect possible changes after re-decoding
            SyntaxBox.Text = Decoded;
        }
        
        private void Generate_Text_Entries()
        {
            if (Buffer == null || Table_Buffer == null)
                return;

            Entry_Count = 0;
            for (int i = 0; i < Table_Buffer.Length; i += 4)
            {
                if (i > 0 && BitConverter.ToUInt32(Table_Buffer.Skip(i).Take(4).Reverse().ToArray(), 0) == 0)
                {
                    break;
                }
                Entry_Count++;
            }

            Parser_Worker.RunWorkerAsync();
        }

        private void Generate_BMG_Text_Entries(BMG BMG_Struct)
        {
            List<ListViewItem> TextItems = (TextListView.ItemsSource as List<ListViewItem>) ?? new List<ListViewItem>();
            TextItems.Clear();

            for (int i = 0; i < BMG_Struct.INF_Section.Items.Length; i++)
            {
                // TODO: Implement BackgroundWorker
                if (!string.IsNullOrEmpty(BMG_Struct.INF_Section.Items[i].Text))
                {
                    ListViewItem TextEntry = new ListViewItem();
                    TextEntry.Content = BMG_Struct.INF_Section.Items[i].Text;
                    TextEntry.HorizontalContentAlignment = HorizontalAlignment.Left;
                    TextEntry.VerticalContentAlignment = VerticalAlignment.Top;

                    TextItems.Add(TextEntry);
                }
            }

            TextListView.ItemsSource = TextItems;

            CollectionView View = (CollectionView)CollectionViewSource.GetDefaultView(TextListView.ItemsSource);
            View.Filter = Filter;

            SearchBox.IsEnabled = true;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            SelectDialog.Filter = "Binary Files|*.bin";
            SelectDialog.FileName = "";

            if (SelectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File_Path = SelectDialog.FileName;
                byte[] Buff = File.ReadAllBytes(File_Path);
                if (BMGUtility.IsBuffBMG(Buff))
                {
                    IsBMG = true;
                    Debug.WriteLine("BMG File Detected!");
                    BMG_Struct = BMGUtility.Decode(File_Path); // Should probably change to a byte array at some point
                    Generate_BMG_Text_Entries(BMG_Struct);
                }
                else
                {
                    IsBMG = false;
                    if (File_Path.Contains("_table.bin"))
                    {
                        Table_Path = File_Path;
                        string File_Name = Path.GetFileNameWithoutExtension(Table_Path);
                        File_Path = Path.GetDirectoryName(Table_Path) + @"\" + File_Name.Substring(0, File_Name.Length - 6);
                    }
                    else
                    {
                        Table_Path = Path.GetDirectoryName(File_Path) + @"\" + Path.GetFileNameWithoutExtension(File_Path) + "_table.bin";
                    }

                    if (File.Exists(File_Path) && File.Exists(Table_Path))
                    {
                        Buffer = File.ReadAllBytes(File_Path);
                        Table_Buffer = File.ReadAllBytes(Table_Path);

                        Generate_Text_Entries();
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (SearchBox.IsEnabled && TextListView.ItemsSource != null && SearchBox.IsFocused)
                CollectionViewSource.GetDefaultView(TextListView.ItemsSource).Refresh();
        }

        private void TextEntry_Clicked(object sender, MouseButtonEventArgs e)
        {
            //ContentTextBox.IsEnabled = true;
            if (TextListView.SelectedItem != null)
            {
                SyntaxBox.Enabled = true;
                var Item = (sender as ListView).SelectedItem as ListViewItem;
                if (Item != null && Item.IsSelected)
                {
                    SyntaxBox.Text = (string)Item.Content;
                    SelectedIndex = ((List<ListViewItem>)TextListView.ItemsSource).IndexOf(Item);
                    OffsetLabel.Content = "Entry: " + SelectedIndex.ToString("X") + " | Offset: 0x" + (Entries == null ? "Unknown" : (Entries[SelectedIndex].Offset).ToString("X"));
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (File_Path != null)
            {
                // Write Table File first
                if (File.Exists(Table_Path))
                    File.Delete(Table_Path);

                using (FileStream Table_Stream = new FileStream(Table_Path, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        byte[] Data = BitConverter.GetBytes(Entries[i].Offset).Reverse().ToArray();
                        Table_Stream.Write(Data, 0, Data.Length);
                    }
                    Table_Stream.Flush();
                }

                // Write String File next
                if (File.Exists(File_Path))
                    File.Delete(File_Path);

                using (FileStream File_Stream = new FileStream(File_Path, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        File_Stream.Write(Entries[i].Data, 0, Entries[i].Data.Length);
                    }
                    File_Stream.Flush();
                }
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
            {
                if (!string.IsNullOrEmpty(SyntaxBox.Text))
                {
                    string Text = SyntaxBox.Text.Replace("\r", String.Empty);

                    /*for (int i = 0; i < Text.Length; i++)
                        if (Entries[SelectedIndex].Text.Length > i)
                        {
                            if (!Entries[SelectedIndex].Text[i].Equals(Text[i]))
                                MessageBox.Show(string.Format("Index {0} didn't match: Old Text: {1} | New Text: {2}", i, ((byte)Entries[SelectedIndex].Text[i]).ToString("X2"), ((byte)Text[i]).ToString("X2")));
                        }
                        else
                            MessageBox.Show("Index was greater than original length. i: " + i.ToString() + " | Value: " + Entries[SelectedIndex].Text[i]);

                            MessageBox.Show(string.Format("Old Size: {0} | New Size: {1}", Entries[SelectedIndex].Text.Length, Text.Length));*/
                    Resize(Entries[SelectedIndex], SelectedIndex, Text);
                    ((List<ListViewItem>)TextListView.ItemsSource)[SelectedIndex].Content = Entries[SelectedIndex].Text;//Text;
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchLabel.Visibility = Visibility.Hidden;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
                SearchLabel.Visibility = Visibility.Visible;
        }

        private void AC_CharSet_Checked(object sender, RoutedEventArgs e)
        {
            if (AC_CharSet.IsChecked == true)
            {
                Character_Set_Type = File_Type.Animal_Crossing;
                TextUtility.Character_Map = Character_Set_Type == File_Type.Doubutsu_no_Mori_Plus
                    ? TextUtility.Doubutsu_no_Mori_Plus_Character_Map : TextUtility.Animal_Crossing_Character_Map;
                Generate_Text_Entries();
            }
        }

        private void DnM_CharSet_Checked(object sender, RoutedEventArgs e)
        {
            if (DnM_CharSet.IsChecked == true)
            {
                Character_Set_Type = File_Type.Doubutsu_no_Mori_Plus;
                TextUtility.Character_Map = Character_Set_Type == File_Type.Doubutsu_no_Mori_Plus
                    ? TextUtility.Doubutsu_no_Mori_Plus_Character_Map : TextUtility.Animal_Crossing_Character_Map;
                Generate_Text_Entries();
            }
        }

        private bool IsHex(string Text)
        {
            return Regex.IsMatch(Text, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private void SyntaxBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            e.ChangedRange.ClearStyle(Cont_Style);
            e.ChangedRange.SetStyle(Cont_Style, "<[^>\n]+>");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var GotoDialog = new GotoWindow();
            if (GotoDialog.ShowDialog() == true)
            {
                Goto(GotoDialog.GotoValue);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            using (var BMC_File_Dialog = new System.Windows.Forms.OpenFileDialog())
            {
                BMC_File_Dialog.Filter = "All Supported Files|*.bmc;*.bin|Binary Color Files|*.bmc|Binary Files|*.bin";
                BMC_File_Dialog.DefaultExt = "All Supported Files|*.bmc; *.bin";
                BMC_File_Dialog.FileName = "";

                if (BMC_File_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BMC bmcFile = new BMC(File.ReadAllBytes(BMC_File_Dialog.FileName));
                    if (bmcFile.Identifier.Equals("MGCLbmc1"))
                    {
                        BMCEditor.Show(bmcFile);
                    }
                    else
                    {
                        MessageBox.Show("The selected file does not appear to be a valid BMC file!", "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
