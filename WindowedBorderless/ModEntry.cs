using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace WindowedBorderless {
	/// <summary>The mod entry point.</summary>
	internal sealed class ModEntry : Mod {
		/*********
        ** Properties
        *********/
		/// <summary>The mod configuration from the player.</summary>
		private ModConfig Config = new(); // empty constructor here so I don't need to make this nullable

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper) {
			Config = this.Helper.ReadConfig<ModConfig>();

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.Input.ButtonPressed += OnButtonPressed;

			ChangeWindowToWindowedBorderless();
		}

		/*********
        ** Private methods
        *********/
		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: this.ModManifest,
				reset: () => this.Config = new ModConfig(),
				save: () => this.Helper.WriteConfig(this.Config)
			);
			Config.SetupGenericConfigMenu(ModManifest, configMenu);
		}

		/// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
			if (e.Button == Config.forceWindowedBorderlessKeybind && Config.enableForceWindowChangeWithKeybind) {
				ChangeWindowToWindowedBorderless();
			}
		}

		private void ChangeWindowToWindowedBorderless() {
			if (!Game1.options.isCurrentlyWindowedBorderless()) {
				Monitor.Log($"Display is now windowed borderless", LogLevel.Debug);
				Game1.options.setWindowedOption("Windowed Borderless");
			} else {
				Monitor.Log($"Display is already windowed borderless, no need to set it again", LogLevel.Debug);
			}
		}
	}
}