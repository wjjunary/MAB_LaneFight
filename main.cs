using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LaneFight
{
    public class MySubModule : MBSubModuleBase
    {
        public override void OnCampaignStart(Game game, object starterObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = starterObject as CampaignGameStarter;
                this.AddBehaviors(gameInitializer);
            }
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            CampaignGameStarter gameInitializer = initializerObject as CampaignGameStarter;
            if (gameInitializer != null)
            {
                this.AddBehaviors(gameInitializer);
            }
        }

        public void AddBehaviors(CampaignGameStarter gameStarterObject)
        {
            gameStarterObject.AddBehavior(new OnGateOpenBehavior());

        }
        protected override void OnSubModuleLoad()
        {
            System.Diagnostics.Debug.WriteLine("加载mod1");
            //InformationManager.DisplayMessage(new InformationMessage(new TextObject("加载mod", null).ToString()));
        }

    }
}