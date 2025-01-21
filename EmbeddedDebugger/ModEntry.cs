using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EmbeddedDebugger {
	internal sealed class ModEntry : Mod {
		/*********
        ** Properties
        *********/
		/// <summary>The mod configuration from the player.</summary>
		private ModConfig Config;
		private IGenericModConfigMenuApi? _configMenu;

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper) {
			Config = this.Helper.ReadConfig<ModConfig>();

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.Input.ButtonPressed += OnButtonPressed;
		}

		/*********
        ** Private methods
        *********/
		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
			Monitor.Log($"Successfully launched the game", LogLevel.Debug);

			SetupConfigMenu();
		}

		/// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
			// ignore if player hasn't loaded a save yet
			if (!Context.IsWorldReady) return;

			if (e.Button == Config.bringUpMenuButton && Game1.activeClickableMenu is not EmbeddedDebuggerMenu)
				ShowEmbeddedDebuggerMenu();
		}

		private void ShowEmbeddedDebuggerMenu() {
			Monitor.Log($"Bringing up the debugger menu", LogLevel.Debug);
			Game1.activeClickableMenu = new EmbeddedDebuggerMenu(monitor: Monitor, helper: Helper, config: Config);
		}

		private void SetupConfigMenu() {
			// get Generic Mod Config Menu's API (if it's installed)
			_configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (_configMenu is null)
				return;

			// register mod
			_configMenu.Register(
				mod: this.ModManifest,
				reset: () => this.Config = new ModConfig(),
				save: () => {
					this.Helper.WriteConfig(this.Config);
				}
			);
			Config.SetupGenericConfigMenu(ModManifest, _configMenu);
		}
	}
}
