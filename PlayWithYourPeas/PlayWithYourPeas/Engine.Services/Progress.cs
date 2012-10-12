using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services.Storage;
using Microsoft.Xna.Framework;
using System.Threading;
using System.IO;

namespace PlayWithYourPeas.Engine.Services
{
    public static class Progress
    {
        private static Progress.Data _current;
        private static FileManager _fileManager;

        private static readonly String DefaultFileName = "Progress-{0}.bin";
        private static readonly Int32 FileOperationTimeout = 2500;

        /// <summary>
        /// 
        /// </summary>
        public static Progress.Data Current { get { return _current;  } }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="game"></param>
        public static void Initialize(Game game)
        {
            _fileManager = (FileManager)game.Services.GetService(typeof(FileManager));
            _current = new Data(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public static Boolean Load(Int32 serialNumber)
        {
            if (_fileManager == null)
                throw new InvalidOperationException("Progress class not initialized");

            try
            {
                // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);

                // Wait until storage container is ready and load the file
                if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, Progress.FileOperationTimeout))
                {
                    if (storage.FileExists(".", String.Format(DefaultFileName, serialNumber)))
                    {
                        storage.Load(".", String.Format(DefaultFileName, serialNumber), Load);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Load default if nothing was loaded  
                LoadDefault(serialNumber);
            }

            return false;
        }

        /// <summary>
        /// Load action
        /// </summary>
        /// <param name="stream"></param>
        private static void Load(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                _current = new Progress.Data(br.ReadInt32());
                _current.Score = br.ReadInt32();
            }
        }

        /// <summary>
        /// Loads a default instance
        /// </summary>
        private static void LoadDefault(Int32 serialNumber)
        {
            _current = new Progress.Data(serialNumber) {
                SessionScore = 0,
                Score = 0,
            };

            // Saves the default instance
            Save();
        }

        /// <summary>
        /// 
        /// </summary>
        public static Boolean Save()
        {
            if (_fileManager == null)
                throw new InvalidOperationException("Progress class not initialized");

            // Get the storage device for the player
            try
            {
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);

                // Wait until storage container is ready and save the file
                if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, Progress.FileOperationTimeout))
                {
                    storage.Save(".", String.Format(DefaultFileName, _current.SerialNumber), Save);
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        /// <summary>
        /// Save action
        /// </summary>
        /// <param name="stream"></param>
        private static void Save(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write(_current.SerialNumber);
                bw.Write(_current.Score);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Reset()
        {
            LoadDefault(_current.SerialNumber);
            Save();
        }

        [Serializable]
        public class Data
        {
            public Int32 SerialNumber { get; protected set; }
            public Int32 SessionScore { get; set; }
            public Int32 Score { get; set; }
            public DateTime SessionTime { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serialnumber"></param>
            public Data(Int32 serialnumber)
            {
                this.SerialNumber = serialnumber;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="points"></param>
            public void Points(Int32 points)
            {
                this.SessionScore += points;
            }

            /// <summary>
            /// 
            /// </summary>
            public void NextLevel()
            {
                this.Score += this.SessionScore;
                Save();
            }
        }
    }


}
