using GenericModConfigMenu;
using StardewModdingAPI;
using System.Collections.Generic;

namespace VillagerCompass {
	public class ModConfig {
		public bool enableMod = false;
		public SButton enableModButton = SButton.P;
		public string villagerToFind = "Emily";
		public List<string> villagerList = new();

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

			configMenu.AddTextOption(
				mod: ModManifest,
				getValue: () => villagerToFind,
				setValue: value => villagerToFind = value,
				name: () => "Villager",
				tooltip: () => "The villager to search for",
				allowedValues: villagerList.ToArray(),
				formatAllowedValue: null,
				fieldId: villagerToFind
			);

			if (villagerList.Count == 0) {
				configMenu.AddParagraph(
					mod: ModManifest,
					text: () => "To populate the villager list, load the save, then restart the game."
				);
			}
		}
	}

	public class ModConfigField {
		public const string enableMod = "enableMod";
		public const string enableModButton = "enableModButton";
		public const string villagerToFind = "villagerToFind";
	}
}
