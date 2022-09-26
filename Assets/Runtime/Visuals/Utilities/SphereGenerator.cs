using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Weaver.Visuals.Utilities
{
    public sealed class SphereGenerator : MonoBehaviour
    {
        [SerializeField]
        private SphereItem _pointPrefab = null!;
        
        [SerializeField, Min(1)]
        private int _items = 1;

        private int _previousItems;

        private ObjectPool<SphereItem> _itemPool = null!;
        private readonly List<SphereItem> _activeItems = new();
        
        private void Start()
        {
            _itemPool = new ObjectPool<SphereItem>(
            () => Instantiate(_pointPrefab, transform),
            item =>
            {
                item.gameObject.SetActive(true);
            }, item =>
            {
                item.gameObject.SetActive(false);
            }, item =>
            {
                Destroy(item.gameObject);
            });
        }

        private void OnDestroy()
        {
            _itemPool.Dispose();   
        }

        private void Update()
        {
            if (_items == _previousItems)
                return;

            _previousItems = _items;
            var target = _items - _activeItems.Count;
            var dir = target > 0;
            if (target != 0)
            {
                for (int i = 0; i < Mathf.Abs(target); i++)
                {
                    if (dir)
                        _activeItems.Add(_itemPool.Get());
                    else
                    {
                        var item = _activeItems[^1];
                        _activeItems.Remove(item);
                        _itemPool.Release(item);
                    }
                }
            }

            var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));
            var radiusModifier = Mathf.Pow(_items, 1f / 3f);

            for (int i = 0; i < _items; i++)
            {
                var y = 1f - i / (_items - 1f) * 2f;
                var radius = Mathf.Sqrt(1f - y * y);

                radius *= radiusModifier;
                
                var theta = phi * i;
                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;

                if (float.IsNaN(x))
                    break;
                
                _activeItems[i].transform.localPosition = new Vector3(x, y, z);
            }
        }
    }
}