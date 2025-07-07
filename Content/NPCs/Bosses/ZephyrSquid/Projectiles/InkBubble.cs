using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace UltraniumUnearthed.Content.NPCs.Bosses.ZephyrSquid.Projectiles
{
    public class InkBubble : ModProjectile
    {
        private int Bounces = 2;

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
                Projectile.rotation += 0.05f;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += -0.05f;
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

            if (Projectile.timeLeft < 100)
                Projectile.scale += 0.02f;

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item112, Projectile.position);

            for (int i = 0; i < 40; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, -2f, 0, default(Color), 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].position.X += Main.rand.Next(-50, 51) * 0.05f - 1.5f;
                Main.dust[dust].position.Y += Main.rand.Next(-50, 51) * 0.05f - 1.5f;

                if (Main.dust[dust].position != Projectile.Center)
                    Main.dust[dust].velocity = Projectile.DirectionTo(Main.dust[dust].position) * 2f;
            }
        }
    }
}
