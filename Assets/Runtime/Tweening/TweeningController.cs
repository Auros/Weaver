using System;
using System.Collections.Generic;
using ElRaccoone.Tweens.Core;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Object = UnityEngine.Object;

namespace Weaver.Tweening
{
    /// <summary>
    /// I wrote this for the high performance tweening operation used to make Weaver look the way it is.
    /// The library that I was using isn't very performant in it's design, since it adds then removes a component
    /// for each tween. This TweeningController only supports floats and Vector3's as that's the only tweening
    /// types Weaver needs at the moment. In the future, I plan on writing my own tweening library and putting
    /// it on OpenUPM, but for the time being, this will do.
    /// </summary>
    public class TweeningController : MonoBehaviour
    {
        [Inject]
        private readonly IObjectPool<TweenContext> _contextPool = null!;

        private readonly List<TweenContext> _tweenContexts = new();

        public void AddTween(float startingValue, float endingValue, Action<float>? changed,
            float duration, EaseType easeType, Object owner, Action? onComplete = null)
        {
            var ctx = _contextPool.Get();
            ctx.Completion = onComplete;
            ctx.Duration = duration;
            ctx.CurrentTime = 0f;
            ctx.Owner = owner;
            ctx.ValueSetter = v =>
            {
                var value = Mathf.Lerp(startingValue, endingValue, Easer.Apply(easeType, v));
                changed?.Invoke(value);
            };
            _tweenContexts.Add(ctx);
        }

        public void AddTween(Vector3 startingValue, Vector3 endingValue, Action<Vector3>? changed,
            float duration, EaseType easeType, Object owner, Action? onComplete = null)
        {
            var ctx = _contextPool.Get();
            ctx.Completion = onComplete;
            ctx.Duration = duration;
            ctx.CurrentTime = 0f;
            ctx.Owner = owner;
            ctx.ValueSetter = v =>
            {
                var value = Vector3.Lerp(startingValue, endingValue, Easer.Apply(easeType, v));
                changed?.Invoke(value);
            };
            _tweenContexts.Add(ctx);
        }

        public void Clear(Object owner)
        {
            var toRemove = ListPool<TweenContext>.Get();
            for (int i = 0; i < _tweenContexts.Count; i++)
            {
                var ctx = _tweenContexts[i];
                if (ctx.Owner != owner)
                    continue;
                toRemove.Add(ctx);
            }
            ListPool<TweenContext>.Release(toRemove);
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var toRemove = ListPool<TweenContext>.Get();
            for (int i = 0; i < _tweenContexts.Count; i++)
            {
                var ctx = _tweenContexts[i];
                if (ctx.Owner == null)
                {
                    toRemove.Add(ctx);
                    continue;
                }
                if (ctx.CurrentTime >= ctx.Duration)
                {
                    ctx.ValueSetter?.Invoke(1f);
                    ctx.Completion?.Invoke();
                    toRemove.Add(ctx);
                    continue;
                }
                ctx.ValueSetter?.Invoke(ctx.CurrentTime / ctx.Duration);
                ctx.CurrentTime += deltaTime;
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                var ctx = toRemove[i];
                _tweenContexts.Remove(ctx);
                _contextPool.Release(ctx);
            }
            ListPool<TweenContext>.Release(toRemove);
        }
    }
}