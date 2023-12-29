using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal_Hangman
{
    public class HangmanWord
    {
        public string Word { get; set; }
        public bool WordIsValid { get => !Word.IsNullOrWhiteSpace(); }
        public string LastLifeHint { get; set; } // is the second hint to be displayed. Meaning when they have 1 guesses left 
        public bool HasLastLifeHint { get => !LastLifeHint.IsNullOrWhiteSpace(); }
        public string SecondToLastHint { get; set; } // is the first hint to be displayed. Meaning when they have 2 guesses left
        public bool HasSecondToLastHint { get => !SecondToLastHint.IsNullOrWhiteSpace(); }

        public HangmanWord(string HangManSelectionFromFile) 
        {
            string[] word_hint1_hint2 = HangManSelectionFromFile.Split(',');

            Word = word_hint1_hint2[0].ToLowerInvariant().Trim();

            if(word_hint1_hint2.Length > 0)
            {
                LastLifeHint = word_hint1_hint2[1].Trim();
            }

            if(word_hint1_hint2.Length > 1)
            {
                SecondToLastHint = word_hint1_hint2[2].Trim();
            }
        }
    }
}
