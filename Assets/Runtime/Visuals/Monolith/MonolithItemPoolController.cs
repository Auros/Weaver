using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Weaver.Visuals.Monolith
{
    public class MonolithItemPoolController : MonoBehaviour, IObjectPool<MonolithItem>
    {
        [Inject]
        private readonly IObjectResolver _container = null!;
        
        [SerializeField, Min(0)]
        private int _defaultSize;
        
        [SerializeField]
        private MonolithItem _monolithItemPrefab = null!;

        [SerializeField]
        private Transform _activeItemContainer = null!;
        
        [SerializeField]
        private Transform _inactiveItemContainer = null!;

        private ObjectPool<MonolithItem> _pool = null!;

        private void Awake()
        {
            _pool = new ObjectPool<MonolithItem>(CreateNode, GetNode, ReleaseNode, DestroyNode);
        }

        private void Start()
        {
            var list = ListPool<MonolithItem>.Get();
            for (int i = 0; i < _defaultSize; i++)
                list.Add(Get());
            
            for (int i = 0; i < list.Count; i++)
                Release(list[i]);
            
            ListPool<MonolithItem>.Release(list);
        }

        private MonolithItem CreateNode() => _container.Instantiate(_monolithItemPrefab, _inactiveItemContainer);

        // Prepare the item coming out of the object pool.
        private void GetNode(MonolithItem node)
        {
            node.gameObject.SetActive(true);
            var nodeTransform = node.transform;
            nodeTransform.localPosition = Vector3.zero;
            nodeTransform.localRotation = Quaternion.identity;
            nodeTransform.SetParent(_activeItemContainer);
        }

        // Restore the item coming into the object pool.
        private void ReleaseNode(MonolithItem node)
        {
            node.transform.SetParent(_inactiveItemContainer);
            node.gameObject.SetActive(false);
        }

        private static void DestroyNode(MonolithItem node) => Destroy(node);

        public MonolithItem Get() => _pool.Get();

        public PooledObject<MonolithItem> Get(out MonolithItem v) => _pool.Get(out v);

        public void Release(MonolithItem element) => _pool.Release(element);

        public void Clear() => _pool.Clear();

        public int CountInactive => _pool.CountInactive;
    }
}