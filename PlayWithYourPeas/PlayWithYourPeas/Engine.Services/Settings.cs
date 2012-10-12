using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PlayWithYourPeas.Engine.Services.Storage;
using System.Xml;
using System.Xml.Serialization;

namespace PlayWithYourPeas.Engine.Services
{
    public static class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        public static GameSettings Get
        {
            get { return _current; }
        }

        private static GameSettings _current;
        private static FileManager _fileManager;

        private static readonly String DefaultFileName = "Settings.xml";
        public static readonly Int32 FileOperationTimeout = 2500;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="game"></param>
        public static void Initialize(Game game)
        {
            _fileManager = (FileManager)game.Services.GetService(typeof(FileManager));

            if (_fileManager != null)
                if (!Load())
                    LoadDefault();
        }

        /// <summary>
        /// Loads the settings from file
        /// </summary>
        /// <param name="name"></param>
        public static Boolean Load()
        {
            if (_fileManager == null)
                throw new InvalidOperationException("Settings class not initialized");

            try
            {
                // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);

                // Wait until storage container is ready and load the file
                if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, Settings.FileOperationTimeout))
                {
                    if (storage.FileExists(".", DefaultFileName))
                    {
                        storage.Load(".", Settings.DefaultFileName, Load);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Load default if nothing was loaded
                LoadDefault();
            }

            return false;
        }

        /// <summary>
        /// Load action
        /// </summary>
        /// <param name="stream"></param>
        private static void Load(Stream stream)
        {
            _current = (GameSettings)new XmlSerializer(typeof(GameSettings)).Deserialize(stream);
        }

        /// <summary>
        /// Loads a default instance
        /// </summary>
        private static void LoadDefault()
        {
            _current = new GameSettings();

            // Saves the default instance
            Save();
        }

        /// <summary>
        /// 
        /// </summary>
        public static Boolean Save()
        {
            if (_fileManager == null)
                throw new InvalidOperationException("Settings class not initialized");

            // Get the storage device for the player
            IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);

            // Wait until storage container is ready and save the file
            if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, Settings.FileOperationTimeout))
            {
                storage.Save(".", Settings.DefaultFileName, Save);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save action
        /// </summary>
        /// <param name="stream"></param>
        private static void Save(Stream stream)
        {
            new XmlSerializer(typeof(GameSettings)).Serialize(stream, _current);
        }
    }

    [Serializable]
    public class GameSettings
    {

        public Double TriggerKeyPressTime { get; set; }
        public Double TriggerKeyReactivationTime { get; set; }
        public Boolean ShowJumpInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public GameSettings()
        {
            TriggerKeyPressTime = 300;
            TriggerKeyReactivationTime = 15;
            ShowJumpInfo = true;
        }
    }
}