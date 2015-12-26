//
//  AntiCombatLog.cs
//
//  Author: False_Chicken
//  Contact: jmdevsupport@gmail.com
//
//  Copyright (c) 2015 False_Chicken
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Rocket;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Core.Logging;

using SDG.Unturned;

using UnityEngine;

using Steamworks;

namespace FC.AntiCombatLog
{
	public class AntiCombatLog : RocketPlugin<AntiCombatLogConfiguration>
	{

		#region STORAGE VARIABLES

		private string serverFolder;

		private List<CSteamID> combatLoggers;

		private Dictionary<byte, DateTime> lastUsedCharMap; //Used to store the players last used char for inventory deletion.

		private CombatLogPlayerComponent tmpComponent;

		private InventoryHelper invHelper;

		public static AntiCombatLog Instance;

		public override TranslationList DefaultTranslations
		{
			get
			{
				return new TranslationList() {
					{ "punish_player_player", "You where punished for combat logging. Inventory cleared."},
					{ "combat_logger_chat", "{0} is a combat logger!"},
					{ "combat_logger_console", "has combat logged!"},
					{ "injured_warning_player", "You have been injured. Please wait {0} seconds before disconnecting to avoid being punished."},
					{ "safe_to_disconnect_player" , "It is now safe to disconnect."},
					{ "seconds_remaining_player" , "{0} seconds remaining until safe logout allowed."},
					{ "bleeding_player", "You are bleeding! Stop bleeding to allow safe logout."},
					{ "version_console","Version {0} Loaded."}
				};
			}
		}

		#endregion

		#region ROCKET FUNCTIONS

		protected override void Load()
		{
			Instance = this;

			invHelper = new InventoryHelper();

			serverFolder = System.IO.Directory.GetParent(System.Environment.CurrentDirectory).ToString();

			combatLoggers = new List<CSteamID>();

			lastUsedCharMap = new Dictionary<byte, DateTime>();

			U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
			U.Events.OnPlayerConnected += OnPlayerConnected;

			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive += OnPlayerRespawn;
			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDead += OnPlayerDead;

			ShowVersionMessage();
		}

		protected override void Unload()
		{
			Logger.LogWarning("Unloading...");

			U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
			U.Events.OnPlayerConnected -= OnPlayerConnected;

			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive -= OnPlayerRespawn;
			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDead -= OnPlayerDead;
		}

		#endregion

		#region PLUGIN COMBAT LOGGER FUNCTIONS

		/**
		 * Adds a combat logger to the list, prints their name to the server console,
		 * "Kills" the player so they drop their loot, and prints the combat loggers
		 * name to the global chat if configured to do so.
		 */
		private void ProcessCombatLogger(UnturnedPlayer _player)
		{
			AddPlayerToCombatLoggersList(_player.CSteamID);

			ShowCombatLoggerToConsole(_player);

			_player.Damage(255, _player.Position, EDeathCause.PUNCH, ELimb.SKULL, _player.CSteamID); //Drop player items.

			if (this.Configuration.Instance.ShowCombatLogMessagesToGlobalChat) ShowCombatLoggerMessageToChat(_player);

			if (this.Configuration.Instance.DeletePlayerInventoryData) DeletePlayerData(_player.CSteamID);

		}

		/**
		 * When the combat logger is "Killed" by ProcessCombatLogger they are added to a list to clear
		 * their inventory when they come back. This is because clearing their inventory when they
		 * are disconnecting doesnt work.
		 */
		private void ProcessReturningCombatLogger(CSteamID _playerID)
		{
			tmpComponent = UnturnedPlayer.FromCSteamID(_playerID).GetComponent<CombatLogPlayerComponent>();

			invHelper.ClearItems(UnturnedPlayer.FromCSteamID(_playerID));
			invHelper.ClearClothes(UnturnedPlayer.FromCSteamID(_playerID));

			ShowCombatLoggerPunishToPlayer(UnturnedPlayer.FromCSteamID(_playerID));

			RemovePlayerFromCombatLoggersList(_playerID);

			tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, 
			                  this.Configuration.Instance.WarningMessageColor,
			                  this.Configuration.Instance.ReminderNotifications,
			                  this.Configuration.Instance.ReminderNotificationInterval);
		}

