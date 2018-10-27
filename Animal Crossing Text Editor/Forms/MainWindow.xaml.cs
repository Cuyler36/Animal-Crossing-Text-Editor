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
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Threading.Tasks;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.Rendering;
using Animal_Crossing_Text_Editor.Classes.TextPreview;
using System.Windows.Media.Imaging;
using System.Net;
using System.Web;
using Gem;

namespace Animal_Crossing_Text_Editor
{
    public enum CharacterSet
    {
        DoubutsuNoMori = 0,
        DoubutsuNoMoriPlus = 1,
        AnimalCrossing = 2,
        DoubutsuNoMoriEPlus = 3,
        DongwuSenlin = 4, // iQue Animal Forest
        WildWorld = 5,
        CityFolk = 6,
        NewLeaf = 7
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool IsBMG { get; private set; }
        public static MainWindow Reference;

        private int m_selectedIndex = -1;
        private int SelectedIndex {
            get => m_selectedIndex;
            set
            {
                m_selectedIndex = value;
                EntryIdStack.Push((ushort)SelectedIndex);
            }
        }

        private BMG BMG_Struct;
        private int Entry_Count = 0;
        private TextEntry[] Entries;
        private string File_Path;
        private string Table_Path;
        private System.Windows.Forms.OpenFileDialog SelectDialog = new System.Windows.Forms.OpenFileDialog();
        private byte[] Buffer;
        private byte[] Table_Buffer; // Doesn't exist for BMG type
        private BackgroundWorker Parser_Worker = new BackgroundWorker();
        private string[] Keywords;
        private BMCEditorWindow BMCEditor = new BMCEditorWindow();
        private List<ListViewItem> TextItems;
        private CompletionWindow completionWindow;
        private bool Changing_Selected_Entry = false;
        private bool AutoSaveEnabled = true;
        private TextRenderer ACRenderer;
        private TextRenderer AFeRenderer;
        private Forms.TextPreviewWindow previewWindow;
        private Forms.TranslateWindow translationWindow;
        private Stack<ushort> EntryIdStack;
        private Stack<ushort> EntryIdStackRedo; // TODO: Hook this up & rename EntryIdStack to EntryIdStackUndo
        private Forms.SearchWindow listWindow = new Forms.SearchWindow();
        private MenuItem[] _characterSetItems;

        // AutoSave Paths
        private string AutoSave_Path;
        private string AutoSaveTable_Path;

        public static CharacterSet SelectedCharacterSet = CharacterSet.AnimalCrossing;
        public static List<System.Drawing.Color> BMC_Colors;

        public MainWindow()
        {
            InitializeComponent();

            _characterSetItems = new[]
            {
                PopulationGrowingItem, DoubutsuNoMoriItem, WildWorldItem
            };

            Parser_Worker.WorkerReportsProgress = true;
            Parser_Worker.DoWork += Parser_Worker_DoWork;
            Parser_Worker.ProgressChanged += Parser_Worker_Progress_Changed;
            Parser_Worker.RunWorkerCompleted += Parser_Worker_RunWorkerCompleted;

            // Load XSHD file for stylesheet
            LoadXSHDStyleSheet();

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

            // Text Renderer Initialization
            ACRenderer = new TextRenderer(TextRenderUtility.Convert(Properties.Resources.AC_Text), 12, 16, 12, 16);
            AFeRenderer = new TextRenderer(TextRenderUtility.Convert(Properties.Resources.AFe__English_Text), 24, 32, 24, 32);

            // Load Custom Link Highlighter Dependancies
            Editor.TextArea.TextView.ElementGenerators.Add(new CustomLinkGenerator());
            Reference = this;

            MainDockPanel.Children.Remove(tabControl);
            tabControl.Width = double.NaN;
            tabControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            listWindow.ListWindowDockPanel.Children.Add(tabControl);
            listWindow.Show();
        }

        private void SetStatusMessage(string Message)
        {
            statusBox.Text = Message;
        }

        private string Translate(string Text)
        {
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
                client.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

                string url = string.Format(@"http://translate.google.com/m?hl=en&sl={0}&tl={1}&ie=UTF-8&prev=_m&q={2}",
                    "ja", "en", Uri.EscapeUriString(Text));

                string page = client.DownloadString(url);
                page = page.Remove(0, page.IndexOf("<div dir=\"ltr\" class=\"t0\">")).Replace("<div dir=\"ltr\" class=\"t0\">", "");
                int last = page.IndexOf("</div>");
                page = page.Remove(last, page.Length - last);

                if (translationWindow == null)
                {
                    translationWindow = new Forms.TranslateWindow();
                    translationWindow.Closed += (object s, EventArgs ea) =>
                    {
                        translationWindow = null;
                    };
                }

                page = HttpUtility.HtmlDecode(page.Replace("! ", "!\r\n").Replace(". ", ".\r\n").Replace("? ", "?\r\n")).Trim();
                translationWindow.TranslationText = page;
                translationWindow.Show();

                return page;
            }
        }

