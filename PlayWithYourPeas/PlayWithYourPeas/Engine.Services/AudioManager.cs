using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace PlayWithYourPeas.Engine.Services
{
#if !SILVERLIGHT
    public class AudioManager : GameComponent
    {
        /// <summary>
        /// 
        /// </summary>
        public List<SoundEffectInstance> ActiveSoundEffects
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<AudioEmitter> ActiveAudioEmmiters
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public AudioListener AvatarListener
        {
            get;
            set;
        }

        public Vector3 AvatarListenerPosition
        {
            get { return this.AvatarListener.Position; }
            set { this.AvatarListener.Position = value; }
        }

        public Vector3 AvatarListenerSpeed
        {
            get { return this.AvatarListener.Velocity; }
            set { this.AvatarListener.Velocity = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Song ActiveSong
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ContentManager ContentManager { get; private set; }


        public AudioManager(Game game)
            : base(game)
        {
            this.ActiveSoundEffects = new List<SoundEffectInstance>();
            this.ActiveAudioEmmiters = new List<AudioEmitter>();

            // ContentManager
            this.ContentManager = new ContentManager(game.Services);
            this.ContentManager.RootDirectory = "Content/Audio";

            // Add as Service
            this.Game.Services.AddService(this.GetType(), this);

            this.Game.Exiting += new EventHandler<EventArgs>(Game_Exiting);
        }

        void Game_Exiting(object sender, EventArgs e)
        {
            foreach (SoundEffectInstance instance in this.ActiveSoundEffects)
            {
                if (!instance.IsDisposed)
                {
                    instance.Stop();
                    instance.Dispose();
                }
            }

            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                    MediaPlayer.Stop();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            this.AvatarListener = new AudioListener();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public SoundEffect LoadSoundEffect(String asset)
        {
            return this.ContentManager.Load<SoundEffect>("Background Effects/" + asset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public Song LoadSong(String asset)
        {
            return this.ContentManager.Load<Song>("Background Music/" + asset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public SoundEffectInstance PlaySoundEffect(String asset, Boolean loop, Single volume = 1)
        {
            SoundEffectInstance instance = LoadSoundEffect(asset).CreateInstance();
            instance.IsLooped = loop;
            instance.Volume = volume;
            instance.Play();

            this.ActiveSoundEffects.Add(instance);

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="loop"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public Song PlaySong(String asset, Boolean loop, Single volume = 1)
        {
            Song song = LoadSong(asset);

            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Volume = volume;
            MediaPlayer.Play(song);

            this.ActiveSong = song;

            return song;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="se"></param>
        public void StopSoundEffect(SoundEffectInstance se)
        {
            se.Stop();

            this.ActiveSoundEffects.Remove(se);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public AudioEmitter AddAudioEmmiter(Vector3 position)
        {
            AudioEmitter emmiter = new AudioEmitter();
            emmiter.Position = position;

            this.ActiveAudioEmmiters.Add(emmiter);

            return emmiter;
        }

        public SoundEffectInstance Play3DSoundEffect(String asset, Boolean loop, AudioEmitter emmiter)
        {
            SoundEffectInstance instance = LoadSoundEffect(asset).CreateInstance();
            instance.IsLooped = true;
            instance.Volume = 0.5f;
            instance.Apply3D(AvatarListener, emmiter);
            instance.Play();

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="se"></param>
        /// <param name="ae"></param>
        /// <returns></returns>
        public SoundEffectInstance Update3DSoundEffect(SoundEffectInstance se, AudioEmitter ae)
        {
            se.Apply3D(AvatarListener, ae);

            return se;
        }

    }
#endif
}
