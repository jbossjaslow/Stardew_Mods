using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace VillagerCompass {
	internal sealed class ModEntry : Mod {
		/*********
        ** Properties
        *********/
		/// <summary>The mod configuration from the player.</summary>
		private ModConfig Config;
		private VillagerCompass _villagerCompass;

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper) {
			Config = this.Helper.ReadConfig<ModConfig>();

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
		}

		/*********
        ** Private methods
        *********/
		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
			Monitor.Log($"Successfully launched the game", LogLevel.Debug);
			_villagerCompass = new(Helper, Monitor, Config);

			SetupConfigMenu();
		}

		private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) {
			Monitor.Log($"Successfully loaded the save", LogLevel.Debug);

			if (Config.enableMod) _villagerCompass.ToggleMod(isOn: true);
		}

		private void SetupConfigMenu() {
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: this.ModManifest,
				reset: () => this.Config = new ModConfig(),
				save: () => {
					this.Helper.WriteConfig(this.Config);
					_villagerCompass.ToggleMod(isOn: Config.enableMod);
				}
			);
			Config.SetupGenericConfigMenu(ModManifest, configMenu);
		}
	}
}