        private void LoadXSHDStyleSheet()
        {
            try
            {
                using (Stream s = new MemoryStream(Properties.Resources.Animal_Crossing_Text_Editor_Style))
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        //Load default Syntax Highlighting
                        Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message + "\r\n" + e.StackTrace); }
        }

        private bool UseBlackBackground(System.Drawing.Color Input) => Input.GetBrightness() >= 0.4;

        private void AddBMCColorHighlightRules()
        {
            var MainRuleSet = Editor.SyntaxHighlighting.MainRuleSet;
            var CommandRuleset = MainRuleSet.Spans[0];
            var ArgumentRuleset = CommandRuleset.RuleSet.Spans[0];

            if (BMC_Colors != null)
            {
                var ArgumentRules = ArgumentRuleset.RuleSet.Rules;
                var BlackBrush = new CustomizedBrush(System.Drawing.Color.FromArgb(0xFF, 0x33, 0x33, 0x33));
                var WhiteBrush = new CustomizedBrush(System.Drawing.Color.White);
                for (int i = 0; i < BMC_Colors.Count; i++)
                {
                    var Color = BMC_Colors[i];

                    var HighlightBrush = new CustomizedBrush(Color);
                    var HighlightColor = new HighlightingColor
                    {
                        Foreground = HighlightBrush,
                        Background = UseBlackBackground(Color) ? BlackBrush : WhiteBrush
                    };

                    var ColorRule = new HighlightingRule
                    {
                        Color = HighlightColor,
                        Regex = new Regex(@"\b(?>" + i.ToString() + @")\b")
                    };

                    ArgumentRules.Add(ColorRule);
                }
            }
        }

        private void Scroll_to_Index(int Index)
        {
            if (TextListView.SelectedItem != TextItems[Index] && TextListView.Items.Contains(TextItems[Index]))
            {
                TextListView.SelectedItem = TextItems[Index];
                if (TextListView.SelectedItem != null)
                {
                    var Item = (TextListView.SelectedItem as ListViewItem);
                    //Item.Focus();
                    TextListView.ScrollIntoView(Item);
                }
            }
        }

        public void Goto(int Index)
        {
            if (Index > -1)
            {
                Changing_Selected_Entry = true;

                if (Entries != null && Index < Entries.Length)
                {
                    Editor.IsEnabled = true;
                    Editor.Text = Entries[Index].Text;
                    SelectedIndex = Index;
                    EntryBox.Text = SelectedIndex.ToString("X");
                    OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                    Scroll_to_Index(Index);
                }
                else if (!string.IsNullOrEmpty(BMG_Struct.FileType) && Index < BMG_Struct.INF_Section.MessageCount)
                {
                    Editor.IsEnabled = true;
                    Editor.Text = BMG_Struct.INF_Section.Items[Index].Text;
                    SelectedIndex = Index;
                    EntryBox.Text = SelectedIndex.ToString("X");
                    OffsetBox.Text = (BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset).ToString("X");
                    Scroll_to_Index(Index);
                }

                Changing_Selected_Entry = false;
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
                try
                {
                    int This_Offset = TableReader.ReadReversedInt32();

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
                        if (true || !string.IsNullOrEmpty(Entries[i].Text))
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

            foreach (KeyValuePair<byte, int> Cont_Param_Rates in TextUtility.ContIdAppearance)
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
            byte[] New_Data = TextUtility.Encode(New_Text, SelectedCharacterSet);
            string Decoded = TextUtility.Decode(New_Data, BMC_Colors); // TODO: Figure out how to not re-decode the bytes
            
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
            byte[] New_Data = TextUtility.Encode(New_Text, SelectedCharacterSet, BMC_Colors);
            string Decoded = TextUtility.Decode(New_Data, BMC_Colors); // TODO: Figure out how to not re-decode the bytes

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

        private bool TransferEntry(ushort entryId, string dialog)
        {
            if (IsBMG)
            {
                var data = TextUtility.Encode(dialog, SelectedCharacterSet, BMC_Colors);
                var text = TextUtility.Decode(data, BMC_Colors);

                return entryId < BMG_Struct.INF_Section.Items.Length &&
                       BMGUtility.UpdateEntry(BMG_Struct, entryId, data, text);
            }

            return false;
        }

        private void Generate_Text_Entries()
        {
            if (Buffer == null || Table_Buffer == null)
                return;

            TextListView.ItemsSource = null;

            Entry_Count = 0;
            for (int i = 0; i < Table_Buffer.Length; i += 4)
            {
                if (i > 0 && BitConverter.ToUInt32(Table_Buffer, i) == 0)
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
            TextItems = (TextListView.ItemsSource as List<ListViewItem>) ?? new List<ListViewItem>();
            TextItems.Clear();

            for (int i = 0; i < BMG_Struct.INF_Section.Items.Length; i++)
            {
                // TODO: Implement BackgroundWorker
                if (true || !string.IsNullOrEmpty(BMG_Struct.INF_Section.Items[i].Text))
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

        private List<System.Drawing.Color> GetBMCColors(BMC bmc)
        {
            List<System.Drawing.Color> BMCColors = new List<System.Drawing.Color>();

            for (int i = 0; i < bmc.CLT_Section.Items.Length; i++)
            {
                BMCColors.Add(System.Drawing.Color.FromArgb((int)bmc.CLT_Section.Items[i]));
            }

            return BMCColors;
        }

        private void GenerateSingleDialogTree(DialogEntry Entry)
        {
            if (IsBMG)
            {
                if (Entry.EntryId < 0 || Entry.EntryId >= BMG_Struct.INF_Section.Items.Length)
                {
                    return;
                }

                // Don't get stuck in an infinite loop
                if (Entry.HasAncestor(Entry.EntryId))
                {
                    return;
                }

                byte[] Data = BMG_Struct.INF_Section.Items[Entry.EntryId].Data;

                for (int i = 0; i < Data.Length; i++)
                {
                    byte CurrentByte = Data[i];
                    if (CurrentByte != 0x80)
                    {
                        continue;
                    }

                    if (Data[i + 2] == 0x02 && Data[i + 4] > 0x04 && Data[i + 4] < 0x0B) // Choice Jumps
                    {
                        DialogEntry Child = new DialogEntry
                        {
                            Parent = Entry,
                            EntryId = (ushort)((Data[i + 5] << 8) | Data[i + 6])
                        };

                        GenerateSingleDialogTree(Child);
                        Entry.Children.Add(Child);
                        i += Data[i + 1] - 1; // i += Size - 1;
                    }
                    else if (Data[i + 2] == 0x0C) // Jumps
                    {
                        if (Data[i + 4] == 0x00)
                        {
                            DialogEntry Child = new DialogEntry
                            {
                                Parent = Entry,
                                EntryId = (ushort)((Data[i + 5] << 8) | Data[i + 6])
                            };

                            GenerateSingleDialogTree(Child);
                            Entry.Children.Add(Child);
                        }
                        // TODO: Other jumps that have more than one entry
                    }
                }
            }
            else
            {
                if (Entry.EntryId < 0 || Entry.EntryId >= Entries.Length)
                {
                    return;
                }

                // Don't get stuck in an infinite loop
                if (Entry.HasAncestor(Entry.EntryId))
                {
                    return;
                }

                byte[] Data = Entries[Entry.EntryId].Data;

                for (int i = 0; i < Data.Length; i++)
                {
                    byte CurrentByte = Data[i];
                    if (CurrentByte != 0x7F)
                    {
                        continue;
                    }

                    if ((Data[i + 1] > 0x0D && Data[i + 1] < 0x13) || Data[i + 1] == 0x77 || Data[i + 1] == 0x78)
                    {
                        DialogEntry Child = new DialogEntry
                        {
                            Parent = Entry,
                            EntryId = (ushort)((Data[i + 2] << 8) | Data[i + 3])
                        };

                        GenerateSingleDialogTree(Child);
                        Entry.Children.Add(Child);
                        i += 3;
                    }
                    /*else if (Data[i + 1] == 0x0C) // Jumps
                    {
                        // TODO: Other jumps that have more than one entry
                    }*/
                }
            }
        }

        private void GenerateTreeViewItems(DialogEntry Entry, TreeViewItem ParentItem)
        {
            TreeViewItem ThisItem = new TreeViewItem
            {
                Header = Entry.EntryId.ToString("X4"),
                Tag = Entry.EntryId
            };

            foreach (DialogEntry SubEntry in Entry.Children)
            {
                GenerateTreeViewItems(SubEntry, ThisItem);
            }

            if (ParentItem != null)
            {
                ParentItem.Items.Add(ThisItem);
            }
            else
            {
                connectionTreeView.Items.Add(ThisItem);
            }
        }

        private List<DialogEntry> GenerateDialogTree()
        {
            connectionTreeView.Items.Clear();
            List<DialogEntry> DialogEntries = new List<DialogEntry>();
            if (File_Path != null)
            {
                if (IsBMG)
                {
                    Console.WriteLine("Entry count " + BMG_Struct.INF_Section.Items.Length.ToString("X4"));
                    for (ushort i = 0; i < BMG_Struct.INF_Section.Items.Length; i++)
                    {
                        var Entry = new DialogEntry()
                        {
                            EntryId = i
                        };

                        GenerateSingleDialogTree(Entry);
                        DialogEntries.Add(Entry);
                    }
                }
                else
                {
                    Console.WriteLine("Entry count " + Entries.Length.ToString("X4"));
                    for (ushort i = 0; i < Entries.Length; i++)
                    {
                        var Entry = new DialogEntry()
                        {
                            EntryId = i
                        };

                        GenerateSingleDialogTree(Entry);
                        DialogEntries.Add(Entry);
                    }
                }
            }

            // Generate TreeView Items
            foreach (var Entry in DialogEntries)
            {
                GenerateTreeViewItems(Entry, null);
            }

            return DialogEntries;
        }

        private void ReportRebuildProgress(int Index, int Count)
        {
            double Percent = ((double)Index / (double)Count) * 100;
            ProgressBar.Value = Percent;
        }

        private async void RebuildDatabase(string messageDirectory, bool GenerateAsBMG = false)
        {
            if (Directory.Exists(messageDirectory))
            {
                if (!GenerateAsBMG)
                {

                }
                else
                {
                    BMG ConstructedBMG = new BMG
                    {
                        FileType = "MESGbmg1",
                        Size = 0,
                        SectionCount = 2,
                        Encoding = 0x03000000,
                        Padding = new byte[12],
                        INF_Section = new BMG_Section_INF
                        {
                            SectionType = "INF1",
                            INF_Size = 4,
                            Unknown = 0
                        },
                        DAT_Section = new BMG_Section_DAT
                        {
                            SectionType = "DAT1"
                        }
                    };

                    string[] ImportableFiles = Directory.GetFiles(messageDirectory, "*.*", SearchOption.AllDirectories).Where(f => f.Contains(".txt")).ToArray();
                    SortedDictionary<ushort, string> SortedMessages = new SortedDictionary<ushort, string>();

                    Console.WriteLine($"Generating BMG started. Encoding {ImportableFiles.Length} messages");
                    bool WasBMG = IsBMG;
                    int idx = 0;

                    await Task.Run(() =>
                    {
                        foreach (string file in ImportableFiles)
                        {
                            if (ushort.TryParse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, null, out ushort MessageId))
                            {
                                if (!SortedMessages.ContainsKey(MessageId))
                                {
                                    SortedMessages.Add(MessageId, File.ReadAllText(file, System.Text.Encoding.UTF8).Replace("\r\n", "\n"));
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("More than one entry with message id: 0x{0} existed! This entry will be skipped: {1}", MessageId.ToString("X4"), file));
                                }
                            }
                            else
                            {
                                Console.WriteLine(string.Format("File: {0} contained an invalid name. It must be an unsigned short number. Example: 3A2E.txt", file));
                            }
                            idx++;

                            Dispatcher.Invoke(new Action(() => ReportRebuildProgress(idx, ImportableFiles.Length)));
                        }
                        // Add any missing entries between the first and last in the import table
                        ushort LargestEntry = SortedMessages.Last().Key;
                        for (ushort i = 0; i < LargestEntry; i++)
                        {
                            if (!SortedMessages.ContainsKey(i))
                            {
                                Console.WriteLine(string.Format("Adding missing entry: 0x{0} as a blank <End Conversation> entry!", i.ToString("X4")));
                                SortedMessages.Add(i, "<End Conversation>");
                            }
                        }

                        IsBMG = true;

                        int INFSize = SortedMessages.Count * 4;
                        int StartDATOffset = INFSize + 0x28;
                        StartDATOffset += 0x20 - (StartDATOffset % 0x20);

                        int CurrentDATOffset = 1; // For some reason, e+'s BMG files all start at 1 for their offset

                        ConstructedBMG.INF_Section.Size = (uint)(0x20 + INFSize); // In e+ BMG's, the size includes the BMG data header. This is probably more like "Offset of next section")
                        ConstructedBMG.INF_Section.MessageCount = (ushort)SortedMessages.Count;
                        ConstructedBMG.DAT_Section.Offset = StartDATOffset;

                        // Create BMG entries list
                        ConstructedBMG.INF_Section.Items = new BMG_INF_Item[SortedMessages.Count];
                        for (ushort i = 0; i < SortedMessages.Count; i++)
                        {
                            ConstructedBMG.INF_Section.Items[i] = new BMG_INF_Item
                            {
                                Data = TextUtility.Encode(SortedMessages[i], CharacterSet.DoubutsuNoMoriEPlus, BMC_Colors),
                                Text = SortedMessages[i],
                                Text_Offset = (uint)CurrentDATOffset
                            };
                            CurrentDATOffset += ConstructedBMG.INF_Section.Items[i].Data.Length;

                            Dispatcher.Invoke(new Action(() => ReportRebuildProgress(i, SortedMessages.Count)));
                        }

                        ConstructedBMG.DAT_Section.Size = (uint)CurrentDATOffset;
                    });

                    // Write constructed BMG file
                    SaveFileDialog SaveBMGFileDialog = new SaveFileDialog
                    {
                        Filter = "BMG File|*.bmg|Binary File|*.bin"
                    };

                    if (SaveBMGFileDialog.ShowDialog().Value)
                    {
                        BMGUtility.Write(ConstructedBMG, SaveBMGFileDialog.FileName);
                    }

                    IsBMG = WasBMG;
                    SetStatusMessage(string.Format("Finshed rebuilding BMG file at: {0}", SaveBMGFileDialog.FileName));
                }
            }
        }

        private void ReportBMGLoadProgress(int Index, int Count)
        {
            ProgressBar.Value = ((double)Index / (double)Count) * 100;
        }

        private delegate void ReportBMGLoadProgressHandle(int Index, int Count);

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            SelectDialog.Filter = "All Supported Files|*.bmg;*.bin|Binary Message Group Files|*.bmg|Binary Files|*.bin|All Files|*.*";
            SelectDialog.FileName = "";

            // Clear Connection TreeView Items & Hide Tab
            connectionTreeView.Items.Clear();
            connectionTab.Visibility = Visibility.Collapsed;
            tabControl.TabIndex = 0;

            if (SelectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                EntryIdStack = new Stack<ushort>();
                AutoSave_Path = null;
                AutoSaveTable_Path = null;
                IsBMG = false;
                BMC_Colors = null;
                File_Path = SelectDialog.FileName;
                byte[] Buff = File.ReadAllBytes(File_Path);
                if (BMGUtility.IsBuffBMG(Buff))
                {
                    IsBMG = true;
                    Debug.WriteLine("BMG File Detected!");
                    
                    // Check for (and load if found) msg_color.bmc
                    string BMC_Location = Path.GetDirectoryName(File_Path) + "\\" + Path.GetFileNameWithoutExtension(File_Path) + "_color.bmc";
                    if (!File.Exists(BMC_Location))
                    {
                        BMC_Location = Path.GetDirectoryName(File_Path) + "\\msg_color.bmc";
                    }

                    if (File.Exists(BMC_Location))
                    {
                        BMC_Colors = GetBMCColors(new BMC(File.ReadAllBytes(BMC_Location)));
                        //AddBMCColorHighlightRules();
                        Debug.WriteLine($"BMC File found! Loaded {BMC_Colors.Count} colors.");
                    }

                    try
                    {
                        BMG_Struct = await BMGUtility.Decode(File_Path, BMC_Colors,
                            new ReportBMGLoadProgressHandle(
                                ReportBMGLoadProgress)); // Should probably change to a byte array at some point
                        Generate_BMG_Text_Entries(BMG_Struct);
                        SetStatusMessage($"{Path.GetFileName(File_Path)} was successfully loaded!");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                    }
                }
                else
                {
                    // Reset Stylesheet
                    LoadXSHDStyleSheet();
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
                        SetStatusMessage(string.Format("{0} was successfully loaded!", Path.GetFileName(File_Path)));
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

                    if (translationWindow != null)
                    {
                        Translate(CopyTextToClipboard(TextUtility.ReplaceCommands(Editor.Text)));
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
                AutoSave_Path = DirectoryPath + "\\" + Path.GetFileNameWithoutExtension(File_Path) + "_AutoSave.bin";
                if (!IsBMG)
                {
                    AutoSaveTable_Path = DirectoryPath + "\\" + Path.GetFileNameWithoutExtension(Table_Path) + "_AutoSave.bin";
                    if (SaveMessageTable(AutoSave_Path, AutoSaveTable_Path))
                    {
                        SetStatusMessage(string.Format("Auto Save {0} was saved at {1}", Path.GetFileName(AutoSave_Path), DateTime.Now.ToString("h:mm:ss tt")));
                    }
                }
                else
                {
                    if (BMGUtility.Write(BMG_Struct, AutoSave_Path))
                    {
                        SetStatusMessage(string.Format("Auto Save {0} was saved at {1}", Path.GetFileName(AutoSave_Path), DateTime.Now.ToString("h:mm:ss tt")));
                    }
                }
            }
        }

        private void DeleteAutoSave()
        {
            if (File.Exists(AutoSave_Path))
            {
                File.Delete(AutoSave_Path);
            }

            if (File.Exists(AutoSaveTable_Path))
            {
                File.Delete(AutoSaveTable_Path);
            }
        }

        private bool SaveMessageTable(string SavePath, string SaveTablePath)
        {
            // Write Table File first
            try
            {
                using (FileStream Table_Stream = new FileStream(SaveTablePath, FileMode.OpenOrCreate))
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
                {
                    File.Delete(File_Path);
                }

                using (FileStream File_Stream = new FileStream(SavePath, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        File_Stream.Write(Entries[i].Data, 0, Entries[i].Data.Length);
                    }
                    File_Stream.Flush();
                }

                // Delete AutoSave on successful write
                DeleteAutoSave();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (File_Path != null)
            {
                if (!IsBMG)
                {
                    if (File.Exists(Table_Path))
                    {
                        File.Delete(Table_Path);
                    }

                    if (SaveMessageTable(File_Path, Table_Path))
                    {
                        SetStatusMessage(string.Format("{0} was saved at {1}", Path.GetFileName(File_Path), DateTime.Now.ToString("h:mm:ss tt")));
                    }
                    else
                    {
                        SetStatusMessage(string.Format("Failed to save file: {0}", Path.GetFileName(File_Path)));
                    }
                }
                else
                {
                    if (File.Exists(File_Path))
                    {
                        File.Delete(File_Path);
                    }

                    if (BMGUtility.Write(BMG_Struct, File_Path))
                    {
                        // Delete AutoSave on successful save
                        DeleteAutoSave();
                        SetStatusMessage(string.Format("{0} was saved at {1}", Path.GetFileName(File_Path), DateTime.Now.ToString("h:mm:ss tt")));
                    }
                    else
                    {
                        SetStatusMessage(string.Format("Failed to save file: {0}", Path.GetFileName(File_Path)));
                    }
                }
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (File_Path != null)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = Path.GetFileName(File_Path),
                    Filter = "Binary File|*.bin|All Files|*.*"
                };

                if (saveFileDialog.ShowDialog().Value)
                {
                    File_Path = saveFileDialog.FileName;
                    Save_Click(null, null);
                }
            }
        }

        private void ReportDumpProgress(int Index)
        {
            double Percent = 0;
            if (!IsBMG)
            {
                Percent = Index / (double)Entries.Length;
            }
            else
            {
                Percent = Index / (double)BMG_Struct.INF_Section.Items.Length;
            }
            ProgressBar.Value = Percent * 100;
        }

        private async void Dump_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                openFolderDialog.SelectedPath = "";
                int Index = 0;
                if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!IsBMG)
                    {
                        await Task.Run(() =>
                        {
                            for (int i = 0; i < Entries.Length; i++)
                            {
                                using (var stream = File.CreateText(openFolderDialog.SelectedPath + "\\" + i.ToString("X4") + ".txt"))
                                {
                                    stream.Write(Entries[i].Text.Replace("\n", "\r\n"));
                                }
                                Index++;
                                Dispatcher.Invoke(new Action(() => ReportDumpProgress(Index)));
                            }
                            SetStatusMessage(string.Format("Finshed dumping contents to: {0}", openFolderDialog.SelectedPath));
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            for (int i = 0; i < BMG_Struct.INF_Section.Items.Length; i++)
                            {
                                using (var stream = File.CreateText(openFolderDialog.SelectedPath + "\\" + i.ToString("X4") + ".txt"))
                                {
                                    stream.Write(BMG_Struct.INF_Section.Items[i].Text.Replace("\n", "\r\n"));
                                }
                                Index++;
                                Dispatcher.Invoke(new Action(() => ReportDumpProgress(Index)));
                            }
                            SetStatusMessage(string.Format("Finshed dumping contents to: {0}", openFolderDialog.SelectedPath));
                        });
                    }
                }
            }
        }

        private void Rebuild_Click(object sender, RoutedEventArgs e)
        {
            using (var SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = "" })
            {
                if (SelectFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && Directory.Exists(SelectFolderDialog.SelectedPath))
                {
                    RebuildDatabase(SelectFolderDialog.SelectedPath, true); // TODO: Remove true at some point (probably ask for which encoding they want)
                }
            }
        }

        private void UpdateTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
            {
                if (!string.IsNullOrEmpty(Editor.Text))
                {
                    string Text = Editor.Text.Replace("\r", string.Empty);

                    if (!IsBMG)
                    {
                        Resize(Entries[SelectedIndex], SelectedIndex, Text);
                        ((List<ListViewItem>)TextListView.ItemsSource)[SelectedIndex].Content = Entries[SelectedIndex].Text;
                    }
                    else
                    {
                        ResizeBMG(BMG_Struct.INF_Section.Items[SelectedIndex], SelectedIndex, Text);
                    }

                    if (IncrementEntryMenuItem.IsChecked)
                    {
                        nextButton_Click(null, null);
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

        private static readonly Regex ReplaceRegex = new Regex(@"<([^>]+)>");
        private static string CopyTextToClipboard(string text)
        {
            //var matches = ReplaceRegex.Matches(Text);
            // Get Furigana
            //Console.WriteLine(new Furigana("誰[だれ]").Reading);
            var strippedText = ReplaceRegex.Replace(text, "").Trim();
            Clipboard.SetText(strippedText);
            return strippedText;
        }

        private static bool IsHex(string text) => Regex.IsMatch(text, @"\A\b[0-9a-fA-F]+\b\Z");

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
                CopyTextToClipboard(TextUtility.ReplaceCommands(Entries[SelectedIndex].Text));
            }
            else if (SelectedIndex > -1 && IsBMG)
            {
                CopyTextToClipboard(TextUtility.ReplaceCommands(BMG_Struct.INF_Section.Items[SelectedIndex].Text));
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Entries != null || !string.IsNullOrEmpty(BMG_Struct.FileType))
            {
                if ((IsBMG && BMG_Struct.INF_Section.Items.Length > SelectedIndex + 1)
                    || (Entries != null && Entries.Length > SelectedIndex + 1))
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
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > 0 && (Entries != null || !string.IsNullOrEmpty(BMG_Struct.FileType)))
            {
                if (SelectedIndex - 1 > -1)
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
        }

        private void EntryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(File_Path) && !Changing_Selected_Entry && ushort.TryParse(EntryBox.Text, NumberStyles.AllowHexSpecifier, null, out ushort Entry_Index))
            {
                if (IsBMG)
                {
                    if (Entry_Index < BMG_Struct.INF_Section.MessageCount)
                    {
                        SelectedIndex = Entry_Index;
                        Editor.IsEnabled = true;
                        Changing_Selected_Entry = true;
                        Editor.Text = BMG_Struct.INF_Section.Items[Entry_Index].Text;
                        OffsetBox.Text = BMG_Struct.INF_Section.Items[Entry_Index].Text_Offset.ToString("X");
                        Changing_Selected_Entry = false;
                    }
                }
                else
                {
                    if (Entry_Index < Entries.Length)
                    {
                        SelectedIndex = Entry_Index;
                        Editor.IsEnabled = true;
                        Changing_Selected_Entry = true;
                        Editor.Text = Entries[SelectedIndex].Text;
                        OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                        Changing_Selected_Entry = false;
                    }
                }
                Scroll_to_Index(SelectedIndex);
            }
        }

        private void OffsetBox_TextChanged(object sender, TextChangedEventArgs e)
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
                Scroll_to_Index(SelectedIndex);
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

        private void UpdateTextPreview()
        {
            AFeRenderer.Reset(BMC_Colors);

            previewWindow.windowBackground.Source = TextRenderUtility.Convert(Properties.Resources.Dialog_Window);
            previewWindow.TextPreviews = Editor.Text.Split(new[] {"<Clear Text>"}, StringSplitOptions.None)
                .Select(page => AFeRenderer.RenderText(page, TextUtility.Doubutsu_no_Mori_Plus_Character_Map,
                    TextUtility.DnMe_Plus_Kanji_Bank_0, TextUtility.DnMe_Plus_Kanji_Bank_1, BMC_Colors))
                .Where(image => image != null).ToArray();
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            if (IsBMG)
            {
                if (previewWindow != null)
                {
                    UpdateTextPreview();
                }
            }
            else
            {
                // TODO: Check here for AF/AF+/AC
                if (previewWindow == null) return;

                previewWindow.windowBackground.Source = TextRenderUtility.Convert(Properties.Resources.Dialog_Window);
                previewWindow.TextPreviews = Editor.Text.Split(new[] {"<Clear Text>"}, StringSplitOptions.None)
                    .Select(page => ACRenderer.RenderText(page, TextUtility.Animal_Crossing_Character_Map))
                    .Where(image => image != null).ToArray();
            }

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

        // From https://stackoverflow.com/questions/11806764/adding-syntax-highlighting-rules-to-avalonedit-programmatically
        internal sealed class CustomizedBrush : HighlightingBrush
        {
            private readonly System.Windows.Media.SolidColorBrush brush;
            public CustomizedBrush(System.Windows.Media.Color color)
            {
                brush = CreateFrozenBrush(color);
            }

            public CustomizedBrush(System.Drawing.Color c)
            {
                var c2 = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
                brush = CreateFrozenBrush(c2);
            }

            public override System.Windows.Media.Brush GetBrush(ITextRunConstructionContext context)
            {
                return brush;
            }

            public override string ToString()
            {
                return brush.ToString();
            }

            private static System.Windows.Media.SolidColorBrush CreateFrozenBrush(System.Windows.Media.Color color)
            {
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.IsEnabled && !string.IsNullOrEmpty(Editor.Text))
            {
                if (previewWindow != null)
                {
                    try
                    {
                        previewWindow.Close();
                    }
                    catch { }
                    previewWindow = null;
                }

                if (previewWindow == null)
                {
                    previewWindow = new Forms.TextPreviewWindow();

                    previewWindow.Closed += (object s, EventArgs ea) =>
                    {
                        previewWindow = null;
                    };
                }

                previewWindow.CurrentPreview = 0;
                UpdateTextPreview();
                previewWindow.Show();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (IsBMG && BMG_Struct.INF_Section.Items.Length > 0)
            {
                SelectedIndex = 0;
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Editor.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text;
                OffsetBox.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset.ToString("X");
                EntryBox.Text = SelectedIndex.ToString("X4");
                Changing_Selected_Entry = false;
                Scroll_to_Index(SelectedIndex);
            }
            else if (Entries != null)
            {
                SelectedIndex = 0;
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Editor.Text = Entries[SelectedIndex].Text;
                OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                EntryBox.Text = SelectedIndex.ToString("X4");
                Changing_Selected_Entry = false;
                Scroll_to_Index(SelectedIndex);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (IsBMG && BMG_Struct.INF_Section.Items.Length > 0)
            {
                SelectedIndex = BMG_Struct.INF_Section.Items.Length - 1;
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Editor.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text;
                OffsetBox.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset.ToString("X");
                EntryBox.Text = SelectedIndex.ToString("X4");
                Changing_Selected_Entry = false;
                Scroll_to_Index(SelectedIndex);
            }
            else if (Entries != null)
            {
                SelectedIndex = Entries.Length - 1;
                Editor.IsEnabled = true;
                Changing_Selected_Entry = true;
                Editor.Text = Entries[SelectedIndex].Text;
                OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
                EntryBox.Text = SelectedIndex.ToString("X4");
                Changing_Selected_Entry = false;
                Scroll_to_Index(SelectedIndex);
            }
        }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1 && Entries != null)
            {
                Translate(CopyTextToClipboard(TextUtility.ReplaceCommands(Entries[SelectedIndex].Text)));
            }
            else if (SelectedIndex > -1 && IsBMG)
            {
                Translate(CopyTextToClipboard(TextUtility.ReplaceCommands(BMG_Struct.INF_Section.Items[SelectedIndex].Text)));
            }
        }

        private void connectionTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Editor.IsEnabled = true;
            ushort Index = (ushort)(e.NewValue as TreeViewItem).Tag;
            Changing_Selected_Entry = true;

            if (IsBMG && Index < BMG_Struct.INF_Section.Items.Length)
            {
                SelectedIndex = Index;
                Editor.Text = BMG_Struct.INF_Section.Items[SelectedIndex].Text;
                OffsetBox.Text = (BMG_Struct.INF_Section.Items[SelectedIndex].Text_Offset).ToString("X");
            }
            else if (Entries != null && Index < Entries.Length)
            {
                SelectedIndex = Index;
                Editor.Text = Entries[SelectedIndex].Text;
                OffsetBox.Text = Entries[SelectedIndex].Offset.ToString("X");
            }

            EntryBox.Text = SelectedIndex.ToString("X");
            Changing_Selected_Entry = false;
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            GenerateDialogTree();
            textTab.Visibility = Visibility.Visible;
            connectionTab.Visibility = Visibility.Visible;
            SetStatusMessage("Dialog Connection Tree was successfully generated!");
        }

        private async void SetCharacterSet(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;

            var lastSelected = SelectedCharacterSet;

            if (menuItem == PopulationGrowingItem)
            {
                SelectedCharacterSet = CharacterSet.AnimalCrossing;
                TextUtility.CharacterMap = TextUtility.Animal_Crossing_Character_Map;
            }
            else if (menuItem == DoubutsuNoMoriItem)
            {
                SelectedCharacterSet = CharacterSet.DoubutsuNoMoriPlus; // TODO: DnM, DnM+, and DnMe+ all have slightly different character sets.
                TextUtility.CharacterMap = TextUtility.Doubutsu_no_Mori_Plus_Character_Map;
            }
            else if (menuItem == WildWorldItem)
            {
                SelectedCharacterSet = CharacterSet.WildWorld;
                TextUtility.CharacterMap = TextUtility.WildWorldCharacterMap; // TODO: Wild World needs its own map. It's different than user text.
            }
            else
            {
                throw new ArgumentException("sender was not a character set menu item!");
            }

            foreach (var item in _characterSetItems)
            {
                item.IsChecked = item == menuItem;
            }

            // Don't bother reevaluating entries if the character set is unchanged.
            if (lastSelected == SelectedCharacterSet) return;

            // Reevaluate the entries
            if (IsBMG && !string.IsNullOrEmpty(File_Path))
            {
                BMG_Struct = await BMGUtility.Decode(File_Path, BMC_Colors);
                Generate_BMG_Text_Entries(BMG_Struct);
            }
            else
            {
                Generate_Text_Entries();
            }
        }
    }
}
