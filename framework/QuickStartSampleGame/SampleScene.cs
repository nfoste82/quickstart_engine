//
// SampleScene.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using QuickStart;
using QuickStart.Physics;
using QuickStart.Physics.CollisionEffects;
using QuickStart.Physics.BEPU;
using QuickStart.Graphics;
using QuickStart.Entities;
using QuickStart.Entities.Heightfield;
using QuickStart.Entities.NavMesh;
using QuickStart.Components;
using QuickStart.Interfaces;
using QuickStart.ParticleSystem;
using QuickStart.GeometricPrimitives;
using QuickStart.Utils;

namespace QuickStartSampleGame
{
    /// <summary>
    /// A test scene demonstrating the physics of QuickStart.
    /// </summary>
    /// <remarks>Ideally we will eventually be able to create a scene with a level editor, and save the level into a file format (like XML)
    ///          and then we would read this file in to load a scene.</remarks>
    public class SampleScene : Scene
    {
        public int AntiGravityBoxEffectID = 0;
        public bool TildePressed = false;

        private Dictionary<Int64, EntityInfoWindow> infoWindows;

        public SampleScene(QSGame game) : base(game)
        {
            LoadFog("EnvironmentalEffects/Fog/GrayLowLayingFog");

            // Load terrain
            this.game.SceneManager.CreateAndAddEntityByTemplateID(4);

            LoadCharacter();

            // Load light
            this.game.SceneManager.CreateAndAddEntityByTemplateID(9);
                       
            LoadSkyDome();

            // Load a space ship
            BaseEntity newEntity = new BaseEntity(this.game, new Vector3(1000.0f, 254.0f, 600.0f), Matrix.CreateRotationX(0.3f), 10.0f);
            this.game.SceneManager.AddEntityByTemplateID(newEntity, 8);

            //LoadRainParticleEmitter();
            LoadSnowParticleEmitter();

            LoadWater();

            BoxStack(2, new Vector3(1015.0f, 185.0f, 700.0f));
            BoxAntiGrav(new Vector3(1000.0f, 200.0f, 730.0f), false);
            BoxAntiGrav(new Vector3(1000.0f, 200.0f, 575.0f), true);

            LoadPrimitives();

            // This allows our scene to hear about messages, in case we want to do any scene-specific events.
            game.GameMessage += this.Game_GameMessage;            
        }

