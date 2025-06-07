using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace MilaPuzzleChat
{
    public class MilaPuzzleChat : RocketPlugin<Config>
    {
        public static MilaPuzzleChat Instance { get; private set; }
        public bool IsGameRunning { get; private set; }
        public string CurrentWord { get; private set; }

        private const string SenderName = "MilaPuzzleChat";
        private const string ChatIconUrl = "https://imgur.com/LK914gE.png";

        private List<string> _parsedWords;
        private Coroutine _gameCoroutine;
        private System.Random _random;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "game_start", "Unscramble the word: {0} - Use /r <word>" },
            { "correct_guess", "{0} correctly guessed the word '{1}' and won a prize!" },
            { "no_game_running", "There is no puzzle game in progress right now." },
            { "round_timeout", "Time's up! The word was '{0}'. A new round will start soon." },
            { "incorrect_syntax", "Incorrect syntax. Use: /r <word>" }
        };

        protected override void Load()
        {
            Instance = this;
            IsGameRunning = false;
            _random = new System.Random();

            Logger.Log("");
            Logger.Log("=================================================");
            Logger.Log("");
            Logger.Log("* *");
            Logger.Log("*         ███╗   ███╗      *");
            Logger.Log("*         ████╗ ████║      *");
            Logger.Log("*         ██╔████╔██║      *");
            Logger.Log("*         ██║╚██╔╝██║      *");
            Logger.Log("*         ██║ ╚═╝ ██║      *");
            Logger.Log("*         ╚═╝     ╚═╝      *");
            Logger.Log("");
            Logger.Log("-------------------------------------------------");
            Logger.Log("    Plugin: MilaPuzzleChat v1.0");
            Logger.Log("    Created by: Mila");
            Logger.Log("    Contact: milahosting.com");
            Logger.Log("=================================================");
            Logger.Log("");

            if (Configuration.Instance.Words != null)
            {
                _parsedWords = Configuration.Instance.Words.Split(',')
                                      .Select(w => w.Trim())
                                      .Where(w => !string.IsNullOrEmpty(w))
                                      .ToList();
            }
            else
            {
                _parsedWords = new List<string>();
            }

            Logger.Log($">> Words Loaded: {_parsedWords.Count}");

            if (Configuration.Instance.Enabled)
            {
                _gameCoroutine = StartCoroutine(GameLoop());
                Logger.Log(">> MilaPuzzleChat Status: Enabled and running.");
            }
            else
            {
                Logger.Log(">> MilaPuzzleChat Status: Disabled in config.");
            }
        }

        protected override void Unload()
        {
            Instance = null;
            if (_gameCoroutine != null) StopCoroutine(_gameCoroutine);
            IsGameRunning = false;
            Logger.Log("MilaPuzzleChat plugin unloaded.");
        }

        private IEnumerator GameLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Configuration.Instance.GameIntervalSeconds);
                if (IsGameRunning || Provider.clients.Count < 1) continue;
                StartRound();
            }
        }

        private void StartRound()
        {
            if (_parsedWords == null || _parsedWords.Count == 0)
            {
                Logger.LogError("Word list is empty! Cannot start a puzzle round.");
                return;
            }

            IsGameRunning = true;
            int randomIndex = _random.Next(0, _parsedWords.Count);
            CurrentWord = _parsedWords[randomIndex];
            string scrambledWord = ScrambleWord(CurrentWord);

            string startMessage = Translate("game_start", scrambledWord);

            string fullMessage = $"<color=#00FFFF>[{SenderName}]</color> {startMessage}";
            ChatManager.serverSendMessage(fullMessage, Color.white, null, null, EChatMode.GLOBAL, ChatIconUrl, true);

            StartCoroutine(RoundTimeout());
        }

        private IEnumerator RoundTimeout()
        {
            yield return new WaitForSeconds(Configuration.Instance.RoundTimeLimitSeconds);
            if (IsGameRunning)
            {
                string timeoutMessage = Translate("round_timeout", CurrentWord);

                string fullMessage = $"<color=yellow>[{SenderName}]</color> {timeoutMessage}";
                ChatManager.serverSendMessage(fullMessage, Color.white, null, null, EChatMode.GLOBAL, ChatIconUrl, true);

                EndRound();
            }
        }

        public void OnPlayerGuessedCorrectly(UnturnedPlayer player)
        {
            if (!IsGameRunning) return;

            string winMessage = Translate("correct_guess", player.CharacterName, CurrentWord);

            string fullMessage = $"<color=green>[{SenderName}]</color> {winMessage}";
            ChatManager.serverSendMessage(fullMessage, Color.white, null, null, EChatMode.GLOBAL, ChatIconUrl, true);

            player.Experience += Configuration.Instance.ExperienceReward;
            foreach (var item in Configuration.Instance.Rewards)
            {
                player.GiveItem(item.Id, item.Amount);
            }
            EndRound();
        }

        private void EndRound()
        {
            IsGameRunning = false;
            CurrentWord = null;
        }

        private string ScrambleWord(string word)
        {
            char[] chars = word.ToCharArray();
            string original = word;
            string scrambled;
            do
            {
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = _random.Next(0, i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }
                scrambled = new string(chars);
            } while (scrambled == original && word.Length > 1);
            return scrambled;
        }
    }
}