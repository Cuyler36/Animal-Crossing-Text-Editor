using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.CodeCompletion;

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
        private string[] Keywords;
        private BMCEditorWindow BMCEditor = new BMCEditorWindow();
        private List<ListViewItem> TextItems;
        private CompletionWindow completionWindow;
        private bool Changing_Selected_Entry = false;
        private bool AutoSaveEnabled = true;

        public static File_Type Character_Set_Type = File_Type.Animal_Crossing;

        public MainWindow()
        {
            InitializeComponent();
            Parser_Worker.WorkerReportsProgress = true;
            Parser_Worker.DoWork += Parser_Worker_DoWork;
            Parser_Worker.ProgressChanged += Parser_Worker_Progress_Changed;
            Parser_Worker.RunWorkerCompleted += Parser_Worker_RunWorkerCompleted;

            // Load XSHD file for stylesheet
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                using (Stream s = new FileStream(Path.GetDirectoryName(assembly.Location) + "\\Resources\\Animal Crossing Text Editor Style.xshd", FileMode.Open))
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        //Load default Syntax Highlighting
                        Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message + "\r\n" + e.StackTrace); }

            Keywords = TextUtility.ContId_Map.Values.ToArray();
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (Keywords[i].Contains("{"))
                {
                    Keywords[i] = Regex.Replace(Keywords[i], @"\{\d\}", "");
                }
            }

            Closed += delegate (object sender, EventArgs e)
            {
                App.Current.Shutdown();
            };

            Editor.TextArea.TextEntered += Editor_TextEntered;
            Editor.TextArea.TextEntering += Editor_TextEntering;
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
                    Editor.IsEnabled = true;
                    Editor.Text = Entries[Index].Text;
                    SelectedIndex = Index;
                    Scroll_to_Index(Index);
                    TextListView.SelectedIndex = Index;
                }
                else if (!string.IsNullOrEmpty(BMG_Struct.FileType) && Index < BMG_Struct.INF_Section.MessageCount)
                {
                    Editor.IsEnabled = true;
                    Editor.Text = BMG_Struct.INF_Section.Items[Index].Text;
                    SelectedIndex = Index;
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
                try
                {
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
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: {0}\n{1}", ex.Message, ex.StackTrace);
                }
            }
        }

        private void Parser_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                TextItems = (TextListView.ItemsSource as List<ListViewItem>) ?? new List<ListViewItem>();
                TextItems.Clear();

                for (int i = 0; i < Entries.Length; i++)
                {
                    if (Entries[i] != null)
                    {
                        if (!string.IsNullOrEmpty(Entries[i].Text))
                        {
                            ListViewItem TextEntry = new ListViewItem
                            {
                                Content = Entries[i].Text,
                                HorizontalContentAlignment = HorizontalAlignment.Left,
                                VerticalContentAlignment = VerticalAlignment.Top
                            };

                            TextItems.Add(TextEntry);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Entry was null! Entry #" + i.ToString("X"));
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
                    Debug.WriteLine(string.Format("Index: {0} | Old Data: {1} | New Data: {2} | Equal: {3}", i, Entry.Data[i].ToString("X2"), New_Data[i].ToString("X2"), Entry.Data[i] == New_Data[i]));
                else
                    Debug.WriteLine("New Data is longer!");
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
            Editor.Text = Decoded;
        }

        private void ResizeBMG(BMG_INF_Item Entry, int Entry_Index, string New_Text)
        {
            byte[] New_Data = TextUtility.Encode(New_Text, Character_Set_Type);
            string Decoded = TextUtility.Decode(New_Data); // TODO: Figure out how to not re-decode the bytes
            
            for (int i = 0; i < New_Data.Length; i++)
            {
                if (i < Entry.Data.Length)
                    Debug.WriteLine(string.Format("Index: {0} | Old Data: {1} | New Data: {2} | Equal: {3}", i, Entry.Data[i].ToString("X2"), New_Data[i].ToString("X2"), Entry.Data[i] == New_Data[i]));
                else
                {
                    Debug.WriteLine("New Data is longer!");
                    break;
                }
            }
            int Size_Delta = New_Data.Length - (int)Entry.Length;
            if (Size_Delta != 0)
            {
                for (int i = Entry_Index + 1; i < BMG_Struct.INF_Section.Items.Length; i++)
                {
                    BMG_Struct.INF_Section.Items[i].Text_Offset = (uint)(BMG_Struct.INF_Section.Items[i].Text_Offset + Size_Delta);
                }
            }

            Entry.Text = Decoded;
            Entry.Length = (uint)New_Data.Length;
            Entry.Data = New_Data;

            // Update Editor Text to reflect possible changes after re-decoding
            Editor.Text = Decoded;

            BMG_Struct.INF_Section.Items[SelectedIndex] = Entry;
            var SelectedListViewItem = ((List<ListViewItem>)TextListView.ItemsSource)[SelectedIndex];
            SelectedListViewItem.Content = BMG_Struct.INF_Section.Items[SelectedIndex].Text;
        }

        private void Generate_Text_Entries()
        {
            if (Buffer == null || Table_Buffer == null)
                return;

            TextListView.ItemsSource = null;

            Entry_Count = 0;
            for (int i = 0; i < Table_Buffer.Length; i += 4)
            {
                if (i > 0 && BitConverter.ToUInt32(Table_Buffer.Skip(i).Take(4).Reverse().ToArray(), 0) == 0)
                {
                    break;
                }
                Entry_Count++;
            }

            Debug.WriteLine("Entry Count: " + Entry_Count.ToString("X"));

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
                    ListViewItem TextEntry = new ListViewItem
                    {
                        Content = BMG_Struct.INF_Section.Items[i].Text,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Top
                    };

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
                IsBMG = false;
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.IsEnabled && TextListView.ItemsSource != null && SearchBox.IsFocused)
                CollectionViewSource.GetDefaultView(TextListView.ItemsSource).Refresh();
        }

        private void TextEntry_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (TextListView.SelectedItem != null)
            {
                Editor.IsEnabled = true;
                if ((sender as ListView).SelectedItem is ListViewItem Item && Item.IsSelected)
                {
                    Editor.Text = (string)Item.Content;
                    SelectedIndex = ((List<ListViewItem>)TextListView.ItemsSource).IndexOf(Item);
                    Changing_Selected_Entry = true;
                    EntryBox.Text = SelectedIndex.ToString("X");
                    if (IsBMG)
                    {
                        OffsetBox.Text = (BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset).ToString("X");
                    }
                    else
                    {
                        OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                    }
                    Changing_Selected_Entry = false;
                }
            }
        }

        private void DoAutoSave()
        {
            if (AutoSaveEnabled && File_Path != null)
            {
                string DirectoryPath = Path.GetDirectoryName(File_Path);
                if (!IsBMG)
                {
                    // Handle non BMG entries here
                }
                else
                {
                    BMGUtility.Write(BMG_Struct, DirectoryPath + "\\" + Path.GetFileNameWithoutExtension(File_Path) + "_AutoSave.bin");
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (File_Path != null)
            {
                if (!IsBMG)
                {
                    // Write Table File first
                    if (File.Exists(Table_Path))
                        File.Delete(Table_Path);

                    using (FileStream Table_Stream = new FileStream(Table_Path, FileMode.OpenOrCreate))
                    {
                        for (int i = 0; i < Entries.Length; i++)
                        {
                            byte[] Data = BitConverter.GetBytes(i + 1 < Entries.Length ? Entries[i + 1].Offset : Entries[i].Offset + Entries[i].Length).Reverse().ToArray();
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
                else
                {
                    if (File.Exists(File_Path))
                        File.Delete(File_Path);

                    BMGUtility.Write(BMG_Struct, File_Path);
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
                if (!string.IsNullOrEmpty(Editor.Text))
                {
                    string Text = Editor.Text.Replace("\r", String.Empty);

                    if (!IsBMG)
                    {
                        Resize(Entries[SelectedIndex], SelectedIndex, Text);
                        ((List<ListViewItem>)TextListView.ItemsSource)[SelectedIndex].Content = Entries[SelectedIndex].Text;
                    }
                    else
                    {
                        ResizeBMG(BMG_Struct.INF_Section.Items[SelectedIndex], SelectedIndex, Text);
                    }

                    // AutoSave
                    DoAutoSave();
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

                if (IsBMG && !string.IsNullOrEmpty(File_Path))
                {
                    BMG_Struct = BMGUtility.Decode(File_Path);
                    Generate_BMG_Text_Entries(BMG_Struct);
                }
                else
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
                if (IsBMG && !string.IsNullOrEmpty(File_Path))
                {
                    BMG_Struct = BMGUtility.Decode(File_Path);
                    Generate_BMG_Text_Entries(BMG_Struct);
                }
                else
                    Generate_Text_Entries();
            }
        }

        private void CopyTextToClipboard(string Text)
        {
            var replaceRegex = new Regex(@"<([^>]+)>");
            var strippedText = replaceRegex.Replace(Text, "").Trim();
            Clipboard.SetText(strippedText);
            Debug.WriteLine(strippedText);
        }

        private bool IsHex(string Text)
        {
            return Regex.IsMatch(Text, @"\A\b[0-9a-fA-F]+\b\Z");
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

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1 && Entries != null)
            {
                var Text = TextUtility.GetRawText(Entries[SelectedIndex].Data);
                Clipboard.SetText(Text);
                Debug.WriteLine(Text);
            }
            else if (SelectedIndex > -1 && IsBMG)
            {
                /*
                string s = "";
                for (int i = 0; i < BMG_Struct.INF_Section.Items[SelectedIndex].Data.Length; i++)
                {
                    s += BMG_Struct.INF_Section.Items[SelectedIndex].Data[i].ToString("X2") + " ";
                }
                Debug.WriteLine(s);*/
                CopyTextToClipboard(BMG_Struct.INF_Section.Items[SelectedIndex].Text);
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Entries != null || !string.IsNullOrEmpty(BMG_Struct.FileType))
            {
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Goto(++SelectedIndex);
                EntryBox.Text = SelectedIndex.ToString("X");
                if (IsBMG)
                {
                    OffsetBox.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset.ToString("X");
                }
                else
                {
                    OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                }
                Changing_Selected_Entry = false;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > 0 && (Entries != null || !string.IsNullOrEmpty(BMG_Struct.FileType)))
            {
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Goto(--SelectedIndex);
                EntryBox.Text = SelectedIndex.ToString("X");
                if (IsBMG)
                {
                    OffsetBox.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset.ToString("X");
                }
                else
                {
                    OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                }
                Changing_Selected_Entry = false;
            }
        }

        private void EntryBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(File_Path) && !Changing_Selected_Entry && ushort.TryParse(EntryBox.Text, NumberStyles.AllowHexSpecifier, null, out ushort Entry_Index))
            {
                SelectedIndex = Entry_Index;
                if (IsBMG)
                {
                    if (Entry_Index < BMG_Struct.INF_Section.MessageCount)
                    {
                        Editor.IsEnabled = true;
                        Changing_Selected_Entry = true;
                        Editor.Text = BMG_Struct.INF_Section.Items[Entry_Index].Text;
                        OffsetBox.Text = BMG_Struct.INF_Section.Items[Entry_Index].Text_Offset.ToString("X");
                        Changing_Selected_Entry = false;
                    }
                }
                else
                {
                    Changing_Selected_Entry = true;
                    Editor.Text = Entries[SelectedIndex].Text;
                    OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                    Changing_Selected_Entry = false;
                }
            }
        }

        private void OffsetBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(File_Path) && !Changing_Selected_Entry && uint.TryParse(OffsetBox.Text, NumberStyles.AllowHexSpecifier, null, out uint Offset))
            {
                if (IsBMG)
                {
                    for (int i = 0; i < BMG_Struct.INF_Section.MessageCount; i++)
                    {
                        uint Current_Offset = BMG_Struct.INF_Section.Items[i].Text_Offset;
                        uint Next_Offset = (i + 1 >= BMG_Struct.INF_Section.MessageCount) ? Current_Offset + BMG_Struct.INF_Section.Items[i].Length : BMG_Struct.INF_Section.Items[i + 1].Text_Offset;
                        if (Offset == Current_Offset || (Offset > Current_Offset && Offset < Next_Offset))
                        {
                            Editor.IsEnabled = true;
                            Changing_Selected_Entry = true;
                            EntryBox.Text = i.ToString("X");
                            Changing_Selected_Entry = false;
                            Editor.Text = BMG_Struct.INF_Section.Items[i].Text;
                            SelectedIndex = i;
                            return;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i] == null)
                            return;

                        uint Current_Offset = (uint)Entries[i].Offset;
                        uint Next_Offset = (i + 1 >= Entries.Length) ? (uint)(Current_Offset + Entries[i].Length) : (uint)Entries[i + 1].Offset;
                        if (Offset == Current_Offset || (Offset > Current_Offset && Offset < Next_Offset))
                        {
                            Editor.IsEnabled = true;
                            Changing_Selected_Entry = true;
                            EntryBox.Text = i.ToString("X");
                            Editor.Text = Entries[i].Text;
                            Changing_Selected_Entry = false;
                            SelectedIndex = i;
                            return;
                        }
                    }
                }

                Changing_Selected_Entry = false;
            }
        }

        private void Editor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void Editor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            /*if (Editor.CaretOffset > 0 && Editor.CaretOffset < Editor.Text.Length &&
                !string.IsNullOrWhiteSpace(Editor.Text.ElementAt(Editor.CaretOffset - 1).ToString()))
            {
                var Line = Editor.Document.GetLineByOffset(Editor.CaretOffset);
                var LineText = Editor.Text.Substring(Line.Offset, Line.Length);
                int StartOffset = -1;

                for (int i = LineText.Length - 1; i > -1; i--)
                {
                    if (string.IsNullOrWhiteSpace(LineText.ElementAt(i).ToString()))
                    {
                        StartOffset = i + 1;
                        break;
                    }
                }

                if (StartOffset > -1)
                {
                    completionWindow = new CompletionWindow(Editor.TextArea)
                    {
                        SizeToContent = SizeToContent.WidthAndHeight
                    };

                    var Data = completionWindow.CompletionList.CompletionData;
                    bool Match = false;

                    string CurrentAutoCompleteString = LineText.Substring(StartOffset);
                    foreach (string s in Keywords)
                    {
                        if (Regex.IsMatch(s, "^" + CurrentAutoCompleteString))
                        {
                            Data.Add(new CompletionData(s, ContDescriptions.Descriptions.ContainsKey(s) ? ContDescriptions.Descriptions[s] : "No Description"));
                            Match = true;
                        }
                    }

                    if (Match)
                    {
                        completionWindow.Show();
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                    }
                    else
                    {
                        completionWindow = null;
                    }
                }
            }*/
        }
    }
}
