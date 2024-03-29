﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Window;
using SFML.Graphics;
using SFML.System;

using TGUI;

using KnightsOfEmpire.Common.GameStates;
using KnightsOfEmpire.Common.Networking;
using KnightsOfEmpire.Common.Networking.TCP;
using KnightsOfEmpire.Common.Networking.UDP;
using System.Net;
using KnightsOfEmpire.Common.Map;
using KnightsOfEmpire.Common.Navigation;
using System.Runtime.InteropServices;
using KnightsOfEmpire.Common.Buildings;

namespace KnightsOfEmpire.GameStates
{
    public class ViewControlState : GameState
    {
        public View View { get; protected set; }

        private float gameZoom = 1;
        private float minimumZoom = 0.5f;
        private float maximumZoom = 5.0f;
        private float gameZoopSpeed = 25.0f;
        private float delay = 0.2f;
        private float timeInEdge = 0;

        public Vector2i MousePosition;
        public int EdgeViewMoveOffset = 50;
        public int ViewCenterLeftBoundX = 300;
        public int ViewCenterRightBoundX = 700;
        public int ViewCenterTopBoundY = 300;
        public int ViewCenterBottomBoundY = 700;
        public float ViewScrollSpeed = 800;
        public int ViewBottomBoundGuiHeight = 0;

        protected EventHandler<MouseWheelScrollEventArgs> mouseScrollZoomHandler;

        void OnMouseScroll(object sender, EventArgs e)
        {
            MouseWheelScrollEventArgs mouseEvent = (MouseWheelScrollEventArgs)e;
            if ((mouseEvent.Delta < 0 && gameZoom >= maximumZoom) || (mouseEvent.Delta > 0 && gameZoom <= minimumZoom)) return;
            gameZoom -= gameZoopSpeed * mouseEvent.Delta * Client.DeltaTime;
            gameZoom = Math.Max(minimumZoom, Math.Min(maximumZoom, gameZoom));
        }

        public void SetCameraBounds((int xA, int xB, int yA, int yB) bounds)
        {
            ViewCenterLeftBoundX = bounds.xA;
            ViewCenterRightBoundX = bounds.xB;
            ViewCenterTopBoundY = bounds.yA;
            ViewCenterBottomBoundY = bounds.yB;
        }

        public override void Initialize()
        {
            View = new View(new Vector2f(400, 400), new Vector2f(800, 800));
            mouseScrollZoomHandler = new EventHandler<MouseWheelScrollEventArgs>(OnMouseScroll);
            Client.RenderWindow.MouseWheelScrolled += mouseScrollZoomHandler;
        }

        public override void LoadDependencies()
        {
            SetCameraBounds(Parent.GetSiblingGameState<MapRenderState>().GetMapBounds());
        }

        public override void Update()
        {
            ViewBottomBoundGuiHeight = Parent.GetSiblingGameState<GameGUIState>().MainPanelHeight;
            View.Size = new Vector2f(Client.RenderWindow.Size.X, Client.RenderWindow.Size.Y);
            View.Zoom(gameZoom);

            if (!Client.RenderWindow.HasFocus()) return;
            MousePosition = Mouse.GetPosition(Client.RenderWindow);
            float ViewScrollSpeedPerFrame = ViewScrollSpeed * Client.DeltaTime;

            bool mouseInEdge = false;


            if (MousePosition.X >= 0 && MousePosition.X <= EdgeViewMoveOffset)
            {
                if (View.Center.X >= ViewCenterLeftBoundX)
                {
                    if(timeInEdge > delay) 
                    {
                        View.Move(new Vector2f(-ViewScrollSpeedPerFrame, 0));
                    }
                    mouseInEdge = true;
                }
            }
            else if (MousePosition.X >= (Client.RenderWindow.Size.X - EdgeViewMoveOffset) && MousePosition.X <= Client.RenderWindow.Size.X)
            {
                if (View.Center.X <= ViewCenterRightBoundX)
                {
                    if (timeInEdge > delay)
                    {
                        View.Move(new Vector2f(ViewScrollSpeedPerFrame, 0));
                    }
                    mouseInEdge = true;
                }
            }
            if (MousePosition.Y >= 0 && MousePosition.Y <= EdgeViewMoveOffset)
            {
                if (View.Center.Y >= ViewCenterTopBoundY)
                {
                    if (timeInEdge > delay)
                    {
                        View.Move(new Vector2f(0, -ViewScrollSpeedPerFrame));
                    }
                    mouseInEdge = true;
                }
            }
            else if (MousePosition.Y >= (Client.RenderWindow.Size.Y - EdgeViewMoveOffset) - ViewBottomBoundGuiHeight && MousePosition.Y <= Client.RenderWindow.Size.Y - ViewBottomBoundGuiHeight)
            {
                if (View.Center.Y <= ViewCenterBottomBoundY)
                {
                    if (timeInEdge > delay)
                    {
                        View.Move(new Vector2f(0, ViewScrollSpeedPerFrame));
                    }
                    mouseInEdge = true;
                }
            }
            if (mouseInEdge) 
            {
                timeInEdge += Client.DeltaTime;
            }
            else 
            {
                timeInEdge = 0;
            }
        }

        public override void Dispose()
        {
            Client.RenderWindow.MouseWheelScrolled -= mouseScrollZoomHandler;
        }
        public void CenterViewOnBuilding(Building building) 
        {
            float PositionX = building.Position.X * Map.TilePixelSize;
            float PositionY = building.Position.Y * Map.TilePixelSize;
            View.Center = new Vector2f(PositionX, PositionY);
        }
    }
}
