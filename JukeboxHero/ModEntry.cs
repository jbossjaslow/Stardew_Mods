using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace JukeboxHero {
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
			helper.Events.Display.MenuChanged += OnMenuChanged;
		}

		/*********
        ** Private methods
        *********/
		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
			Monitor.Log($"Successfully launched the game", LogLevel.Debug);
			//_villagerCompass = new(Helper, Monitor, Config);

			SetupConfigMenu();
		}

		private void OnMenuChanged(object? sender, MenuChangedEventArgs e) {
			if (Config.enableMod && e.NewMenu is ChooseFromListMenu && Helper.Reflection.GetField<bool>(e.NewMenu, "isJukebox").GetValue() == true)
				ShowCustomJukeboxMenu();

			// Old jukebox menuL: StardewValley.Menus.ChooseFromListMenu
		}

		private void ShowCustomJukeboxMenu() {

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
