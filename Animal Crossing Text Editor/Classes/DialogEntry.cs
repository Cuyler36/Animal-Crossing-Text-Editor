using System.Collections.Generic;

namespace Animal_Crossing_Text_Editor
{
    public class DialogEntry
    {
        public DialogEntry Parent; // The parent node of this entry (NOTE: Multiple entries can have the same EntryId!)
        public List<DialogEntry> Children; // All the child dialogs that spawn off this dialog (NOTE: Does not include dialogs that the game sets!)
        public ushort EntryId; // Corresponding entry id

        public DialogEntry()
        {
            Children = new List<DialogEntry>();
        }

        public bool HasDescendant(DialogEntry entry)
        {
            if (entry == this)
            {
                return false;
            }

            if (Children != null)
            {
                foreach(DialogEntry child in Children)
                {
                    if (child.EntryId == EntryId || child.HasDescendant(entry))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasAncestor(DialogEntry parent)
        {
            if (parent == this || parent == null)
            {
                return false;
            }

            if (Parent.EntryId == EntryId || Parent.HasAncestor(parent))
            {
                return true;
            }

            return false;
        }

        public bool HasAncestor(ushort EntryId)
        {
            if (Parent == null)
            {
                return false;
            }

            if (Parent.EntryId == EntryId || Parent.HasAncestor(EntryId))
            {
                return true;
            }

            return false;
        }
    }
}
