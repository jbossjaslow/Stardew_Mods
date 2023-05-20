using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace VillagerCompass {
	internal class VillagerCompass : IDisposable {
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		private readonly ModConfig _config;

		private readonly PerScreen<int> _pauseTicks = new(createNewState: () => 60);
		private float _rotation = 0;
		private readonly int scale = 3;

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
			//_helper.Events.GameLoop.UpdateTicked -= UpdateTicked;
			_helper.Events.Display.RenderingHud -= OnRenderingHud;

			if (isOn) {
				//_helper.Events.Player.Warped += OnWarped;
				//_helper.Events.GameLoop.UpdateTicked += UpdateTicked;
				_helper.Events.Display.RenderingHud += OnRenderingHud;
			}
		}

		private void OnRenderingHud(object? sender, RenderingHudEventArgs e) {
			//_monitor.Log($"{Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea}", LogLevel.Debug);
			RotateCompassToFind(Game1.getCharacterFromName("Emily"));
			Rectangle destRect = new(
				x: Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 250,
				y: Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - 50,
				width: 7 * scale,
				height: 19 * scale
			);

			Game1.spriteBatch.Draw(
				texture: defaultIndicatorTexture,
				destinationRectangle: destRect,
				sourceRectangle: defaultIndicatorBounds,
				color: Color.White,
				rotation: _rotation,
				origin: new(4, 10),
				effects: SpriteEffects.None,
				layerDepth: 1f
			);
		}

		private void RotateCompassToFind(NPC npc) {
			//Utility.ForAllLocations
			// Utility.drawLineWithScreenCoordinates
			if (PlayerLocation() == FindLocationOf(npc)) {
				Vector2 vector = Game1.player.position;
				Vector2 vector2 = npc.position;
				Vector2 vector3 = vector - vector2;
				_rotation = (float)Math.Atan2(vector3.Y, vector3.X) - MathF.PI / 2;
			}
		}

		private static GameLocation PlayerLocation() {
			return Game1.player.currentLocation;
		}

		private static GameLocation FindLocationOf(NPC npc) {
			return npc.currentLocation;
		}

		private void OnWarped(object? sender, WarpedEventArgs e) {
			if (e.IsLocalPlayer) {
				_pauseTicks.Value = 60;
			}
		}

		private void UpdateTicked(object? sender, UpdateTickedEventArgs e) {
			if (Game1.eventUp || Game1.activeClickableMenu != null)
				return;

			--_pauseTicks.Value;

			// update chat draw. If _pauseTicks is positive then don't render.
			if (e.IsMultipleOf(2) && _pauseTicks.Value <= 0) {
				//_yMovementPerDraw.Value += 0.3f;
				_rotation -= MathF.PI / 30;
				if (_rotation <= -MathF.PI * 2) {
					// reset point
					//_pauseTicks.Value = 60;
					//_yMovementPerDraw.Value = -3f;
					_rotation = 0f;
				}
			}
		}

		public void Dispose() {
			ToggleMod(false);
		}
	}
}
