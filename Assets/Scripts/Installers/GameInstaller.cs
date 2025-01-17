using System;
using Combat.Domain;
using Combat.Scene;
using Constructor.Ships;
using Domain.Player;
using Domain.Quests;
using Economy;
using Economy.ItemType;
using Economy.Products;
using Galaxy;
using Galaxy.StarContent;
using Game;
using Game.Exploration;
using GameModel.Quests;
using GameServices;
using GameServices.Database;
using GameServices.Economy;
using GameServices.GameManager;
using GameServices.Gui;
using GameServices.Multiplayer;
using GameServices.Player;
using GameServices.Quests;
using GameServices.Random;
using GameServices.Research;
using GameServices.Settings;
using GameStateMachine;
using GameStateMachine.States;
using Services.Advertisements;
using Services.Gui;
using Services.InAppPurchasing;
using Services.Input;
using Services.InternetTime;
using Services.Messenger;
using Session;
using Session.Content;
using UnityEngine;
using Zenject;
using PlayerInventory = GameServices.Player.PlayerInventory;

namespace Installers
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] GameModel.Config _config;

        public override void InstallBindings()
        {
            Container.BindAllInterfaces<RandomGenerator>().To<RandomGenerator>().AsSingle();

#if LICENSE_OPENSOURCE || IAP_DISABLED
            Container.BindAllInterfaces<InAppPurchasingStub>().To<InAppPurchasingStub>().AsSingle();
#elif IAP_UNITY
            Container.BindAllInterfaces<InAppPurchasing>().To<InAppPurchasing>().AsSingle();
#endif

            Container.Bind<IapPurchaseProcessor>().To<IapPurchaseProcessor>().AsSingle();

            Container.Bind<GameModel.Config>().FromInstance(_config);

            Container.Bind<IGameDataManager>().To<GameDataManager>().FromGameObject().AsSingle().NonLazy();

            Container.BindAllInterfacesAndSelf<StarMap>().To<StarMap>().AsSingle();

            Container.BindAllInterfacesAndSelf<Research>().To<Research>().AsSingle();

            Container.Bind<OfflineMultiplayer>().To<OfflineMultiplayer>().AsSingle().NonLazy();

            Container.Bind<ItemTypeFactory>();
            Container.Bind<ProductFactory>();
            Container.Bind<LootGenerator>();
            Container.Bind<ModificationFactory>();

            Container.BindFactory<CombatModelBuilder, CombatModelBuilder.Factory>();
            Container.BindSignal<ShipCreatedSignal>();
            Container.BindTrigger<ShipCreatedSignal.Trigger>();
            Container.BindSignal<ShipDestroyedSignal>();
            Container.BindTrigger<ShipDestroyedSignal.Trigger>();

            Container.Bind<Cheats>();
            Container.Bind<DatabaseCodesProcessor>();

            Container.Bind<GuiHelper>();
            Container.Bind<HolidayManager>().AsSingle();
            Container.Bind<NotificationManager>().AsSingle().NonLazy();
            Container.BindAllInterfacesAndSelf<GameTime>().To<GameTime>().AsSingle().NonLazy();

            Container.BindAllInterfaces<Technologies>().To<Technologies>().AsSingle();
            Container.Bind<Skills>().AsSingle();
            Container.Bind<InventoryFactory>().AsSingle();
            Container.Bind<Planet.Factory>().AsCached();

            Container.BindAllInterfaces<SignalsTranslator>().To<SignalsTranslator>().AsSingle().NonLazy();

            BindPlayerData();
            BindQuestManager();
            BindStarContent();
            BindDatabase();
            BindStateMachine();
            BindLegacyServices();
            BindSignals();
        }

        private void BindPlayerData()
        {
            Container.BindAllInterfacesAndSelf<PlayerSkills>().To<PlayerSkills>().AsSingle();
            Container.BindAllInterfacesAndSelf<PlayerFleet>().To<PlayerFleet>().AsSingle();
            Container.BindAllInterfacesAndSelf<PlayerResources>().To<PlayerResources>().AsSingle();
            Container.BindAllInterfacesAndSelf<MotherShip>().To<MotherShip>().AsSingle();
            Container.BindAllInterfacesAndSelf<PlayerInventory>().To<PlayerInventory>().AsSingle();
            Container.BindAllInterfacesAndSelf<SupplyShip>().To<SupplyShip>().AsSingle().NonLazy();
            Container.BindAllInterfacesAndSelf<DailyReward>().To<DailyReward>().AsSingle().NonLazy();
            Container.BindAllInterfacesAndSelf<StarMapManager>().To<StarMapManager>().AsSingle().NonLazy();
        }

        private void BindQuestManager()
        {
            Container.BindAllInterfaces<QuestManagerContext>().To<QuestManagerContext>().AsSingle();
            Container.BindAllInterfaces<QuestBuilderContext>().To<QuestBuilderContext>().AsSingle();
            Container.BindAllInterfaces<QuestManagerEventProvider>().To<QuestManagerEventProvider>().AsSingle();
            Container.BindAllInterfaces<QuestDataProvider>().To<QuestDataProvider>().AsSingle();
            Container.BindAllInterfaces<StarMapDataProvider>().To<StarMapDataProvider>().AsSingle();
            Container.BindAllInterfaces<GameDataProvider>().To<GameDataProvider>().AsSingle();
            Container.BindAllInterfaces<CharacterDataProvider>().To<CharacterDataProvider>().AsSingle();
            Container.BindAllInterfaces<InventoryDataProvider>().To<InventoryDataProvider>().AsSingle();
            Container.BindAllInterfaces<LootItemFactory>().To<LootItemFactory>().AsSingle();

            Container.Bind<QuestCombatModelFacctory>().AsSingle();
            Container.Bind<QuestFactory>().AsSingle();
            Container.Bind<RequirementsFactory>().AsSingle();

            Container.BindAllInterfaces<QuestManager>().To<QuestManager>().AsSingle().NonLazy();

            Container.BindSignal<QuestListChangedSignal>();
            Container.BindTrigger<QuestListChangedSignal.Trigger>();
            Container.BindSignal<QuestActionRequiredSignal>();
            Container.BindTrigger<QuestActionRequiredSignal.Trigger>();
            Container.BindSignal<QuestEventSignal>();
            Container.BindTrigger<QuestEventSignal.Trigger>();

            //Container.Bind<CharacterFactory>();
            //Container.Bind<ConditionFactory>();
            //Container.Bind<QuestBuilderFactory>();
            //Container.Bind<NodeFactory>();
        }

        private void BindStarContent()
        {
            Container.BindAllInterfacesAndSelf<StarData>().To<StarData>().AsSingle();
            Container.Bind<Occupants>().AsSingle();
            Container.Bind<Boss>().AsSingle();
            Container.Bind<Ruins>().AsSingle();
            Container.Bind<Challenge>().AsSingle();
            Container.Bind<LocalEvent>().AsSingle();
            Container.Bind<Survival>().AsSingle();
            Container.Bind<Wormhole>().AsSingle();
            Container.Bind<StarBase>().AsSingle();
            Container.Bind<XmasTree>().AsSingle();
            Container.Bind<Hive>().AsSingle();
        }

        private void BindDatabase()
        {
#if EDITOR_MODE
            Container.BindAllInterfacesAndSelf<SessionDataStub>().To<SessionDataStub>().AsSingle();
#else
            Container.BindAllInterfacesAndSelf<SessionData>().To<SessionData>().AsSingle();
#endif
            Container.Bind<ContentFactory>();

            Container.BindSignal<SessionCreatedSignal>();
            Container.BindTrigger<SessionCreatedSignal.Trigger>();
            Container.BindSignal<SessionDataLoadedSignal>();
            Container.BindTrigger<SessionDataLoadedSignal.Trigger>();

            Container.BindSignal<PlayerPositionChangedSignal>();
            Container.BindTrigger<PlayerPositionChangedSignal.Trigger>();
            Container.BindSignal<NewStarSecuredSignal>();
            Container.BindTrigger<NewStarSecuredSignal.Trigger>();
            Container.BindSignal<MoneyValueChangedSignal>();
            Container.BindTrigger<MoneyValueChangedSignal.Trigger>();
            Container.BindSignal<FuelValueChangedSignal>();
            Container.BindTrigger<FuelValueChangedSignal.Trigger>();
            Container.BindSignal<StarsValueChangedSignal>();
            Container.BindTrigger<StarsValueChangedSignal.Trigger>();
            Container.BindSignal<PlayerSkillsResetSignal>();
            Container.BindTrigger<PlayerSkillsResetSignal.Trigger>();
            Container.BindSignal<TokensValueChangedSignal>();
            Container.BindTrigger<TokensValueChangedSignal.Trigger>();
            Container.BindSignal<ResourcesChangedSignal>();
            Container.BindTrigger<ResourcesChangedSignal.Trigger>();
        }

        private void BindStateMachine()
        {
            Container.BindAllInterfaces<StateMachine>().To<StateMachine>().AsSingle().NonLazy();
            Container.Bind<GameStateFactory>();

            Container.Bind<TravelState>();
            Container.BindFactory<int, TravelState, TravelState.Factory>();

			Container.Bind<RetreatState>();
			Container.BindFactory<RetreatState, RetreatState.Factory>();

            Container.Bind<InitializationState>();
            Container.BindFactory<InitializationState, InitializationState.Factory>();

            Container.Bind<EditorInitializationState>();
            Container.BindFactory<EditorInitializationState, EditorInitializationState.Factory>();

            Container.Bind<MainMenuState>();
            Container.BindFactory<MainMenuState, MainMenuState.Factory>();

            Container.Bind<StarMapState>();
            Container.BindFactory<StarMapState, StarMapState.Factory>();

            Container.Bind<StartingNewGameState>();
            Container.BindFactory<StartingNewGameState, StartingNewGameState.Factory>();
            
            Container.Bind<QuestState>();
			Container.BindFactory<IUserInteraction, QuestState, QuestState.Factory>();

            Container.Bind<SkillTreeState>();
            Container.BindFactory<SkillTreeState, SkillTreeState.Factory>();

            Container.Bind<ConstructorState>();
            Container.BindFactory<IShip, ConstructorState, ConstructorState.Factory>();

            Container.Bind<DialogState>();
            Container.BindFactory<string, WindowArgs, Action<WindowExitCode>, DialogState, DialogState.Factory>();

            Container.Bind<TestingState>();
            Container.BindFactory<TestingState, TestingState.Factory>();

            Container.Bind<CombatState>();
            Container.BindFactory<ICombatModel, Action<ICombatModel>, CombatState, CombatState.Factory>();

            Container.Bind<ExplorationState>();
            Container.BindFactory<Planet, ExplorationState, ExplorationState.Factory>();

            Container.Bind<EhopediaState>();
            Container.BindFactory<EhopediaState, EhopediaState.Factory>();

            Container.Bind<CombatRewardState>();
            Container.BindFactory<IReward, CombatRewardState, CombatRewardState.Factory>();

            Container.Bind<DailyRewardState>();
            Container.BindFactory<DailyRewardState, DailyRewardState.Factory>();

            Container.Bind<AnnouncementState>();
            Container.BindFactory<AnnouncementState, AnnouncementState.Factory>();

            Container.BindSignal<GameStateChangedSignal>();
            Container.BindTrigger<GameStateChangedSignal.Trigger>();
            Container.BindSignal<StartGameSignal>();
            Container.BindTrigger<StartGameSignal.Trigger>();
            Container.BindSignal<StartTravelSignal>();
            Container.BindTrigger<StartTravelSignal.Trigger>();
			Container.BindSignal<RetreatSignal>();
			Container.BindTrigger<RetreatSignal.Trigger>();
            Container.BindSignal<StartBattleSignal>();
            Container.BindTrigger<StartBattleSignal.Trigger>();
            Container.BindSignal<StartQuickBattleSignal>();
            Container.BindTrigger<StartQuickBattleSignal.Trigger>();
            Container.BindSignal<ExitSignal>();
            Container.BindTrigger<ExitSignal.Trigger>();
            Container.BindSignal<OpenSkillTreeSignal>();
            Container.BindTrigger<OpenSkillTreeSignal.Trigger>();
            Container.BindSignal<OpenConstructorSignal>();
            Container.BindTrigger<OpenConstructorSignal.Trigger>();
            Container.BindSignal<OpenShopSignal>();
            Container.BindTrigger<OpenShopSignal.Trigger>();
            Container.BindSignal<OpenWorkshopSignal>();
            Container.BindTrigger<OpenWorkshopSignal.Trigger>();
            Container.BindSignal<OpenEhopediaSignal>();
            Container.BindTrigger<OpenEhopediaSignal.Trigger>();
            Container.BindSignal<ConfigureControlsSignal>();
            Container.BindTrigger<ConfigureControlsSignal.Trigger>();
            Container.BindSignal<ShipSelectedSignal>();
            Container.BindTrigger<ShipSelectedSignal.Trigger>();
            Container.BindSignal<CombatCompletedSignal>();
            Container.BindTrigger<CombatCompletedSignal.Trigger>();
            Container.BindSignal<OpenShipyardSignal>();
            Container.BindTrigger<OpenShipyardSignal.Trigger>();
            Container.BindSignal<StartExplorationSignal>();
            Container.BindTrigger<StartExplorationSignal.Trigger>();
            Container.BindSignal<SupplyShipActivatedSignal>();
            Container.BindTrigger<SupplyShipActivatedSignal.Trigger>();
        }

        private void BindSignals()
        {
            Container.BindSignal<InAppPurchaseCompletedSignal>();
            Container.BindTrigger<InAppPurchaseCompletedSignal.Trigger>();
            Container.BindSignal<InAppPurchaseFailedSignal>();
            Container.BindTrigger<InAppPurchaseFailedSignal.Trigger>();
            Container.BindSignal<SupporterPackPurchasedSignal>();
            Container.BindTrigger<SupporterPackPurchasedSignal.Trigger>();
            Container.BindSignal<InAppItemListUpdatedSignal>();
            Container.BindTrigger<InAppItemListUpdatedSignal.Trigger>();
            Container.BindSignal<SessionAboutToSaveSignal>();
            Container.BindTrigger<SessionAboutToSaveSignal.Trigger>();
            Container.BindSignal<MultiplayerStatusChangedSignal>();
            Container.BindTrigger<MultiplayerStatusChangedSignal.Trigger>();
            Container.BindSignal<EnemyFleetLoadedSignal>();
            Container.BindTrigger<EnemyFleetLoadedSignal.Trigger>();
            Container.BindSignal<EnemyFoundSignal>();
            Container.BindTrigger<EnemyFoundSignal.Trigger>();
            Container.BindSignal<DailyRewardAwailableSignal>();
            Container.BindTrigger<DailyRewardAwailableSignal.Trigger>();
            Container.BindSignal<GameModel.BaseCapturedSignal>();
            Container.BindTrigger<GameModel.BaseCapturedSignal.Trigger>();
            Container.BindSignal<GameModel.RegionFleetDefeatedSignal>();
            Container.BindTrigger<GameModel.RegionFleetDefeatedSignal.Trigger>();
            Container.BindSignal<StarContentChangedSignal>();
            Container.BindTrigger<StarContentChangedSignal.Trigger>();
            Container.BindSignal<KeyBindingsChangedSignal>();
            Container.BindTrigger<KeyBindingsChangedSignal.Trigger>();
            Container.BindSignal<MouseEnabledSignal>();
            Container.BindTrigger<MouseEnabledSignal.Trigger>();
        }

        private void BindLegacyServices()
        {
            Container.BindAllInterfacesAndSelf<GameModel.RegionMap>().To<GameModel.RegionMap>().AsSingle();
        }
    }
}
