using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.ObjectModel;

namespace Resux.UI
{
    [DisallowMultipleComponent]
    public class ExtendToggleGroup : UIBehaviour
    {
        public class SelectChangedEvent : UnityEvent<Toggle> { }

        [SerializeField] private bool m_AllowSwitchOff = false;
        [SerializeField] private bool m_DontResetOnAwake = false;
        [SerializeField] protected List<Toggle> m_Toggles = new List<Toggle>();
        public readonly SelectChangedEvent onSelectChanged = new SelectChangedEvent();
        private Toggle selecingToggle = null;

        public bool allowSwitchOff
        {
            get => m_AllowSwitchOff;
            set => m_AllowSwitchOff = value;
        }
        public ReadOnlyCollection<Toggle> Toggles => m_Toggles.AsReadOnly();

        protected override void Awake()
        {
            foreach (var toggle in m_Toggles)
            {
                toggle.onValueChanged.AddListener(value => OnToggleValueChanged(toggle, value));
            }
            EnsureValidState();
            base.Awake();
        }

        private void ValidateToggleIsInGroup(Toggle toggle)
        {
            if (toggle == null || !m_Toggles.Contains(toggle))
            {
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[2]
                {
                    toggle,
                    this
                }));
            }
        }

        private void OnToggleValueChanged(Toggle toggle, bool value)
        {
            if (value)
            {
                foreach (var t in m_Toggles)
                {
                    if (t.isOn && t != toggle)
                    {
                        t.isOn = false;
                    }
                }
                if (selecingToggle == toggle) { return; }
                selecingToggle = toggle;
                onSelectChanged.Invoke(toggle);
            }
            else
            {
                var haveOtherTrue = false;
                foreach (var t in m_Toggles)
                {
                    if (t.isOn && t != toggle)
                    {
                        haveOtherTrue = true;
                        break;
                    }
                }
                if (!haveOtherTrue)
                {
                    toggle.isOn = true;
                    return;
                }
            }
        }

        public void Initialize(int index)
        {
            if (index < m_Toggles.Count)
            {
                m_Toggles[index].Select();
            }
        }

        public void NotifyToggleOn(Toggle toggle, bool sendCallback = true)
        {
            ValidateToggleIsInGroup(toggle);
            for (int i = 0; i < m_Toggles.Count; i++)
            {
                if (!(m_Toggles[i] == toggle))
                {
                    if (sendCallback)
                    {
                        m_Toggles[i].isOn = false;
                    }
                    else
                    {
                        m_Toggles[i].SetIsOnWithoutNotify(false);
                    }
                }
            }
        }

        public void UnregisterToggle(Toggle toggle)
        {
            if (m_Toggles.Contains(toggle))
            {
                m_Toggles.Remove(toggle);
                toggle.onValueChanged.RemoveListener(value => OnToggleValueChanged(toggle, value));
            }
        }

        public void RegisterToggle(Toggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
            {
                m_Toggles.Add(toggle);
                toggle.onValueChanged.AddListener(value => OnToggleValueChanged(toggle, value));
            }
        }

        public void EnsureValidState()
        {
            if (m_DontResetOnAwake)
            {
                return;
            }

            if (!allowSwitchOff && !AnyTogglesOn() && m_Toggles.Count != 0)
            {
                m_Toggles[0].isOn = true;
                NotifyToggleOn(m_Toggles[0]);
            }
        }

        public bool AnyTogglesOn()
        {
            return m_Toggles.Find((Toggle x) => x.isOn) != null;
        }

        public IEnumerable<Toggle> ActiveToggles()
        {
            return m_Toggles.Where((Toggle x) => x.isOn);
        }

        public void SetAllTogglesOff(bool sendCallback = true)
        {
            bool allowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;
            if (sendCallback)
            {
                for (int i = 0; i < m_Toggles.Count; i++)
                {
                    m_Toggles[i].isOn = false;
                }
            }
            else
            {
                for (int j = 0; j < m_Toggles.Count; j++)
                {
                    m_Toggles[j].SetIsOnWithoutNotify(value: false);
                }
            }

            m_AllowSwitchOff = allowSwitchOff;
        }
    }
}