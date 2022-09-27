using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElRaccoone.Tweens.Core;
using SmartImage;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Weaver.Models;
using Weaver.Tweening;
using Random = UnityEngine.Random;

namespace Weaver.Visuals.Monolith
{
    public sealed class MonolithOwner : MonoBehaviour
    {
        [Inject]
        private readonly SmartImageManager _smartImageManager = null!;
        
        [Inject]
        private readonly TweeningController _tweeningController = null!;
        
        [Inject]
        private readonly IObjectPool<MonolithAction> _actionPool = null!;

        [Inject]
        private readonly MonolithLaserPoolController _laserPoolController = null!;

        [SerializeField]
        private ParticleSystem _particleSystem = null!;

        [SerializeField]
        private AnimationCurve _actionCurve = null!;

        [SerializeField]
        private float _idealDistanceFromNode = 5f;

        [SerializeField]
        private float _timeToReachTarget = 1f;

        [SerializeField]
        private EaseType _movementEasing = EaseType.CubicOut;

        [SerializeField]
        private float _inactivityTimeUntilDeactivation = 5f;

        [SerializeField]
        private Color _itemAddedColor = Color.green;

        [SerializeField]
        private Color _itemChangedColor = Color.gray;

        [SerializeField]
        private Color _itemDeletedColor = Color.red;

        [SerializeField]
        private float _timeForSpawn = 1f;

        [SerializeField]
        private EaseType _spawnEasing = EaseType.Linear;

        [SerializeField]
        private Vector3 _spawnScaleTarget = Vector3.one;

        [SerializeField]
        private Vector3 _despawnScaleTarget = Vector3.zero;

        [SerializeField]
        private float _postDeactivationLifetime = 2f;

        [SerializeField]
        private Renderer _renderer = null!;
        
        private bool _deactivating;
        private System.Random? _random;
        private float _timeSpentInactive;
        private int _actionCountLastFrame;
        private float _timeUntilNextAction;
        private float _timeSinceDeactivation;
        private MonolithNode? _currentlyMovingTo;
        private Action<MonolithOwner>? _onDeactivation;
        private readonly List<MonolithAction> _actions = new();
        private readonly List<MonolithLaser> _activeLasers = new();

        private CancellationTokenSource? _cts = new();
        private static readonly int _textureId = Shader.PropertyToID("_BaseMap");
        private static readonly int _emissionMapId = Shader.PropertyToID("_EmissionMap");

        private static readonly ImageLoadingOptions _imageOptions = new();
        
        [field: SerializeField]
        public string Id { get; private set; } = string.Empty;

        private void Start()
        {
            TweenIn();
        }

