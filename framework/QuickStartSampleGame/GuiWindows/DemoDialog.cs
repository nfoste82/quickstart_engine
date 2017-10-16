using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nuclex.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using Microsoft.Xna.Framework.Input;

namespace QuickStartSampleGame
{
    class TestButton : ButtonControl
    {
        protected override void OnMouseDoublePressed(Nuclex.Input.MouseButtons button)
        {
            if (this.Parent is MinimizableWindowControl)
            {
                (this.Parent as MinimizableWindowControl).Minimizable = false;
            }
        }
    }

    class TestLabel : LabelControl
    {
        protected override void OnMouseDoublePressed(Nuclex.Input.MouseButtons button)
        {
            this.Text = "Double clicked with: " + button.ToString();
        }
    }

    class DemoDialog : MinimizableWindowControl
    {
        /// <summary>A label used to display a 'hello world' message</summary>
        private TestLabel helloWorldLabel;

        /// <summary>Button which exits the dialog and takes over the settings</summary>
        private TestButton minimizeButton;

        /// <summary>Button which exits the dialog and discards the settings</summary>        
        private ButtonControl closeButton;

        public DemoDialog()
            : base()
        {
            Initialize();
            this.EnableDragging = true;

            this.MinimizeWidth = 512;
            this.MinimizeHeight = 25;
        }

        protected override void OnMouseEntered()
        {
            this.helloWorldLabel.Text = "Rolled Over";
        }

        private void Initialize()
        {
            this.helloWorldLabel = new TestLabel();

            this.minimizeButton = new TestButton();
            this.minimizeButton.AutoUnfocuses = false;

            //
            // closeButton
            //
            this.closeButton = new ButtonControl();
            this.closeButton.Pressed += new EventHandler(closeButton_Pressed);

            this.closeButton.Text = "Close";
            this.closeButton.Bounds = new UniRectangle(new UniScalar(1.0f, -90.0f),
                                                        new UniScalar(1.0f, -40.0f),
                                                        80, 24);

            //
            // helloWorldLabel
            //            
            this.helloWorldLabel.Text = "Hello World! This is a label.\n";
            this.helloWorldLabel.Bounds = new UniRectangle(10.0f, 15.0f, 110.0f, 30.0f);

            //
            // okButton
            //
            this.minimizeButton.Bounds = new UniRectangle(
              new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24);

            this.minimizeButton.Text = "OK";

            this.Title = "Window Title Goes Here";

            //
            // DemoDialog
            //
            this.Bounds = new UniRectangle(600.0f, 100.0f, 512.0f, 384.0f);
            Children.Add(this.helloWorldLabel);
            Children.Add(this.minimizeButton);
            Children.Add(this.closeButton);
        }

        void closeButton_Pressed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
