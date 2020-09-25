using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LaneFight
{

    public class BehaviorRangedLaneFight : BehaviorLaneFight
    {

        public BehaviorRangedLaneFight(Formation formation):base(formation)
        {
        }


        protected override float GetGatherDistance(WorldPosition fleePosition)
        {
            if (middlePosition != null && middlePosition.IsValid)
            {
                return middlePosition.Distance(fleePosition.Position) * 0.3f;
            }
            return 25f;
        }

        protected override void OnBehaviorActivatedAux()
        {
            base.OnBehaviorActivatedAux();
            base.formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
        }

        protected override bool OnEnemyClosed(float distance)
        {
            //TODO 远程策略
            WorldPosition fleePosition = Mission.Current.GetClosestFleePositionForFormation(formation);
            float fleeDistance = fleePosition.Position.Distance(this.formation.QuerySystem.MedianPosition.Position);
            if(fleeDistance < GetGatherDistance(fleePosition) * 1.2f)
            {
                if (distance <= 10f) {
                    SetMovement(MovementOrder.MovementOrderCharge);
                }
                else if (distance <= 50f)
                {
                    SetMovement(MovementOrder.MovementOrderFallBack);
                    return true;
                }
                else if (distance <= 60f)
                {
                    SetMovement(MovementOrder.MovementOrderCharge);
                    return true;
                }
            }
            return  base.OnEnemyClosed(distance); ;
        }

    }

}
