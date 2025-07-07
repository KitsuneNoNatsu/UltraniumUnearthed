using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace UltraniumUnearthed.Content.NPCs.Bosses.ZephyrSquid.Projectiles
{
    public class SquidChargeTelegraph : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] <= 20)
            {
                if (Projectile.alpha > 255)
                    return;
                Projectile.alpha -= 12;
            }
            else
            {
                if (Projectile.ai[0] < 40)
                    return;
                Projectile.alpha += 5;
                if (Projectile.alpha < 255)
                    return;
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.position - new Vector2(3f, 4000f) - Main.screenPosition;
            Color drawColor = new Color(55, 69, 188) * (1f - Projectile.alpha / 255f);
            Vector2 scale = new Vector2(6f, 4f);

            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, drawPos, null, drawColor, Projectile.rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
