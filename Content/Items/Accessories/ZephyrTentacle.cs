

using Terraria;
using Terraria.ModLoader;

namespace UltraniumUnearthed.Content.Items.Accessories;

public class ZephyrTentacle : ModItem
{
  

  public override void SetDefaults()
  {
    Item.width = 32;
    Item.height = 32;
    Item.rare = 4;
    Item.value = 200000;
    Item.accessory = true;
    Item.expert = true;
  }

  public override void UpdateAccessory(Player player, bool hideVisual)
  {
        player.moveSpeed += 0.10f;
        player.jumpSpeedBoost += 4f;
    }
}
