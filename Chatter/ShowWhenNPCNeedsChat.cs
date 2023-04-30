using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;

namespace Chatter
{
    internal class ShowWhenNPCNeedsChat : IDisposable
    {
        private readonly PerScreen<float> _yMovementPerDraw = new();
        private readonly PerScreen<float> _alpha = new();
        private readonly PerScreen<int> _pauseTicks = new(createNewState: () => 60);
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public ShowWhenNPCNeedsChat(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void ToggleOption(bool showWhenNPCNeedsChat)
        {
            _helper.Events.Player.Warped -= OnWarped;
            _helper.Events.Display.RenderingHud -= OnRenderingHud_DrawNPCHasChat;
            //_helper.Events.Display.RenderingHud -= OnRenderingHud_DrawNeedsChatTooltip;
            _helper.Events.GameLoop.UpdateTicked -= UpdateTicked;

            if (showWhenNPCNeedsChat)
            {
                _helper.Events.Player.Warped += OnWarped;
                _helper.Events.Display.RenderingHud += OnRenderingHud_DrawNPCHasChat;
                //_helper.Events.Display.RenderingHud += OnRenderingHud_DrawNeedsChatTooltip;
                _helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            }
        }

        // Moves with player if in house, shouldn't do this

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud_DrawNPCHasChat(object sender, RenderingHudEventArgs e)
        {
            // draws the static icon
            if (!Game1.eventUp &&
                Game1.activeClickableMenu == null &&
                Game1.currentLocation != null)
            {
                var NPCsInCurrentLocation = GetNPCsInCurrentLocation();
                if (NPCsInCurrentLocation != null)
                {
                    foreach (var npc in NPCsInCurrentLocation)
                    {
                        if (_pauseTicks.Value % 60 == 0)
                        {
                            _monitor.Log($"Checking if {Game1.player.Name} can chat with {npc.Name}: {!HasChattedWithNPC(npc)}", LogLevel.Debug);
                        }

                        if (npc.CanSocialize && !HasChattedWithNPC(npc))
                        {
                            var positionAboveNPC = GetChatPositionAboveNPC(npc);
                            positionAboveNPC.Y += (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 300.0 + npc.Name.GetHashCode()) * 5.0);
                            positionAboveNPC.Y -= 100f;
                            positionAboveNPC.X += 16f;
                            Game1.spriteBatch.Draw(
                                Game1.emoteSpriteSheet,
                                Utility.ModifyCoordinatesForUIScale(new Vector2(positionAboveNPC.X, positionAboveNPC.Y)),
                                new Rectangle(3 * (Game1.tileSize / 4) % Game1.emoteSpriteSheet.Width, 3 * (Game1.tileSize / 4) / Game1.emoteSpriteSheet.Width * (Game1.tileSize / 4), Game1.tileSize / 4, Game1.tileSize / 4),
                                Color.White * 0.9f,
                                0.0f,
                                Vector2.Zero,
                                2f,
                                SpriteEffects.None,
                                1f);
                        }
                    }
                }
            }
        }

        private Vector2 GetChatPositionAboveNPC(Character npc)
        {
            //if (Game1.player.currentLocation is FarmHouse)
            //{
            //    float xPosFarmhouse = npc.position.X - Game1.viewport.X;
            //    float yPosFarmhouse = npc.position.Y - Game1.viewport.Y;
            //    return new Vector2(xPosFarmhouse, yPosFarmhouse);
            //}

            // if the map is larger than the screen, the player is likely outside
            // if the map is smaller than the screen, the player is usually inside any house but theirs
            // farmhouse is edge case, use outside coordinates
            bool isOutsideX = Game1.viewport.Width <= Game1.currentLocation.map.DisplayWidth;
            bool isOutsideY = Game1.viewport.Height <= Game1.currentLocation.map.DisplayHeight;
            bool isFarmHouse = Game1.player.currentLocation is FarmHouse;

            float outsideXPos = npc.position.X - Game1.viewport.X;
            float insideXPos = npc.position.X + ((Game1.viewport.Width - Game1.currentLocation.map.DisplayWidth) / 2);
            float outsideYPos = npc.position.Y - Game1.viewport.Y;
            float insideYPos = npc.position.Y + ((Game1.viewport.Height - Game1.currentLocation.map.DisplayHeight) / 2);

            float xPos = (isOutsideX || isFarmHouse) ? outsideXPos : insideXPos;
            float yPos = (isOutsideY || isFarmHouse) ? outsideYPos : insideYPos;
            return new Vector2(xPos, yPos);
        }

        private NetCollection<NPC> GetNPCsInCurrentLocation()
        {
            NetCollection<NPC> npcs = Game1.currentLocation.characters;
            //npcs.Filter(c => c is NPC);
            return npcs;
        }

        private bool HasChattedWithNPC(NPC npc)
        {
            // if npc is sleeping, they can't chat with us
            if (npc.isSleeping.Value)
            {
                return true;
            }

            // check friendship values with npc
            //if (Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendshipValues))
            //{
            //    int maxHeartPoints = Utility.GetMaximumHeartsForCharacter(npc) * 250;
            //    if (friendshipValues.Points >= maxHeartPoints)
            //    {
            //        // if player has reached max hearts with npc, they don't need to talk to them
            //        return true;
            //    }
            //}

            // At this point, player has not reached max hearts with npc
            // Return if player has talked to npc today
            return Game1.player.hasPlayerTalkedToNPC(npc.Name);
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                _pauseTicks.Value = 60;
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.eventUp || Game1.activeClickableMenu != null)
                return;

            --_pauseTicks.Value;

            // update chat draw. If _pauseTicks is positive then don't render.
            if (e.IsMultipleOf(2) && _pauseTicks.Value <= 0)
            {
                _yMovementPerDraw.Value += 0.3f;
                _alpha.Value -= 0.014f;
                if (_alpha.Value < 0.1f)
                {
                    _pauseTicks.Value = 60;
                    _yMovementPerDraw.Value = -3f;
                    _alpha.Value = 1f;
                }
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        //private void OnRenderingHud_DrawNeedsChatTooltip(object sender, RenderingHudEventArgs e)
        //{
        //    // This draws the "pets" icon
        //    // If _pauseTicks is positive then don't render.
        //    if (!Game1.eventUp && Game1.activeClickableMenu == null && _pauseTicks.Value <= 0)
        //    {
        //        DrawIconForNPCs();
        //    }
        //}

        //private void DrawIconForNPCs()
        //{
        //    var NPCsInCurrentLocation = GetNPCsInCurrentLocation();

        //    if (NPCsInCurrentLocation != null)
        //    {
        //        foreach (var npc in NPCsInCurrentLocation)
        //        {
        //            if (CanChatWithNPC(npc))
        //            {
        //                var positionAboveNPC = GetChatPositionAboveNPC(npc);
        //                positionAboveNPC.X += 50f;
        //                positionAboveNPC.Y += 50f;
        //                Game1.spriteBatch.Draw(
        //                    Game1.mouseCursors,
        //                    Utility.ModifyCoordinatesForUIScale(new Vector2(positionAboveNPC.X, positionAboveNPC.Y + _yMovementPerDraw.Value)),
        //                    new Rectangle(32, 0, 16, 16),
        //                    Color.White * _alpha.Value,
        //                    0.0f,
        //                    Vector2.Zero,
        //                    4f,
        //                    SpriteEffects.None,
        //                    1f);
        //            }
        //        }
        //    }
        //}
    }
}
