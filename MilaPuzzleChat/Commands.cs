using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MilaPuzzleChat
{
    public class Commands : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "r";
        public string Help => "Guess the word for the MilaPuzzleChat game.";
        public string Syntax => "<word>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var plugin = MilaPuzzleChat.Instance;
            var player = (UnturnedPlayer)caller;

            if (!plugin.IsGameRunning)
            {
                UnturnedChat.Say(player, plugin.Translate("no_game_running"), Color.red);
                return;
            }

            if (command.Length != 1)
            {
                UnturnedChat.Say(player, plugin.Translate("incorrect_syntax"), Color.red);
                return;
            }

            string guess = command[0];

            if (guess.Equals(plugin.CurrentWord, StringComparison.InvariantCultureIgnoreCase))
            {
                plugin.OnPlayerGuessedCorrectly(player);
            }
        }
    }
}