        private void OnEnable()
        {
            if (_tweeningController == null)
                return;
            
            TweenIn();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void TweenIn()
        {
            _tweeningController.AddTween(
                transform.localScale,
                _spawnScaleTarget,
                vector => transform.localScale = vector,
                _timeForSpawn,
                _spawnEasing,
                this
            );
        }

        private void Update()
        {
            if (_deactivating)
            {
                _timeSinceDeactivation += Time.deltaTime;

                if (_timeSinceDeactivation > _postDeactivationLifetime)
                {
                    Deactivate();
                    _deactivating = false;
                }
            }
            
            var actionCount = _actions.Count;
            
            // If the number of actions has recently grown, recalculate the time until next action.
            // We want to avoid having the action list getting clogged up because it's waiting the amount of time
            // expected of it within the past.
            if (actionCount > _actionCountLastFrame)
                _timeUntilNextAction = _actionCurve.Evaluate(actionCount);
            
            _actionCountLastFrame = actionCount;
            
            // Subtract the frame time every frame.
            _timeUntilNextAction -= Time.deltaTime;
            
            // If we're not allowed to make another action, skip.
            if (_timeUntilNextAction > 0)
                return;
            
            // If there are no actions to perform, start running inactivity behaviour.
            if (actionCount == 0)
            {
                // Exit early if we're already deactivating, we don't need to check if we're deactivating.
                if (_deactivating)
                    return;
                
                _timeSpentInactive += Time.deltaTime;
                
                // If we haven't hit our deactivity threshold, skip.
                if (_inactivityTimeUntilDeactivation > _timeSpentInactive)
                    return;
                
                // Start the deactivation process for this owner.
                
                _deactivating = true;
                _timeSpentInactive = 0;
                
                _tweeningController.AddTween(
                    transform.localScale,
                    _despawnScaleTarget,
                    vector => transform.localScale = vector,
                    _timeForSpawn,
                    _spawnEasing,
                    this,
                    () =>
                    {
                        _particleSystem.Stop();
                        _particleSystem.Play();
                    }
                );
                return;
            }

            if (_deactivating)
                TweenIn();
            
            _deactivating = false;
            _timeSpentInactive = 0;
            
            var action = _actions[0];
            _actions.Remove(action);
            
            // Skip over an invalid action
            if (!action.IsValid())
                return;

            // Calculate the amount of time until we're allowed to make an action again.
            _timeUntilNextAction = _actionCurve.Evaluate(actionCount);

            // Even when it's not executed, having a LINQ statement in an update method will cause
            // some garbage to be generated. I'd like to look more into this, as I'm not entirely
            // sure why. Maybe an issue with Mono or some JIT wizardry backfiring?
            MonolithItem? physicalItem = null;

            for (int i = 0; i < action.PhysicalNode.ActiveItems.Count; i++)
            {
                var localItem = action.PhysicalNode.ActiveItems[i];
                if (localItem.Path != action.Item.Name)
                    continue;
                physicalItem = localItem;
                break;
            }

            if (physicalItem != null)
                _ = true;
            
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (action.Type == MonolithActionType.Created && physicalItem == null)
            {
                physicalItem = action.PhysicalNode.AddItem(action.Item);
                physicalItem.RunHeartbeat();
                PerformActionVisualization(action, physicalItem.transform, _itemAddedColor);
            }
            else if (action.Type == MonolithActionType.Changed && physicalItem != null)
            {
                physicalItem.RunHeartbeat();
                PerformActionVisualization(action, physicalItem.transform, _itemChangedColor);
            }
            else if (action.Type == MonolithActionType.Destroyed)
            {
                action.PhysicalNode.RemoveItem(action.Item);
                PerformActionVisualization(action, action.PhysicalNode.transform, _itemDeletedColor);
            }
        }

        private void PerformActionVisualization(MonolithAction action, Transform on, Color color)
        {
            if (_currentlyMovingTo != action.PhysicalNode)
            {
                _currentlyMovingTo = action.PhysicalNode;
                
                // Move to the node
                // Generate a random point away from the center of the node,
                // add that to the node's position, then tween to that location.
                var point = Random.onUnitSphere * _idealDistanceFromNode;
                var targetPosition = action.PhysicalNode.transform.position + point;

                _tweeningController.AddTween(
                    transform.position,
                    targetPosition,
                    vector => transform.position = vector,
                    _timeToReachTarget,
                    _movementEasing,
                    this
                );
            }

            var laser = _laserPoolController.Get();
            _activeLasers.Add(laser);
            laser.Flash(transform, on, color, LaserFinished);
        }

        private void LaserFinished(MonolithLaser laser)
        {
            _activeLasers.Remove(laser);
            _laserPoolController.Release(laser);
        }

        private void Deactivate()
        {
            _onDeactivation?.Invoke(this);
        }

        public void AddAction(MonolithNode node, WeaverItem item, MonolithActionType type)
        {
            // Cancel the action in the queue if necessary, if not, spawn in a new one.
            var action = _actions.FirstOrDefault(a => a.Item.Name == item.Name);
            if (action is not null)
            {
                // If the added action is a change event right and it's creation event hasn't occured,
                // just chill! We want to ensure that the owner who created the file handles it.
                if (action.Type == MonolithActionType.Created && type == MonolithActionType.Changed)
                    return;
                
                _actions.Remove(action);
            }
            else
                action = _actionPool.Get();
            
            action.Type = type;
            action.PhysicalNode = node;
            action.Item = item;
            _actions.Add(action);
        }

        public void ClearActionsForNode(MonolithNode node)
        {
            var actionsToRemove = ListPool<MonolithAction>.Get();
            foreach (var action in _actions)
                if (action.PhysicalNode == node)
                    actionsToRemove.Add(action);

            foreach (var action in actionsToRemove)
            {
                _actions.Remove(action);
                _actionPool.Release(action);
            }
            ListPool<MonolithAction>.Release(actionsToRemove);
        }

        public void ClearActionsForItem(WeaverItem item)
        {
            var actionsToRemove = ListPool<MonolithAction>.Get();
            foreach (var action in _actions)
                if (action.Item == item)
                    actionsToRemove.Add(action);

            foreach (var action in actionsToRemove)
            {
                _actions.Remove(action);
                _actionPool.Release(action);
            }
            ListPool<MonolithAction>.Release(actionsToRemove);
        }

        public void SetData(string id, System.Random random, Action<MonolithOwner>? onDeactivation)
        {
            var previousId = Id;
            
            Id = id;
            _random = random;
            _onDeactivation = onDeactivation;

            // We don't need to reload the image if the id hasn't changed.
            if (previousId == Id)
                return;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            _renderer.material.SetTexture(_textureId, null);
            _renderer.material.SetTexture(_emissionMapId, null);
            UniTask.Void(async () =>
            {
                var sprite = await _smartImageManager.LoadAsync(Id, _imageOptions, _cts.Token);
                if (sprite == null)
                    return;
                
                _renderer.material.SetTexture(_textureId, sprite.Active.Texture);
                _renderer.material.SetTexture(_emissionMapId, sprite.Active.Texture);
            });
        }
    }
}