// InstructionsScreen.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using QuickStart.Entities;
using QuickStart.Interfaces;

namespace QuickStart.Compositor
{
    /// <summary>
    /// A GUI WindowControl for showing controls information
    /// </summary>
    public class ControlsScreenGUI : MinimizableWindowControl
    {
        public LabelControl Label
        {
            get { return this.label; }
        }
        private LabelControl label;

        private ButtonControl closeButton;

        public ControlsScreenGUI(int xPos, int yPos)
            : base()
        {
            Initialize(xPos, yPos);
        }

        private void Initialize(int xPos, int yPos)
        {
            this.Title = "Controls - Double-click to Hide";

            this.EnableDragging = true;
            this.MinimizeWidth = 320;
            this.MinimizeHeight = 25;

            this.label = new LabelControl();
            this.label.Bounds = new UniRectangle(10.0f, 15.0f, 110.0f, 30.0f);

            this.closeButton = new ButtonControl();
            this.closeButton.Text = "Close";
            this.closeButton.Bounds = new UniRectangle(new UniScalar(1.0f, -90.0f),
                                                        new UniScalar(1.0f, -40.0f),
                                                        80, 24);
            this.closeButton.Pressed += new EventHandler(closeButton_Pressed);

            this.Bounds = new UniRectangle(xPos, yPos, 350, 350);

            this.Children.Add(this.label);
            this.Children.Add(this.closeButton);

            this.Minimize();
        }

        void closeButton_Pressed(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnWindowMinimized()
        {
            this.Title = "Controls - Double-click to Open";
        }

        protected override void OnWindowRestored()
        {
            this.Title = "Controls - Double-click to Hide";
        }
    }

    /// <summary>
    /// A screen that gives information about how to use the Sample Game that comes with
    /// the QuickStartEngine.
    /// </summary>
    public class InstructionsScreen : IScreen
    {
        /// <summary>
        /// Gets/sets the horizontal position.
        /// </summary>
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Gets/sets the vertical position.
        /// </summary>
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// The <see cref="StatsScreen"/> never needs the previous output as texture
        /// </summary>
        /// <remarks>
        /// This always returns false
        /// </remarks>
        public bool NeedBackgroundAsTexture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the name of the <see cref="StatsScreen"/>
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private int x;
        private int y;
        private ContentManager content;
        private SpriteFont font;
        private QSGame game;

        private const string name = "Instructions";

        private ControlsScreenGUI guiScreen;

        /// <summary>
        /// A screen with instructions on things that can be done within the game demo.
        /// </summary>
        /// <param name="xPos">The horizontal position</param>
        /// <param name="yPos">The vertical position</param>
        public InstructionsScreen(int xPos, int yPos, QSGame game)
        {
            this.game = game;            
            this.x = xPos;
            this.y = yPos;

            this.guiScreen = new ControlsScreenGUI(xPos, yPos);

            // Add the screen the to GuiManager's screen
            this.game.Gui.Screen.Desktop.Children.Add(this.guiScreen);
        }

        /// <summary>
        /// Loads all content needed
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> instance to use for all content loading.</param>
        public void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor)
        {
            this.content = contentManager;
            this.content.RootDirectory = "Content";

            this.font = this.content.Load<SpriteFont>("Textures/Fonts/Tahoma12PtBold");
        }

        /// <summary>
        /// Unloads all previously loaded content.
        /// </summary>
        public void UnloadContent()
        {
            this.content.Unload();
        }

        /// <summary>
        /// Draws the debug scene
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> to use for 2D drawing.</param>
        /// <param name="background">The previous screen's output as a texture, if NeedBackgroundAsTexture is true.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawScreen(SpriteBatch batch, Texture2D background, GameTime gameTime)
        {
            // No need to create the string for a closed window
            if (!this.guiScreen.IsOpen)
            {
                return;
            }

            // We don't want the window to drag if the cursor is invisible.
            this.guiScreen.EnableDragging = this.game.IsMouseVisible;

            if (!this.game.IsMouseVisible)
            {
                this.guiScreen.Title = "Controls - F3 to enable cursor";
            }
            else
            {
                if (this.guiScreen.Minimized)
                {
                    this.guiScreen.Title = "Controls - Double-click to Open";
                }
                else
                {
                    this.guiScreen.Title = "Controls - Double-click to Hide";
                }
            }

            StringBuilder sb = new StringBuilder();
            
            MsgGetName msgName = ObjectPool.Aquire<MsgGetName>();
            msgName.UniqueTarget = QSGame.SceneMgrRootEntityID;
            this.game.SendMessage(msgName);
                        
            sb.AppendLine("Press F3 to toggle mouse cursor");
            sb.AppendLine();

            MsgGetControlledEntity msgControlled = ObjectPool.Aquire<MsgGetControlledEntity>();
            this.game.SendInterfaceMessage(msgControlled, InterfaceType.SceneManager);

            if (msgControlled.ControlledEntityID != QSGame.UniqueIDEmpty)
            {
                MsgGetIsACharacter msgIsChar = ObjectPool.Aquire<MsgGetIsACharacter>();
                msgIsChar.UniqueTarget = msgControlled.ControlledEntityID;
                this.game.SendMessage(msgIsChar);

                if (msgIsChar.IsCharacter)
                {                    
                    sb.AppendLine("HOLD Left Mouse Button - Rotate Character");
                    sb.AppendLine("SPACEBAR - Jump");
                    sb.AppendLine("W, A, S, D - Move/Strafe");
                }                

                sb.AppendLine("F2 - Switch to Free-Cam Mode");
                sb.AppendLine("TAB - Attach to another entity");
            }
            else
            {
                sb.AppendLine("-- Keyboard --");
                sb.AppendLine("LEFT CTRL - Fire lightweight sphere");
                sb.AppendLine("LEFT SHIFT - Fire heavy sphere");
                sb.AppendLine("SPACEBAR - Fire a medium weight sphere");
                sb.AppendLine("W, A, S, D - Move Camera");
                sb.AppendLine("Q, E - Roll Camera");
                sb.AppendLine("TAB - Attach camera to an entity");
                sb.AppendLine("Hold left mouse button to fly faster");
                sb.AppendLine();
                sb.AppendLine("-- Gamepad Controls --");
                sb.AppendLine("Gamepad Thumbsticks to fly/look");
                sb.AppendLine("Gamepad buttons to fire spheres");
                sb.AppendLine();
                sb.AppendLine("-- GUI Controls --");
                sb.AppendLine("Double-click GUI to minimize/restore");
                sb.AppendLine("Click and hold to drag GUI windows");                
            }

            this.guiScreen.Label.Text = sb.ToString();
        }
    }
}
