using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resux.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ExtendToggleGroup))]
    public class ToggleGroupAnim : MonoBehaviour
    {
        private ExtendToggleGroup _toggleGroup;
        private Animator _animator;

        private void Awake()
        {
            _toggleGroup = GetComponent<ExtendToggleGroup>();
            _animator = GetComponent<Animator>();
            _toggleGroup.onSelectChanged.AddListener(t =>
            {
                _animator.SetTrigger(t.name);
            });
        }
    }
}
