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
using Steamworks;

namespace FC.AntiCombatLog
{
	public class CombatLogPlayerComponent : UnturnedPlayerComponent
	{
		public ushort SecondsRemaining;

		public bool Bleeding;

		public bool InCombat;

		public byte OldHealth;

		private DateTime lastCalled = DateTime.Now;

		private DateTime now;

		public void FixedUpdate()
		{
			now = DateTime.Now;

			if((now - lastCalled).TotalSeconds > 1) //Update once per second.
			{
				UpdateStatus();
				UpdateLastCalled(now);
			}
		}

		private void UpdateLastCalled(DateTime _now)
		{
			lastCalled = _now;
		}

		private void UpdateStatus()
		{
			if (InCombat)
			{
				SecondsRemaining--;
			}

			if (SecondsRemaining = 0)
			{
				InCombat = false;
			}
		}
	}
}

