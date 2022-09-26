using UnityEngine;
using Weaver.UI.Navigation;
using Weaver.Utilities;

namespace Weaver.UI.Components
{
    public class StartWeaver : MonoBehaviour
    {
        [SerializeField]
        private WeaverSetupUI _weaverSetupUI = null!;
        
        [SerializeField]
        private NavigationController _navigationController = null!;

        [SerializeField]
        private AutomaticRepositoryAssigner _automaticRepositoryAssigner = null!;

        public void Run()
        {
            _navigationController.Hide();
            _automaticRepositoryAssigner.RepositoryPath = _weaverSetupUI.Repository;
        }
    }
}