using System;
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

        private readonly List<MonolithAction> _actions = new();

        private void Update()
        {
            var actionsToRemove = ListPool<MonolithAction>.Get();

            // Activate the enqueued actions.
            foreach (var action in _actions)
            {
                // We are going to remove this action regardless of what happens next
                actionsToRemove.Add(action);
                
                // Skip over invalid actions
                if (!action.IsValid())
                    continue;

                var physicalItem = action.PhysicalNode.ActiveItems.FirstOrDefault(i => i.Path == action.Item.Name);
                
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                // We're using the switch statement just because it looks more clear in this scenario
                // We don't want to do anything if any one of these four conditions are not met.
                switch (action.Type)
                {
                    case MonolithActionType.Created when physicalItem == null:
                        action.PhysicalNode.AddItem(action.Item);
                        break;
                    case MonolithActionType.Changed when physicalItem != null:
                        physicalItem.RunHeartbeat();
                        break;
                    case MonolithActionType.Destroyed:
                        action.PhysicalNode.RemoveItem(action.Item);
                        break;
                    case MonolithActionType.None:
                        throw new Exception("Invalid Monolith Action"); /* This should never happen! */
                }
            }

            foreach (var action in actionsToRemove)
            {
                _actions.Remove(action);
                _actionPool.Release(action);
            }
            
            ListPool<MonolithAction>.Release(actionsToRemove);
        }

        public void AddAction(MonolithNode node, WeaverItem item, MonolithActionType type)
        {
            // Cancel the action in the queue if necessary, if not, spawn in a new one.
            var action = _actions.FirstOrDefault(a => a.Item == item);
            if (action is not null)
                _actions.Remove(action);
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