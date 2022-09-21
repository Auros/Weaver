using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Weaver.Visuals.Monolith
{
    public class MonolithNodePoolController : MonoBehaviour, IObjectPool<MonolithNode>
    {
        [Inject]
        private readonly IObjectResolver _container = null!;
        
        [SerializeField, Min(0)]
        private int _defaultSize;
        
        [SerializeField]
        private MonolithNode _monolithNodePrefab = null!;

        [SerializeField]
        private Transform _activeNodeContainer = null!;
        
        [SerializeField]
        private Transform _inactiveNodeContainer = null!;

        private ObjectPool<MonolithNode> _pool = null!;

        private void Awake()
        {
            _pool = new ObjectPool<MonolithNode>(CreateNode, GetNode, ReleaseNode, DestroyNode);
        }

        private void Start()
        {
            var list = ListPool<MonolithNode>.Get();
            for (int i = 0; i < _defaultSize; i++)
                list.Add(Get());
            
            for (int i = 0; i < list.Count; i++)
                Release(list[i]);
            
            ListPool<MonolithNode>.Release(list);
        }

        private MonolithNode CreateNode() => _container.Instantiate(_monolithNodePrefab, _inactiveNodeContainer);

        // Prepare the node coming out of the object pool.
        private void GetNode(MonolithNode node)
        {
            node.gameObject.SetActive(true);
            var nodeTransform = node.transform;
            nodeTransform.localPosition = Vector3.zero;
            nodeTransform.localRotation = Quaternion.identity;
            nodeTransform.SetParent(_activeNodeContainer);
        }

        // Restore the node coming into the object pool.
        private void ReleaseNode(MonolithNode node)
        {
            node.transform.SetParent(_inactiveNodeContainer);
            node.gameObject.SetActive(false);
        }

        private static void DestroyNode(MonolithNode node) => Destroy(node);

        public MonolithNode Get() => _pool.Get();

        public PooledObject<MonolithNode> Get(out MonolithNode v) => _pool.Get(out v);

        public void Release(MonolithNode element) => _pool.Release(element);

        public void Clear() => _pool.Clear();

        public int CountInactive => _pool.CountInactive;
    }
}