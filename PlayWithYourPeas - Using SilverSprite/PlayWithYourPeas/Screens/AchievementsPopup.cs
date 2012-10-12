using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Drawing;
using System.IO;

namespace PlayWithYourPeas.Screens
{
    /// <summary>
    /// 
    /// </summary>
    internal class AchievementsPopup : GameScreen
    {
        protected Texture2D _achievementHolder;
        protected Texture2D _achievementFilled;

        protected Color _color, _colorFilled, _colorTextShadow;
        protected Vector2 _position, _positionTarget;

        /// <summary>
        /// 
        /// </summary>
        public static Int32 Count { get; protected set;}
        
        /// <summary>
        /// 
        /// </summary>
        //[Unused]
        public static Int32 Slot { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public String Name { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Achievement.Mood Mood { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        //[Unused]
        public String Description { get; protected set; }
        
        protected Int32 _width;
        protected Camera2D _camera;

        /// <summary>
        /// 
        /// </summary>
        public AchievementsPopup(Achievement data, Camera2D camera, Int32 width)
            : base()
        {
            this.Name = data.Name;
            this.Description = data.Description;
            this.Mood = data.Alignment;

            _width = width;
            _camera = camera;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            this.IsPopup = true;
            this.TransitionOnTime = TimeSpan.FromSeconds(3);
            this.TransitionOffTime = TimeSpan.FromSeconds(3);
            this.TransitionPosition = 1;

            base.Initialize();

            // TODO better counting
            ++Count;

            _color = Color.Transparent;
            _colorFilled = Color.Transparent;
            _colorTextShadow = Color.Transparent;
            _position = new Vector2(_width / 2 - 160, -100);
            _positionTarget = Vector2.UnitX * _position.X + Vector2.UnitY * 70 * Count;

            //++Slot;

            this.Exited += new EventHandler(AchievementsPopup_Exited);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AchievementsPopup_Exited(object sender, EventArgs e)
        {
            --Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            var moods = new String[] { "Pea-Standard", "Pea-Happy", "Pea-Hurt" };
            _achievementFilled = this.ContentManager.Load<Texture2D>(Path.Combine("Graphics/", moods[Math.Max(0, (Int32)this.Mood - 1)]));
            _achievementHolder = this.ContentManager.Load<Texture2D>("Graphics/Achievement-Bar");

            this.ScreenManager.SpriteFonts.LoadFont("Interface", "Fonts/Interface");
            this.ScreenManager.SpriteFonts.LoadFont("Interface.Small", "Fonts/InterfaceSmall");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            var transionIn = this.ScreenState == Engine.Services.ScreenState.TransitionOn;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // TODO: clean up pipes and make normal if else branch
            _color = Color.Lerp(transionIn ? Color.White : _color, transionIn ? _color : Color.Transparent, this.TransitionPosition);
            _colorFilled = Color.Lerp(transionIn ? Color.White : _colorFilled, transionIn ? _colorFilled : Color.Transparent, (this.TransitionPosition) / (transionIn ? 0.8f : 1f));
            _colorTextShadow = Color.Lerp(transionIn ? Color.Black : _colorTextShadow, transionIn ? _colorTextShadow : Color.Transparent, (this.TransitionPosition) / (transionIn ? 0.8f : 1f));
            _position = Vector2.Lerp(_positionTarget, _position, this.TransitionPosition);

            if (this.IsActive && !this.IsTransitioning)
                ExitScreen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _camera.View);
            this.ScreenManager.SpriteBatch.Draw(_achievementHolder, _position, _color);
            this.ScreenManager.SpriteBatch.Draw(_achievementFilled, _position + Vector2.One * 6 + Vector2.UnitY, _colorFilled);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], this.Name,
               _position + Vector2.One * 11 + Vector2.UnitX * 45, _colorTextShadow);
 
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], this.Name, 
                _position + Vector2.One * 10 + Vector2.UnitX * 45, _colorFilled);
            this.ScreenManager.SpriteBatch.End();
        }
    }
}
