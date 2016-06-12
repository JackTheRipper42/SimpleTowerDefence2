using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts
{
    public class Settings : MonoBehaviour
    {
        public List<Level> Levels { get; private set; }

        public int LevelIndex { get; set; }

        public Level Level
        {
            get { return Levels[LevelIndex]; }
        }

        protected virtual void Start()
        {
            DontDestroyOnLoad(this);
            Levels = ParseLevels();
            LevelIndex = Levels.Count - 1;
        }

        private static List<Level> ParseLevels()
        {
            var files = Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Levels"), "*.xml");
            var serializer = new XmlSerializer(typeof(Level));
            var levels = new List<Level>(files.Length);
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var level = (Level)serializer.Deserialize(stream);
                    levels.Add(level);
                }
            }
            return levels;
        }
    }
}
