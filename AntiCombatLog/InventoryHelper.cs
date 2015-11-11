//
//  InventoryHelper - Used to clear players inventory and clothes.
//  
//	Uses code from https://github.com/Zamirathe/ZaupClearInventoryLib
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

using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using SDG.Unturned;

namespace FC.AntiCombatLog
{
	public class InventoryHelper
	{
		public bool ClearInv(UnturnedPlayer player)
		{
			bool returnv = false;
			try
			{
				player.Player.equipment.dequip();
				for (byte p = 0; p < PlayerInventory.PAGES; p++)
				{
					byte itemc = player.Player.inventory.getItemCount(p);
					if (itemc > 0)
					{
						for (byte p1 = 0; p1 < itemc; p1++)
						{
							player.Player.inventory.removeItem(p, 0);
						}
					}
				}
				player.Player.SteamChannel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				                                {
					(byte)0,
					(byte)0,
					new byte[0]
				});
				player.Player.SteamChannel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				                                {
					(byte)1,
					(byte)0,
					new byte[0]
				});
				returnv = true;
			}
			catch (Exception e)
			{
				Logger.Log("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}

		public bool ClearClothes(UnturnedPlayer player)
		{
			bool returnv = false;
			try
			{
				player.Player.Clothing.askWearBackpack(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearGlasses(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearHat(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearMask(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearPants(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearShirt(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				player.Player.Clothing.askWearVest(0, 0, new byte[0]);
				for (byte p2 = 0; p2 < player.Player.Inventory.getItemCount(2); p2++)
				{
					player.Player.Inventory.removeItem(2, 0);
				}
				returnv = true;
			}
			catch (Exception e)
			{
				Logger.Log("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}
	}
}

