﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animal_Crossing_Text_Editor
{
    public static class ContDescriptions
    {
        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { "<End Conversation>", "Forces the conversation to end, even if there is more text after" },
            { "<Goto Jump Entry>", "Makes the game go to the dialog specified by <Set Jump Entry []>" },
            { "<New Page>", "Causes all text after to be drawn on a blank textbox" },
            { "<Pause []>", "Pauses for the specified time in game ticks" },
            { "<Press A>", "Waits for button input before continuing the dialog" },
            { "<Color Line []>", "Colors the entire line with the color specified after. Color is in hex.\nExample: #12FF6A" },
            { "<Instant Text Skip>", "Skips text delay until the next <Press A>. If there is no <Press A> then it will close the text box." },
            { "<Unskippable>", "Until the next <Press A> text won't be able to be sped up or skipped" },
            { "<Player Emotion [{0}] [{1}]>", "Causes an expression/action to occur for the player. The first argument is the emotion/action, and the second is the modifier." },
            { "<Expression []>", "Causes an expression/action to occur. TODO: List of emotions" },
            { "<Open Choice Selection Menu>", "Opens the choice selection menu" },
            { "<Set Jump Entry []>", "Sets the jump entry to the specified message id. Message id is in hex." },
            { "<Choice #1 MessageId []>", "Sets the message to be displayed after selecting choice #1. Message id is in hex." },
            { "<Choice #2 MessageId []>", "Sets the message to be displayed after selecting choice #2. Message id is in hex." },
            { "<Choice #3 MessageId []>", "Sets the message to be displayed after selecting choice #3. Message id is in hex." },
            { "<Choice #4 MessageId []>", "Sets the message to be displayed after selecting choice #4. Message id is in hex." },
            // 13 - 15 here when known
            { "<Set 2 Choices [] []>", "Sets two choices for the next choice selection. Choices come from select.bin and the Choice Ids are in hex." },
            { "<Set 3 Choices [] [] []>", "Sets three choices for the next choice selection. Choices come from select.bin and the Choice Ids are in hex." },
            { "<Set 4 Choices [] [] [] []>", "Sets four choices for the next choice selection. Choices come from select.bin and the Choice Ids are in hex." },
            { "<Force Switch Dialog>", "Forces the dialog to switch to the next dialog set." },
            { "<Player Name>", "Displays the currently selected player's name" },
            { "<NPC Name>", "Displays the name of the NPC who is being talked to" },
            { "<Catchphrase>", "Displays the catchphrase of the NPC who is being talked to, if they have one" },
            { "<Year>", "Displays the current year as set in the game" },
            { "<Month>", "Displays the current month as set in the game" },
            { "<Day of Week" , "Displays the current day's name as set in the game" },
            { "<Day>", "Displays the current day of the month as set in the game" },
            { "<Hour>", "Displays the current hour as set in the game" },
            { "<Minute>", "Displays the current minute as set in the game" },
            { "<Second>", "Displays the current second as set in the game" },
            { "<String 0>", "Pulls the first string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 1>", "Pulls the second string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 2>", "Pulls the third string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 3>", "Pulls the fourth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 4>", "Pulls the fifth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 5>", "Pulls the sixth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 6>", "Pulls the seventh string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 7>", "Pulls the eighth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 8>", "Pulls the ninth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 9>", "Pulls the tenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<Last Choice Selected>", "Displays the choice the player last chose" },
            { "<Town Name>", "Displays the town's name" },
            { "<Random Number>", "Displays a random number between 0 and 99" },
            { "<Item 0>", "Pulls an item name from the game's stack. The item is set by the code which runs dialog" },
            { "<Item 1>", "Pulls an item name from the game's stack. The item is set by the code which runs dialog" },
            { "<Item 2>", "Pulls an item name from the game's stack. The item is set by the code which runs dialog" },
            { "<Item 3>", "Pulls an item name from the game's stack. The item is set by the code which runs dialog" },
            { "<Item 4>", "Pulls an item name from the game's stack. The item is set by the code which runs dialog" },
            { "<String 10>", "Pulls the eleventh string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 11>", "Pulls the twelth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 12>", "Pulls the thirteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 13>", "Pulls the fourteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 14>", "Pulls the fifteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 15>", "Pulls the sixteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 16>", "Pulls the seventeenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 17>", "Pulls the eighteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 18>", "Pulls the ninteenth string from the game's stack. The string is set by the code which runs dialog" },
            { "<String 19>", "Pulls the twentyth string from the game's stack. The string is set by the code which runs dialog" },
            { "<Adjective>", "Pulls an adjective from the game's stack. The adjective is set by the code which runs dialog" },
            { "<Other Town>", "Pulls another Town's name from the game's stack. Used in mail. The nam eis set by the code which runs dialog" },
            { "<Neutral Luck>", "Resets the player's luck state." },
            { "<Relationship Luck>", "Makes villagers of the opposite gender more friendly towards the player." },
            { "<Unpopular Luck>", "Makes villagers unfriendly towards the player." },
            { "<Bad Luck>", "Causes the player to have bad luck. This will make the player trip." },
            { "<Bell Luck>", "Bell drops are of a higher value and items sell for more." },
            { "<Item Luck>", "Causes the player to find rarer items." },
            { "<Color [] Characters []", "Colors a set number of characters the specified hex color code" },
            { "<Actor Speech Type []>", "Sets the current NPC's speech type for the dialog" },
            { "<Next Character Size []>", "Sets the next character's size" },
            { "<Play Music []>", "Plays the specified music track" },
            { "<Stop Music []>", "Stops the specified music track" },
            { "<End Conversation after []>" , "Ends the conversation after the specified amount of ticks" },
            { "<Play Sound Effect []>", "Plays the specified sound effect" },
            { "<Line Size []>", "Sets the text size for a whole line" },
            { "<Main Nookling>", "Specifies the text should be said by the main Nookling when at Nookington's" },
            { "<Secondary Nookling>", "Specifies the text should be said by the secondary Nookling when at Nookington's" },
            { "<B Button Selects Last Choice>", "If the B Button is pressed, the last choice option will be selected" },
            { "<Character Margin []>", "Sets additional spacing before the next character" },
            { "<Island Name>", "Displays the name of the island" },
            { "<Transferred Item>", "Specifies an item will be a transferred one. Used with villagers buying/giving items" },
            { "<AM/PM>", "Displays either AM or PM depending on the time of day set in game" },
            { "<Choice #5 MessageId []>", "Sets the message to be displayed after selecting choice #5. Message id is in hex." },
            { "<Choice #6 MessageId []>", "Sets the message to be displayed after selecting choice #6. Message id is in hex." },
            { "<Set 5 Choices [] [] [] [] []>", "Sets five choices for the next choice selection. Choices come from select.bin and the Choice Ids are in hex." },
            { "<Set 6 Choices [] [] [] [] [] []>", "Sets six choices for the next choice selection. Choices come from select.bin and the Choice Ids are in hex." },
        };
    }
}
