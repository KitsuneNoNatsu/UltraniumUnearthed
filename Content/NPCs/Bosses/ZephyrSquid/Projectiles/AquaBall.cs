using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace UltraniumUnearthed.Content.NPCs.Bosses.ZephyrSquid.Projectiles
{
    public class AquaBall : ModProjectile
    {
        private int Bounces = 5;

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 600;
            Projectile.light = 0f;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            if (Projectile.spriteDirection == 1)
                Projectile.rotation += 0.7f;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += -0.7f;

            Projectile.velocity.Y += 0.15f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Bounces--;
            if (Bounces <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X * 0.8f;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
            }
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
