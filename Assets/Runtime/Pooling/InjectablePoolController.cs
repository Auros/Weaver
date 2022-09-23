using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Weaver
{
    public abstract class InjectablePoolController<T> : MonoBehaviour, IObjectPool<T> where T : MonoBehaviour
    {
        protected IObjectResolver _container = null!;
        
        [SerializeField, Min(0)]
        private int _defaultSize;

        [SerializeField]
        private T _prefab = null!;

        [SerializeField]
        private Transform _activeContainer = null!;

        [SerializeField]
        private Transform _inactiveContainer = null!;

        [SerializeField]
        private bool _shouldInject = true;
        
        private ObjectPool<T> _pool = null!;

        private void Awake()
        {
            _pool = new ObjectPool<T>(CreateObject, GetObject, ReleaseObject, DestroyObject);
        }
        
        protected virtual void Start()
        {
            var list = ListPool<T>.Get();
            for (int i = 0; i < _defaultSize; i++)
                list.Add(Get());
            
            for (int i = 0; i < list.Count; i++)
                Release(list[i]);
            
            ListPool<T>.Release(list);
        }

        private T CreateObject()
        {
            return _shouldInject ? _container.Instantiate(_prefab, _inactiveContainer) : Instantiate(_prefab, _inactiveContainer);
        }
        
        private void GetObject(T obj)
        {
            obj.gameObject.SetActive(true);
            var nodeTransform = obj.transform;
            nodeTransform.localPosition = Vector3.zero;
            nodeTransform.localRotation = Quaternion.identity;
            nodeTransform.SetParent(_activeContainer);
        }
        
        // Restore the object coming into the object pool.
        protected virtual void ReleaseObject(T obj)
        {
            obj.transform.SetParent(_inactiveContainer);
            obj.gameObject.SetActive(false);
        }
        
        private static void DestroyObject(T obj) => Destroy(obj);
        
        public T Get() => _pool.Get();

        public PooledObject<T> Get(out T v) => _pool.Get(out v);

        public void Release(T element) => _pool.Release(element);

        public void Clear() => _pool.Clear();

        public int CountInactive => _pool.CountInactive;
    }
}
