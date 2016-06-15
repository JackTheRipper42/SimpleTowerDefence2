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
        public Transform TowersContainer;
        public GameObject EnemyPrefab;
        public GameObject TowerPrefab;
        public string PrimaryMouseButtonAxis = "Fire1";
        public LayerMask TerrainLayerMask;
        public LayerMask PlacementObstacleLayerMask;

        private Settings _settings;
        private List<Enemy> _enemies;
        private bool _levelLoaded;

        public IEnumerable<Enemy> Enemies
        {
            get { return _enemies; }
        }

        public void Killed(Enemy enemy)
        {
            _enemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }

        protected virtual void Start()
        {
            _settings = FindObjectOfType<Settings>();
            _enemies = new List<Enemy>();
            _levelLoaded = false;
            StartCoroutine(LoadLevel());
        }

        protected virtual void Update()
        {
            if (!_levelLoaded)
            {
                return;
            }

            if (Input.GetButtonDown(PrimaryMouseButtonAxis))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity, TerrainLayerMask.value))
                {
                    var rasterizedPosition = new Vector3(
                        Mathf.Floor(hit.point.x) + 0.5f,
                        0f,
                        Mathf.Floor(hit.point.z) + 0.5f);

                    Vector3 finalPosition;
                    if (IsRasterizedPositionValid(rasterizedPosition, out finalPosition))
                    {
                        var obj = Instantiate(TowerPrefab);
                        obj.transform.position = finalPosition;
                        obj.transform.parent = TowersContainer;
                    }
                }
            }
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

            _levelLoaded = true;

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

        private bool IsRasterizedPositionValid(Vector3 rasterizedPosition, out Vector3 finalPosition)
        {
            var sampleOffsets = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(-0.5f, 0f, 0f),
                new Vector3(0.25f, 0f, 0f),
                new Vector3(-0.25f, 0f, 0f),
                new Vector3(0f, 0f, 0.5f),
                new Vector3(0f, 0f, -0.5f),
                new Vector3(0f, 0f, 0.25f),
                new Vector3(0f, 0f, -0.25f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.25f, 0f, 0.25f),
                new Vector3(-0.25f, 0f, -0.25f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.25f, 0f, -0.25f),
                new Vector3(-0.25f, 0f, 0.25f),
            };
            var heights = new List<float>(sampleOffsets.Length);
            finalPosition = new Vector3();
            foreach (var offset in sampleOffsets)
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    new Ray(
                        rasterizedPosition + offset + new Vector3(0f, 1000f, 0f),
                        Vector3.down),
                    out hit,
                    float.PositiveInfinity,
                    PlacementObstacleLayerMask.value | TerrainLayerMask.value))
                {
                    if (1 << hit.transform.gameObject.layer == TerrainLayerMask.value)
                    {
                        heights.Add(hit.point.y);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            var mediumHeight = heights.Sum()/heights.Count;

            if (heights.Any(height => Mathf.Abs(height - mediumHeight) > 0.05))
            {
                return false;
            }

            finalPosition = new Vector3(
                rasterizedPosition.x,
                mediumHeight,
                rasterizedPosition.z);
            return true;
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
