using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Enemy Enemy;

        private List<Enemy> _enemies;

        public IEnumerable<Enemy> Enemies
        {
            get { return _enemies; }
        }

        protected virtual void Start()
        {
            _enemies = new List<Enemy> {Enemy};

            var path = FindObjectsOfType<Path>().First();
            Enemy.Initialize(path.GetPath());
        }
    }
}
