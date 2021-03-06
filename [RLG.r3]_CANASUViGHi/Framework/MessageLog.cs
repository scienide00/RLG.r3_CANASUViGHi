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

namespace RLG.R3_CANASUViGHi.Framework
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using RLG.R3_CANASUViGHi.Enums;
    using RLG.R3_CANASUViGHi.Framework;
    using RLG.R3_CANASUViGHi.Contracts;
    using System;
    using System.Text;

    /// <summary>
    /// A Message Log used for sending and drawing messages on the screen.
    /// Supports colored messages, see Models/ColorMessages.txt for more info.
    /// </summary>
    internal sealed class MessageLog : IMessageLog
    {
        private const int TextLeftPad = 5;
        private readonly int spaceScreenWidth;

        private Color textColor;
        private StringBuilder[] lines;
        private SpriteFont spriteFont;
        private Rectangle rectangle;
        private Vector2[] lineVectors;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLog" /> class.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used to draw the log on the screen.</param>
        /// <param name="logRectangle">The rectangle in which to show the log.</param>
        /// <param name="spriteFont">The Message Log font.</param>
        public MessageLog(Rectangle logRectangle, SpriteFont spriteFont)
        {
            this.rectangle = logRectangle;
            this.spriteFont = spriteFont;

            spaceScreenWidth = this.TextScreenLength(" ");

            // Default text color
            this.textColor = Color.Gray;

            int lineCount = this.rectangle.Height / this.FontHeight;
            this.lines = new StringBuilder[lineCount];
            this.lineVectors = new Vector2[lineCount];

            // Initialize line coordinates, from bottom [0], to top [Length - 1].
            for (int i = 0; i < lineCount; i++)
            {
                this.lines[i] = new StringBuilder();

                this.lineVectors[i] = new Vector2(
                    this.rectangle.Left + TextLeftPad,
                    this.rectangle.Bottom - i * this.FontHeight);
            }

            this.ShowGreeting();
        }

        /// <summary>
        /// Gets or sets the color in which to draw the log text.
        /// </summary>
        public Color TextColor
        {
            get { return this.textColor; }
            set { this.textColor = value; }
        }

        /// <summary>
        /// Gets the height of the font.
        /// </summary>
        private int FontHeight
        {
            get { return this.spriteFont.LineSpacing; }
        }

        /// <summary>
        /// Send a message to the log to be displayed.
        /// </summary>
        /// <param name="text">The string message.</param>
        /// <returns>True if the message was sent successfuly, false otherwise.</returns>
        public bool SendMessage(string text)
        {
            // Check if the text sent fits in the message rectangle.
            if (this.TextScreenLength(text) <= this.rectangle.Width)
            {
                for (int i = this.lines.Length - 1; i > 0; i--)
                {
                    lines[i].Clear();
                    lines[i].Append(lines[i - 1]);
                }

                lines[0].Clear();
                lines[0].Append(text);

                // Save log to a file
                // WriteLogFile(text);

                return true;
            }
            else
            {
                // The text line is too long, split in several lines.
                string[] splitText = text.Split(' ');
                int nextUnappanededString = 0;
                StringBuilder textFirstPart = new StringBuilder();

                for (int i = nextUnappanededString; i < splitText.Length; i++)
                {
                    int textLength = TextLeftPad + this.TextScreenLength(textFirstPart) + this.TextScreenLength(splitText[i]);

                    if (textLength + this.spaceScreenWidth < this.rectangle.Width)
                    {
                        textFirstPart.Append(splitText[i]);
                        textFirstPart.Append(" ");
                    }
                    else
                    {
                        nextUnappanededString = i;
                        break;
                    }
                }

                StringBuilder textSecondPart = new StringBuilder();
                for (int i = nextUnappanededString; i < splitText.Length; i++)
                {
                    textSecondPart.Append(splitText[i]);
                    textSecondPart.Append(" ");
                }

                // Recursively send splitted messages.
                SendMessage(textFirstPart.ToString());
                SendMessage(textSecondPart.ToString());
            }

            return false;
        }

        /// <summary>
        /// Implements the ISoundReceiver interface, sending sounds to the log.
        /// </summary>
        /// <param name="sound">The Sound object to send to the log.</param>
        public void ReceiveSound(ISound sound)
        {
            Color soundColor = Color.DarkGray;
            StringBuilder soundCreated = new StringBuilder();
            soundCreated.AppendFormat("{0} ", (sound.Source as IGameObject).Name);

            switch (sound.Type)
            {
                case SoundType.Say:
                    {
                        soundCreated.Append("says: ");
                        soundColor = Color.Gray;
                    }
                    break;

                case SoundType.Yell:
                    {
                        soundCreated.Append("yells: ");
                        soundColor = Color.Red;
                    }
                    break;

                case SoundType.Bump:
                    {
                        soundCreated.Append("bumps into ");
                    }
                    break;

                case SoundType.Hit:
                    {
                        soundCreated.Append("hits ");
                        soundColor = Color.OrangeRed;
                    }
                    break;
            }

            soundCreated.Append(sound.StringValue);

            string colorSequence = string.Format("~S{0}!", soundColor.ToUInt());
            soundCreated.Insert(0, colorSequence);

            this.SendMessage(soundCreated.ToString());
        }

        /// <summary>
        /// Clear the log line content.
        /// </summary>
        public void ClearLog()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                this.lines[i].Clear();
            }
        }

        /// <summary>
        /// Draw the Message Log on the screen.
        /// </summary>
        /// <remarks>To color string/part of a string to a specific color check ColorMessages.txt.</remarks>
        /// <param name="spriteBatch">The SpriteBatch object used to draw the log.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            for (int i = 0; i < lines.Length; i++)
            {
                #region Colored string draw

                int linePosition = 0;
                string workText = lines[i].ToString();
                char selector = '\0';
                StringBuilder color = new StringBuilder(10);

                for (int k = 0; k < workText.Length; k++)
                {
                    // Beggining of an escape formatting sequence.
                    if (workText[k] == '~')
                    {
                        // Get the selector character.
                        selector = workText[++k];

                        // Get the color code.
                        while (workText[++k] != '!')
                        {
                            color.Append(workText[k]);
                        }

                        k++;

                        // Select text to color.
                        string selectedText = "";

                        #region Text Select
                        switch (selector)
                        {
                            case 'L':
                            case 'l':
                                {
                                    selectedText = workText[k].ToString();
                                    break;
                                }

                            case 'W':
                            case 'w':
                                {
                                    StringBuilder sb = new StringBuilder(20);

                                    for (int j = k; j < workText.Length; j++)
                                    {
                                        bool endOfWord = char.IsWhiteSpace(workText[j]);

                                        if (!endOfWord)
                                        {
                                            sb.Append(workText[j]);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    selectedText = sb.ToString();
                                    break;
                                }

                            case 'S':
                            case 's':
                                {
                                    selectedText = new string(workText.ToCharArray(), k, workText.Length - k);
                                    selectedText = this.RemoveColorSeq(selectedText, true);
                                    break;
                                }

                            default:
                                {
                                    throw new ArgumentException(
                                        "Invalid selector char in MessageLog color string sequence!",
                                        "selector");
                                }
                        }

                        k--;
                        #endregion

                        uint colorUInt = uint.Parse(color.ToString());
                        Color parsedColor = colorUInt.ToColor();
                        color.Clear();

                        Vector2 newPosition = new Vector2(
                            lineVectors[i].X + linePosition, 
                            lineVectors[i].Y);

                        // Set the line position for the next string.
                        linePosition += this.TextScreenLength(this.RemoveColorSeq(selectedText, true));

                        spriteBatch.DrawString(
                            this.spriteFont,
                            this.RemoveColorSeq(selectedText),
                            newPosition,
                            parsedColor);

                        k += this.RemoveColorSeq(selectedText, true).Length;
                    }
                    else
                    {
                        Vector2 newPosition = new Vector2(
                            lineVectors[i].X + linePosition,
                            lineVectors[i].Y);

                        spriteBatch.DrawString(
                            this.spriteFont,
                            this.lines[i][k].ToString(),
                            newPosition,
                            this.textColor);

                        linePosition += this.TextScreenLength(this.lines[i][k].ToString());
                    }
                }
                #endregion
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Returns the length of a string displayed on the screen.
        /// </summary>
        /// <param name="text">The string text.</param>
        /// <returns>Length of the text drawn on the screen, using this SpriteFont.</returns>
        private int TextScreenLength(string text)
        {
            return (int)this.spriteFont.MeasureString(this.RemoveColorSeq(text)).X;
        }

        /// <summary>
        /// Returns the length of a StringBuilder displayed on the screen.
        /// </summary>
        /// <param name="text">The StringBuilder text.</param>
        /// <returns>Length of the text drawn on the screen, using this SpriteFont.</returns>
        private int TextScreenLength(StringBuilder text)
        {
            return this.TextScreenLength(text.ToString());
        }

        /// <summary>
        /// Remove color sequences existant in a string.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <param name="breakOnSequence">Indicates whether we should break on 
        /// sequence start and return the string up to this point.</param>
        /// <returns>A string cleared from color sequence codes.</returns>
        private string RemoveColorSeq(string text, bool breakOnSequence = false)
        {
            StringBuilder actualText = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '~')
                {
                    if (breakOnSequence)
                    {
                        return actualText.ToString();
                    }

                    i += 13;
                }

                actualText.Append(text[i]);
            }

            return actualText.ToString();
        }

        /// <summary>
        /// Show a greeting message.
        /// </summary>
        private void ShowGreeting()
        {
            System.Collections.Generic.List<Color> gradient = 
                new System.Collections.Generic.List<Color>();

            for (double i = 0; i < 1; i += 0.02)
            {
                gradient.Add(Tools.HSL2RGB(i, 0.5, 0.5));
            }

            string greeting = "Welcome-to-Canas-Uvighi-RL/RP-Game!-@SCiENiDE-2015";

            StringBuilder gradientGreeting = new StringBuilder();

            for (int i = 0; i < greeting.Length; i++)
            {
                gradientGreeting.AppendFormat("~L{0}!{1}", gradient[i].ToUInt(), greeting[i]);
            }

            this.SendMessage(gradientGreeting.ToString());
        }
    }
}