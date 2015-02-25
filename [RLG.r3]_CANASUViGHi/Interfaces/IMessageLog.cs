﻿/* *
* Canas Uvighi, a RogueLike Game / RPG project.
* Copyright (C) 2015 Aleksandar Dimitrov (screen name SCiENiDE)
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* */

namespace RLG.R3_CANASUViGHi.Interfaces
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using RLG.R3_CANASUViGHi.Framework;

    internal interface IMessageLog
    {
        /// <summary>
        /// Gets or sets the color of the text in the log.
        /// </summary>
        Color TextColor { get; set; }

        /// <summary>
        /// Send a message to the log to be displayed.
        /// </summary>
        /// <param name="text">String text message.</param>
        /// <returns>Indicates whether the message was successfuly shown.</returns>
        bool SendMessage(string text);

        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Clear the log.
        /// </summary>
        void ClearLog();
    }
}