        /// <summary>
        /// Message listener for all entities
        /// </summary>
        /// <param name="message">Incoming message</param>
        protected virtual void Game_GameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ButtonDown:
                    {
                        var msgButtonDown = message as MsgGamePadButtonPressed;
                        message.TypeCheck(msgButtonDown);

                        switch (msgButtonDown.Button)
                        {
                            case Buttons.Y:
                                {
                                    HandleActionPressed(false, false);
                                }
                                break;
                            case Buttons.B:
                                {
                                    HandleActionPressed(true, false);
                                }
                                break;
                            case Buttons.X:
                                {
                                    HandleActionPressed(false, true);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case MessageType.KeyDown:
                    {
                        var msgKeyDown = message as MsgKeyPressed;
                        message.TypeCheck(msgKeyDown);

                        switch (msgKeyDown.Key)
                        {
                            case Keys.Space:
                                {
                                    HandleActionPressed(false, false);
                                }
                                break;
                            case Keys.LeftShift:
                                {
                                    HandleActionPressed(true, false);
                                }
                                break;
                            case Keys.LeftControl:
                                {
                                    HandleActionPressed(false, true);
                                }
                                break;
                            case Keys.F3:
                                {
                                    this.game.ToggleMouseVisiblity();
                                }
                                break;
#if WINDOWS
                            case Keys.C:
                                {
                                    this.game.PhysicsRenderer.Enabled = !this.game.PhysicsRenderer.Enabled;
                                }
                                break;
#endif //!WINDOWS
                            case Keys.B:
                                {
                                    this.game.DrawShadowMapTextureToScreen = !this.game.DrawShadowMapTextureToScreen;
                                }
                                break;
                            case Keys.M:
                                {
                                    var msgGetControl = ObjectPool.Aquire<MsgGetControlledEntity>();
                                    this.game.SendInterfaceMessage(msgGetControl, InterfaceType.SceneManager);

                                    // If a controlled entity is found
                                    if ( msgGetControl.ControlledEntityID != QSGame.UniqueIDEmpty )
                                    {
                                        // Check whether or not the controlled entity is movable.
                                        var msgGetMovable = ObjectPool.Aquire<MsgGetHasDynamicPhysics>();
                                        msgGetMovable.UniqueTarget = msgGetControl.ControlledEntityID;
                                        this.game.SendMessage(msgGetMovable);

                                        // Toggle the controlled entity's movable state.
                                        var msgSetMovable = ObjectPool.Aquire<MsgSetPhysicsMovableState>();
                                        msgSetMovable.Movable = !msgGetMovable.HasDynamicPhysics;
                                        msgSetMovable.UniqueTarget = msgGetControl.ControlledEntityID;
                                        this.game.QueueMessage(msgSetMovable);
                                    }
                                }
                                break;
                            case Keys.O:
                                {
                                    if (this.game.TerrainID != QSGame.UniqueIDEmpty)
                                    {
                                        var msgToggleBoxes = ObjectPool.Aquire<MsgToggleDisplayBoundingBoxes>();
                                        msgToggleBoxes.UniqueTarget = this.game.TerrainID;
                                        this.game.QueueMessage(msgToggleBoxes);
                                    }
                                }
                                break;
                            case Keys.F2:
                                {
                                    var msgGetRender = ObjectPool.Aquire<MsgGetRenderEntity>();
                                    this.game.SendInterfaceMessage(msgGetRender, InterfaceType.Camera);

                                    if (msgGetRender.EntityID != QSGame.UniqueIDEmpty)
                                    {
                                        var msgSetParent = ObjectPool.Aquire<MsgSetParent>();
                                        msgSetParent.ParentEntity = null;
                                        msgSetParent.UniqueTarget = msgGetRender.EntityID;
                                        this.game.SendMessage(msgSetParent);
                                    }

                                    var msgSetControlled = ObjectPool.Aquire<MsgSetControlledEntity>();
                                    msgSetControlled.ControlledEntityID = QSGame.UniqueIDEmpty;
                                    this.game.SendInterfaceMessage(msgSetControlled, InterfaceType.SceneManager);
                                }
                                break;
                            case Keys.Tab:
                                {
                                    var msgGetRender = ObjectPool.Aquire<MsgGetRenderEntity>();
                                    this.game.SendInterfaceMessage(msgGetRender, InterfaceType.Camera);

                                    if (msgGetRender.EntityID != QSGame.UniqueIDEmpty)
                                    {
                                        var msgGetControlled = ObjectPool.Aquire<MsgGetControlledEntity>();
                                        this.game.SendInterfaceMessage(msgGetControlled, InterfaceType.SceneManager);

                                        bool ControlledEntityExists = (msgGetControlled.ControlledEntityID != QSGame.UniqueIDEmpty);

                                        // Get all entities in the scene
                                        var msgGetIDList = ObjectPool.Aquire<MsgGetEntityIDList>();
                                        this.game.SendInterfaceMessage(msgGetIDList, InterfaceType.SceneManager);

                                        Int64[] entityList = msgGetIDList.EntityIDList;
                                        
                                        // If the scene root entity is the only entity, we can't do anything else here
                                        if (entityList.Length == 1)
                                            break;

                                        for (int i = 0; i < entityList.Length; ++i)
                                        {
                                            // If there is no controlled entity, or the entity we just found is the controlled entity
                                            if ( !ControlledEntityExists || (entityList[i] == msgGetControlled.ControlledEntityID) )
                                            {
                                                int EntityIndex = i;

                                                // If this entity is at the end of the list, grab the entity at the beginning of the list.
                                                if (EntityIndex == entityList.Length - 1)
                                                {
                                                    EntityIndex = QSGame.FirstEntityIDAbleToBeAltered;
                                                }
                                                else
                                                {
                                                    ++EntityIndex;
                                                }

                                                // Make sure we don't set the camera as its own parent
                                                if (entityList[EntityIndex] == msgGetRender.EntityID)
                                                {
                                                    if (EntityIndex == entityList.Length - 1)
                                                    {
                                                        EntityIndex = QSGame.FirstEntityIDAbleToBeAltered;
                                                    }
                                                    else
                                                    {
                                                        ++EntityIndex;
                                                    }

                                                    // If camera was chosen again, it is the only entity, so we can't do anything
                                                    if (entityList[EntityIndex] == msgGetRender.EntityID)
                                                    {
                                                        break;
                                                    }
                                                }

                                                var msgGetEntity = ObjectPool.Aquire<MsgGetEntityByID>();
                                                msgGetEntity.EntityID = entityList[EntityIndex];
                                                this.game.SendInterfaceMessage(msgGetEntity, InterfaceType.SceneManager);

                                                if ( msgGetEntity.Entity != null )
                                                {
                                                    var msgSetControlled = ObjectPool.Aquire<MsgSetControlledEntity>();
                                                    msgSetControlled.ControlledEntityID = entityList[EntityIndex];
                                                    this.game.SendInterfaceMessage(msgSetControlled, InterfaceType.SceneManager);
                                                }

                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case Keys.Back:
                                {
                                    var msgGetIDList = ObjectPool.Aquire<MsgGetEntityIDList>();
                                    this.game.SendInterfaceMessage(msgGetIDList, InterfaceType.SceneManager);

                                    Int64[] entityList = msgGetIDList.EntityIDList;
                                    for (int i = entityList.Length - 1; i > QSGame.FirstEntityIDAbleToBeAltered; --i)
                                    {
                                        if ( entityList[i] >= QSGame.FirstEntityIDAbleToBeAltered )
                                        {
                                            var msgRemEntity = ObjectPool.Aquire<MsgRemoveEntity>();
                                            msgRemEntity.EntityID = entityList[i];
                                            this.game.SendInterfaceMessage(msgRemEntity, InterfaceType.SceneManager);
                                        }
                                    }
                                }
                                break;
                            case Keys.OemTilde:
                                {
                                    TildePressed = true;
                                }
                                break;
                            default:
                                break;
                        }                                                
                    }
                    break;
                case MessageType.KeyUp:
                    {
                        var msgKeyRelease = message as MsgKeyReleased;
                        message.TypeCheck(msgKeyRelease);

                        switch (msgKeyRelease.Key)
                        {
                            case Keys.OemTilde:
                                {
                                    TildePressed = false;
                                }
                                break;
                        }
                    }
                    break;
                case MessageType.MouseDown:
                    {
                        var msgMousePress = message as MsgMouseButtonPressed;
                        message.TypeCheck(msgMousePress);

                        switch (msgMousePress.Button)
                        {
                            case MouseButton.Left:
                                {
                                    if ( this.game.IsMouseVisible && TildePressed)
                                    {
                                        RayCastForEntityReport();
                                    }
                                }
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void RayCastForEntityReport()
        {
            // First we determine the line segment between the camera's position and the cursor
            // as if it were at the far plane.
            var msgGetSegment = ObjectPool.Aquire<MsgGetLineSegmentToCursor>();
            this.game.SendInterfaceMessage(msgGetSegment, InterfaceType.Camera);
            
            // Now we use that line segment to check for physics collisions
            PhysicsInterface physics = this.game.SceneManager.GetInterface(InterfaceType.Physics) as PhysicsInterface;

            if (null == physics)
            {
                throw new Exception("Cannot perform a physics ray cast without a registered PhysicsInterface");
            }

            SegmentIntersectInfo info = physics.PerformSegmentIntersectQuery(msgGetSegment.lineSegment);

            // Check if the ray hit anything
            if (QSGame.UniqueIDEmpty == info.entityID)
            {
                return;
            }

            List<TypeReflectionInfo> entityInfo;
            this.game.SceneManager.GetEntityReflectionInfo(info.entityID, out entityInfo);

            if (null == this.infoWindows)
            {
                this.infoWindows = new Dictionary<Int64, EntityInfoWindow>();
            }

            EntityInfoWindow window;
            if (!this.infoWindows.TryGetValue(info.entityID, out window))
            {
                // If a window doesn't yet exist for this entity, then we create one
                window = new EntityInfoWindow(100, 100, info.entityID, this.game);

                this.infoWindows.Add(info.entityID, window);

                this.game.Gui.Screen.Desktop.Children.Add(window);
            }
            else
            {
                if (!window.IsOpen)
                {
                    this.game.Gui.Screen.Desktop.Children.Add(window);
                }

                window.Label.Items.Clear();
            }

            for (int i = 0; i < entityInfo.Count; ++i)
            {
                TypeReflectionInfo rInfo = entityInfo[i];

                window.Label.Items.Add(rInfo.typeName);

                List<string> details = QSUtils.ConvertTypeReflectionInfoToString(rInfo.properties);

                for (int j = 0; j < details.Count; ++j)
                {                                        
                    window.Label.Items.Add(details[j]);
                }                
            }
        }

        public void BoxStack(int totalRows, Vector3 offset)
        {
            int currentColumnMax = totalRows;

            float boxScale = 9.0f;
            StaticModel mesh = this.game.ModelLoader.LoadStaticModel("Models/WoodCrate/Crate1");
            BoxShapeDesc boxShape = PhysicsComponent.CreateBoxShapeFromMesh(mesh, boxScale);            

            for (int rows = 0; rows < totalRows; ++rows, --currentColumnMax)
            {
                for (int columns = 0; columns < currentColumnMax; ++columns)
                {
                    Vector3 boxPos = offset + new Vector3(columns * boxShape.Extents.X,
                                                          rows * boxShape.Extents.Y + (boxShape.Extents.Y * 0.5f),
                                                          0.0f);

                    boxPos.X -= currentColumnMax * boxShape.Extents.X * 0.5f;

                    int boxTemplate = 2;
                    if ( rows == (totalRows - 1))                    
                    {
                        // Use a smoking box if we're on the top row
                        boxTemplate = 3;
                    }

                    var boxEntity = new BaseEntity(this.game, boxPos, Matrix.Identity, 9.0f);                    
                    this.game.SceneManager.AddEntityByTemplateID(boxEntity, boxTemplate);

                    if ( boxTemplate == 3 )
                    {
                        ParticleEmitterComponent emitter = boxEntity.GetComponentByType(ComponentType.ParticleEmitterComponent) as ParticleEmitterComponent;
                        // These give offsets for the emitter and how many particles per second to emit.
                        emitter.InitializeEmitterSettings(Vector3.Zero, 7, Vector3.Zero);
                    }
                }
            }
        }

        public void LoadPrimitives()
        {
            // Load a cylinder primitive shape from XML
            {
                int cylinderTemplate = 14;
                var cylinderEntity = new BaseEntity(this.game, new Vector3(1200.0f, 200.0f, 600.0f), Matrix.Identity, 1.0f);
                this.game.SceneManager.AddEntityByTemplateID(cylinderEntity, cylinderTemplate);
            }

            // Load a cylinder primitive from code
            {
                var cylinderEntity = new BaseEntity(this.game, new Vector3(1200.0f, 197.5f, 615.0f), Matrix.Identity, 1.0f);

                // We load a render component manually from code
                var cylRenderDef = new RenderComponentDefinition();
                cylRenderDef.CanCreateShadows = true;
                cylRenderDef.CanReceiveShadows = true;
                cylRenderDef.Diameter = 7.0f;
                cylRenderDef.Height = 15.0f;
                cylRenderDef.MaterialPath = "Material/SimpleColored";
                cylRenderDef.MeshColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                cylRenderDef.PrimitiveType = GeometricPrimitiveType.Cylinder;
                cylRenderDef.RendersAsSky = false;
                new RenderComponent(cylinderEntity, cylRenderDef);

                // We load a physics component manually from code
                var cylPhysDef = new PhysicsComponentDefinition();
                cylPhysDef.ShapeType = ShapeType.Cylinder;
                cylPhysDef.IsDynamic = true;    // Dynamic means it is affected by collision
                cylPhysDef.AffectedByGravity = false;
                cylPhysDef.CollisionGroupType = CollisionGroups.RigidBody;
                cylPhysDef.Diameter = cylRenderDef.Diameter;
                cylPhysDef.Height = cylRenderDef.Height;
                cylPhysDef.Mass = 1000.0f;
                new PhysicsComponent(cylinderEntity, cylPhysDef);

                this.game.SceneManager.AddEntity(cylinderEntity);
            }

            // Load a cone primitive from code
            {
                var coneEntity = new BaseEntity(this.game, new Vector3(1200.0f, 197.5f, 630.0f), Matrix.Identity, 1.0f);

                var coneRenderDef = new RenderComponentDefinition();
                coneRenderDef.CanCreateShadows = true;
                coneRenderDef.CanReceiveShadows = true;
                coneRenderDef.Diameter = 7.0f;
                coneRenderDef.Height = 15.0f;
                coneRenderDef.MaterialPath = "Material/SimpleColored";
                coneRenderDef.MeshColor = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                coneRenderDef.PrimitiveType = GeometricPrimitiveType.Cone;
                coneRenderDef.RendersAsSky = false;
                new RenderComponent(coneEntity, coneRenderDef);

                var conePhysDef = new PhysicsComponentDefinition();
                conePhysDef.ShapeType = ShapeType.Cone;
                conePhysDef.IsDynamic = true;
                conePhysDef.AffectedByGravity = false;
                conePhysDef.CollisionGroupType = CollisionGroups.RigidBody;
                conePhysDef.Diameter = coneRenderDef.Diameter;
                conePhysDef.Height = coneRenderDef.Height;
                conePhysDef.Mass = 1000.0f;
                new PhysicsComponent(coneEntity, conePhysDef);

                this.game.SceneManager.AddEntity(coneEntity);
            }

            // Load a sphere from code
            {
                var sphereEntity = new BaseEntity(this.game, new Vector3(1200.0f, 195.0f, 645.0f), Matrix.Identity, 1.0f);

                var sphereRenderDef = new RenderComponentDefinition();
                sphereRenderDef.CanCreateShadows = true;
                sphereRenderDef.CanReceiveShadows = true;
                sphereRenderDef.Diameter = 10.0f;
                sphereRenderDef.MaterialPath = "Material/SimpleColored";
                sphereRenderDef.MeshColor = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                sphereRenderDef.PrimitiveType = GeometricPrimitiveType.Sphere;
                sphereRenderDef.RendersAsSky = false;
                new RenderComponent(sphereEntity, sphereRenderDef);

                var spherePhysDef = new PhysicsComponentDefinition();
                spherePhysDef.ShapeType = ShapeType.Sphere;
                spherePhysDef.IsDynamic = true;
                spherePhysDef.AffectedByGravity = false;
                spherePhysDef.CollisionGroupType = CollisionGroups.RigidBody;
                spherePhysDef.Diameter = sphereRenderDef.Diameter;
                spherePhysDef.Mass = 800.0f;
                new PhysicsComponent(sphereEntity, spherePhysDef);

                this.game.SceneManager.AddEntity(sphereEntity);
            }

            // Load a rectangular box from code
            {
                var boxEntity = new BaseEntity(this.game, new Vector3(1200.0f, 197.5f, 660.0f), Matrix.Identity, 1.0f);

                var boxRenderDef = new RenderComponentDefinition();
                boxRenderDef.CanCreateShadows = true;
                boxRenderDef.CanReceiveShadows = true;
                boxRenderDef.Height = 15.0f;
                boxRenderDef.Width = 12.0f;
                boxRenderDef.Depth = 6.0f;
                boxRenderDef.MaterialPath = "Material/SimpleColored";
                boxRenderDef.MeshColor = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
                boxRenderDef.PrimitiveType = GeometricPrimitiveType.Box;
                boxRenderDef.RendersAsSky = false;
                new RenderComponent(boxEntity, boxRenderDef);

                var boxPhysDef = new PhysicsComponentDefinition();
                boxPhysDef.ShapeType = ShapeType.Box;
                boxPhysDef.IsDynamic = true;
                boxPhysDef.AffectedByGravity = false;
                boxPhysDef.CollisionGroupType = CollisionGroups.RigidBody;
                boxPhysDef.Height = boxRenderDef.Height;
                boxPhysDef.Width = boxRenderDef.Width;
                boxPhysDef.Depth = boxRenderDef.Depth;
                boxPhysDef.Mass = 800.0f;
                new PhysicsComponent(boxEntity, boxPhysDef);

                this.game.SceneManager.AddEntity(boxEntity);
            }

            // Load a cube from code
            {
                var cubeEntity = new BaseEntity(this.game, new Vector3(1200.0f, 196.0f, 675.0f), Matrix.Identity, 1.0f);

                var boxRenderDef = new RenderComponentDefinition();
                boxRenderDef.CanCreateShadows = true;
                boxRenderDef.CanReceiveShadows = true;
                boxRenderDef.Height = 12.0f;
                boxRenderDef.Width = 12.0f;
                boxRenderDef.Depth = 12.0f;
                boxRenderDef.MaterialPath = "Material/SimpleColored";
                boxRenderDef.MeshColor = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
                boxRenderDef.PrimitiveType = GeometricPrimitiveType.Box;
                boxRenderDef.RendersAsSky = false;
                new RenderComponent(cubeEntity, boxRenderDef);

                var boxPhysDef = new PhysicsComponentDefinition();
                boxPhysDef.ShapeType = ShapeType.Box;
                boxPhysDef.IsDynamic = true;
                boxPhysDef.AffectedByGravity = false;
                boxPhysDef.CollisionGroupType = CollisionGroups.RigidBody;
                boxPhysDef.Height = boxRenderDef.Height;
                boxPhysDef.Width = boxRenderDef.Width;
                boxPhysDef.Depth = boxRenderDef.Depth;
                boxPhysDef.Mass = 800.0f;
                new PhysicsComponent(cubeEntity, boxPhysDef);

                this.game.SceneManager.AddEntity(cubeEntity);
            }
        }

        public void BoxAntiGrav(Vector3 position, bool invisible)
        {                        
            int boxTemplate = 5;
            if (!invisible)
            {
                boxTemplate = 6;
            }

            var newEntity = new BaseEntity(this.game, position, Matrix.Identity, 1.0f);
            this.game.SceneManager.AddEntityByTemplateID(newEntity, boxTemplate);
        }

        public void HandleActionPressed(bool redPressed, bool bluePressed)
        {
            bool ShouldShootSphere = true;

            // If camera is attached to a character, then do not shoot a sphere because
            // the spacebar is also the 'jump' key for the character.
            var msgGetRender = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.game.SendInterfaceMessage(msgGetRender, InterfaceType.Camera);

            if (msgGetRender.EntityID != QSGame.UniqueIDEmpty)
            {
                var msgGetParent = ObjectPool.Aquire<MsgGetParentID>();
                msgGetParent.UniqueTarget = msgGetRender.EntityID;
                this.game.SendMessage(msgGetParent);

                if (msgGetParent.ParentEntityID != QSGame.UniqueIDEmpty)
                {
                    var msgIsChar = ObjectPool.Aquire<MsgGetIsACharacter>();
                    msgIsChar.UniqueTarget = msgGetParent.ParentEntityID;
                    this.game.SendMessage(msgIsChar);

                    if (msgIsChar.IsCharacter)
                    {
                        ShouldShootSphere = false;
                    }
                }
            }

            if (ShouldShootSphere)
            {
                ShootSphere(redPressed, bluePressed);
            }
        }

        public void ShootSphere(bool redPressed, bool bluePressed)
        {
            // We want to fire a sphere out from the camera            

            // First we need to get the current rendering camera.
            var camMsg = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.game.SendInterfaceMessage(camMsg, InterfaceType.Camera);

            // Make sure the camera's unique ID is valid
            if (camMsg.EntityID != QSGame.UniqueIDEmpty)
            {
                // Now that we have our camera, grab it's forward vector
                var getVectMsg = ObjectPool.Aquire<MsgGetVectorForward>();                
                getVectMsg.UniqueTarget = camMsg.EntityID;
                this.game.SendMessage(getVectMsg);

                var getPosMsg = ObjectPool.Aquire<MsgGetPosition>();
                getPosMsg.UniqueTarget = camMsg.EntityID;
                this.game.SendMessage(getPosMsg);

                // We'll start the sphere just a bit in front of the camera
                Vector3 spherePos = getPosMsg.Position + (getVectMsg.Forward * 20);

                int sphereTemplateID = 11;
                if (redPressed)
                {
                    sphereTemplateID = 12;
                }
                else if (bluePressed)
                {
                    sphereTemplateID = 13;
                }

                // Create an entity, and give it a render and physics component
                var newEntity = new BaseEntity(this.game, spherePos, Matrix.Identity, 10.0f);
                this.game.SceneManager.AddEntityByTemplateID(newEntity, sphereTemplateID);
                
                // Now that our sphere is in the scene, let's give it a nudge
                var setLinVelMsg = ObjectPool.Aquire<MsgSetLinearVelocity>();
                setLinVelMsg.LinearVelocity = (getVectMsg.Forward * 150.0f);
                setLinVelMsg.UniqueTarget = newEntity.UniqueID;
                this.game.SendMessage(setLinVelMsg);
            }
        }     

        private void LoadSkyDome()
        {
            var sceneSky = new BaseEntity(this.game, new Vector3(0.0f, 0.0f, 0.0f), Matrix.CreateRotationZ(MathHelper.Pi), 2000.0f);
            this.game.SceneManager.AddEntityByTemplateID(sceneSky, 7);

            // The skydome asset was not exported with Y as the up vector, so we can fix that here.
            var msgModRot = ObjectPool.Aquire<MsgModifyRotation>();
            msgModRot.Rotation = Matrix.CreateRotationX(-MathHelper.PiOver2);
            msgModRot.UniqueTarget = sceneSky.UniqueID;
            this.game.QueueMessage(msgModRot);
        }
        
        private void LoadCharacter()
        {            
            BaseEntity newEntity = new BaseEntity(this.game, new Vector3(1170.0f, 197.5f, 615.0f), Matrix.CreateRotationY(-0.3f), 1.0f);
            this.game.SceneManager.AddEntityByTemplateID(newEntity, 1);

            // Uncomment this to start the demo in control of the character entity
            //MsgSetControlledEntity setControlled = ObjectPool.Aquire<MsgSetControlledEntity>();
            //setControlled.ControlledEntityID = newEntity.UniqueID;
            //this.game.SendInterfaceMessage(setControlled, InterfaceType.SceneManager);

            var lockCharToCamMsg = ObjectPool.Aquire<MsgLockCharacterRotationToCamera>();            
            lockCharToCamMsg.LockRotation = true;
            lockCharToCamMsg.UniqueTarget = newEntity.UniqueID;
            this.game.QueueMessage(lockCharToCamMsg);
        }

        private void LoadRainParticleEmitter()
        {
            // Lets find the render camera and attach a particle emitter to it.
            var msgGetRender = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.game.SendInterfaceMessage(msgGetRender, InterfaceType.Camera);

            if (msgGetRender.EntityID == QSGame.UniqueIDEmpty)
            {
                return;
            }

            var msgGetEntity = ObjectPool.Aquire<MsgGetEntityByID>();
            msgGetEntity.EntityID = msgGetRender.EntityID;
            this.game.SendInterfaceMessage(msgGetEntity, InterfaceType.SceneManager);

            if (msgGetEntity.Entity == null)
            {
                return;
            }

            ParticleEmitterComponent rainEmitter = EntityLoader.LoadComponent(msgGetEntity.Entity,
                                                           ComponentType.ParticleEmitterComponent,
                                                           this.game.Content,
                                                           "Entities/ComponentDefinitions/ParticleEmitter/RainWeatherEmitter")
                                                           as ParticleEmitterComponent;

            // Give the emitter some settings.
            rainEmitter.InitializeEmitterSettings(new Vector3(0.0f, 250.0f, 0.0f), 500, new Vector3(325.0f, 10.0f, 325.0f));
        }

        private void LoadSnowParticleEmitter()
        {
            // Lets find the render camera and attach a particle emitter to it.
            var msgGetRender = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.game.SendInterfaceMessage(msgGetRender, InterfaceType.Camera);

            if (msgGetRender.EntityID == QSGame.UniqueIDEmpty)
            {
                return;
            }

            var msgGetEntity = ObjectPool.Aquire<MsgGetEntityByID>();
            msgGetEntity.EntityID = msgGetRender.EntityID;
            this.game.SendInterfaceMessage(msgGetEntity, InterfaceType.SceneManager);

            if (msgGetEntity.Entity == null)
            {
                return;
            }

            ParticleEmitterComponent snowEmitter = EntityLoader.LoadComponent(msgGetEntity.Entity,
                                                           ComponentType.ParticleEmitterComponent,
                                                           this.game.Content,
                                                           "Entities/ComponentDefinitions/ParticleEmitter/SnowWeatherEmitter")
                                                           as ParticleEmitterComponent;

            // Give the emitter some settings.
            snowEmitter.InitializeEmitterSettings(new Vector3(0.0f, 100.0f, 0.0f), 500, new Vector3(300.0f, 10.0f, 300.0f));
        }

        private void LoadWater()
        {
            var newEntity = new BaseEntity(this.game, new Vector3(0.0f, 165.0f, 0.0f), Matrix.Identity, 1.0f);
            this.game.SceneManager.AddEntityByTemplateID(newEntity, 10);

            int size = 4096;
            if (this.game.TerrainID != QSGame.UniqueIDEmpty)
            {
                var msgGetTerrainProps = ObjectPool.Aquire<MsgGetTerrainProperties>();
                msgGetTerrainProps.UniqueTarget = this.game.TerrainID;
                this.game.SendMessage(msgGetTerrainProps);

                size = msgGetTerrainProps.Size;
                size *= msgGetTerrainProps.TerrainScale;
            }

            WaterComponent waterComp = newEntity.GetComponentByType(ComponentType.WaterComponent) as WaterComponent;
            waterComp.SetupWaterVertices(size, size);
        }

        private void LoadNavMesh()
        {
            var mesh = new NavMesh(game);
            mesh.Name = "Nav Mesh";

            var genNavMeshMessage = ObjectPool.Aquire<MsgGenerateNavMesh>();
            game.QueueMessage(genNavMeshMessage);
            
            this.game.SceneManager.AddEntity(mesh);
        }
    }
}
