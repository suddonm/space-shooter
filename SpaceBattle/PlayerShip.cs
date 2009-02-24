﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceBattle
{
    class PlayerShip : Actor
    {
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override bool Dead { get { return false; } }
        public override float Radius { get { return 0.75f; } }
        Vector2 velocity;

        PlayerIndex player;
        Texture2D texture;
        Texture2D crosshair;
        EnemyFactory[] factories;
        int factoryIndex;

        float bulletTimeout = 0.0f;

        const float BULLETTIME = 0.1f;
        const float SPEED = 5.0f;
        const float BULLETSPEED = 18.0f;
        const float TRIGGERLENGTH = 10.0f;
        const float SPAWNTHRESH = 10.0f;

        public PlayerShip(PlayerIndex player) { 
            this.player = player;
            factories = new EnemyFactory[3] { null, null, null };
            factoryIndex = 0;
        }

        public void LoadContent(ContentManager Content)
        {
            if (player == PlayerIndex.One)
            {
                texture = Content.Load<Texture2D>("RedShip");
                crosshair = Content.Load<Texture2D>("RedCrosshair");
            }
            else if (player == PlayerIndex.Two)
            {
                texture = Content.Load<Texture2D>("BlueShip");
                crosshair = Content.Load<Texture2D>("BlueCrosshair");
            }
        }

        public override void Update(float dt)
        {
            velocity = SPEED * GamePad.GetState(player).ThumbSticks.Left;
            position += dt * velocity;

            foreach (var i in factories) { if (i != null) { i.Update(dt); } }

            var input = GamePad.GetState(player);

            bulletTimeout -= dt;
            Vector2 dir = input.ThumbSticks.Right;
            if (dir.LengthSquared() > 0.125f)
            {
                var pos = position + TRIGGERLENGTH * dir;
                var other = Util.GetPlayer(Util.OtherPlayer(player));
                Action<int> spawn = z =>
                {
                    if (factories[z] != null)
                    {
                        Enemy e = factories[z].Spawn(pos, other);
                        if (e != null)
                        {
                            Util.Actors.Add(e);
                        }
                    }
                };

                if (input.Triggers.Left > 0.5f) { spawn(0); }
                if (input.Buttons.LeftShoulder == ButtonState.Pressed) { spawn(1); }
                if (input.Buttons.RightShoulder == ButtonState.Pressed) { spawn(2); }

                if (input.Triggers.Right > 0.5f && bulletTimeout <= 0) {
                    Util.Actors.Add(new Bullet(position + dir, BULLETSPEED * dir));
                    bulletTimeout = BULLETTIME;
                }
            }

        }

        public override void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(texture, position, rot - (float)Math.PI/2, 1);

            Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
            if (dir.Length() >= 0.125f)
            {
                Util.DrawSprite(crosshair, position + TRIGGERLENGTH * dir, 0, 0.5f);
            }
        }

        public override void Collision(Actor other)
        {
        }

        public void Equip(EnemyFactory factory)
        {
            factories[factoryIndex++] = factory;
            factoryIndex %= 3;
        }
    }
}
