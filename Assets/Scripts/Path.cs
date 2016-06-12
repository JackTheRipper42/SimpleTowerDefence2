using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Path : MonoBehaviour
    {
        public Transform[] Nodes;

        public Vector3[] GetPath()
        {
            return Nodes.Select(node => node.position).ToArray();
        }

        protected virtual void Start()
        {
            foreach (var node in Nodes)
            {
                node.gameObject.SetActive(false);
            }
        }
    }
}
