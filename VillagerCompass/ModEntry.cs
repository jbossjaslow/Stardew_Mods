using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

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

			if (Config.villagerList.Count == 0) {
				List<string> villagers = new();
				foreach (NPC npc in Utility.getAllCharacters()) villagers.Add(npc.Name);
				villagers.Sort();
				Config.villagerList = villagers;
				this.Helper.WriteConfig(this.Config);
			}

			_villagerCompass.npcToFind = Game1.getCharacterFromName(Config.villagerToFind);
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
					_villagerCompass.npcToFind = Game1.getCharacterFromName(Config.villagerToFind);
				}
			);
			Config.SetupGenericConfigMenu(ModManifest, configMenu);
		}

		/// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
			// ignore if player hasn't loaded a save yet
			if (!Context.IsWorldReady) return;

			if (e.Button == Config.enableModButton) {
				Config.enableMod = !Config.enableMod;
				_villagerCompass.ToggleMod(Config.enableMod);
				this.Helper.WriteConfig(this.Config);
			}
		}
	}
}
