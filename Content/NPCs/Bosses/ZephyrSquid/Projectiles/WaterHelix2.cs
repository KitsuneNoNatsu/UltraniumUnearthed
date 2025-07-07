using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace UltraniumUnearthed.Content.NPCs.Bosses.ZephyrSquid.Projectiles
{
    public class WaterHelix2 : ModProjectile
    {
        public override string Texture => "UltraniumUnearthed/Content/NPCs/Bosses/ZephyrSquid/Projectiles/WaterHelix1";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.alpha = 255;
            Projectile.timeLeft = 600;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.penetrate = 4;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D trailTexture = ModContent.Request<Texture2D>("UltraniumUnearthed/Content/NPCs/Bosses/ZephyrSquid/Projectiles/WaterHelixTrail").Value;
            Vector2 drawOrigin = new Vector2(trailTexture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(trailTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.localAI[0] += 0.075f;
            if (Projectile.localAI[0] > 8f)
                Projectile.localAI[0] = 8f;

            float rotationSpeed = Projectile.localAI[0];
            float phaseLength = 16f;

            Projectile.ai[0]++;

            if (Projectile.ai[1] == 0)
            {
                if (Projectile.ai[0] <= phaseLength)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(rotationSpeed));
                }
                else
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-rotationSpeed));
                }

                if (Projectile.ai[0] >= phaseLength * 2f)
                {
                    Projectile.ai[0] = 0f;
                }
            }
            else if (Projectile.ai[0] > phaseLength * 0.5f)
            {
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 1f;
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-rotationSpeed));
            }
        }
    }
}
