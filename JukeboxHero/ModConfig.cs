using GenericModConfigMenu;
using StardewModdingAPI;

namespace JukeboxHero {
	public class ModConfig {
		public bool enableMod = false;
		public SButton enableModButton = SButton.P;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable Jukebox Hero",
				tooltip: () => "Improves the usability of the jukebox",
				getValue: () => enableMod,
				setValue: value => enableMod = value,
				fieldId: ModConfigField.enableMod
			);

			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => "Enable Jukebox Hero keybind",
				tooltip: () => "The keybind to enable or disable Jukebox Hero",
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
