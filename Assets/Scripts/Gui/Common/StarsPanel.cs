﻿using Economy;
using GameServices.Player;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Common
{
    public class StarsPanel : MonoBehaviour
    {
        [SerializeField] private Text _starsText;

        [Inject]
        private void Initialize(IMessenger messenger, PlayerResources playerResources)
        {
#if IAP_DISABLED
            gameObject.SetActive(false);
#else
            messenger.AddListener<int>(EventType.StarsValueChanged, SetValue);
            SetValue(playerResources.Stars);
#endif
        }

        private void SetValue(int value)
        {
            if (_stars == value) return;
            _stars = value;
            _starsText.text = _stars.ToString("N0");
        }

        private int _stars = -1;
    }
}
