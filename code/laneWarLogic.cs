using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LaneFight
{

    public class LaneWarLogic : MissionLogic
    {
        public bool innerGateIsDestroyed = false;
        public bool outerGateIsDestroyed = false;

        public Vec3 lastPosition;

        public override void AfterStart()
        {
            base.AfterStart();
            Mission mission = Mission.Current;
            foreach (Team team in mission.Teams)
            {
                if (team.IsDefender)
                {
                    team.OnFormationsChanged += new Action<Team, Formation>(this.OnFormationsChanged);
                    team.OnOrderIssued += new OnOrderIssuedDelegate(this.OnOrderIssued);
                }

            }
        }


        public void OnFormationsChanged(Team team, Formation formation)
        {
            System.Diagnostics.Debug.WriteLine("LaneWarLogic OnFormationsChanged");
        }
        private void OnOrderIssued(OrderType orderType, IEnumerable<Formation> appliedFormations, params object[] delegateParams)
        {
            System.Diagnostics.Debug.WriteLine("LaneWarLogic OnOrderIssued");
        }

        protected override void OnObjectDisabled(DestructableComponent destructionComponent)
        {
            base.OnObjectDisabled(destructionComponent);

            System.Diagnostics.Debug.WriteLine("LaneWarLogic OnObjectDisabled");
            if (destructionComponent.IsDestroyed && destructionComponent.GameEntity != null)
            {
                GameEntity gameEntity = destructionComponent.GameEntity;
                if (gameEntity.HasTag("inner_gate"))
                { //内门损坏
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("内门损坏", null).ToString()));
                    innerGateIsDestroyed = true;
                    this.OnGateDestroyed();
                }
                else if (gameEntity.HasTag("outer_gate"))
                { //外门损坏
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("外门损坏", null).ToString()));
                    outerGateIsDestroyed = true;
                    this.OnGateDestroyed();
                }
            }
        }

        private void OnGateDestroyed()
        {
            if(!innerGateIsDestroyed || !outerGateIsDestroyed)
            //if (!innerGateIsDestroyed)
            {
                return;
            }

            Mission mission = Mission.Current;
            if (mission.MissionTeamAIType == Mission.MissionTeamAITypeEnum.Siege)
            {
                System.Diagnostics.Debug.WriteLine("攻城战");
                foreach (Team team in from t in (IEnumerable<Team>)mission.Teams select t)
                {
                    if (team.Side == BattleSideEnum.Defender)
                    {

                        //防守方策略
                        team.ClearTacticOptions();
                        team.AddTacticOption(new TacticLaneFight(team));
                        team.ResetTactic();
                    }
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            //System.Diagnostics.Debug.WriteLine("LaneWarLogic OnMissionTick");
            //this.Check();
        }

        public void Check()
        {
            if (Mission.Current.MainAgent == null)
            {
                return;
            }
            Vec3 playerPosition = Mission.Current.MainAgent.Position;

            if (playerPosition == null || (lastPosition != null && playerPosition.AsVec2.Distance(lastPosition.AsVec2) <= 1f))
            {
                return;
            }
            InformationManager.AddHintInformation("X:" + playerPosition.X + ", \nY:" + playerPosition.Y + ",\nZ:" + playerPosition.Z);
            lastPosition = playerPosition;
            CheckEntity(playerPosition);
        }

        public void CheckMissionObject(Vec3 playerPosition)
        {
            using (IEnumerator<MissionObject> enumerator2 = Enumerable.Where<MissionObject>(from obj in Mission.Current.ActiveMissionObjects select obj, delegate (MissionObject obj)
            {
                if (playerPosition.AsVec2.Distance(obj.GameEntity.GlobalPosition.AsVec2) < 1f)
                {
                    return true;
                }
                return false;
            }).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    System.Diagnostics.Debug.WriteLine(enumerator2.Current.GameEntity.Name);
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("靠近目标:" + enumerator2.Current.GetType().ToString(), null).ToString()));
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("目标Name:" + enumerator2.Current.GameEntity.Name, null).ToString()));
                    for (int i = 0; i < enumerator2.Current.GameEntity.Tags.Length; i++)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("目标Tag:" + enumerator2.Current.GameEntity.Tags[i], null).ToString()));
                    }
                }
            }
        }

        public void CheckEntity(Vec3 playerPosition)
        {
            IEnumerable<GameEntity> entitys = Mission.Current.Scene.FindEntitiesWithTagExpression(".*");

            using (IEnumerator<GameEntity> enumerator2 = Enumerable.Where<GameEntity>(from item in entitys select item, delegate (GameEntity item)
            {
                //过滤没用的
                if (item.Name.Equals("debris_holder") || item.HasTag("shop_prop") || item.Name.Contains("chair") || item.Name.Equals("shop_sign_merchantpottery"))
                {
                    return false;
                }
                if (playerPosition.AsVec2.Distance(item.GlobalPosition.AsVec2) < 5f)
                {
                    return true;
                }
                return false;
            }).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    System.Diagnostics.Debug.WriteLine(enumerator2.Current.Name);
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("靠近目标:" + enumerator2.Current.Name, null).ToString()));
                    for (int i = 0; i < enumerator2.Current.Tags.Length; i++)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("目标Tag:" + enumerator2.Current.Tags[i], null).ToString()));
                    }
                }
            }
        }
    }
}
