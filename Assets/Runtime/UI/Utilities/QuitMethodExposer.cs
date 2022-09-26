using JetBrains.Annotations;
using UnityEngine;

namespace Weaver
{
    /// <summary>
    /// Exposes the Application.Quit method publicly so it can be used as a Unity event
    /// </summary>
    public sealed class QuitMethodExposer : MonoBehaviour
    {
        [UsedImplicitly]
        public void Quit()
        {
            Application.Quit();
        }
    }
}