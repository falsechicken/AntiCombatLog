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

			invHelper = new InventoryHelper();

			playerDatabase = new Dictionary<CSteamID, CombatLogEntry>();

			lastCalled = DateTime.Now;

			U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
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

			//TODO: Add all players not already in the database to the database first.

			foreach (CSteamID playerID in playerDatabase.Keys)
			{
				tmpEntry = playerDatabase[playerID];

				if (tmpEntry.Damaged)
				{
					if (tmpEntry.SecondsRemaining > 0) tmpEntry.SecondsRemaining--; //Decrement the players seconds remaining until they can log out safely.
					else tmpEntry.Damaged = false;
				}
			}
		}

		#endregion

		#region PLUGIN EVENT HANDLERS

		private void OnPlayerDisconnected(UnturnedPlayer _player)
		{
			if(playerDatabase[_player.CSteamID].Damaged)
			{
				//TODO: KILL PLAYER
				invHelper.ClearInv(_player);
				invHelper.ClearClothes(_player);
			}
		}

		#endregion
	}
}