		/**
		 * Adds a player to the combat loggers list. Used to punish them
		 * when they come back online.
		 */
		private void AddPlayerToCombatLoggersList(CSteamID _playerID)
		{
			combatLoggers.Add(_playerID);
		}

		/**
		 * Removes a player from the combat loggers list. 
		 */
		private void RemovePlayerFromCombatLoggersList(CSteamID _playersID)
		{
			combatLoggers.Remove(_playersID);
		}

		/*
		 * Deletes a players last used character files.
		 */
		private void DeletePlayerData(CSteamID _playerID)
		{
			byte latestCharUsed = 0; //Store the number of the character the combat logger used.

			DateTime latestCharUsedDate = DateTime.MinValue;; //Store the date and time the last character was used.

			lastUsedCharMap.Clear();

			for (byte charNum = 0; charNum <= 3; charNum++) //Populate the lastUsedCharMap
			{
				try
				{
					lastUsedCharMap.Add(charNum, System.IO.Directory.GetLastWriteTime(serverFolder + "/Players/" + _playerID.ToString() + "_" + charNum));
				}
				catch (DirectoryNotFoundException e){}
			}

			foreach (byte charNum in lastUsedCharMap.Keys) //Loop through the character folder modification dates to find the latest used.
			{
				if (lastUsedCharMap[charNum] > latestCharUsedDate)
				{
					latestCharUsedDate = lastUsedCharMap[charNum];
					latestCharUsed = charNum;
				}
			}

			try
			{
				System.IO.Directory.Delete(Path.GetFullPath(serverFolder + "/Players/" + _playerID.ToString() + "_" + latestCharUsed), true); //Delete the latest character files/inventory.
			}
			catch (IOException e) {}
		
		}

		#endregion

		#region PLUGIN MESSAGING FUNCTIONS

		/**
		 * Inform the player that they have been punished for combat logging.
		 */
		private void ShowCombatLoggerPunishToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, Translate("punish_player_player"), 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Inform everyone who the combat logger is.
		 */
		private void ShowCombatLoggerMessageToChat(UnturnedPlayer _player)
		{
			UnturnedChat.Say(Translate("combat_logger_chat", _player.CharacterName), 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Print the plugin init message to the console.
		 */
		private void ShowVersionMessage()
		{
			Logger.Log(Translate("version_console", Assembly.GetExecutingAssembly().GetName().Version));
		}

		/**
		 * Print the combat logger's information to the console.
		 */
		private void ShowCombatLoggerToConsole(UnturnedPlayer _player)
		{
			Logger.Log(_player.CharacterName + " (" + _player.SteamName + ") " + Translate("combat_logger_console") + " | " + DateTime.Now);
		}

		#endregion

		#region PLUGIN EVENT HANDLERS

		private void OnPlayerDisconnected(UnturnedPlayer _player)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			if(tmpComponent.InCombat)
			{
				ProcessCombatLogger(_player);
			}
		}

		private void OnPlayerConnected(UnturnedPlayer _player)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			Logger.Log(Path.GetFullPath(serverFolder + "/Players/" + _player.CSteamID.ToString()));

			if (combatLoggers.Contains(_player.CSteamID)) ProcessReturningCombatLogger(_player.CSteamID);
			else tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, 
			                       this.Configuration.Instance.WarningMessageColor,
			                       this.Configuration.Instance.ReminderNotifications,
			                       this.Configuration.Instance.ReminderNotificationInterval);
		}

		private void OnPlayerDead(UnturnedPlayer _player, Vector3 _position)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			tmpComponent.OnDead();
		}

		private void OnPlayerRespawn(UnturnedPlayer _player, Vector3 _position, byte _angle)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, 
			                  this.Configuration.Instance.WarningMessageColor,
			                  this.Configuration.Instance.ReminderNotifications,
			                  this.Configuration.Instance.ReminderNotificationInterval);
		}

		#endregion

	}
}
