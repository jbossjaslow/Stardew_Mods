using GenericModConfigMenu;
using StardewModdingAPI;

namespace Chatter {
	public class ModConfig {
		public bool enableIndicators = false;
		public SButton enableIndicatorsButton = SButton.O;
		public float indicatorScale = 2f;

		// Debug
		public bool enableDebugOutput = false;
		public bool useDebugOffsetsForAllNPCs = false;
		public float debugIndicatorXOffset = 16f;
		public float debugIndicatorYOffset = -100f;
		public bool useArrowKeysToAdjustDebugOffsets = false;

		public void SetupGenericConfigMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable chat indicators",
				tooltip: () => "Show an indicator above NPCs you haven't yet talked to today",
				getValue: () => enableIndicators,
				setValue: value => enableIndicators = value,
				fieldId: ModConfigField.enableIndicators
			);

			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => "Enable chat indicators keybind",
				tooltip: () => "The keybind to enable or disable chat indicators",
				getValue: () => enableIndicatorsButton,
				setValue: value => enableIndicatorsButton = value,
				fieldId: ModConfigField.enableIndicatorsKeybind
			);

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "Indicator scale",
				tooltip: () => "The size of the indicators",
				getValue: () => indicatorScale,
				setValue: value => indicatorScale = value,
				min: 0.5f,
				max: 6f,
				interval: 0.5f,
				fieldId: ModConfigField.indicatorScale
			);

			configMenu.AddPageLink(
				mod: ModManifest,
				pageId: ModConfigPageID.debug,
				text: () => "Show Debug Options",
				tooltip: () => "Configs used for debugging, if you want more control over the mod"
			);

			SetupDebugMenu(ModManifest, configMenu);
		}
		private void SetupDebugMenu(IManifest ModManifest, IGenericModConfigMenuApi configMenu) {
			configMenu.AddPage(
				mod: ModManifest,
				pageId: ModConfigPageID.debug,
				pageTitle: () => "Debug Options"
			);

			//configMenu.AddSectionTitle(
			//	mod: ModManifest,
			//	text: () => "Debug Options"
			//);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Enable debug output",
				tooltip: () => "Show output in the debug console",
				getValue: () => enableDebugOutput,
				setValue: value => enableDebugOutput = value,
				fieldId: ModConfigField.enableDebugOutput
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Use debug offsets for all NPCs",
				tooltip: () => "Overrides the indicator offets for all NPCs",
				getValue: () => useDebugOffsetsForAllNPCs,
				setValue: value => useDebugOffsetsForAllNPCs = value,
				fieldId: ModConfigField.useDebugOffsetsForAllNPCs
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Adjust Y offset with arrow keys",
				tooltip: () => "Allows more fine adjustment of the debug y offset using the up and down arrow keys",
				getValue: () => useArrowKeysToAdjustDebugOffsets,
				setValue: value => useArrowKeysToAdjustDebugOffsets = value,
				fieldId: ModConfigField.useArrowKeysToAdjustDebugOffsets
			);

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "Indicator X offset",
				tooltip: () => "The X offset from the NPC's origin to draw the indicator, if debug offsets is enabled",
				getValue: () => debugIndicatorXOffset,
				setValue: value => debugIndicatorXOffset = value,
				min: 0f,
				max: 32f,
				interval: 1f,
				fieldId: ModConfigField.indicatorXOffset
			);

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => "Indicator Y offset",
				tooltip: () => "The Y offset from the NPC's origin to draw the indicator, if debug offsets is enabled",
				getValue: () => debugIndicatorYOffset,
				setValue: value => debugIndicatorYOffset = value,
				min: -150f,
				max: 0f,
				interval: 2f,
				fieldId: ModConfigField.indicatorYOffset
			);

			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => "Version " + ModManifest.Version
			);
		}
	}

	public class ModConfigField {
		public const string enableIndicators = "enableIndicators";
		public const string enableIndicatorsKeybind = "enableIndicatorsKeybind";
		public const string indicatorScale = "indicatorScale";

		// Debug
		public const string enableDebugOutput = "enableDebugOutput";
		public const string useDebugOffsetsForAllNPCs = "useDebugOffsetsForAllNPCs";
		public const string indicatorXOffset = "debugIndicatorXOffset";
		public const string indicatorYOffset = "debugIndicatorYOffset";
		public const string useArrowKeysToAdjustDebugOffsets = "useArrowKeysToAdjustDebugOffsets";
	}

	public class ModConfigPageID {
		public const string debug = "debug";
	}
}
