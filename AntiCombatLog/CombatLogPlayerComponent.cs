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

			if((now - lastCalled).TotalSeconds > 1) //Update once per second.
			{
				UpdateStatus();
				UpdateLastCalled(now);
			}
		}

		public void Event_OnHit(ushort _secondsRemaining, string _warningMessageColor)
		{
			InCombat = true;
			OldHealth = Player.Health;
			SecondsRemaining = _secondsRemaining;
			configCombatLogTimeout = _secondsRemaining;
			messageColor = _warningMessageColor;

			if (Bleeding == false) ShowHurtWarning();

			Bleeding = Player.Bleeding;
			
		}

		public void ResetStatus()
		{
			if(InCombat) ShowSafeToDisconnect();

			SecondsRemaining = 0;
			Bleeding = Player.Bleeding;
			OldHealth = Player.Health;
			InCombat = false;
		}

		private void UpdateLastCalled(DateTime _now)
		{
			lastCalled = _now;
		}

		private void UpdateStatus()
		{
			if (InCombat) SecondsRemaining--;

			if (SecondsRemaining == 0 && InCombat) 
			{
				InCombat = false;
				ShowSafeToDisconnect();
			}

			Bleeding = Player.Bleeding;
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

