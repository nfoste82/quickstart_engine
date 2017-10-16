// QuickStartLogoScreen.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.using System;

using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Compositor
{
    public class QuickStartLogoScreen : IScreen
    {
        /// <summary>
        /// Retrieves the width of the logo.
        /// </summary>
        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        /// <summary>
        /// Retrieves the height of the logo.
        /// </summary>
        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        /// <summary>
        /// Retrieves the horizontal position of the logo.
        /// </summary>
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Retrieves the vertical position of the logo.
        /// </summary>
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// Retrieves the name of the logo.  Currently, "QuickStart Logo"
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private int width;
        private int height;
        private int x;
        private int y;
        private ContentManager content;
        private Texture2D logo;

        private bool top;
        private bool left;
        private int border;
        private Color logoColor;

        private const string name = "QuickStart Logo";

        /// <summary>
        /// A screen that displays the QuickStart Engine logo.
        /// </summary>
        /// <param name="drawTop">True if logo should be drawn on the top of window, false otherwise.</param>
        /// <param name="drawLeft">True if logo should be drawn on the left side of the window, false otherwise.</param>
        /// <param name="windowBorder">The size of the border between the edge of the window and the logo.</param>
        /// <param name="logoAlpha">The alpha to use for rendering the logo, in the range [0, 255].  255 = opaque, 0 = completely transparent.</param>
        public QuickStartLogoScreen(bool drawTop, bool drawLeft, int windowBorder, byte logoAlpha)
        {
            this.top = drawTop;
            this.left = drawLeft;
            this.border = windowBorder;
            this.logoColor = new Color(255, 255, 255, logoAlpha);
        }

        /// <summary>
        /// Loads all content needed by the FPS counter.
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> instance to use for all content loading.</param>
        public void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor)
        {
            this.content = contentManager;
            this.content.RootDirectory = "Content";

            this.logo = this.content.Load<Texture2D>("Textures/logo");

            this.width = this.logo.Width;
            this.height = this.logo.Height;

            GraphicsDevice graphics = (this.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService).GraphicsDevice;

            if(this.top)
            {
                this.y = this.border;
            }
            else
            {
                this.y = graphics.PresentationParameters.BackBufferHeight - this.height - this.border;
            }

            if(this.left)
            {
                this.x = this.border;
            }
            else
            {
                this.x = graphics.PresentationParameters.BackBufferWidth - this.width - this.border;
            }
        }

        /// <summary>
        /// Unloads all previously loaded content.
        /// </summary>
        public void UnloadContent()
        {
            this.content.Unload();
        }

        /// <summary>
        /// Draws the FPS counter.
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> to use for 2D drawing.</param>
        /// <param name="background">The previous screen's output as a texture, if NeedBackgroundAsTexture is true.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawScreen(SpriteBatch batch, Texture2D background, GameTime gameTime)
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            batch.Draw(logo, new Vector2(this.x, this.y), this.logoColor);

            batch.End();
        }
    }
}
