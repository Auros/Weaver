using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Weaver.Visuals.Monolith
{
    public class MonolithOwnerPoolController : MonoBehaviour, IObjectPool<MonolithOwner>
    {
        [Inject]
        private readonly IObjectResolver _container = null!;
        
        [SerializeField, Min(0)]
        private int _defaultSize;
        
        [SerializeField]
        private MonolithOwner _monolithOwnerPrefab = null!;

        [SerializeField]
        private Transform _activeOwnerContainer = null!;
        
        [SerializeField]
        private Transform _inactiveOwnerContainer = null!;

        private ObjectPool<MonolithOwner> _pool = null!;

        private void Awake()
        {
            _pool = new ObjectPool<MonolithOwner>(CreateOwner, GetOwner, ReleaseOwner, DestroyOwner);
        }

        private void Start()
        {
            var list = ListPool<MonolithOwner>.Get();
            for (int i = 0; i < _defaultSize; i++)
                list.Add(Get());
            
            for (int i = 0; i < list.Count; i++)
                Release(list[i]);
            
            ListPool<MonolithOwner>.Release(list);
        }

        private MonolithOwner CreateOwner() => _container.Instantiate(_monolithOwnerPrefab, _inactiveOwnerContainer);

        // Prepare the owner coming out of the object pool.
        private void GetOwner(MonolithOwner owner)
        {
            owner.gameObject.SetActive(true);
            var ownerTransform = owner.transform;
            ownerTransform.localPosition = Vector3.zero;
            ownerTransform.localRotation = Quaternion.identity;
            ownerTransform.SetParent(_activeOwnerContainer);
        }

        // Restore the owner coming into the object pool.
        private void ReleaseOwner(MonolithOwner owner)
        {
            owner.transform.SetParent(_inactiveOwnerContainer);
            owner.gameObject.SetActive(false);
        }

        private static void DestroyOwner(MonolithOwner owner) => Destroy(owner);

        public MonolithOwner Get() => _pool.Get();

        public PooledObject<MonolithOwner> Get(out MonolithOwner v) => _pool.Get(out v);

        public void Release(MonolithOwner element) => _pool.Release(element);

        public void Clear() => _pool.Clear();

        public int CountInactive => _pool.CountInactive;
    }
}