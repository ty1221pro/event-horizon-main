﻿using System;
using DataModel.Technology;
using Economy.Products;
using GameServices.Player;
using GameServices.Random;
using GameServices.Research;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Gui.Craft
{
    public class CraftPanel : MonoBehaviour
    {
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly PlayerResources _resources;
        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly Research _research;
        [Inject] private readonly Session.ISessionData _session;

        [SerializeField] private ItemCreatedEvent _itemCreatedEvent = new ItemCreatedEvent();

        [SerializeField] private CraftItemQuality _itemQuality;
        [SerializeField] private Button _createButton;
        [SerializeField] private CraftPricePanel _craftPricePanel;

        [SerializeField] private Text _levelText;
        [SerializeField] private Color _enoughColor;
        [SerializeField] private Color _notEnoughColor;
        [SerializeField] private GameObject _notAvailablePanel;

        [Serializable]
        public class ItemCreatedEvent : UnityEvent<IProduct> {}

        public void Initialize(ITechnology tech, int level)
        {
            if (_itemQuality != CraftItemQuality.Common && tech is SatelliteTechnology)
            {
                Cleanup();
                _notAvailablePanel.gameObject.SetActive(true);
                return;
            }

            WorkshopLevel = level;
            _technology = tech;

            var price = tech.GetCraftPrice(_itemQuality)*_playerSkills.CraftingPriceScale;

            var requiredLevel = RequiredLevel;

            _notAvailablePanel.gameObject.SetActive(false);

            _levelText.text = requiredLevel.ToString();
            _levelText.color = requiredLevel <= level ? _enoughColor : _notEnoughColor;

            _craftPricePanel.gameObject.SetActive(true);
            _craftPricePanel.Initialize(price, tech.Faction);

            _createButton.interactable = requiredLevel <= level && _craftPricePanel.HaveEnoughResources;
        }

        public void Cleanup()
        {
            _technology = null;
            _craftPricePanel.gameObject.SetActive(false);
            _levelText.color = _enoughColor;
            _levelText.text = "0";
            _createButton.interactable = false;
            _notAvailablePanel.gameObject.SetActive(false);
        }

        public void CreateButtonClicked()
        {
            if (!TryConsumeResources())
                return;

            var seed = _session.Game.Seed + _session.Game.Counter + _resources.Money + _motherShip.CurrentStar.Id;
            var item = _technology.CreateItem(_itemQuality, new System.Random(seed));
            item.Consume(1);
            _itemCreatedEvent.Invoke(item);
        }

        private bool TryConsumeResources()
        {
            if (RequiredLevel > WorkshopLevel)
                return false;

            var price = _technology.GetCraftPrice(_itemQuality)*_playerSkills.CraftingPriceScale;
            if (price.Credits > _resources.Money || price.Stars > _resources.Stars)
                return false;
            if (price.Techs > 0 && _research.GetAvailablePoints(_technology.Faction) < price.Techs)
                return false;

            _resources.Money -= price.Credits;
            _resources.Stars -= price.Stars;
            _research.AddResearchPoints(_technology.Faction, -price.Techs);

            return true;
        }

        private int WorkshopLevel { get; set; }
        private int RequiredLevel { get { return Math.Max(0, _itemQuality.GetWorkshopLevel(_technology.GetWorkshopLevel()) + _playerSkills.CraftingLevelModifier); } }

        private ITechnology _technology;
    }
}
