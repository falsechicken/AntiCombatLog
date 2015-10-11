//
//  CombatLogPlayerComponent.cs
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

using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;

using Steamworks;

using UnityEngine;

namespace FC.AntiCombatLog
{
	public class CombatLogPlayerComponent : UnturnedPlayerComponent
	{
		public ushort SecondsRemaining;

		public bool Bleeding;

		public bool InCombat;

		public byte OldHealth;

		private DateTime lastCalled = DateTime.Now;

		private ushort configCombatLogTimeout;

		private DateTime now;

		private string messageColor;

		public void FixedUpdate()
		{
			now = DateTime.Now;

			UpdateStatus();
		}

		public void Init(ushort _configCombatLogTimeout, string _warningMessageColor)
		{
			configCombatLogTimeout = _configCombatLogTimeout;
			messageColor = _warningMessageColor;

			ResetStatus();
		}

		public void OnHit()
		{
			InCombat = true;
			OldHealth = Player.Health;
			SecondsRemaining = configCombatLogTimeout;

			if (Bleeding == false) ShowHurtWarning();
		}

		public void OnDead()
		{
			ShowSafeToDisconnect();
		}

		public void ResetStatus()
		{
			SecondsRemaining = 0;
			Bleeding = false;
			OldHealth = Player.Health;
			InCombat = false;
		}

		private void UpdateLastCalled(DateTime _now)
		{
			lastCalled = _now;
		}

		private void UpdateStatus()
		{
			if (Player.Health < OldHealth) //Player has been hit.
			{
				OnHit();
			}

			if (InCombat)
			{
				if ((now - lastCalled).TotalSeconds > 1)
				{
					SecondsRemaining--;
					UpdateLastCalled(now);
				}
			}

			if (SecondsRemaining == 0 && InCombat) 
			{
				ResetStatus();
				ShowSafeToDisconnect();
			}

			Bleeding = Player.Bleeding;

			OldHealth = Player.Health;
		}

		/**
		 * Inform the player that they just got hurt and need to wait
		 * to be able to disconnect without being punished.
		 */
		private void ShowHurtWarning()
		{
			UnturnedChat.Say(Player, "You have been injured. Please wait " + configCombatLogTimeout + " seconds before disconnecting to avoid being punished.", 
			                 UnturnedChat.GetColorFromName(messageColor, Color.red));
		}

		/**
		 * Inform the player that it is now safe to log out.
		 */
		private void ShowSafeToDisconnect()
		{
			UnturnedChat.Say(Player, "It is now safe to disconnect.", UnturnedChat.GetColorFromName(messageColor, Color.red));
		}
	}
}

