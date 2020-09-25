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

    // 巷战决策
    public class TacticLaneFight : TacticComponent
    {
        public Team team;
        public TeamAISiegeDefender _teamAISiegeDefender;
        public bool laneFighting = false;
        private Vec3 middlePosition;

        public TacticLaneFight(Team team) : base(team)
        {
            this._teamAISiegeDefender = team.TeamAI as TeamAISiegeDefender;
            this.team = team;
            CalculateMiddlePositon();
        }

        public void CalculateMiddlePositon()
        {
            GameEntity middlePos = Mission.Current.Scene.FindEntityWithTag("middle_pos");
            if (middlePos != null)
            {
                this.middlePosition = middlePos.GlobalPosition;
            }
        }

        protected override void OnApply()
        {
            System.Diagnostics.Debug.WriteLine("LaneWarTactic OnApply ");
            base.OnApply();
            team.OnFormationAIActiveBehaviourChanged += new Action<Formation>(this.OnFormationAIActiveBehaviourChanged);
        }

        public void OnFormationAIActiveBehaviourChanged(Formation formation)
        {
            System.Diagnostics.Debug.WriteLine("LaneWarLogic OnFormationAIActiveBehaviourChanged");
            if (formation.AI.ActiveBehavior is BehaviorStop)
            {
                this.SetBehaviorWeight<BehaviorStop>(formation, 1E-8f);
            }
            else if (formation.AI.ActiveBehavior is BehaviorLaneFight)
            {
                System.Diagnostics.Debug.WriteLine(formation.PrimaryClass.ToString() + "开始巷战");
                InformationManager.DisplayMessage(new InformationMessage(new TextObject(formation.PrimaryClass.ToString() + "开始巷战", null).ToString()));
            }
        }

        protected override void TickOccasionally()
        {
            System.Diagnostics.Debug.WriteLine("LaneWarTactic TickOccasionally");

            base.TickOccasionally();
            if (_teamAISiegeDefender.InnerGate.IsDestroyed && _teamAISiegeDefender.InnerGate.IsGateOpen && _teamAISiegeDefender.OuterGate.IsDestroyed && _teamAISiegeDefender.OuterGate.IsGateOpen)
            {
                if (!laneFighting)
                {
                    SetLaneFightBehavior();
                }
                else if( IsChargeApplicable() )
                {//冲锋
                    foreach(Formation formation in team.Formations)
                    {
                        SetBehaviorWeight<BehaviorCharge>(formation, 1F);
                    }
                }
            }
        }

        private bool IsChargeApplicable()
        {
            float attackerPower = (from t in Mission.Current.Teams
                          where t.Side.GetOppositeSide() == BattleSideEnum.Defender
                          select t).SelectMany((Team t) => t.FormationsIncludingSpecial).Sum((Formation formation) => formation.QuerySystem.FormationPower);

            float defenerPower = (from t in Mission.Current.Teams
                                  where t.Side.GetOppositeSide() == BattleSideEnum.Attacker
                                  select t).SelectMany((Team t) => t.FormationsIncludingSpecial).Sum((Formation formation) => formation.QuerySystem.FormationPower);

            return defenerPower > 0 && defenerPower / attackerPower > 3.5f;
        }

        private void SetLaneFightBehavior()
        {
            base.StopUsingAllMachines();
            laneFighting = true;
            foreach (Formation formation in team.FormationsIncludingSpecial)
            {
                if (formation.CountOfUnits > 0 && formation.AI != null)
                {

                    //只有把已有的所有Behavior(除了BehaviorStop)的Weight设置成小于1E-7f,才会调用SpecialBehavior
                    formation.AI.ResetBehaviorWeights();
                    SetBehaviorWeight<BehaviorDefendCastleKeyPosition>(formation,1e-10F);
                    SetBehaviorWeight<BehaviorRetakeCastleKeyPosition>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorShootFromCastleWalls>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorCharge>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorAdvance>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorPullBack>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorReserve>(formation, 1e-10F);
                    SetBehaviorWeight<BehaviorStop>(formation, 100F);

                    //添加巷战行为
                    if( formation.IsRanged())
                    {
                        formation.AI.AddSpecialBehavior(new BehaviorRangedLaneFight(formation), true);
                    }
                    else
                    {
                        formation.AI.AddSpecialBehavior(new BehaviorInfantryLaneFight(formation), true);
                    }
                }
            }
        }

        private void SetBehaviorWeight<T>(Formation formation, float weight) where T : BehaviorComponent
        {
            if( formation == null || formation.AI == null)
            {
                System.Diagnostics.Debug.WriteLine("LaneWarTactic SetBehaviorWeight IS NULL");
                return;
            }
            BehaviorComponent behavior = formation.AI.GetBehavior<T>();
            if (behavior != null)
            {
                behavior.WeightFactor = weight;
            }
        }

        protected float GetRangedDefenerDistance(WorldPosition fleePosition)
        {
            if (middlePosition != null && middlePosition.IsValid)
            {
                return middlePosition.Distance(fleePosition.Position) * 0.5f;
            }
            return 40f;
        }

        protected float GetInfantryDefenerDistance(WorldPosition fleePosition)
        {
            if (middlePosition != null && middlePosition.IsValid)
            {
                return middlePosition.Distance(fleePosition.Position) * 0.7f;
            }
            return 75f;
        }
    }

}
