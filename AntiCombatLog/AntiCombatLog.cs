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
using System.Collections.Generic;

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
		#region CONSTANTS

		private const string C_VERSION = "0.1";

		#endregion

		#region STORAGE VARIABLES

		private Dictionary<CSteamID, CombatLogEntry> playerDatabase;

		private List<CSteamID> combatLoggers;

		private DateTime now;

		private DateTime lastCalled;

		private CombatLogEntry tmpEntry;

		private InventoryHelper invHelper;

		public static AntiCombatLog Instance;

		#endregion

		#region ROCKET FUNCTIONS

		protected override void Load()
		{
			Instance = this;

			combatLoggers = new List<CSteamID>();

			invHelper = new InventoryHelper();

			playerDatabase = new Dictionary<CSteamID, CombatLogEntry>();

			lastCalled = DateTime.Now;

			U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
			U.Events.OnPlayerConnected += OnPlayerConnected;

			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateHealth += OnPlayerHealthChange;
			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDead += OnPlayerDead;

			ShowVersionMessage();
		}

		void FixedUpdate()
		{
			UpdateTime();

			if ((now - lastCalled).TotalSeconds > 1) //Update once per second.
			{
				UpdatePlayerDatabase();
				UpdateLastCalled();
			}
		}

		#endregion

		#region PLUGIN FUNCTIONS

		private void UpdateTime()
		{
			now = DateTime.Now;
		}

		private void UpdateLastCalled()
		{
			lastCalled = DateTime.Now;
		}

		/**
		 * Update the player database. Decrement the seconds remaining, changed damaged state, etc.
		 */
		private void UpdatePlayerDatabase()
		{
			foreach (CSteamID playerID in playerDatabase.Keys)
			{
				tmpEntry = playerDatabase[playerID];

				if (tmpEntry.Damaged)
				{
					if (tmpEntry.SecondsRemaining > 0) tmpEntry.SecondsRemaining--; //Decrement the players seconds remaining until they can log out safely.
					else 
					{
						tmpEntry.Damaged = false;
						ShowSafeToDisconnectToPlayer(UnturnedPlayer.FromCSteamID(playerID));
					}
				}
			}
		}


		/**
		 * Add players to the database.
		 */
		private void AddPlayerToPlayerDatabase(CSteamID _steamID)
		{
			playerDatabase.Add(_steamID, new CombatLogEntry(_steamID, false, 0, UnturnedPlayer.FromCSteamID(_steamID).Health));
		}

		/**
		 * Remove player from database.
		 */
		private void RemovePlayerFromDatabase(CSteamID _steamID)
		{
			playerDatabase.Remove(_steamID);
		}

		private void ProcessCombatLogger(UnturnedPlayer _player)
		{
			AddPlayerToCombatLoggersList(_player.CSteamID);

			Logger.Log(_player.CharacterName + " (" + _player.SteamName + ") has combat logged!");

			_player.Damage(255, _player.Position, EDeathCause.PUNCH, ELimb.SKULL, _player.CSteamID); //Drop player items.

			if (this.Configuration.Instance.ShowCombatLogMessagesToGlobalChat) ShowCombatLoggerMessageToChat(_player);
		}

		private void ProcessReturningCombatLogger(CSteamID _playerID)
		{
			if (combatLoggers.Contains(_playerID))
			{
				invHelper.ClearInv(UnturnedPlayer.FromCSteamID(_playerID));
				invHelper.ClearClothes(UnturnedPlayer.FromCSteamID(_playerID));

				ShowCombatLoggerPunishToPlayer(UnturnedPlayer.FromCSteamID(_playerID));
			}
		}

		private void AddPlayerToCombatLoggersList(CSteamID _playerID)
		{
			combatLoggers.Add(_playerID);
		}

		private void RemovePlayerFromCombatLoggersList(CSteamID _playersID)
		{
			combatLoggers.Remove(_playersID);
		}

		#region MESSAGING FUNCTIONS

		/**
		 * Show the player a message informing them that they just got hurt
		 * and need to wait to be able to disconnect without being punished.
		 */
		private void ShowHurtWarningToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, "You have been injured. Please wait " + 
			                 this.Configuration.Instance.CombatLogGracePeriod + " seconds before disconnecting to avoid being punished.", 
			                 	UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		private void ShowSecondsRemainingToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, this.Configuration.Instance.CombatLogGracePeriod + 
			                 " seconds remaining until safe logout.", UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		private void ShowSafeToDisconnectToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, "It is now safe to disconnect.", UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		private void ShowCombatLoggerPunishToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, "You where punished for combat logging. Inventory cleared.", 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		private void ShowCombatLoggerMessageToChat(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player.CharacterName + " is a combat logger!", 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		private void ShowVersionMessage()
		{
			Logger.Log("Version " + C_VERSION + " Loaded.");
		}

		#endregion

		#endregion

		#region PLUGIN EVENT HANDLERS

		private void OnPlayerDisconnected(UnturnedPlayer _player)
		{
			if(playerDatabase[_player.CSteamID].Damaged)
			{
				ProcessCombatLogger(_player);
				RemovePlayerFromDatabase(_player.CSteamID);
			}
			else RemovePlayerFromDatabase(_player.CSteamID);
		}

		private void OnPlayerConnected(UnturnedPlayer _player)
		{
			AddPlayerToPlayerDatabase(_player.CSteamID);

			ProcessReturningCombatLogger(_player.CSteamID);
		}

		private void OnPlayerHealthChange(UnturnedPlayer _player, byte _health)
		{
			if (_health < playerDatabase[_player.CSteamID].Health) //They have gotten hurt not healed set them as damaged and set their seconds remaining to the config.
			{
				ShowHurtWarningToPlayer(_player);

				playerDatabase[_player.CSteamID].Damaged = true;
				playerDatabase[_player.CSteamID].Health =_health;
				playerDatabase[_player.CSteamID].SecondsRemaining = this.Configuration.Instance.CombatLogGracePeriod;
			}
			else
			{
				playerDatabase[_player.CSteamID].Health =_health;
			}
		}

		private void OnPlayerDead(UnturnedPlayer _player, Vector3 _position)
		{
			playerDatabase[_player.CSteamID].Damaged = false;
			ShowSafeToDisconnectToPlayer(_player);
		}

		#endregion
	}
}