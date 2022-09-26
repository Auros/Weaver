using System;
using UnityEngine;
using UnityEngine.Windows;
using Weaver.UI.Navigation;
using Weaver.Utilities;

namespace Weaver.UI.Components
{
    public class SelectRepositoryPath : MonoBehaviour
    {
        [SerializeField]
        private WeaverSetupUI _weaverSetupUI = null!;

        [SerializeField]
        private NavigationController _navigationController = null!;

        [SerializeField]
        private string _navigateAfterSelection = string.Empty;
        
        public void SelectFolder()
        {
            FolderPicker picker = new()
            {
                InputPath = @"c:\windows\system32"
            };
            
            if (picker.ShowDialog(IntPtr.Zero) != true)
                return;

            if (!Directory.Exists(picker.ResultPath))
                return;
            
            _weaverSetupUI.SetRepository(picker.ResultPath!);
            _navigationController.NavigateTo(_navigateAfterSelection);
        }
    }
}