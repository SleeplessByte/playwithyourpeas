using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Shows the alert messages such as ninja and trap above a pea.
    /// </summary>
    internal class SpriteAlert : Sprite
    {
        protected Single _alertTime;

        /// <summary>
        /// Alert active time
        /// </summary>
        protected Single MaxAlertTime { get; set; }

        /// <summary>
        /// Alert state
        /// </summary>
        protected SpriteState CurrentState { get; set; }

        /// <summary>
        /// Generates a trap-alert
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteAlert GenerateTrap(SceneLayer layer)
        {
            return new SpriteAlert(layer) { TextureName = "Graphics/Alert-Trap", MaxAlertTime = 2 };
        }

        /// <summary>
        /// Generates a ninja-alert
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteAlert GenerateNinja(SceneLayer layer)
        {
            return new SpriteAlert(layer) { TextureName = "Graphics/Alert-Ninja", MaxAlertTime = 2 };
        }

        /// <summary>
        /// Generates a boom-alert
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteAlert GenerateBoom(SceneLayer layer)
        {
            return new SpriteAlert(layer) { TextureName = "Graphics/Alert-Boom", MaxAlertTime = 1 };
        }

        /// <summary>
        /// Generates a jump spot alert
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteAlert GenerateJumpSpot(SceneLayer layer)
        {
            return new SpriteAlert(layer) { TextureName = "Graphics/Alert-JumpSpot", MaxAlertTime = 2f };
        }

        /// <summary>
        /// Generates a completed spot alert
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteAlert GenerateCompleted(SceneLayer layer)
        {
            return new SpriteAlert(layer) { TextureName = "Graphics/Alert-Completed", MaxAlertTime = 2f, Position = Vector2.UnitY * -50 };
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OnFinished = delegate { };

        /// <summary>
        /// Creates a new alert sprite
        /// </summary>
        /// <param name="layer"></param>
        protected SpriteAlert(SceneLayer layer) : base(layer)
        {
            this.Color = new Color(255, 255, 255, 0); // transparant white
        }

        /// <summary>
        /// Initializes alert (activates)
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _alertTime = 0;
            this.Enabled = true;
            this.Visible = true;
            this.CurrentState = SpriteState.Active;
        }

        /// <summary>
        /// Updates the alert
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            if (!this.Enabled)
                return;

            _alertTime += (Single)gameTime.ElapsedGameTime.TotalSeconds;

            // Update opacity
            this.Color = Color.Lerp(this.Color, this.CurrentState == SpriteState.TransitionOff ? 
                Color.Transparent : Color.White, (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);

            // Kill transition
            if (_alertTime > MaxAlertTime)
            {
                this.CurrentState = SpriteState.TransitionOff;
                if (this.Color.A < 5)
                {
                    this.CurrentState = SpriteState.Hidden;
                    this.Enabled = false;
                    this.Visible = false;

                    this.OnFinished.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Draws the alert
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Vector2 offset)
        {
            base.Draw(gameTime, offset + Vector2.UnitY * 5 * (Single)Math.Sin(_alertTime * 5));
        }
    }
}
