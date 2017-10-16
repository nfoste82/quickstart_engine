//
// QuickStartSampleGame.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.


using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Graphics;
using QuickStart.Entities;
using QuickStart.Components;
using QuickStart.Compositor;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.Input;

namespace QuickStartSampleGame
{
    /// <summary>
    /// Sample QuickStart Game implementation
    /// </summary>
    public class QuickStartSampleGame : QSGame
    {             
        /// <summary>
        /// Input poller so we can query multi-key combos, like CTRL + Enter
        /// </summary>
        private InputPollingHandler inputs;

        /// <summary>
        /// Creates a new instance of the sample game.
        /// </summary>
        public QuickStartSampleGame() 
        {            
            this.IsMouseVisible = QSConstants.MouseDefaultVisible;
        }

        /// <summary>
        /// Initializes the current sample game instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Set up a window with maximum MSAA
            GraphicsSettings settings   = new GraphicsSettings();
            settings.BackBufferWidth    = this.Settings.Resolution.X;
            settings.BackBufferHeight   = this.Settings.Resolution.Y;
            settings.EnableVSync        = this.Settings.IsVerticalSynchronized;
            settings.EnableMSAA         = this.Settings.IsMultiSampling;
            settings.EnableFullScreen   = this.Settings.IsFullscreen;
            settings.EnableShadowMapping = this.Settings.IsShadowMapping;
            settings.GraphicsLevel      = this.Settings.GraphicsLevel;

            this.IsFixedTimeStep        = this.Settings.IsFixedTimeStep;

            // Uncomment this line for PIX debugging
            //settings.EnableMSAA = false;

            Graphics.ApplySettings(settings);

            Graphics.RegisterRenderChunkProvider(this.SceneManager);

            MsgSetPhysicsTimeStep msgSetPhysStep = ObjectPool.Aquire<MsgSetPhysicsTimeStep>();            
            msgSetPhysStep.TimeStep = this.Settings.PhysicsStepsPerSecond;
            SendInterfaceMessage(msgSetPhysStep, InterfaceType.Physics);

            this.inputs = new InputPollingHandler(this);
            InitInputListeners();

            SendDebugMessage("Initialize");

            // ===============================================================================
            // Set up screen compositor                        
            this.Compositor.InsertScreen(new FPSScreen(10, 10, this), false);                        
            this.Compositor.InsertScreen(new QuickStartLogoScreen(false, false, 10, 150), false);
            this.Compositor.InsertScreen(new StatsScreen(this.Settings.Resolution.X - 330, 17, this), false);
            this.Compositor.InsertScreen(new InstructionsScreen(8, 37, this), false);
            // ===============================================================================

            // This is the screen that renders the 3D world
            this.Compositor.InsertScreen(this.SceneManager, true);
        }        

        /// <summary>
        /// Loads the sample game's content assets.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            LoadScenes();

            SendDebugMessage("Load Content");
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// Add input listeners for input polling
        /// </summary>
        private void InitInputListeners()
        {
            this.inputs.AddInputListener(Keys.Enter);
            this.inputs.AddInputListener(Keys.LeftControl);
            this.inputs.AddInputListener(Keys.RightControl);
            this.inputs.AddInputListener(Keys.LeftAlt);
            this.inputs.AddInputListener(Keys.RightAlt);

            this.inputs.SetLockable(Keys.LeftAlt, true);
            this.inputs.SetLockable(Keys.RightAlt, true);
        }

        /// <summary>
        /// Temporary method, simply to demonstrate how to load up a scene. Later this
        /// type of thing should be done from an XML file or similar.
        /// </summary>
        private void LoadScenes()
        {
            Scene newScene = new SampleScene(this);

            this.SceneManager.LoadScene(newScene, true);
            this.SceneManager.ActiveScene = newScene;
        }

        /// <summary>
        /// Updates the sample game for the current cycle.
        /// </summary>
        /// <param name="gameTime">A snapshot of the game's time.</param>
        protected override void Update(GameTime gameTime)
        {            
            ProcessInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// Process polling-based input
        /// </summary>
        private void ProcessInput()
        {
            if (this.inputs.IsDown(Keys.Enter))
            {
                if (this.inputs.IsDown(Keys.LeftControl) || this.inputs.IsDown(Keys.RightControl))
                {
                    if (this.inputs.IsDown(Keys.LeftAlt) || this.inputs.IsDown(Keys.RightAlt))
                    {
                        this.Graphics.ToggleFullscreen();
                    }
                }
            }
        }

        /// <summary>
        /// Renders a single frame for the sample game.
        /// </summary>
        /// <param name="gameTime">A snapshot of the game's time.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Render the whole frame with one call!
            this.Compositor.DrawCompositorChain(gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Handles game messages
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> send to the game</param>
        protected override bool OnGameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.KeyDown:
                    MsgKeyPressed msgKeyPressed = message as MsgKeyPressed;
                    message.TypeCheck(msgKeyPressed);

                    switch (msgKeyPressed.Key)
                    {
                        case Keys.Escape:
                            this.QueueMessage(new ExitMessage());
                            break;
                    }
                    break;
            }

            return base.OnGameMessage(message);
        }

        /// <summary>
        /// Sends a text message to the debug output
        /// </summary>
        /// <param name="message">Text to send to the debug output</param>
        private void SendDebugMessage(string message)
        {
            Message<string> msg = ObjectPool.Aquire<Message<string>>();
            msg.Data = message;
            msg.Type = MessageType.DebugText;
            msg.Protocol = MessageProtocol.Broadcast;

            this.SendMessage(msg);
        }
    }
}
