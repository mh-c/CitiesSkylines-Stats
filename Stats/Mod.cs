﻿using Stats.Configuration;
using Stats.Localization;
using Stats.Ui;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using System.IO;
using UnityEngine;

namespace Stats
{
    public class Mod : ILoadingExtension, IUserMod
    {
        private ConfigurationService configurationService;
        private ConfigurationModel configuration;
        private LanguageResourceService languageResourceService;
        private LanguageResourceModel languageResource;
        private GameEngineService gameEngineService;
        private GameObject mainPanelGameObject;

        public string SystemName => "Stats";
        public string Name => "Stats";
        public string Description => "Adds a configurable panel to display all vital city stats at a glance.";
        public string Version => "1.0.1";
        public string WorkshopId => "1410077595";

        public void OnEnabled()
        {
            var configurationFileFullName = Path.Combine(DataLocation.localApplicationData, SystemName + ".xml");
            this.configurationService = new ConfigurationService(configurationFileFullName);
            if (File.Exists(configurationFileFullName))
            {
                var configurationDto = configurationService.Load();
                this.configuration = new ConfigurationModel(this.configurationService, configurationDto);
            }
            else
            {
                var configurationDto = new ConfigurationDto();
                this.configuration = new ConfigurationModel(this.configurationService, configurationDto);
            }

            this.languageResourceService = new LanguageResourceService(this.SystemName, this.WorkshopId);
            var localeManager = SingletonLite<LocaleManager>.instance;
            var languageResourceDto = this.languageResourceService.Load(localeManager.language);
            this.languageResource = new LanguageResourceModel(localeManager, languageResourceService, languageResourceDto);

            this.gameEngineService = new GameEngineService();
        }

        public void OnDisabled()
        {
            this.configurationService = null;
            this.configuration = null;
            this.languageResourceService = null;
            this.languageResource = null;
            this.gameEngineService = null;
        }

        public void OnCreated(ILoading loading)
        {
        }

        public void OnReleased()
        {
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            {
                return;
            }
            
            this.mainPanelGameObject = new GameObject(this.SystemName);
            var mainPanel = mainPanelGameObject.AddComponent<MainPanel>();
            var mapHasSnowDumps = this.gameEngineService.CheckIfMapHasSnowDumps();
            UIView.GetAView().AttachUIComponent(mainPanelGameObject);
            mainPanel.Initialize(this.SystemName, mapHasSnowDumps, this.configuration, this.languageResource);
            mainPanel.relativePosition = new Vector3(this.configuration.MainPanelPositionX, this.configuration.MainPanelPositionY);
        }

        public void OnLevelUnloading()
        {
            if (this.mainPanelGameObject == null)
                return;

            SaveMainPanelPosition();
            GameObject.Destroy(this.mainPanelGameObject);
        }

        private void SaveMainPanelPosition()
        {
            var mainPanel = this.mainPanelGameObject.GetComponent<MainPanel>();
            this.configuration.MainPanelPositionX = mainPanel.relativePosition.x;
            this.configuration.MainPanelPositionY = mainPanel.relativePosition.y;
            this.configuration.Save();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            ////so that the options ui is created with the correct language on language switch
            var localeManager = SingletonLite<LocaleManager>.instance;
            var languageResourceDto = this.languageResourceService.Load(localeManager.language);
            var languageResource = new LanguageResourceModel(localeManager, this.languageResourceService, languageResourceDto);

            var modFullTitle = new ModFullTitle(this.Name, this.Version);
            var configurationUiBuilder = new ConfigurationPanel(helper, modFullTitle, this.configuration, this.languageResource);
            configurationUiBuilder.Initialize();
        }

        //TODO: split item configuration from main panel 
        //TODO: reset position button
        //TODO: color picker
        //TODO: helicopters in use
        //TODO: disaster buses and helis in use
        //TODO: rebuild vehicles in use
        //TODO: add happiness values
        //TODO: maybe natural resources used
        //TODO: icons
        //TODO: performance
        //TODO: refactoring
        //TODO: move itempanel logic out of mainpanel
        //TODO: values per building type
        //TODO: values per district
    }
}
