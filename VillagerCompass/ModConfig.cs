using GenericModConfigMenu;
using StardewModdingAPI;

namespace VillagerCompass {
	public class ModConfig {
		public bool enableMod = false;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable the villager compass",
				tooltip: () => "Show an arrow pointing towards the desired villager",
				getValue: () => enableMod,
				setValue: value => enableMod = value,
				fieldId: ModConfigField.enableMod
			);
		}
	}

	public class ModConfigField {
		public const string enableMod = "enableMod";
	}
}
