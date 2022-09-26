using TMPro;
using UnityEngine;

namespace Weaver.UI
{
    public class WeaverSetupUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _repositoryText = null!;

        public string Repository { get; set; } = string.Empty;

        public void SetRepository(string path)
        {
            _repositoryText.text = path;
            Repository = path;
        }
    }
}