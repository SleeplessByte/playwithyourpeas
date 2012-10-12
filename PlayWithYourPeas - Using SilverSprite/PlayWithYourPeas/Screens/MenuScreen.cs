using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace PlayWithYourPeas.Screens
{
    public class MenuScreen : GameScreen
    {
        protected Int32 _cursorIndex;

        protected Texture2D _backgroundTexture;
        protected Int32 _width, _height;

        /// <summary>
        /// 
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MenuScreen(Int32 width, Int32 height)
        {
            //this.IsPopup = true;
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);
            this.TransitionPosition = 1;

            _width = width;
            _height = height;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            this.ScreenManager.SpriteFonts.LoadFont("Interface", "Fonts/Interface");
            _backgroundTexture = this.ContentManager.Load<Texture2D>("Graphics/Background");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            this.Color = Color.Lerp(Color.White, Color.Transparent, TransitionPosition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (!this.IsActive || this.IsTransitioning)
                return;

            if (this.InputManager.Keyboard.IsKeyPressed(Keys.Enter))
            {
                switch (_cursorIndex)
                {
                    case 0:
                        this.Exited += new EventHandler(MenuScreen_Exited);
                        _hasCalledExited = true;
                        ExitScreen();

                        
                        break;
                    case 1:
                        this.ScreenManager.AddScreen(new AchievementsScreen(_width, _height));
                        break;
                    case 2:
                        this.ScreenManager.ExitAll();
                        break;
                }
            }
            else if (this.InputManager.Keyboard.IsKeyPressed(Keys.Escape))
            {
                this.ScreenManager.ExitAll();
            }
            else if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Down))
            {
                _cursorIndex = _cursorIndex > 1 ? 0 : _cursorIndex + 1;
            }
            else if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Up))
            {
                _cursorIndex = _cursorIndex < 1 ? 2 : _cursorIndex - 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MenuScreen_Exited(object sender, EventArgs e)
        {
            PeaScreen.Singleton.IsEnabled = true;
            PeaScreen.Singleton.IsVisible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (this.ScreenState == Engine.Services.ScreenState.Hidden || this.ScreenState == Engine.Services.ScreenState.WaitingForTransition)
                return;

            this.ScreenManager.SpriteBatch.Begin();

            this.ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, this.Color);

            var optionsText = new String[] { "Play", "Achievements", "Exit" };
            var basePosition = this.ScreenManager.ScreenCenter - Vector2.UnitY * (Single)Math.Round(25 * optionsText.Length / 2f);

            for (Int32 i = 0; i < optionsText.Length; i++)
            {
                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], optionsText[i], 
                    basePosition + Vector2.UnitY * i * 25 + Vector2.One, 
                    new Color(0, 0, 0, this.Color.A / 255f), 
                    0, 
                    this.ScreenManager.SpriteFonts["Interface"].MeasureString(optionsText[i]) / 2,
                    (Single)(1 + (_cursorIndex == i ? Math.Abs(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2)) * 0.2f : 0)), 
                    SpriteEffects.None, 
                    0);

                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], optionsText[i],
                    basePosition + Vector2.UnitY * i * 25 + Vector2.Zero,
                    this.Color,
                    0,
                    this.ScreenManager.SpriteFonts["Interface"].MeasureString(optionsText[i]) / 2,
                    (Single)(1 + (_cursorIndex == i ? Math.Abs(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2)) * 0.2f : 0)),
                    SpriteEffects.None,
                    0);
            }

            this.ScreenManager.SpriteBatch.End();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void DoResize(object sender, EventArgs args)
        {
            base.DoResize(sender, args);
        }
    }
}
