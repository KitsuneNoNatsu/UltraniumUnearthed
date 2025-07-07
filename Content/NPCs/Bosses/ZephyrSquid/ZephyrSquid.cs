using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using UltraniumUnearthed.Common.Systems;

namespace UltraniumUnearthed.Content.NPCs.Bosses.ZephyrSquid
{
    [AutoloadBossHead]
    public class ZephyrSquid : ModNPC
    {
        private int timer;
        private int BoltTimer;
        private int moveSpeed;
        private int moveSpeedY;
        private float HomeY = 100f;
        public int players;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.TrailCacheLength[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 98;
            NPC.height = 256;
            NPC.damage = 20;
            NPC.lifeMax = 3800;
            NPC.defense = 20;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 1f;
            NPC.lavaImmune = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
            players = 1;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/ZephyrSquid");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
               BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
               new FlavorTextBestiaryInfoElement("A massive squid that rules the ocean depths, wielding powerful water magic and ink attacks.")
           });
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            players = numPlayers;
            NPC.lifeMax = (int)(4300 + numPlayers * 430 * bossAdjustment);
            NPC.damage = (int)(35 * balance);
            NPC.defense = (int)(30 * balance);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.velocity != Vector2.Zero)
            {
                Vector2 drawOrigin = new Vector2(ModContent.Request<Texture2D>("UltraniumUnearthed/Content/NPCs/Bosses/ZephyrSquid/ZephyrSquidTrail").Value.Width * 0.2f, NPC.height * 0.2f);

                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length / 2f);
                    spriteBatch.Draw(ModContent.Request<Texture2D>("UltraniumUnearthed/Content/NPCs/Bosses/ZephyrSquid/ZephyrSquidTrail").Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }

        public override bool PreAI()
        {
            NPC.rotation = NPC.velocity.X * 0.02f;
            Player player = Main.player[NPC.target];
            int damage = Main.expertMode ? 14 : 20;

            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                NPC.velocity.Y = -100f;
            }

            NPC.netUpdate = true;
            NPC.TargetClosest(true);

            if (Main.player[NPC.target].dead)
            {
                NPC.velocity.Y = 30f;
                NPC.ai[0]++;
                if (NPC.ai[0] >= 120)
                    NPC.active = false;
            }

            if (NPC.ai[0] == 0)
            {
                if (NPC.Center.X >= player.Center.X && moveSpeed >= -53)
                    moveSpeed--;
                else if (NPC.Center.X <= player.Center.X && moveSpeed <= 53)
                    moveSpeed++;

                NPC.velocity.X = moveSpeed * 0.09f;

                if (NPC.Center.Y >= player.Center.Y - HomeY && moveSpeedY >= -30)
                {
                    moveSpeedY--;
                    HomeY = 150f;
                }
                else if (NPC.Center.Y <= player.Center.Y - HomeY && moveSpeedY <= 30)
                    moveSpeedY++;

                NPC.velocity.Y = moveSpeedY * 0.1f;
            }

            if (NPC.ai[0] == 1)
            {
                if (NPC.Center.X >= player.Center.X && moveSpeed >= -100)
                    moveSpeed--;
                else if (NPC.Center.X <= player.Center.X && moveSpeed <= 100)
                    moveSpeed++;

                NPC.velocity.X = moveSpeed * 0.09f;

                if (NPC.Center.Y >= player.Center.Y - HomeY && moveSpeedY >= -30)
                {
                    moveSpeedY--;
                    HomeY = 150f;
                }
                else if (NPC.Center.Y <= player.Center.Y - HomeY && moveSpeedY <= 30)
                    moveSpeedY++;

                NPC.velocity.Y = moveSpeedY * 0.1f;
            }

            if (NPC.ai[0] == 2)
            {
                NPC.velocity.X *= 0f;
                NPC.velocity.Y = -13f;
            }

            if (NPC.ai[0] == 3)
            {
                NPC.velocity *= 0f;
            }

            timer++;

            if (timer == 100 || timer == 200 || timer == 300 || timer == 400 || timer == 500)
            {
                SoundEngine.PlaySound(SoundID.Item112, NPC.position);
                Vector2 direction = player.Center - NPC.Center;
                direction.Normalize();
                direction *= 5f;

                int numBubbles = Main.rand.Next(2, 4);
                for (int i = 0; i < numBubbles; i++)
                {
                    float spread = Main.rand.Next(-100, 100) * 0.01f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction + new Vector2(spread, spread),
                        ModContent.ProjectileType<Projectiles.Bubble>(), damage, 1f, Main.myPlayer);
                }
            }

            if (timer == 600)
                NPC.ai[0] = 1f;

            if (timer == 630 || timer == 660 || timer == 690 || timer == 720 || timer == 750 || timer == 780 || timer == 810 || timer == 840)
            {
                SoundEngine.PlaySound(SoundID.Item111, NPC.position);
                float speed = 6f;
                float angle = (float)Math.Atan2(NPC.Center.Y - player.Center.Y, NPC.Center.X - player.Center.X);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                    new Vector2((float)(Math.Cos(angle) * speed * -1), (float)(Math.Sin(angle) * speed * -1)),
                    ModContent.ProjectileType<Projectiles.InkGlob>(), damage, 0f, Main.myPlayer);
            }

            if (timer == 960)
            {
                for (int i = 0; i < 60; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, 0f, 0f, 0, default, 1f);
                    Main.dust[dust].scale = 1.5f;
                }

                Vector2 velocity1 = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 8f;
                Vector2 velocity2 = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 3f;

                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                        velocity1.RotatedBy(MathHelper.Pi * (i + Main.rand.NextDouble() - 0.5)),
                        ModContent.ProjectileType<Projectiles.InkCloud>(), damage, 0f, Main.myPlayer);
                }

                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                        velocity2.RotatedBy(MathHelper.Pi * (i + Main.rand.NextDouble() - 0.5)),
                        ModContent.ProjectileType<Projectiles.InkBubble>(), damage, 0f, Main.myPlayer);
                }

                NPC.position.X = player.position.X - 100f;
                NPC.position.Y = player.position.Y + 300f;
            }

            if (timer > 960 && timer < 1000)
            {
                NPC.velocity *= 0f;
            }

            if (timer == 980)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<Projectiles.SquidChargeTelegraph>(), 0, 0f, Main.myPlayer);

            if (timer == 1000)
                NPC.ai[0] = 2f;

            if (timer == 1060)
                NPC.ai[0] = 3f;

            if (timer == 1100)
            {
                Vector2 spawnPos = new Vector2(NPC.position.X + 55f, NPC.position.Y);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(-4f, -6f), ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(-2f, -6f), ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(0f, -6f), ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(2f, -6f), ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(4f, -6f), ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
            }

            if (timer >= 1150)
            {
                timer = 0;
                NPC.ai[0] = 0f;
            }

            if (NPC.life < NPC.lifeMax / 2 && NPC.ai[0] < 2)
            {
                BoltTimer++;
                if (BoltTimer == 300)
                {
                    float speed = 11f;
                    float angle = (float)Math.Atan2(NPC.Center.Y - player.Center.Y, NPC.Center.X - player.Center.X);
                    Vector2 velocity = new Vector2((float)(Math.Cos(angle) * speed * -1), (float)(Math.Sin(angle) * speed * -1));

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<Projectiles.WaterHelix1>(), damage, 0f, 0);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<Projectiles.WaterHelix2>(), damage, 0f, 0);
                    BoltTimer = 0;
                }
            }

            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter < 8)
                return;

            NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[NPC.type] * frameHeight);
            NPC.frameCounter = 1;
        }

        public override bool CheckDead()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore1").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore2").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore3").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore4").Type, 1f);
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.SquidBag>()));

            //var normalModeDrops = npcLoot.DefineNormalOnlyDrops();
            //normalModeDrops.Add(ItemDropRule.OneFromOptions(1,
            //ModContent.ItemType<Items.ZephyrBlade>(),
            //ModContent.ItemType<Items.ZephyrKnife>(),
            //ModContent.ItemType<Items.ZephyrTrident>()));

            //normalModeDrops.Add(ItemDropRule.Common(ModContent.ItemType<Items.OceanScale>(), 1, 8, 12));
            //normalModeDrops.Add(ItemDropRule.Common(ModContent.ItemType<Items.WormPet>(), 20));

            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.SquidMask>(), 7));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.SquidTrophyItem>(), 10));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedZephyrSquid, -1);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
    }
}
