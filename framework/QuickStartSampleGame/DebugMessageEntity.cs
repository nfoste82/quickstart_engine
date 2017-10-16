//
// DebugMessageEntity.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using QuickStart.Entities;
using QuickStart;
using System.Diagnostics;
using System.Globalization;

namespace QuickStartSampleGame
{
    public class DebugMessageEntity : BaseEntity
    {
        public DebugMessageEntity(QSGame game) : base(game)
        {
            this.Game.GameMessage += new EngineMessageHandler(Game_GameMessage);
        }

        protected override void Game_GameMessage(IMessage message)
        {
            if (message.Type != MessageType.DebugText)
            {
                return;
            }

            Message<string> msg = message as Message<string>;
            if (msg.Type != MessageType.DebugText)
            {
                return;
            }

            Debug.IndentLevel = 2;
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,"Id: {0} - Message: {1}", msg.Type, msg.Data));
        }
    }
}
