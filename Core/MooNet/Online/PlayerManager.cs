/*
 * Copyright (C) 2011 - 2018 NullD project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.Collections.Generic;
using NullD.Net.LogNet;

namespace NullD.Core.LogNet.Online
{
    // probably will not need this when we actually send players from last game to recent players window.
    public static class PlayerManager
    {
        public static readonly List<LogNetClient> OnlinePlayers = new List<LogNetClient>();

        public static void PlayerConnected(LogNetClient client)
        {
            OnlinePlayers.Add(client);
        }

        public static void PlayerDisconnected(LogNetClient client)
        {
            OnlinePlayers.Remove(client);
        }
    }
}
