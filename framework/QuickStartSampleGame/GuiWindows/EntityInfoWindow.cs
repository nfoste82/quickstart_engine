using System;
using System.Collections.Generic;
using System.Text;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using QuickStart;
using QuickStart.Components;
using QuickStart.Entities;
using QuickStart.Mathmatics;

namespace QuickStartSampleGame
{
    public class EntityInfoWindow : MinimizableWindowControl
    {
        public ListControl Label
        {
            get { return this.label; }
        }
        private ListControl label;

        private ButtonControl closeButton;

        public EntityInfoWindow(int xPos, int yPos, Int64 EntityID, QSGame game)
            : base()
        {
            MsgGetName msgName = ObjectPool.Aquire<MsgGetName>();
            msgName.UniqueTarget = EntityID;
            game.SendMessage(msgName);
            
            Initialize(xPos, yPos, msgName.Name);
        }

        private void Initialize(int xPos, int yPos, string entityName)
        {
            this.Title = "Entity Report - " + entityName;

            UInt32 windowWidth = (UInt32)QSMath.Max( ( 10 * this.Title.Length ) + 20, 400);
            UInt32 windowHeight = 400;

            this.EnableDragging = true;
            this.MinimizeWidth = windowWidth;
            this.MinimizeHeight = 25;

            this.label = new ListControl();
            this.label.Bounds = new UniRectangle(4, 25, windowWidth - 10, windowHeight - 70);
            this.label.Slider.Bounds.Location.X.Offset -= 1.0f;
            this.label.Slider.Bounds.Location.Y.Offset += 1.0f;
            this.label.Slider.Bounds.Size.Y.Offset -= 2.0f;
            this.label.SelectionMode = ListSelectionMode.Single;

            this.closeButton = new ButtonControl();
            this.closeButton.Text = "Close";
            this.closeButton.Bounds = new UniRectangle(new UniScalar(1.0f, -90.0f),
                                                        new UniScalar(1.0f, -40.0f),
                                                        80, 24);
            this.closeButton.Pressed += new EventHandler(closeButton_Pressed);

            this.Bounds = new UniRectangle(xPos, yPos, windowWidth, windowHeight);

            this.Children.Add(this.label);
            this.Children.Add(this.closeButton);
        }

        void closeButton_Pressed(object sender, EventArgs e)
        {
            this.Close();
        }

        //protected override void OnWindowMinimized()
        //{
        //    this.Title = "Controls - Double-click to Open";
        //}

        //protected override void OnWindowRestored()
        //{
        //    this.Title = "Controls - Double-click to Hide";
        //}
    }
}
