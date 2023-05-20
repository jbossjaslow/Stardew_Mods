using GenericModConfigMenu;
using StardewModdingAPI;

namespace VillagerCompass {
	public class ModConfig {
		public bool enableMod = false;
		public SButton enableModButton = SButton.P;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable the villager compass",
				tooltip: () => "Show an arrow pointing towards the desired villager",
				getValue: () => enableMod,
				setValue: value => enableMod = value,
				fieldId: ModConfigField.enableMod
			);

			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => "Enable villager compass keybind",
				tooltip: () => "The keybind to enable or disable the villager compass",
				getValue: () => enableModButton,
				setValue: value => enableModButton = value,
				fieldId: ModConfigField.enableModButton
			);
		}
	}

	public class ModConfigField {
		public const string enableMod = "enableMod";
		public const string enableModButton = "enableModButton";
	}
}
