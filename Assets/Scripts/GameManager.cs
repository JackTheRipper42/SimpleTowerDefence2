using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Enemy Enemy;

        protected virtual void Start()
        {
            var path = FindObjectsOfType<Path>().First();

            Enemy.Initialize(path.GetPath());
        }
    }
}
