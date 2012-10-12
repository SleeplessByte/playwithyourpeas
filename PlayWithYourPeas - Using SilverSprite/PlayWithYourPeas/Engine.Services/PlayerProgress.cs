using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Services.Storage;

namespace PlayWithYourPeas.Engine.Services
{
    public static class PlayerProgress
    {
        private static PlayerProgress.Data _current;
        private static FileManager _fileManager;

        /// <summary>
        /// 
        /// </summary>
        private static readonly String DefaultFileName = "Progress-{0}.bin";

        /// <summary>
        /// 
        /// </summary>
        private static readonly Int32 FileOperationTimeout = 2500;

        /// <summary>
        /// 
        /// </summary>
        public static PlayerProgress.Data Current { get { return _current;  } }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="game"></param>
        public static void Initialize(Game game)
        {
            _fileManager = (FileManager)game.Services.GetService(typeof(FileManager));
            if (!Load("0"))
                LoadDefault("0");
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public static Boolean Load(String serialNumber)
        {
            if (_fileManager == null)
                throw new InvalidOperationException("Progress class not initialized");

            try
            {
               
#if SILVERLIGHT
                // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Isolated);
                // Wait until storage container is ready and load the file
                Boolean timeout = false;
                Timer timer = new Timer((a) => timeout = true, null, PlayerProgress.FileOperationTimeout, System.Threading.Timeout.Infinite);

                while (!storage.IsReady && !timeout)
                {
                    Thread.SpinWait(1000); Thread.MemoryBarrier();
                }

                if (timeout)
                    throw new NotSupportedException();

                if (storage.FileExists(".", String.Format(DefaultFileName, serialNumber)))
                {
                    storage.Load(".", String.Format(DefaultFileName, serialNumber), Load);
                    return true;
                }

                LoadDefault(serialNumber);
#else
                 // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);
                // Wait until storage container is ready and load the file
                if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, PlayerProgress.FileOperationTimeout))
                {
                    if (storage.FileExists(".", String.Format(DefaultFileName, serialNumber)))
                    {
                        storage.Load(".", String.Format(DefaultFileName, serialNumber), Load);
                        return true;
                    }
                }
#endif
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
            DataContractSerializer dcs = new DataContractSerializer(typeof(PlayerProgress.Data),
                new List<Type>() { typeof(Single), typeof(Double), typeof(Achievement), typeof(HashSet<DataPea>), typeof(DataPea) });
            _current = dcs.ReadObject(stream) as PlayerProgress.Data;

        }

        /// <summary>
        /// Loads a default instance
        /// </summary>
        private static void LoadDefault(String serialNumber)
        {
            _current = new PlayerProgress.Data(serialNumber) {
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
           
#if SILVERLIGHT
            // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Isolated);
               
                Boolean timeout = false;
                Timer timer = new Timer((a) => timeout = true, null, PlayerProgress.FileOperationTimeout, System.Threading.Timeout.Infinite);

                while (!storage.IsReady && !timeout)
                {
                    Thread.SpinWait(1000); Thread.MemoryBarrier();
                }

                if (timeout)
                    return false;

                storage.Save(".", String.Format(DefaultFileName, _current.SerialNumber), Save);
                return true;
#else
            try
            {
                        // Get the storage device for the player
                IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.Player);
               
                // Wait until storage container is ready and save the file
                if (SpinWait.SpinUntil(() => { Thread.MemoryBarrier(); return storage.IsReady; }, PlayerProgress.FileOperationTimeout))
                {
                    storage.Save(".", String.Format(DefaultFileName, _current.SerialNumber), Save);
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
#endif
        }

        /// <summary>
        /// Save action
        /// </summary>
        /// <param name="stream"></param>
        private static void Save(Stream stream)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(PlayerProgress.Data), 
                new List<Type>() { typeof(Single), typeof(Double), typeof(Achievement), typeof(HashSet<DataPea>), typeof(DataPea) });
            dcs.WriteObject(stream, _current);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Reset()
        {
            LoadDefault(_current.SerialNumber);
            Save();
        }

        [DataContract]
        public class Data
        {
            /// <summary>
            /// SerialNumber/FacebookId
            /// </summary>
            [DataMember]
            public String SerialNumber { get; set; }

            /// <summary>
            /// Session Score
            /// </summary>
            [DataMember]
            public Int32 SessionScore { get; set; }

            /// <summary>
            /// Total score
            /// </summary>
            [DataMember]
            public Int32 Score { get; set; }

            /// <summary>
            /// Session start time ???? TODO
            /// </summary>
            [DataMember]
            public DateTime SessionStartTime { get; set; }

            /// <summary>
            /// Time played
            /// </summary>
            [DataMember]
            public TimeSpan Time { get; set; }

            /// <summary>
            /// Achievements Obtained
            /// </summary>
            [DataMember]
            public List<Achievement> Achievements { get; set;}


            /// <summary>
            /// 
            /// </summary>
            /// <param name="serialnumber"></param>
            public Data(String serialnumber)
            {
                this.SerialNumber = serialnumber;
                this.Achievements = new List<Achievement>();
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
            /// <param name="achievement"></param>
            public void Achievement(Achievement achievement)
            {
                this.Achievements.Add(achievement);
                Save();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="passed"></param>
            public void TimePassed(TimeSpan passed)
            {
                this.Time += passed;
            }

            /// <summary>
            /// 
            /// </summary>
            public void NextLevel()
            {
                this.Score += this.SessionScore;
                this.SessionScore = 0;
                this.SessionStartTime = DateTime.Now;
                Save();
            }
        }
    }


}
