using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Transform EnemiesContainer;
        public GameObject EnemyPrefab;


        private Settings _settings;
        private List<Enemy> _enemies;

        public IEnumerable<Enemy> Enemies
        {
            get { return _enemies; }
        }

        protected virtual void Start()
        {
            _settings = FindObjectOfType<Settings>();
            _enemies = new List<Enemy>();
            StartCoroutine(LoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            var level = _settings.Level;
            var sceneName = GetSceneName(level.Item.GetType());
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncOperation.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            var paths = FindObjectsOfType<Path>().ToDictionary(path => path.transform.name, path => path.GetPath());
            StartSpawnCoroutines(level.Item, paths);
        }

        private void StartSpawnCoroutines(object map, IDictionary<string, Vector3[]> paths)
        {
            var map1Type = map as Map1Type;
            if (map1Type != null)
            {
                StartCoroutine(SpawnWaves(map1Type.Path, paths["Path"]));
                return;
            }
            var map2Type = map as Map2Type;
            if (map2Type != null)
            {
                StartCoroutine(SpawnWaves(map2Type.Path1, paths["Path1"]));
                StartCoroutine(SpawnWaves(map2Type.Path2, paths["Path2"]));
                return;
            }
            throw new NotSupportedException(string.Format("The map type '{0}' is not supported.", map.GetType().Name));

        }
        
        private IEnumerator SpawnWaves(object[][] waves, Vector3[] path)
        {
            var currentWaveIndex = 0;
            while (currentWaveIndex < waves.Length)
            {
                Debug.Log(string.Format("start wave {0}", currentWaveIndex));
                foreach (var item in LinearizeWave(waves[currentWaveIndex]))
                {
                    yield return ProcessWaveItem(item, path);
                }
                currentWaveIndex++;
            }
            Debug.Log("all waves completed");
        }

        private YieldInstruction ProcessWaveItem(object item, Vector3[] path)
        {
            var delay = item as DelayType;
            if (delay != null)
            {
                return new WaitForSeconds(delay.Value);
            }
            var spawnType = item as SpawnType;
            if (spawnType != null)
            {
                SpawnEnemy(EnemyPrefab, path);
                return new WaitForEndOfFrame();
            }
            throw new NotSupportedException(string.Format(
                "The wave item type '{0}' is not supported.",
                item.GetType().Name));
        }

        private void SpawnEnemy(GameObject prefab, Vector3[] path)
        {
            var obj = Instantiate(prefab);
            obj.transform.parent = EnemiesContainer;
            var enemy = obj.GetComponent<Enemy>();
            enemy.Initialize(path);
            _enemies.Add(enemy);
        }

        private static string GetSceneName(Type mapType)
        {
            if (mapType == typeof(Map1Type))
            {
                return "Map1";
            }
            if (mapType == typeof(Map2Type))
            {
                return "Map2";
            }
            throw new NotSupportedException(string.Format("The map type '{0}' is not supported.", mapType.Name));
        }

        private static IEnumerable<object> LinearizeWave(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var loop = item as LoopType;
                if (loop == null)
                {
                    yield return item;
                }
                else
                {
                    for (var i = 0; i < loop.Count; i++)
                    {
                        foreach (var loopItem in LinearizeWave(loop.Items))
                        {
                            yield return loopItem;
                        }
                    }
                }
            }
        }
    }
}
