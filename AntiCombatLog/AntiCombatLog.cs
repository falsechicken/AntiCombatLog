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

		private const string C_VERSION = "0.2.0";

		#endregion

		#region STORAGE VARIABLES

		private List<CSteamID> combatLoggers;

		private DateTime now;

		private DateTime lastCalled;

		private InventoryHelper invHelper;

		private CombatLogPlayerComponent tmpComponent;

		public static AntiCombatLog Instance;

		#endregion

		#region ROCKET FUNCTIONS

		protected override void Load()
		{
			Instance = this;

			combatLoggers = new List<CSteamID>();

			invHelper = new InventoryHelper();

			lastCalled = DateTime.Now;

			U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
			U.Events.OnPlayerConnected += OnPlayerConnected;

			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive += OnPlayerRespawn;
			Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDead += OnPlayerDead;

			ShowVersionMessage();
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

			Logger.Log(_player.CharacterName + " (" + _player.SteamName + ") has combat logged!");

			_player.Damage(255, _player.Position, EDeathCause.PUNCH, ELimb.SKULL, _player.CSteamID); //Drop player items.

			if (this.Configuration.Instance.ShowCombatLogMessagesToGlobalChat) ShowCombatLoggerMessageToChat(_player);
		}

		/**
		 * When the combat logger is "Killed" by ProcessCombatLogger they are added to a list to clear
		 * their inventory when they come back. This is because clearing their inventory when they
		 * are disconnecting doesnt work.
		 */
		private void ProcessReturningCombatLogger(CSteamID _playerID)
		{
			tmpComponent = UnturnedPlayer.FromCSteamID(_playerID).GetComponent<CombatLogPlayerComponent>();

			invHelper.ClearInv(UnturnedPlayer.FromCSteamID(_playerID));
			invHelper.ClearClothes(UnturnedPlayer.FromCSteamID(_playerID));

			ShowCombatLoggerPunishToPlayer(UnturnedPlayer.FromCSteamID(_playerID));

			RemovePlayerFromCombatLoggersList(_playerID);

			tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, this.Configuration.Instance.WarningMessageColor);
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

		#endregion

		#region PLUGIN MESSAGING FUNCTIONS

		/**
		 * Inform the player of the number of seconds remaining until they can safely logout.
		 */
		private void ShowSecondsRemainingToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, this.Configuration.Instance.CombatLogGracePeriod + 
			                 " seconds remaining until safe logout.", UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Inform the player that it is now safe to log out.
		 */
		private void ShowSafeToDisconnectToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, "It is now safe to disconnect.", UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Inform the player that they have been punished for combat logging.
		 */
		private void ShowCombatLoggerPunishToPlayer(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player, "You where punished for combat logging. Inventory cleared.", 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Inform everyone who the combat logger is.
		 */
		private void ShowCombatLoggerMessageToChat(UnturnedPlayer _player)
		{
			UnturnedChat.Say(_player.CharacterName + " is a combat logger!", 
			                 UnturnedChat.GetColorFromName(this.Configuration.Instance.WarningMessageColor, Color.red));
		}

		/**
		 * Print the plugin init message to the console.
		 */
		private void ShowVersionMessage()
		{
			Logger.Log("Version " + C_VERSION + " Loaded.");
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

			if (combatLoggers.Contains(_player.CSteamID)) ProcessReturningCombatLogger(_player.CSteamID);
			else tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, this.Configuration.Instance.WarningMessageColor);
		}

		private void OnPlayerDead(UnturnedPlayer _player, Vector3 _position)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			tmpComponent.OnDead();
		}

		private void OnPlayerRespawn(UnturnedPlayer _player, Vector3 _position, byte _angle)
		{
			tmpComponent = _player.GetComponent<CombatLogPlayerComponent>();

			tmpComponent.Init(this.Configuration.Instance.CombatLogGracePeriod, this.Configuration.Instance.WarningMessageColor);
		}

		#endregion
	}
}
