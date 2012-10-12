using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace PlayWithYourPeas.Screens
{
    /// <summary>
    /// Provides an overlay for the game
    /// </summary>
    public class PauseOverlay : GameScreen
    {
        /// <summary>
        /// 
        /// </summary>
        public PauseOverlay() : base()
        {
            this.IsPopup = true;
            this.IsCapturingInput = true;

            this.TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            this.TransitionOffTime = TimeSpan.FromSeconds(0.5f);
            this.TransitionPosition = 1f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            this.ScreenManager.SpriteFonts.LoadFont("Interface.Large", "Fonts/InterfaceLarge");
        }


        /// <summary>
        /// Handles input
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            // Only runs when game is active
            if (this.InputManager.Keyboard.IsKeyPressed(Keys.Enter) || 
                this.InputManager.Keyboard.IsKeyPressed(Keys.Escape) || 
                this.InputManager.Mouse.IsButtonReleased(MouseButton.Left))
                this.ExitScreen();
        }

        /// <summary>
        /// Draws screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.ScreenState == Engine.Services.ScreenState.Hidden || this.ScreenState == Engine.Services.ScreenState.WaitingForTransition)
                return;

            this.ScreenManager.FadeBackBufferToBlack((Byte)((this.TransitionAlpha) * 0.7f));

            this.ScreenManager.SpriteBatch.Begin();

            var measurement = this.ScreenManager.SpriteFonts["Interface.Large"].MeasureString("Game Paused");
            var origin = new Vector2((Int32)(measurement.X / 2), (Int32)(measurement.Y / 2));

            // Draws text
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface.Large"], 
                "Game Paused", new Vector2(1280, 720) / 2 + Vector2.One, 
                Color.Lerp(Color.Transparent, Color.Black, (this.TransitionAlpha / 255f)), 0,
                origin, 1f, SpriteEffects.None, 0);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface.Large"],
                "Game Paused", new Vector2(1280, 720) / 2 + Vector2.Zero,
                Color.Lerp(Color.Transparent, Color.White, (this.TransitionAlpha / 255f)), 0,
                origin, 1f, SpriteEffects.None, 0);

            this.ScreenManager.SpriteBatch.End();
        }
    }
}
