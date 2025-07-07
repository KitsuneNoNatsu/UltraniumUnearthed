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
        private ref float BossPhase => ref NPC.ai[0];
        private ref float AttackTimer => ref NPC.ai[1];
        private ref float MovementState => ref NPC.ai[2];
        private ref float SecondaryTimer => ref NPC.ai[3];

        private int horizontalMoveSpeed;
        private int verticalMoveSpeed;
        private float targetHoverHeight = 100f;
        private int playerCount;

        private const int BUBBLE_ATTACK_INTERVAL = 100;
        private const int INK_ATTACK_START = 630;
        private const int INK_ATTACK_INTERVAL = 30;
        private const int PHASE_TRANSITION_TIME = 600;
        private const int TELEPORT_TIME = 960;
        private const int CHARGE_PREPARATION_TIME = 980;
        private const int CHARGE_START_TIME = 1000;
        private const int CHARGE_END_TIME = 1060;
        private const int AQUABALL_BARRAGE_TIME = 1100;
        private const int CYCLE_RESET_TIME = 1150;
        private const int HELIX_ATTACK_INTERVAL = 300;

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
            playerCount = 1;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/ZephyrSquid");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
                new FlavorTextBestiaryInfoElement("A colossal squid from the ocean depths, commanding powerful aquatic magic.")
            });
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            playerCount = numPlayers;
            NPC.lifeMax = (int)(4300 + numPlayers * 430 * balance);
            NPC.damage = (int)(35 * bossAdjustment);
            NPC.defense = (int)(30 * bossAdjustment);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.velocity != Vector2.Zero)
            {
                DrawTrailEffect(spriteBatch, screenPos, drawColor);
            }
            return true;
        }

        private void DrawTrailEffect(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D trailTexture = ModContent.Request<Texture2D>("UltraniumUnearthed/Content/NPCs/Bosses/ZephyrSquid/ZephyrSquidTrail").Value;
            Vector2 drawOrigin = new Vector2(trailTexture.Width * 0.2f, NPC.height * 0.2f);

            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                Color trailColor = NPC.GetAlpha(drawColor) * ((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length / 2f);
                spriteBatch.Draw(trailTexture, drawPos, NPC.frame, trailColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
        }

        public override bool PreAI()
        {
            NPC.rotation = NPC.velocity.X * 0.02f;
            Player targetPlayer = Main.player[NPC.target];

            HandlePlayerTargeting(targetPlayer);

            if (IsPlayerDeadOrInactive(targetPlayer))
            {
                HandleBossDespawn();
                return true;
            }

            ExecuteBossAI(targetPlayer);
            return true;
        }

        private void HandlePlayerTargeting(Player player)
        {
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                NPC.velocity.Y = -100f;
            }
            NPC.netUpdate = true;
            NPC.TargetClosest(true);
        }

        private bool IsPlayerDeadOrInactive(Player player)
        {
            return player.dead;
        }

        private void HandleBossDespawn()
        {
            NPC.velocity.Y = 30f;
            AttackTimer++;
            if (AttackTimer >= 120)
                NPC.active = false;
        }

        private void ExecuteBossAI(Player targetPlayer)
        {
            switch (BossPhase)
            {
                case 0:
                    StandardMovementPhase(targetPlayer);
                    break;
                case 1:
                    AggressiveMovementPhase(targetPlayer);
                    break;
                case 2:
                    ChargePreparationPhase();
                    break;
                case 3:
                    ChargeExecutionPhase();
                    break;
            }

            AttackTimer++;
            ExecuteTimedAttacks(targetPlayer);
            ExecutePhaseTransitions();

            if (IsSecondPhase())
            {
                ExecuteHelixAttacks(targetPlayer);
            }
        }

        private void StandardMovementPhase(Player target)
        {
            ExecuteMovementTowardsPlayer(target, maxHorizontalSpeed: 53, hoverHeight: 150f);
        }

        private void AggressiveMovementPhase(Player target)
        {
            ExecuteMovementTowardsPlayer(target, maxHorizontalSpeed: 100, hoverHeight: 150f);
        }
        private void ChargePreparationPhase()
        {
            NPC.velocity.X *= 0f;
            NPC.velocity.Y = -13f;
        }

        private void ChargeExecutionPhase()
        {
            NPC.velocity *= 0f;
        }

        private void ExecuteMovementTowardsPlayer(Player target, int maxHorizontalSpeed, float hoverHeight)
        {
            if (AttackTimer >= CHARGE_PREPARATION_TIME && AttackTimer <= CYCLE_RESET_TIME)
                return;

            if (NPC.Center.X >= target.Center.X && horizontalMoveSpeed >= -maxHorizontalSpeed)
                horizontalMoveSpeed--;
            else if (NPC.Center.X <= target.Center.X && horizontalMoveSpeed <= maxHorizontalSpeed)
                horizontalMoveSpeed++;

            NPC.velocity.X = horizontalMoveSpeed * 0.09f;

            if (NPC.Center.Y >= target.Center.Y - targetHoverHeight && verticalMoveSpeed >= -30)
            {
                verticalMoveSpeed--;
                targetHoverHeight = hoverHeight;
            }
            else if (NPC.Center.Y <= target.Center.Y - targetHoverHeight && verticalMoveSpeed <= 30)
                verticalMoveSpeed++;

            NPC.velocity.Y = verticalMoveSpeed * 0.1f;
        }

        private void ExecuteTimedAttacks(Player target)
        {
            int damage = GetProjectileDamage();

            if (AttackTimer % BUBBLE_ATTACK_INTERVAL == 0 && AttackTimer <= 500)
            {
                ExecuteBubbleBarrage(target, damage);
            }

            if (AttackTimer >= INK_ATTACK_START && AttackTimer <= 840 && (AttackTimer - INK_ATTACK_START) % INK_ATTACK_INTERVAL == 0)
            {
                ExecuteInkGlobAttack(target, damage);
            }

            if (AttackTimer == TELEPORT_TIME)
            {
                ExecuteTeleportAndInkCloudAttack(target, damage);
            }

            if (AttackTimer > TELEPORT_TIME && AttackTimer < CHARGE_PREPARATION_TIME)
            {
                NPC.velocity *= 0f;
            }

            if (AttackTimer == CHARGE_PREPARATION_TIME)
            {
                CreateChargeTelegraph();
            }

            if (AttackTimer == AQUABALL_BARRAGE_TIME)
            {
                ExecuteAquaBallBarrage(damage);
            }
        }

        private void ExecuteBubbleBarrage(Player target, int damage)
        {
            SoundEngine.PlaySound(SoundID.Item112, NPC.position);
            Vector2 shootDirection = NPC.DirectionTo(target.Center) * 5f;

            int bubbleCount = Main.rand.Next(2, 4);
            for (int i = 0; i < bubbleCount; i++)
            {
                Vector2 velocity = shootDirection + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<Projectiles.Bubble>(), damage, 1f, Main.myPlayer);
            }
        }

        private void ExecuteInkGlobAttack(Player target, int damage)
        {
            SoundEngine.PlaySound(SoundID.Item111, NPC.position);
            Vector2 velocity = NPC.DirectionTo(target.Center) * 6f;

            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<Projectiles.InkGlob>(), damage, 0f, Main.myPlayer);
        }

        private void ExecuteTeleportAndInkCloudAttack(Player target, int damage)
        {
            CreateTeleportEffect();

            CreateInkCloudPattern(damage);
            CreateInkBubblePattern(damage);

            ExecuteTeleportToPlayer(target);
        }

        private void CreateTeleportEffect()
        {
            for (int i = 0; i < 60; i++)
            {
                int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, 0f, 0f, 0, default(Color), 1f);
                Main.dust[dustIndex].scale = 1.5f;
            }
        }

        private void CreateInkCloudPattern(int damage)
        {
            Vector2 baseVelocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 8f;

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = baseVelocity.RotatedBy(MathHelper.Pi * (i + Main.rand.NextDouble() - 0.5));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<Projectiles.InkCloud>(), damage, 0f, Main.myPlayer);
            }
        }

        private void CreateInkBubblePattern(int damage)
        {
            Vector2 baseVelocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 3f;

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = baseVelocity.RotatedBy(MathHelper.Pi * (i + Main.rand.NextDouble() - 0.5));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<Projectiles.InkBubble>(), damage, 0f, Main.myPlayer);
            }
        }

        private void ExecuteTeleportToPlayer(Player target)
        {
            NPC.position.X = target.position.X - 100f;
            NPC.position.Y = target.position.Y + 300f;
        }

        private void CreateChargeTelegraph()
        {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                ModContent.ProjectileType<Projectiles.SquidChargeTelegraph>(), 0, 0f, Main.myPlayer);
        }

        private void ExecuteAquaBallBarrage(int damage)
        {
            Vector2 spawnPosition = new Vector2(NPC.position.X + 55f, NPC.position.Y);
            Vector2[] velocities = {
                new Vector2(-4f, -6f),
                new Vector2(-2f, -6f),
                new Vector2(0f, -6f),
                new Vector2(2f, -6f),
                new Vector2(4f, -6f)
            };

            foreach (Vector2 velocity in velocities)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity,
                    ModContent.ProjectileType<Projectiles.AquaBall>(), damage, 0.4f, Main.myPlayer);
            }
        }

        private void ExecutePhaseTransitions()
        {
            if (AttackTimer == PHASE_TRANSITION_TIME)
                BossPhase = 1f;

            if (AttackTimer == CHARGE_START_TIME)
                BossPhase = 2f;

            if (AttackTimer == CHARGE_END_TIME)
                BossPhase = 3f;

            if (AttackTimer >= CYCLE_RESET_TIME)
            {
                AttackTimer = 0;
                BossPhase = 0f;
            }
        }

        private void ExecuteHelixAttacks(Player target)
        {
            SecondaryTimer++;
            if (SecondaryTimer >= HELIX_ATTACK_INTERVAL)
            {
                LaunchHelixProjectiles(target, GetProjectileDamage());
                SecondaryTimer = 0;
            }
        }

        private void LaunchHelixProjectiles(Player target, int damage)
        {
            Vector2 velocity = NPC.DirectionTo(target.Center) * 11f;

            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<Projectiles.WaterHelix1>(), damage, 0f, 0);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<Projectiles.WaterHelix2>(), damage, 0f, 0);
        }

        private bool IsSecondPhase()
        {
            return NPC.life < NPC.lifeMax / 2 && BossPhase < 2;
        }

        private int GetProjectileDamage()
        {
            return Main.expertMode ? 14 : 20;
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
            CreateDeathGore();
            return true;
        }

        private void CreateDeathGore()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore1").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore2").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore3").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SquidGore4").Type, 1f);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.ZephyrSquidBag>()));

            //if (!Main.expertMode)
            //{
            //    npcLoot.Add(ItemDropRule.OneFromOptions(1,
            //        ModContent.ItemType<Items.ZephyrBlade>(),
            //        ModContent.ItemType<Items.ZephyrKnife>(),
            //        ModContent.ItemType<Items.ZephyrTrident>()));

            //    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.OceanScale>(), 1, 8, 12));
            //    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.WormPet>(), 20));
            //}

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