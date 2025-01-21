using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace EmbeddedDebugger {
	internal class EmbeddedDebuggerMenu : IClickableMenu {
		/*********
        ** Properties
        *********/
		/// <summary>The number of pixels between visual elements.</summary>
		private const int _spacingPixels = 32;

		private const int _textureBoxBorderWidth = 4 * 4;

		/// <summary>The play button.</summary>
		public ClickableTextureComponent RunCommandButton;

		/// <summary>The SMAPI Monitor for logging messages.</summary>
		private readonly IMonitor Monitor;

		private readonly IModHelper Helper;

		/// <summary></summary>
		private readonly ModConfig Config;

		/// <summary>The text that displays when a tab is hovered over.</summary>
		//private string HoverText;

		private readonly SearchTextBox SearchTextBox;

		private string[] commandWords = Array.Empty<string>();
		private bool commandHasBeenRun = false;
		private bool commandSuccessful = false;
		private string commandSuccessfulMessage = "Command was successful";
		private string commandUnsuccessfulMessage = "Command was not successful";

		private float messageHeight => Game1.smallFont.MeasureString(commandSuccessfulMessage).Y;

		/*********
        ** Public methods
        *********/
		public EmbeddedDebuggerMenu(IMonitor monitor, IModHelper helper, ModConfig config) {
			Monitor = monitor;
			Helper = helper;
			Config = config;

			Game1.playSound("bigSelect");

			SearchTextBox = new SearchTextBox(Game1.smallFont, Color.Black);

			int w = 500;
			int h = SearchTextBox.Bounds.Height + (int)messageHeight;
			this.width = w + borderWidth * 2;
			this.height = h + borderWidth * 1;
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (w + borderWidth * 2) / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (h + borderWidth * 2) / 2;

			SetUpPositions();
		}

		/***************** 
         * Private methods
         *****************/
		private void SetUpPositions() {
			RunCommandButton = new ClickableTextureComponent(
				name: "enter",
				bounds: new Rectangle(
					xPositionOnScreen + width - borderWidth - spaceToClearSideBorder,// - _spacingPixels * 4,
					yPositionOnScreen + 3 * borderWidth / 4,// + _spacingPixels,
					16 * 4,
					15 * 4),
				label: "",
				hoverText: null,
				texture: Game1.mouseCursors,
				sourceRect: new Rectangle(175, 379, 16, 15),
				scale: 4f) { // ... and set ids for controller support
							 //myID = PlayID,
							 //leftNeighborID = hideRandom ? ReverseSortID : RandomID,
							 //rightNeighborID = StopID,
							 //downNeighborID = BaseID,
				fullyImmutable = true
			};

			// create components
			//SearchTextBox = new SearchTextBox(Game1.smallFont, Color.Black);
			SearchTextBox.Bounds = new Rectangle(
				x: xPositionOnScreen + borderWidth / 2,
				y: yPositionOnScreen + borderWidth,
				width: width - borderWidth / 2 - RunCommandButton.bounds.Width,
				height: SearchTextBox.Bounds.Height
			);

			SearchTextBox.Select();
			SearchTextBox.OnChanged += (_, text) => this.ReceiveSearchTextboxChanged(text);
		}

		private void RunCommand() {
			if (commandWords.Length == 0) {
				Monitor.Log($"Empty search text", LogLevel.Debug);
				return;
			}

			string mainCommand = Config.requirePrependDebug ? commandWords[0] : "debug";
			string[] parameters = Config.requirePrependDebug ? commandWords.Skip(1).ToArray() : commandWords;
			//Monitor.Log($"Main command: {mainCommand}, parameters: {parameters}", LogLevel.Debug);
			//commandSuccessful = Helper.ConsoleCommands.Trigger(mainCommand, parameters);
			if (commandSuccessful)
				Monitor.Log($"Success!", LogLevel.Debug);
			else
				Monitor.Log($"Failure!", LogLevel.Debug);

			commandHasBeenRun = true;
		}

		/// <summary>The method invoked when the player changes the search text.</summary>
		/// <param name="search">The new search text.</param>
		private void ReceiveSearchTextboxChanged(string? search) {
			commandWords = (search ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		/******************
         * Override methods
         ******************/

		/// <summary>The method invoked when the player left-clicks on the menu.</summary>
		/// <param name="x">The X-position of the cursor.</param>
		/// <param name="y">The Y-position of the cursor.</param>
		/// <param name="playSound">Whether to enable sound.</param>
		public override void receiveLeftClick(int x, int y, bool playSound = true) {
			base.receiveLeftClick(x, y, playSound);
			if (Game1.activeClickableMenu == null)
				return;

			if (RunCommandButton.containsPoint(x, y)) {
				RunCommand();
				Game1.playSound("select");
			}
		}

		/// <summary>The method invoked when the player presses an input button.</summary>
		/// <param name="key">The pressed input.</param>
		public override void receiveKeyPress(Keys key) {
			// deliberately avoid calling base, which may let another key close the menu
			if (key.Equals(Keys.Escape))
				this.exitThisMenu();
			else if (key.Equals(Keys.Enter)) {
				RunCommand();
				Game1.playSound("select");
			}
		}

		/// <summary>Draw the menu to the screen.</summary>
		/// <param name="b">The sprite batch.</param>
		public override void draw(SpriteBatch b) {
			// Implementation derived from ChooseFromListMenu

			// from ShopMenu: draw menuBackground if enabled
			if (Game1.options.showMenuBackground) {
				base.drawBackground(b);
			} else {
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f); // 0.4 is default(?); used in GameMenu
			}

			// draw menu box
			//drawTextureBox(
			//	b,
			//	xPositionOnScreen,
			//	yPositionOnScreen,
			//	width,
			//	height,
			//	Color.White);

			// draw options' submenu box
			drawTextureBox(
				b: b,
				texture: Game1.mouseCursors,
				sourceRect: new Rectangle(384, 373, 18, 18),    // shopMenu's nail corners
				x: xPositionOnScreen,
				y: yPositionOnScreen,
				width: width + _textureBoxBorderWidth * 2,
				height: height + _textureBoxBorderWidth * 2,
				color: Color.White,
				scale: 4f,
				drawShadow: false);

			// draw menu title
			StardewValley.BellsAndWhistles.SpriteText.drawStringWithScrollCenteredAt(
				b,
				"Debug Terminal",
				xPositionOnScreen + width / 2 + borderWidth / 2,
				yPositionOnScreen - _spacingPixels);

			//Utility.drawTextWithShadow(
			//	b,
			//	"Goodnight Moon",
			//	Game1.dialogueFont,
			//	new Vector2(
			//		xPositionOnScreen + width / 2 - Game1.dialogueFont.MeasureString("Goodnight Moon").X / 2f + borderWidth / 2,
			//		yPositionOnScreen + _spacingPixels),
			//	Game1.textColor);

			//StardewValley.BellsAndWhistles.SpriteText.drawStringHorizontallyCenteredAt(
			//    b,
			//    song_name,
			//    xPositionOnScreen + width / 2,
			//    yPositionOnScreen + _spacingPixels * 3 / 2 + (int) Game1.dialogueFont.MeasureString(cur_play).Y);

			RunCommandButton.draw(b);

			SearchTextBox.Draw(b);

			if (commandHasBeenRun) {
				//StardewValley.BellsAndWhistles.SpriteText.drawString(
				//	b,
				//	s: commandSuccessful ? "Command was successful" : "Command was not successful",
				//	x: 0,
				//	y: 0,
				//	color: commandSuccessful ? StardewValley.BellsAndWhistles.SpriteText.color_Green : StardewValley.BellsAndWhistles.SpriteText.color_Red
				//);

				Utility.drawTextWithShadow(
					b,
					text: commandSuccessful ? commandSuccessfulMessage : commandUnsuccessfulMessage,
					font: Game1.smallFont,
					position: new Vector2(
						SearchTextBox.Bounds.X,
						SearchTextBox.Bounds.Y + SearchTextBox.Bounds.Height),
					color: commandSuccessful ? Color.Green : Color.Red
				);
			}

			// from ShopMenu: draw tooltip (hover)
			//if (!HoverText.Equals("")) {
			//	IClickableMenu.drawHoverText(b, HoverText, Game1.smallFont);
			//}

			// draw the upper right close button
			base.draw(b);

			// draw cursor
			drawMouse(b);
		}
	}
}
