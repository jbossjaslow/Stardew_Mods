using System;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Chatter
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration from the player.</summary>
        internal ModConfig Config;
        private ShowWhenNPCNeedsChat _showWhenNPCNeedsChat;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _showWhenNPCNeedsChat = new ShowWhenNPCNeedsChat(Helper, this.Monitor, Config);

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable chat indicators",
                tooltip: () => "Show an indicator above NPCs you haven't yet talked to today",
                getValue: () => this.Config.enableIndicators,
                setValue: value => this.Config.enableIndicators = value,
                fieldId: ModConfigField.enableIndicators
            );

            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => "Enable chat indicators keybind",
                tooltip: () => "The keybind to enable or disable chat indicators",
                getValue: () => this.Config.enableIndicatorsButton,
                setValue: value => this.Config.enableIndicatorsButton = value,
                fieldId: ModConfigField.enableIndicatorsKeybind
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Debug Options"
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable debug output",
                tooltip: () => "Show output in the debug console",
                getValue: () => this.Config.enableDebugOutput,
                setValue: value => this.Config.enableDebugOutput = value,
                fieldId: ModConfigField.enableDebugOutput
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Indicator X offset",
                tooltip: () => "The X offset from the NPC's origin to draw the indicator",
                getValue: () => this.Config.indicatorXOffset,
                setValue: value => this.Config.indicatorXOffset = value,
                min: 0f,
                max: 32f,
                interval: 1f,
                fieldId: ModConfigField.indicatorXOffset
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Indicator Y offset",
                tooltip: () => "The Y offset from the NPC's origin to draw the indicator",
                getValue: () => this.Config.indicatorYOffset,
                setValue: value => this.Config.indicatorYOffset = value,
                min: -150f,
                max: 0f,
                interval: 2f,
                fieldId: ModConfigField.indicatorYOffset
            );

            // watch for changes
            configMenu.OnFieldChanged(
                mod: this.ModManifest,
                onChange: (name, value) =>
                {
                    if (value is null) return;

                    switch(name)
                    {
                        case ModConfigField.enableIndicators:
                            if (value is bool unwrappedValue)
                            {
                                _showWhenNPCNeedsChat.ToggleOption(unwrappedValue);
                            }
                            break;
                    }
                }
            );
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Config.enableIndicators)
                _showWhenNPCNeedsChat.ToggleOption(true);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;

            if (e.Button == Config.enableIndicatorsButton)
            {
                Config.enableIndicators = !Config.enableIndicators;
                _showWhenNPCNeedsChat.ToggleOption(Config.enableIndicators);
                this.Helper.WriteConfig(this.Config);
            }
        }
    }
}