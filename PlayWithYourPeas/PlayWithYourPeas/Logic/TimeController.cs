using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Logic
{
    class TimeController : GameComponent
    {
        public Single Speed { get; set; }
        public Single TargetSpeed { get; set; }
        public Single ResumeSpeed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public TimeController(Game game) : base(game)
        {
            this.TargetSpeed = 1f;
            this.Game.Services.AddService(typeof(TimeController), this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Speed = MathHelper.Lerp(this.Speed, this.TargetSpeed, (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);
        }

        internal void Stop()
        {
            this.ResumeSpeed = this.TargetSpeed;
            this.TargetSpeed = 0;
        }

        internal void Resume()
        {
            this.TargetSpeed = this.ResumeSpeed;
        }
    }
}
