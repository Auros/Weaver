using System;
using Object = UnityEngine.Object;

namespace Weaver.Tweening
{
    public class TweenContext
    {
        public Object Owner { get; set; } = null!;
        public float CurrentTime { get; set; }
        public float Duration { get; set; }

        public Action<float>? ValueSetter { get; set; }
        public Action? Completion { get; set; }
    }
}