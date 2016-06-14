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
            var levelParser = new LevelParser();
            Levels = levelParser.ParseLevels();
            LevelIndex = 0;
        }
    }
}
