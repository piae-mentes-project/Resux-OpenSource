using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Toggle))]
    public class ToggleAnim : MonoBehaviour
    {
        private Toggle _toggle;
        private Animator _animator;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _animator = GetComponent<Animator>();
            _animator.SetBool("Selecting", _toggle.isOn);
            _toggle.onValueChanged.AddListener(v =>
            {
                _animator.SetBool("Selecting", v);
            });
        }
    }
}
