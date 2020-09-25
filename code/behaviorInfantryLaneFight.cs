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

    public class BehaviorInfantryLaneFight : BehaviorLaneFight
    {

        public BehaviorInfantryLaneFight(Formation formation):base(formation)
        {
        }

        protected override void OnBehaviorActivatedAux()
        {
            base.OnBehaviorActivatedAux();
            base.formation.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
        }

        protected override float GetGatherDistance(WorldPosition fleePosition)
        {
            if (middlePosition != null && middlePosition.IsValid)
            {
                return middlePosition.Distance(fleePosition.Position) * 0.55f;
            }
            return 50f;
        }

        protected override bool OnEnemyClosed(float distance)
        {
            WorldPosition fleePosition = Mission.Current.GetClosestFleePositionForFormation(formation);
            float enemyToFleeDistance = formation.QuerySystem.ClosestEnemyFormation.MedianPosition.Position.Distance(fleePosition.Position);
            if (enemyToFleeDistance <= GetGatherDistance(fleePosition) * 1.25f)
            {
                SetMovement(MovementOrder.MovementOrderCharge);
                return true;
            }
            return base.OnEnemyClosed(distance);
        }

    }

}
