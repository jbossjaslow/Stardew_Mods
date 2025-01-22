using GenericModConfigMenu;
using StardewModdingAPI;

namespace WindowedBorderless {
	public class ModConfig {
		public bool enableForceWindowChangeWithKeybind = false;
		public SButton forceWindowedBorderlessKeybind = SButton.K;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable keybind force",
				tooltip: () => "Allows forcing the game into windowed borderless mode with a button press",
				getValue: () => enableForceWindowChangeWithKeybind,
				setValue: value => enableForceWindowChangeWithKeybind = value,
				fieldId: ModConfigField.enableForceWindowChangeWithKeybind
			);

			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => "Windowed borderless keybind",
				tooltip: () => "Forces the game into windowed borderless mode",
				getValue: () => forceWindowedBorderlessKeybind,
				setValue: value => forceWindowedBorderlessKeybind = value,
				fieldId: ModConfigField.forceWindowedBorderlessKeybind
			);

			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => "To prevent unneeded behavior, the keybind for forcing the game into windowed borderless mode is locked behind the above flag. To use the keybind, set the flag to True, use the keybind, and then set it to False again"
			);
		}
	}

	public class ModConfigField {
		public const string enableForceWindowChangeWithKeybind = "enableForceWindowChangeWithKeybind";
		public const string forceWindowedBorderlessKeybind = "forceWindowedBorderlessKeybind";
	}
}
