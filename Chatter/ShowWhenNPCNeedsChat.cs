﻿using Microsoft.Xna.Framework;
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
		private readonly Texture2D? customTexture = null;

		public ShowWhenNPCNeedsChat(IModHelper helper, IMonitor monitor, ModConfig config, Dictionary<string, int> npcOffsets) {
			_helper = helper;
			_monitor = monitor;
			_config = config;
			_npcOffsets = npcOffsets;

			try {
				customTexture = _helper.ModContent.Load<Texture2D>("Customization/indicator.png");
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
			var positionAboveNPC = GetChatPositionAboveNPC(npc);
			positionAboveNPC.Y += (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 300.0 + npc.Name.GetHashCode()) * 5.0);

			Texture2D texture;
			Rectangle rectangle;
			if (_config.useCustomIndicatorImage && customTexture != null) {
				texture = customTexture;
				rectangle = texture.Bounds;
			} else {
				texture = Game1.emoteSpriteSheet;
				rectangle = new(
					3 * (Game1.tileSize / 4) % Game1.emoteSpriteSheet.Width,
					3 * (Game1.tileSize / 4) / Game1.emoteSpriteSheet.Width * (Game1.tileSize / 4),
					Game1.tileSize / 4,
					Game1.tileSize / 4
				);
			}

			//Vector2 position = Utility.ModifyCoordinatesForUIScale(positionAboveNPC);
			Color color = Color.White * 0.9f;
			float rotation = 0.0f;
			Vector2 origin = Vector2.Zero;
			float scale = _config.indicatorScale;
			SpriteEffects spriteEffects = SpriteEffects.None;
			float layerDepth = 1f;

			Game1.spriteBatch.Draw(
				texture,
				positionAboveNPC,
				rectangle,
				color,
				rotation,
				origin,
				scale,
				spriteEffects,
				layerDepth);
		}

		private Vector2 GetChatPositionAboveNPC(Character npc) {
			// if the map is larger than the screen, the player is likely outside
			// if the map is smaller than the screen, the player is usually inside any house but theirs
			// farmhouse is edge case, use outside coordinates
			//bool isOutsideX = Game1.viewport.Width <= Game1.currentLocation.map.DisplayWidth;
			//bool isOutsideY = Game1.viewport.Height <= Game1.currentLocation.map.DisplayHeight;
			//bool isFarmHouse = Game1.player.currentLocation is FarmHouse;

			//float outsideXPos = npc.position.X - Game1.viewport.X;
			//float insideXPos = npc.position.X + ((Game1.viewport.Width - Game1.currentLocation.map.DisplayWidth) / 2);
			//float outsideYPos = npc.position.Y - Game1.viewport.Y;
			//float insideYPos = npc.position.Y + ((Game1.viewport.Height - Game1.currentLocation.map.DisplayHeight) / 2);

			Vector2 position = npc.getLocalPosition(Game1.viewport);

			if (_config.useDebugOffsetsForAllNPCs) {
				position.X += _config.debugIndicatorXOffset; // default: 16f
				position.Y += _config.debugIndicatorYOffset; // default: -100f
			} else if (_npcOffsets.TryGetValue(npc.Name, out int offset)) {
				position.X += 16;
				position.Y += offset;
			} else {
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
