using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PlayWithYourPeas.Engine.Services.Storage;
using PlayWithYourPeas.Engine.Services;
using PlayWithYourPeas.Engine.Drawing;
using PlayWithYourPeas.Drawing;
using PlayWithYourPeas.Screens;

namespace PlayWithYourPeas
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PeasGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Graphics hook
        /// </summary>
        internal GraphicsDeviceManager Graphics
        {
            get;
            private set;
        }

        /// <summary>
        /// SpriteBatch hook
        /// </summary>
        internal SpriteBatch SpriteBatch
        {
            get;
            private set;
        }

        /// <summary>
        /// ScreenManager hook
        /// </summary>
        internal ScreenManager ScreenManager
        {
            get;
            private set;
        }

        /// <summary>
        /// InputManager hook
        /// </summary>
        internal InputManager InputManager
        {
            get;
            private set;
        }

        /// <summary>
        /// FileManager hook
        /// </summary>
        internal FileManager FileManager
        {
            get;
            private set;
        }

#if !SILVERLIGHT
        /// <summary>
        /// 
        /// </summary>
        internal AudioManager AudioManager
        {
            get;
            private set;
        }
#endif

        #region FRAMERATE
        private TimeSpan _elapsedTime;
        private Int32 _frameCount, _frameRate;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public PeasGame()
        {
            // Set Graphics profile
            this.Graphics = new GraphicsDeviceManager(this);
#if !SILVERLIGHT
            this.Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            this.Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            this.Graphics.SupportedOrientations = DisplayOrientation.Default;
                        this.Graphics.SynchronizeWithVerticalRetrace = true;
#endif
            this.Graphics.PreferredBackBufferHeight = 720;
            this.Graphics.PreferredBackBufferWidth = 1280;

#if DEBUG
            // Unlimted FPS
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60f);

            // Windowed
            this.Graphics.IsFullScreen = false;
#else
            // Capped FPS
            this.IsFixedTimeStep = true;
            this.Graphics.SynchronizeWithVerticalRetrace = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60f);

            // Fullscreen
            this.Graphics.IsFullScreen = false;
#endif

            // Apply Graphics
            this.Graphics.ApplyChanges();
            this.Content.RootDirectory = "Content";

            // Set components
            this.InputManager = new InputManager(this);
            this.ScreenManager = new ScreenManager(this);
            this.FileManager = new FileManager(this);

#if !SILVERLIGHT
            this.AudioManager = new AudioManager(this);
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            this.Components.Add(this.InputManager);
            this.Components.Add(this.FileManager);
            this.Components.Add(this.ScreenManager);
            
#if !SILVERLIGHT
            this.Components.Add(this.AudioManager);
#endif

            // Initialize all components
            base.Initialize();

            Settings.Initialize(this);
            Progress.Initialize(this);

            this.ScreenManager.AddScreen(new PeaScreen());
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Loads font
            this.ScreenManager.SpriteFonts.LoadFont("Framerate", "Fonts/Framerate");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // FRAMERATE AREA
            // This area is reserved to count the current frame rate.
            // Each second the framerate is updated (any more is not needed)

            // Add to ElapsedTime
            _elapsedTime += gameTime.ElapsedGameTime;
            // If More then one second passed
            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                // Set the FrameRate
                _frameRate = _frameCount;
                // Reset the FrameCount
                _frameCount = 0;
                // Remove this second
                _elapsedTime -= TimeSpan.FromSeconds(1);
            }

            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            // FRAMERATE AREA
            // This area is reserved to count the current frame rate.
            // Each second the framerate is updated (more is not needed)
            _frameCount++;

            this.SpriteBatch.Begin();
            this.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Framerate"], String.Format("Framerate: {0} f/s\n", _frameRate), Vector2.One * 11, Color.Black);
            this.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Framerate"], String.Format("Framerate: {0} f/s\n", _frameRate), Vector2.One * 10, Color.White);
            this.SpriteBatch.End();
        }
    }
}
