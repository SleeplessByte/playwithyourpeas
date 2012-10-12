using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Logic;

namespace PlayWithYourPeas.Screens
{
    public class AchievementsScreen : GameScreen
    {
        protected Texture2D[] _achievementFilled;
        protected Texture2D _achievementHolder;
        protected Texture2D _backgroundTexture;

        protected Color _color, _colorFilled, _colorTextShadow;
        protected Vector2 _position, _positionTarget;
        protected List<Tuple<Achievement, Int32>> _achievements;

        public Int32 _width, _heigth;

        /// <summary>

        public AchievementsScreen(Int32 width, Int32 height)
        {
            this.TransitionOnTime = TimeSpan.FromSeconds(3);
            this.TransitionOffTime = TimeSpan.FromSeconds(3);
            this.TransitionPosition = 1;

            _width = width; 
            _heigth = height;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _color = Color.Transparent;
            _colorFilled = Color.Transparent;
            _colorTextShadow = Color.Transparent;
            _position = new Vector2(20, 20);
            //_positionTarget = Vector2.UnitX * _position.X + Vector2.UnitY * 70 * Count;
            this.Color = Color.White * 0;

            _achievements = new List<Tuple<Achievement, int>>();

            // get
            foreach (var bp in Achievement.BluePrint) {
                _achievements.Add(new Tuple<Achievement, int>(bp, 0));
            }

            // Count
            foreach (var p in PlayerProgress.Current.Achievements)
            {
                var tuple = _achievements.IndexOf(_achievements.First(i => i.Item1.Equals(p)));
                _achievements[tuple] = new Tuple<Achievement, int>(_achievements[tuple].Item1, _achievements[tuple].Item2 + 1);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            var moods = new String[] { "Pea-Standard", "Pea-Happy", "Pea-Hurt" };
            _achievementFilled = new Texture2D[moods.Length];
            for (int m = 0; m < moods.Length; m++)
                _achievementFilled[m] = this.ContentManager.Load<Texture2D>(Path.Combine("Graphics/", moods[m]));
            _achievementHolder = this.ContentManager.Load<Texture2D>("Graphics/Achievement-Bar");
            _backgroundTexture = this.ContentManager.Load<Texture2D>("Graphics/Background");

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
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            var transionIn = this.ScreenState == Engine.Services.ScreenState.TransitionOn;

            _color = Color.Lerp(transionIn ? Color.White : _color, transionIn ? _color : Color.Transparent, this.TransitionPosition);
            _colorFilled = Color.Lerp(transionIn ? Color.White : _colorFilled, transionIn ? _colorFilled : Color.Transparent, (this.TransitionPosition) / (transionIn ? 0.8f : 1f));
            _colorTextShadow = Color.Lerp(transionIn ? Color.Black : _colorTextShadow, transionIn ? _colorTextShadow : Color.Transparent, (this.TransitionPosition) / (transionIn ? 0.8f : 1f));
            //_position = Vector2.Lerp(_positionTarget, _position, this.TransitionPosition);
            this.Color = Color.White * (1 - this.TransitionPosition);
        }

        /// <summary>
        /// 
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            this.ScreenManager.SpriteBatch.Begin();

            this.ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, this.Color);

            var basepos = _position;

            foreach (var data in _achievements)
            {
                if (data.Item1.Name.StartsWith("Stat"))
                    continue;

                this.ScreenManager.SpriteBatch.Draw(_achievementHolder, _position, _color);
                if (data.Item2 > 0)
                    this.ScreenManager.SpriteBatch.Draw(_achievementFilled[Math.Max(0, (Int32)data.Item1.Alignment - 1)], _position + Vector2.One * 6 + Vector2.UnitY, _colorFilled);
                
                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], data.Item1.Name,
                   _position + Vector2.One * 11 + Vector2.UnitX * 45, _colorTextShadow);
                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Interface"], data.Item1.Name,
                    _position + Vector2.One * 10 + Vector2.UnitX * 45, _colorFilled);

                if (data.Item1.Scope != Achievement.Times.Single && data.Item2 > 1)
                {
                    this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["InterfaceSmall"], String.Format("{0}x", data.Item2),
                       _position + Vector2.One * 30 + Vector2.UnitX * 0, _colorTextShadow);
                    this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["InterfaceSmall"], String.Format("{0}x", data.Item2),
                        _position + Vector2.One * 30 + Vector2.UnitX * 0, _colorFilled);
                }

                _position.Y += 70;

                if (_position.Y > _heigth)
                {
                    _position.Y = basepos.Y;
                    _position.X += 350;
                }
            }

            _position = basepos;
            this.ScreenManager.SpriteBatch.End();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);

            if (this.ScreenManager.InputManager.Keyboard.IsKeyTriggerd(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                this.ExitScreen();
            }
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
