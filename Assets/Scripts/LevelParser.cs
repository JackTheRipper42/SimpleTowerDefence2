using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts
{
    public class LevelParser
    {
        private const string LevelsFolder = "Levels";

        public List<Level> ParseLevels()
        {
            var schemaFile = System.IO.Path.Combine(
                System.IO.Path.Combine(Application.streamingAssetsPath, LevelsFolder),
                "Level.xsd");
            var files = Directory.GetFiles(
                System.IO.Path.Combine(Application.streamingAssetsPath, LevelsFolder),
                "*.xml");
            var schemaReader = new XmlTextReader(schemaFile);
            var schema = XmlSchema.Read(schemaReader, (o, e) => { });
            var serializer = new XmlSerializer(typeof(Level));
            var levels = new List<Level>(files.Length);
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var document = new XmlDocument();
                    document.Schemas.Add(schema);
                    document.Load(stream);
                    document.Validate((o, e) => { });

                    stream.Position = 0;
                    var level = (Level) serializer.Deserialize(stream);
                    levels.Add(level);
                }
            }
            return levels;
        }
    }
}
