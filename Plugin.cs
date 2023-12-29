using BepInEx;
using TerminalApi;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine.Assertions.Must;

namespace Terminal_Hangman
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class TerminalHangmanBase : BaseUnityPlugin
    {
        private const string modGUID = "yahwehne.Terminal_Hangman";
        private const string modName = "Terminal Hangman";
        private const string modVersion = "1.0.0.0";

        //private readonly Harmony harmony = new Harmony(modGUID);

        private static TerminalHangmanBase Instance;

        private static string hangmanEvent = "HangManGuess";
        private static string hangmanViewEvent = "HangManView";
        private static string hangmanResetEvent = "HangManReset";
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Logger.LogInfo("\n\n +++++ we are starting hangman \n\n");

            TerminalParsedSentence += TextSubmitted;

            HangmanGame.GetAHangman();

            TerminalNode guessStartNode = CreateTerminalNode($"Starting hangman\n\n{HangmanGame.CreateHangedMan()}", true, hangmanEvent);
            TerminalNode viewStartNode = CreateTerminalNode($"Starting hangman\n\n{HangmanGame.CreateHangedMan()}", true, hangmanViewEvent);
            TerminalNode resetNode = CreateTerminalNode($"Starting hangman\n\n{HangmanGame.StartAHangMan()}", true, hangmanResetEvent);

            TerminalKeyword verbKeyword = CreateTerminalKeyword("hangman", true);
            TerminalKeyword guessNounKeyword = CreateTerminalKeyword("guess");
            TerminalKeyword viewNounKeyword = CreateTerminalKeyword("hangman_observe");
            TerminalKeyword resetNountKeyword = CreateTerminalKeyword("hangman_reset");

            verbKeyword = verbKeyword.AddCompatibleNoun(viewNounKeyword, viewStartNode);
            viewNounKeyword.defaultVerb = verbKeyword;

            verbKeyword = verbKeyword.AddCompatibleNoun(guessNounKeyword, guessStartNode);
            guessNounKeyword.defaultVerb = verbKeyword;

            verbKeyword = verbKeyword.AddCompatibleNoun(resetNountKeyword, resetNode);
            resetNountKeyword.defaultVerb = verbKeyword;

            AddTerminalKeyword(verbKeyword);
            AddTerminalKeyword(guessNounKeyword);
            AddTerminalKeyword(viewNounKeyword);
            AddTerminalKeyword(resetNountKeyword);
        }

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
            if(e.ReturnedNode.terminalEvent == hangmanEvent)
            {
                // maybe look at terminalEvent to see if we should start or make a guess.
                Logger.LogMessage($"Node displayText Returned: {e.ReturnedNode.displayText}");
                Logger.LogMessage($"Node terminalEvent Returned: {e.ReturnedNode.terminalEvent}");
                Logger.LogMessage($"Node clearPreviousText Returned: {e.ReturnedNode.clearPreviousText}");
                Logger.LogMessage($"Node maxCharactersToType Returned: {e.ReturnedNode.maxCharactersToType}");
                Logger.LogMessage($"Node overrideOptions Returned: {e.ReturnedNode.overrideOptions}");
                Logger.LogMessage($"Node terminalOptions Returned: {e.ReturnedNode.terminalOptions}");

                string TheGuessedLettersOutput = TheGuessedLetters(e.SubmittedText);

                string theNewOutput = HangmanGame.GuessCharacter(TheGuessedLettersOutput);
                e.ReturnedNode.displayText = theNewOutput;
            }
            else if(e.ReturnedNode.terminalEvent == hangmanViewEvent)
            {
                e.ReturnedNode.displayText = HangmanGame.CreateHangedMan();
            }
            else if(e.ReturnedNode.terminalEvent == hangmanResetEvent)
            {
                Logger.LogMessage($"hangmanResetEvent: {HangmanGame.StartAHangMan()}");
                e.ReturnedNode.displayText = $"Starting hangman\n\n{HangmanGame.StartAHangMan()}";
            }
        }

        private string TheGuessedLetters(string submittedText)
        {
            string guessLiteral = "guess ";

            int guessIndex = submittedText.IndexOf(guessLiteral);

            if (guessIndex == -1)
            {
                return "";
            }

            return submittedText.Substring(guessIndex + guessLiteral.Length).ToLowerInvariant();
        }

        /*private void OnBeginUsing(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Player has just started using the terminal");
        }

        private void BeganUsing(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Player is using terminal");
        }*/

    }
}
