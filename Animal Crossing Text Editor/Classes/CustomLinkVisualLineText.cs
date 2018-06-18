using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Animal_Crossing_Text_Editor
{
    /// <summary>
    /// VisualLineElement that represents a piece of text and is a clickable link.
    /// </summary>
    public class CustomLinkVisualLineText : VisualLineText
    {

        public delegate void CustomLinkClickHandler(string link);

        public event CustomLinkClickHandler CustomLinkClicked;

        private string Link { get; set; }

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click the link.
        /// The default value is true.
        /// </summary>
        public bool RequireControlModifierForClick { get; set; }

        /// <summary>
        /// Creates a visual line text element with the specified length.
        /// It uses the <see cref="ITextRunConstructionContext.VisualLine"/> and its
        /// <see cref="VisualLineElement.RelativeTextOffset"/> to find the actual text string.
        /// </summary>
        public CustomLinkVisualLineText(string theLink, VisualLine parentVisualLine, int length)
            : base(parentVisualLine, length)
        {
            RequireControlModifierForClick = true;
            Link = theLink;
        }


        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            TextRunProperties.SetForegroundBrush(Brushes.LightBlue);
            TextRunProperties.SetTextDecorations(TextDecorations.Underline);
            return base.CreateTextRun(startVisualColumn, context);
        }

        bool LinkIsClickable()
        {
            if (string.IsNullOrEmpty(Link))
                return false;
            if (RequireControlModifierForClick)
                return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            else
                return true;
        }


        protected override void OnQueryCursor(QueryCursorEventArgs e)
        {
            if (LinkIsClickable())
            {
                e.Handled = true;
                e.Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !e.Handled && LinkIsClickable())
            {

                if (CustomLinkClicked != null)
                {
                    CustomLinkClicked(Link);
                    e.Handled = true;
                }

            }
        }

        protected override VisualLineText CreateInstance(int length)
        {

            var a = new CustomLinkVisualLineText(Link, ParentVisualLine, length)
            {
                RequireControlModifierForClick = RequireControlModifierForClick
            };

            a.CustomLinkClicked += link => MainWindow.Reference.Goto(ushort.Parse(Link, System.Globalization.NumberStyles.HexNumber, null));
            return a;
        }
    }
}
