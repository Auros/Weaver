using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SmartImage.Sources;
using UnityEngine;
using UnityEngine.Networking;

namespace Weaver
{
    public class AuthorIconRequestSource : MonoSourceStreamBuilder
    {
        private readonly List<string> _files = new();

        private void Awake()
        {
            _files.AddRange(Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Icons")));
        }

        public override bool IsSourceValid(string source)
        {
            foreach (var file in _files)
                if (Path.GetFileNameWithoutExtension(file) == source)
                    return true;
            return false;
        }

        public override async UniTask<Stream?> GetStreamAsync(string source, CancellationToken token = default)
        {
            source = _files.First(f => Path.GetFileNameWithoutExtension(f) == source);
            
            await UniTask.SwitchToMainThread();
            using var req = await UnityWebRequest.Get(source).SendWebRequest().WithCancellation(token);
            var bytes = req.downloadHandler.data;
            await UniTask.SwitchToThreadPool();
            return new MemoryStream(bytes);
        }
    }
}