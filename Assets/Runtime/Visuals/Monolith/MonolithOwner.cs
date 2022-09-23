using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Weaver.Models;

namespace Weaver.Visuals.Monolith
{
    public class MonolithOwner : MonoBehaviour
    {
        [Inject]
        private readonly IObjectPool<MonolithAction> _actionPool = null!;

        [SerializeField]
        private AnimationCurve _actionCurve = null!;

        private int _actionCountLastFrame;
        private float _timeUntilNextAction;
        private readonly List<MonolithAction> _actions = new();

        private void Update()
        {
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
            
            // If there are no actions to perform, skip.
            if (actionCount == 0)
                return;
            
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
                action.PhysicalNode.AddItem(action.Item).RunHeartbeat();
            }
            else if (action.Type == MonolithActionType.Changed && physicalItem != null)
            {
                physicalItem.RunHeartbeat();
            }
            else if (action.Type == MonolithActionType.Destroyed)
            {
                action.PhysicalNode.RemoveItem(action.Item);
            }
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
    }
}