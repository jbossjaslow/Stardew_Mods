using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace VillagerCompass {
	internal class VillagerCompass : IDisposable {
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		private readonly ModConfig _config;

		// Custom indicator properties
		private readonly Texture2D defaultIndicatorTexture;
		//private readonly Texture2D? customIndicatorTexture;
		private readonly Rectangle defaultIndicatorBounds;
		//private readonly Rectangle? customIndicatorBounds;

		public VillagerCompass(IModHelper helper, IMonitor monitor, ModConfig config) {
			_helper = helper;
			_monitor = monitor;
			_config = config;

			defaultIndicatorTexture = Game1.mouseCursors;
			defaultIndicatorBounds = new(x: 324, y: 477, width: 7, height: 19);

			//try {
			//	customIndicatorTexture = _helper.ModContent.Load<Texture2D>("Customization/compassPointer.png");
			//	customIndicatorBounds = customIndicatorTexture.Bounds;
			//	_monitor.Log($"Custom indicator exists", LogLevel.Debug);
			//} catch {
			//	_monitor.Log($"Custom indicator does not exist", LogLevel.Info);
			//}
		}
		public void ToggleMod(bool isOn) {
			//_helper.Events.Player.Warped -= OnWarped;
			//_helper.Events.Display.RenderedWorld -= OnRenderedWorld_DrawNPCHasChat;
			//_helper.Events.GameLoop.UpdateTicked -= UpdateTicked;

			if (isOn) {
				//_helper.Events.Player.Warped += OnWarped;
				//_helper.Events.Display.RenderedWorld += OnRenderedWorld_DrawNPCHasChat;
				//_helper.Events.GameLoop.UpdateTicked += UpdateTicked;
			}
		}

		public void Dispose() {
			ToggleMod(false);
		}
	}
}
