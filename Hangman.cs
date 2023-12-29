using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Terminal_Hangman
{
    public static class HangmanGame
    {
        private static System.Random random = new System.Random();

        private static string HangmanFilesDirectoryPath = Application.persistentDataPath + "/Resources/";

        private static HashSet<char> GuessedCharacters = new HashSet<char>();

        public static string GuessCharactersString { get => String.Join(", ", GuessedCharacters); }

        private static int NumberOfIncorrectGuesses { get; set; }

        private static int MAX_INCORRECT_GUESSES = 6;

        private static HangmanWord _CurrentWordInfo { get; set; }
        public static HangmanWord CurrentWordInfo { get => _CurrentWordInfo; }

        private static string _CurrentWordUnderscores { get; set; }

        public static string CurrentWordUnderscores { get => _CurrentWordUnderscores; }

        private static bool GameIsBeingPlayed {  get; set; }

        /// <summary>
        /// Returns a psuedo-random file from the directory.
        /// </summary>
        /// <returns>file path</returns>
        private static string GetAFile()
        {
            if (Directory.Exists(HangmanFilesDirectoryPath))
            {
                DirectoryInfo hangmanFilesDirectoryInfo = new DirectoryInfo(HangmanFilesDirectoryPath);
                FileInfo[] hangmanFiles = hangmanFilesDirectoryInfo.GetFiles();

                int num = random.Next(0, hangmanFiles.Length);

                return hangmanFiles[num].DirectoryName;
            }
            else
            {
                Directory.CreateDirectory(HangmanFilesDirectoryPath);
                return HangmanFilesDirectoryPath;
            }
        }

        /// <summary>
        /// The load function to start a new game
        /// </summary>
        public static void GetAHangman()
        {
            // need to get the file path

            string[] HangManLines = File.ReadAllLines(GetAFile());
            int wordSelectedIndex = random.Next(0, HangManLines.Length);

            string hangManWordSelected = HangManLines[wordSelectedIndex]; // should be word, hint1, hint2

            //string hangManWordSelected = "test, hint 1, hint 2";

            _CurrentWordInfo = new HangmanWord(hangManWordSelected); // update the word info
            _CurrentWordUnderscores = CreateUnderscoredWord(CurrentWordInfo.Word);
            
            GuessedCharacters.Clear();
            GameIsBeingPlayed = true;
        }

        public static string StartAHangMan()
        {
            GetAHangman();
            return $"Reseting hangman\n\n{CreateHangedMan()}";
        }

        private static string CreateUnderscoredWord(string word)
        {
            string underscoredWord = "";

            foreach (char character in word)
            {
                if(character.IsLetter()) // the word is always lowercase
                {
                    underscoredWord += '_';
                }
                else
                {
                    underscoredWord += character;
                }
            }

            return underscoredWord;
        }

        public static string GuessCharacter(string characters)
        {
            if (GameIsBeingPlayed)
            {
                foreach (char character in characters.ToLower())
                {

                    if (GuessedCharacters.Contains(character))
                    {
                        // already guessed
                        continue;
                    }

                    if (!character.IsLetter())
                    {
                        // invalid guess;
                        continue;
                    }

                    GuessedCharacters.Add(character);

                    if (CurrentWordInfo.Word.Contains(character))
                    {
                        // this is a good guess
                        UpdateUnderscoredWord(character);
                    }
                    else
                    {
                        // bad guess, bozo
                        NumberOfIncorrectGuesses++;
                    }

                    if (NumberOfIncorrectGuesses >= MAX_INCORRECT_GUESSES)
                    {
                        // you lose
                        GameIsBeingPlayed = false;
                        return "you lose\n" + _getHowToReset();
                    }

                    if (CurrentWordUnderscores == CurrentWordInfo.Word)
                    {
                        // you win
                        GameIsBeingPlayed = false;
                        return "you win\n" + _getHowToReset();
                    }
                }
            }
            else
            {
                if (NumberOfIncorrectGuesses >= MAX_INCORRECT_GUESSES)
                {
                    // you lose
                    GameIsBeingPlayed = false;
                    return CreateHangedMan() + "\nyou lose\n" + _getHowToReset();
                }

                if (CurrentWordUnderscores == CurrentWordInfo.Word)
                {
                    // you win
                    GameIsBeingPlayed = false;
                    return "you win\n" + _getHowToReset();
                }
            }

            return CreateHangedMan();
        }

        /// <summary>
        /// generates the hangman to be displayed to the player based on the current state of the game;
        /// </summary>
        /// <returns></returns>
        public static string CreateHangedMan()
        {
            string hangedMan = $"   |-----|" +
                               $"\n   |     {_createHead()}" +
                               $"\n   |    {_createTorso()}" +
                               $"\n   |    {_createLegs()}" +
                               $"\n   |" +
                               $"\n  ---" +
                               $"\n" +
                               $"\n Previous Guesses: {GuessCharactersString}" +
                               $"\n {CurrentWordUnderscores}" +
                               $"\n" +
                               $"\n{_getSecondToLastHint()}" +
                               $"\n{_getLastChanceHint()}" +
                               $"\n" +
                               $"\n";

            return hangedMan;
        }

        /// <summary>
        /// updates the underscored word with the letter provided.
        /// </summary>
        /// <param name="guessedCharacter">Lowercase letter</param>
        private static void UpdateUnderscoredWord(char guessedCharacter)
        {
            string underscoredWord = "";

            for (int i = 0; i < CurrentWordInfo.Word.Length; i++)
            {
                if (CurrentWordInfo.Word[i] == guessedCharacter)
                {
                    underscoredWord += guessedCharacter;
                }
                else
                {
                    underscoredWord += _CurrentWordUnderscores[i];
                }
            }

            _CurrentWordUnderscores = underscoredWord;
        }

        #region body parts and hints
        private static string _createHead()
        {
            return NumberOfIncorrectGuesses >= 1 ? "o" : ""; 
        }

        private static string _createTorso()
        {
            string torso = "";
            
            switch (NumberOfIncorrectGuesses)
            { 
                case 2:
                    torso += " | ";
                    break;
                case 3:
                    torso += "/| ";
                    break;
                case 4:
                case 5:
                case 6:
                    torso += "/|\\";
                    break;
            }
            
            return torso;
        }

        private static string _createLegs()
        {
            string legs = "";

            switch (NumberOfIncorrectGuesses)
            {
                case 5:
                    legs += "/";
                    break;
                case 6:
                    legs += "/ \\";
                    break;
            }

            return legs;
        }

        private static string _getSecondToLastHint()
        {
            if(NumberOfIncorrectGuesses >= MAX_INCORRECT_GUESSES - 2 && CurrentWordInfo.HasSecondToLastHint)
            {
                return $"{_getHintLabel()} {CurrentWordInfo.SecondToLastHint}";
            }

            return "";
        }

        private static string _getLastChanceHint()
        {
            if(NumberOfIncorrectGuesses >= MAX_INCORRECT_GUESSES - 1 && CurrentWordInfo.HasLastLifeHint)
            {
                return $"{_getHintLabel()} {CurrentWordInfo.LastLifeHint}";
            }

            return "";
        }

        private static string _getHintLabel()
        {
            string initialHintLabel = "Here is a hint:";
            string secondHintLabel = "Here is another hint:";

            if (CurrentWordInfo.HasSecondToLastHint)
            {
                return initialHintLabel;
            }

            if(!CurrentWordInfo.HasSecondToLastHint && CurrentWordInfo.HasLastLifeHint)
            {
                return initialHintLabel;
            }

            return secondHintLabel;
        }

        private static string _getHowToReset()
        {
            return "\nTo start a new hangman game, type 'hangman_reset'\nIf you want to see the gameboard type 'hangman_observe'\n";
        }
        #endregion

    }

    public static class CharExtension
    { 
        public static bool IsLetter(this char ch) 
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }
    }
}
