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

using NullD.Net.LogNet;

namespace NullD.Core.LogNet.Services
{
    public interface IServerService
    {
        /// <summary>
        ///  Last client that made a RPC call for the service.
        /// </summary>
        LogNetClient Client { get; set; }

        /// <summary>
        /// Last rpc call's header.
        /// </summary>
        bnet.protocol.Header LastCallHeader { get; set; }

        /// <summary>
        /// Sets the outgoing header status if needed, default = 0
        /// </summary>
        uint Status { get; set; }
    }
}
