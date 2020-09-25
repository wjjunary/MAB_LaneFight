using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace LaneFight
{
    public class OnGateOpenBehavior : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            System.Diagnostics.Debug.WriteLine("OnOpenDoorBehavior RegisterEvents");
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.AfterMissionStarted));
        }
        public override void SyncData(IDataStore dataStore)
        {
            System.Diagnostics.Debug.WriteLine("OnOpenDoorBehavior SyncData");
        }

        // Methods
        public void AfterMissionStarted(IMission iMission)
        {
            Mission mission = (Mission)iMission;
            mission.AddMissionBehaviour(new LaneWarLogic());

        }

    }

}
