using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace Chatter {
	internal class ShowWhenNPCNeedsChat : IDisposable {
		private readonly PerScreen<float> _yMovementPerDraw = new();
		private readonly PerScreen<float> _alpha = new();
		private readonly PerScreen<int> _pauseTicks = new(createNewState: () => 60);
		private readonly IModHelper _helper;
		private readonly IMonitor _monitor;
		private readonly ModConfig _config;
		private readonly Dictionary<string, int> _npcOffsets;

		// Indicator properties
		private readonly Texture2D defaultIndicatorTexture;
		private readonly Texture2D? customIndicatorTexture;
		private readonly Rectangle defaultIndicatorBounds;
		private readonly Rectangle? customIndicatorBounds;
		private readonly Color indicatorColor = Color.White * 0.9f;
		private readonly float indicatorRotation = 0.0f;
		private readonly Vector2 indicatorOrigin = Vector2.Zero;
		private readonly SpriteEffects indicatorSpriteEffects = SpriteEffects.None;
		private readonly float indicatorLayerDepth = 1f;

		public ShowWhenNPCNeedsChat(IModHelper helper, IMonitor monitor, ModConfig config, Dictionary<string, int> npcOffsets) {
			_helper = helper;
			_monitor = monitor;
			_config = config;
			_npcOffsets = npcOffsets;

			defaultIndicatorTexture = Game1.emoteSpriteSheet;
			int spriteSize = Game1.tileSize / 4;
			defaultIndicatorBounds = new(
				3 * spriteSize % Game1.emoteSpriteSheet.Width,
				3 * spriteSize / Game1.emoteSpriteSheet.Width * spriteSize,
				spriteSize,
				spriteSize
			);

			try {
				customIndicatorTexture = _helper.ModContent.Load<Texture2D>("Customization/indicator.png");
				customIndicatorBounds = customIndicatorTexture.Bounds;
				_monitor.Log($"Custom file exists", LogLevel.Debug);
			} catch {
				_monitor.Log($"Custom file does not exist", LogLevel.Debug);
			}
		}

		public void ToggleOption(bool showWhenNPCNeedsChat) {
			_helper.Events.Player.Warped -= OnWarped;
			_helper.Events.Display.RenderedWorld -= OnRenderedWorld_DrawNPCHasChat;
			_helper.Events.GameLoop.UpdateTicked -= UpdateTicked;

			if (showWhenNPCNeedsChat) {
				_helper.Events.Player.Warped += OnWarped;
				_helper.Events.Display.RenderedWorld += OnRenderedWorld_DrawNPCHasChat;
				_helper.Events.GameLoop.UpdateTicked += UpdateTicked;
			}
		}

		/// <summary>Raised before drawing the world</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		private void OnRenderedWorld_DrawNPCHasChat(object sender, RenderedWorldEventArgs e) {
			if (Game1.activeClickableMenu != null || Game1.currentLocation == null) return;

			// draws the static icon
			foreach (var npc in GetNPCsInCurrentLocation()) {
				if (_config.enableDebugOutput && _pauseTicks.Value % 60 == 0) {
					_monitor.Log($"Checking if {Game1.player.Name} can chat with {npc.Name}: {!ShouldShowIndicatorFor(npc)}", LogLevel.Debug);
				}

				if (npc.CanSocialize && ShouldShowIndicatorFor(npc)) {
					DrawNPC(npc);
				}
			}
		}

		private void DrawNPC(Character npc) {
			Texture2D texture = _config.useCustomIndicatorImage ? (customIndicatorTexture ?? defaultIndicatorTexture) : defaultIndicatorTexture;
			Rectangle bounds = _config.useCustomIndicatorImage ? (customIndicatorBounds ?? defaultIndicatorBounds) : defaultIndicatorBounds;
			float scale = _config.indicatorScale;

			var position = GetChatPositionAboveNPC(npc, bounds);
			if (!_config.disableIndicatorBob)
				position.Y += (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 300.0 + npc.Name.GetHashCode()) * 5.0);

			Game1.spriteBatch.Draw(
				texture,
				position,
				bounds,
				indicatorColor,
				indicatorRotation,
				indicatorOrigin,
				scale,
				indicatorSpriteEffects,
				indicatorLayerDepth);
		}

		private Vector2 GetChatPositionAboveNPC(Character npc, Rectangle textureBounds) {
			Vector2 position = npc.getLocalPosition(Game1.viewport);
			float scaleAdjustmentX = (_config.indicatorScale * -8) + 32;
			float scaleAdjustmentY = (_config.indicatorScale * -16) - 68;

			if (_config.useDebugOffsetsForAllNPCs) {
				position.X += scaleAdjustmentX;
				position.Y += _config.debugIndicatorYOffset;
			} else if (_npcOffsets.TryGetValue(npc.Name, out int offset)) {
				position.X += scaleAdjustmentX;
				position.Y += scaleAdjustmentY + offset;
			} else {
				// only looks good at scale == 2
				position.X += 16;
				position.Y += -100;
			}
			return position;
		}

		private NetCollection<NPC> GetNPCsInCurrentLocation() {
			NetCollection<NPC> npcs;

			bool eventIsOcurring = _config.showIndicatorsDuringCutscenes ? Game1.CurrentEvent != null : Game1.isFestival();
			if (eventIsOcurring) {
				npcs = new NetCollection<NPC>(Game1.CurrentEvent.actors);
			} else {
				npcs = Game1.currentLocation.characters;
			}
			npcs.Filter(c => c.GetType() != typeof(FarmAnimal));
			return npcs;
		}

		private bool ShouldShowIndicatorFor(NPC npc) {
			// if npc is sleeping, they can't chat with us
			if (npc.isSleeping.Value) {
				return false;
			}

			// check friendship values with npc
			if (Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendshipValues)) {
				int maxHeartPoints = Utility.GetMaximumHeartsForCharacter(npc) * 250;
				if (_config.disableIndicatorsForMaxHearts && friendshipValues.Points >= maxHeartPoints) {
					// if player has reached max hearts with npc, they don't need to talk to them
					return false;
				}
			}

			// At this point, player has not reached max hearts with npc
			// Return if player has talked to npc today
			return !Game1.player.hasPlayerTalkedToNPC(npc.Name);
		}

		/// <summary>Raised after a player warps to a new location.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		private void OnWarped(object sender, WarpedEventArgs e) {
			if (e.IsLocalPlayer) {
				_pauseTicks.Value = 60;
			}
		}

		/// <summary>Raised after the game state is updated (≈60 times per second).</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		private void UpdateTicked(object sender, UpdateTickedEventArgs e) {
			if (Game1.eventUp || Game1.activeClickableMenu != null)
				return;

			--_pauseTicks.Value;

			// update chat draw. If _pauseTicks is positive then don't render.
			if (e.IsMultipleOf(2) && _pauseTicks.Value <= 0) {
				_yMovementPerDraw.Value += 0.3f;
				_alpha.Value -= 0.014f;
				if (_alpha.Value < 0.1f) {
					_pauseTicks.Value = 60;
					_yMovementPerDraw.Value = -3f;
					_alpha.Value = 1f;
				}
			}
		}

		public void Dispose() {
			ToggleOption(false);
		}
	}
}
