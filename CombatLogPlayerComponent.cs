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


using Rocket.API;
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

		private ushort configCombatLogGracePeriod;
		private ushort configNotificationInterval;
		private bool configNotifications;
		private string configMessageColor;

		private DateTime now;
		private DateTime lastCalled = DateTime.Now;
		private DateTime notificationLastShown;

		#region ENGINE FUNCTIONS

		public void FixedUpdate()
		{
			now = DateTime.Now;

			UpdateStatus();
		}

		#endregion

		public void Init(ushort _configCombatLogGracePeriod, string _configWarningMessageColor, bool _configNotifications, ushort _configNotificationInterval)
		{
			configCombatLogGracePeriod = _configCombatLogGracePeriod;

			configMessageColor = _configWarningMessageColor;

			configNotifications = _configNotifications;

			configNotificationInterval = _configNotificationInterval;

			ResetStatus();
		}

		public void ResetStatus()
		{
			SecondsRemaining = 0;
			Bleeding = false;
			OldHealth = Player.Health;
			InCombat = false;
		}

		#region "EVENTS"

		public void OnHit()
		{
			if (InCombat)
			{
				OldHealth = Player.Health; //TODO: Needed?
				SecondsRemaining = configCombatLogGracePeriod;
			}
			else
			{
				InCombat = true;

				ShowHurtWarning();

				if (configNotifications) notificationLastShown = now;

				OldHealth = Player.Health; //TODO: Needed?
				SecondsRemaining = configCombatLogGracePeriod;
			}
		}

		public void OnDead()
		{
			ShowSafeToDisconnect();
		}

		#endregion

		#region UPDATE FUNCTIONS

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

					UpdateNotifications(now);
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

		private void UpdateNotifications(DateTime _now)
		{
			if (Player.HasPermission("nocl") || Player.IsAdmin) return;

			if (configNotifications && (_now - notificationLastShown).Seconds >= configNotificationInterval)
			{
				if (Bleeding)
				{
					ShowBleeding();
					notificationLastShown = _now;
				}
				else
				{
					ShowSecondsRemaining();
					notificationLastShown = _now;
				}
			}
		}

		#endregion

		#region MESSAGING FUNCTIONS

		/**
		 * Inform the player that they just got hurt and need to wait
		 * to be able to disconnect without being punished.
		 */
		private void ShowHurtWarning()
		{
			UnturnedChat.Say(Player, AntiCombatLog.Instance.Translate("injured_warning_player", configCombatLogGracePeriod), 
			                 UnturnedChat.GetColorFromName(configMessageColor, Color.red));
		}

		/**
		 * Inform the player that it is now safe to log out.
		 */
		private void ShowSafeToDisconnect()
		{
			UnturnedChat.Say(Player, AntiCombatLog.Instance.Translate("safe_to_disconnect_player"), UnturnedChat.GetColorFromName(configMessageColor, Color.red));
		}

		/**
		 * Inform the player of the number of seconds remaining until they can safely logout.
		 */
		private void ShowSecondsRemaining()
		{
			UnturnedChat.Say(Player, AntiCombatLog.Instance.Translate("seconds_remaining_player", SecondsRemaining), UnturnedChat.GetColorFromName(configMessageColor, Color.red));
		}

		private void ShowBleeding()
		{
			UnturnedChat.Say(Player, AntiCombatLog.Instance.Translate("bleeding_player"), UnturnedChat.GetColorFromName(configMessageColor, Color.red));
		}

		#endregion
	}
}

