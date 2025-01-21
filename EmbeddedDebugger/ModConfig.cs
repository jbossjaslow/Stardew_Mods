using GenericModConfigMenu;
using StardewModdingAPI;

namespace EmbeddedDebugger {
	public class ModConfig {
		public bool enableMod = false;
		public SButton bringUpMenuButton = SButton.OemBackslash;
		public bool requirePrependDebug = true;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable Embedded Debugger",
				tooltip: () => "Allows you to type debug commands in-game",
				getValue: () => enableMod,
				setValue: value => enableMod = value,
				fieldId: ModConfigField.enableMod
			);

			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => "Bring up the debugger menu",
				tooltip: () => "The keybind to show the embedded debugger",
				getValue: () => bringUpMenuButton,
				setValue: value => bringUpMenuButton = value,
				fieldId: ModConfigField.enableModButton
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Require debug prepend",
				tooltip: () => "If enabled, \"debug\" must be the first word in the command. If disabled, it is not needed",
				getValue: () => requirePrependDebug,
				setValue: value => requirePrependDebug = value,
				fieldId: ModConfigField.requirePrependDebug
			);
		}
	}

	public class ModConfigField {
		public const string enableMod = "enableMod";
		public const string enableModButton = "enableModButton";
		public const string requirePrependDebug = "requirePrependDebug";
	}
}
