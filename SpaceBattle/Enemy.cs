﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class Enemy : Actor
    {
        public Enemy(Vector2 pos, Actor targ,
                     BehaviorComponent b, SeekerComponent s, DamageComponent d)
        {
            position = pos;
            target = targ;
            Behavior = b;
            Seeker = s;
            Damage = d;
            if (Behavior != null) Behavior.Reassign(this);
            if (Seeker != null) Seeker.Reassign(this);
            if (Damage != null) Damage.Reassign(this);
        }

        // only for use in components:
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 accel;
        public bool dead = false;
        public Actor target = null;

        public BehaviorComponent Behavior = null;
        public SeekerComponent Seeker = null;
        public DamageComponent Damage = null;

        public Enemy Clone()
        {
            return new Enemy(position, target, 
                Behavior == null ? null : Behavior.Clone(), 
                Seeker == null ? null : Seeker.Clone(), 
                Damage == null ? null : Damage.Clone());
        }

        public override void Draw()
        {
            if (Behavior == null && Seeker == null && Damage == null)
            {
                Util.DrawSprite(Textures.EmptyEnemy, position, 0, 1.0f);
            }
            else
            {
                if (Behavior != null) Behavior.Draw();
                if (Seeker != null) Seeker.Draw();
                if (Damage != null) Damage.Draw();
            }
        }

        public override void Update(float dt)
        {
            if (Behavior != null) Behavior.Update(dt);
            if (Seeker != null) Seeker.Update(dt); 
            velocity += dt * accel;
            position += dt * velocity;
            accel = new Vector2();
        }

        public override bool Dead { get { return dead; } }
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.50f; } }

        public override void Collision(Actor other)
        {
            if (dead) return;
            if (Damage != null)
            {
                Damage.OnHit(other);
            }
            else {
                Bullet b = other as Bullet;
                if (b != null) { 
                    dead = true;
                    b.SetDead();
                    Util.RandomExplosion(position);
                }
            }
            if (dead) Util.EnemyDeath(position);
        }

        public void Absorb(Enemy other)
        {
            if ((Behavior == null || other.Behavior == null)
             && (Seeker == null || other.Seeker == null)
             && (Damage == null || other.Damage == null))
            {
                Behavior = Join(Behavior, other.Behavior);
                Seeker = Join(Seeker, other.Seeker);
                Damage = Join(Damage, other.Damage);
                other.dead = true;
            }
        }

        static T Join<T>(T x, T y) where T:class
        {
            if (x == null) { return y; }
            if (y == null) { return x; }
            return null;
        }

    }
}
