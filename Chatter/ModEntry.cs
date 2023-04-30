using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Chatter
{
    /// <summary>The configuration data model.</summary>
    public class ModConfig
    {
        public bool indicatorsEnabled = false;
    }

    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        internal ModConfig config = new(); // create (and throw away) a default value to keep nullability check happy
        private ShowWhenNPCNeedsChat _showWhenNPCNeedsChat;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<ModConfig>();
            //helper.Events.GameLoop.GameLaunched += onLaunched;

            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }


        /*********
        ** Private methods
        *********/
        private void onLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // integrate with Generic Mod Config Menu, if installed
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api != null)
            {
                var apiExt = Helper.ModRegistry.GetApi<GMCMOptionsAPI>("jltaylor-us.GMCMOptions");
                if (apiExt is null)
                {
                    Monitor.Log("Generic Mod Config Menu not installed", LogLevel.Info);
                }
                api.Register(
                    mod: ModManifest,
                    reset: () => config = new ModConfig(),
                    save: () => { Helper.WriteConfig(config); /*overlay.Value?.ConfigSaved();*/ });
                //api.AddBoolOption(
                //    mod: ModManifest,
                //    name: "Enable Indicators",
                //    tooltip: "Show the indicators above the villagers if they have not yet been interacted with",
                //    getValue: () => config.indicatorsEnabled,
                //    setValue: (bool val) => config.indicatorsEnabled = val);

                //api.AddKeybind(
                //    mod: ModManifest,
                //    name: I18n.Config_Hotkey,
                //    tooltip: I18n.Config_Hotkey_Desc,
                //    getValue: () => config.hotkey,
                //    setValue: (SButton val) => config.hotkey = val);
                //api.AddKeybind(
                //    mod: ModManifest,
                //    name: I18n.Config_SecondaryCloseButton,
                //    tooltip: I18n.Config_SecondaryCloseButton_Desc,
                //    getValue: () => config.secondaryCloseButton,
                //    setValue: (SButton val) => config.secondaryCloseButton = val);
                //api.AddBoolOption(
                //    mod: ModManifest,
                //    name: I18n.Config_Debug,
                //    tooltip: I18n.Config_Debug_Desc,
                //    getValue: () => config.debug,
                //    setValue: (bool val) => config.debug = val);
                //OverlayConfig.RegisterConfigMenuOptions(() => config.overlay, api, apiExt, ModManifest);
            }
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _showWhenNPCNeedsChat = new ShowWhenNPCNeedsChat(Helper, this.Monitor);
            _showWhenNPCNeedsChat.ToggleOption(true);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }
    }


    // See https://github.com/spacechase0/StardewValleyMods/blob/develop/GenericModConfigMenu/IGenericModConfigMenuApi.cs for full API
    public interface GenericModConfigMenuAPI
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
        void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null, string? fieldId = null);
        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string>? tooltip = null, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null, string? fieldId = null);
    }
    // See https://github.com/jltaylor-us/StardewGMCMOptions/blob/default/StardewGMCMOptions/IGMCMOptionsAPI.cs
    public interface GMCMOptionsAPI
    {
        void AddColorOption(IManifest mod, Func<Color> getValue, Action<Color> setValue, Func<string> name,
            Func<string>? tooltip = null, bool showAlpha = true, uint colorPickerStyle = 0, string? fieldId = null);
        #pragma warning disable format
        [Flags]
        public enum ColorPickerStyle : uint {
            Default = 0,
            RGBSliders    = 0b00000001,
            HSVColorWheel = 0b00000010,
            HSLColorWheel = 0b00000100,
            AllStyles     = 0b11111111,
            NoChooser     = 0,
            RadioChooser  = 0b01 << 8,
            ToggleChooser = 0b10 << 8
        }
        #pragma warning restore format
    }
}