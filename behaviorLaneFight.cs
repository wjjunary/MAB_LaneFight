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

    public class BehaviorLaneFight : BehaviorComponent
    {
        protected Formation formation;
        protected Vec3 middlePosition;
        protected bool isGatherd = false;

        public BehaviorLaneFight(Formation formation):base()
        {
            this.formation = formation;

            //利用放射来初始化父类属性
            object obj = (BehaviorLaneFight)this;
            Type type = obj.GetType();
            while( !type.Name.Equals("BehaviorComponent") )
            {
                type = type.BaseType;
            }
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //设置父类的formation
            FieldInfo baseformation = type.GetField("formation", flag);
            baseformation.SetValue(obj, formation);
            //设置父类的timeer
            FieldInfo _navmeshlessTargetPenaltyTime = type.GetField("_navmeshlessTargetPenaltyTime", flag);
            TaleWorlds.Core.Timer timer = new TaleWorlds.Core.Timer(MBCommon.GetTime(MBCommon.TimeType.Mission), 20f, true);
            _navmeshlessTargetPenaltyTime.SetValue(obj, timer);

            this.Init();
        }

        private void Init()
        {
            base.CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
            base.CurrentOrder = MovementOrder.MovementOrderStop;

            this.CalculateMiddlePositon();
        }

        protected void CalculateMiddlePositon()
        {
            GameEntity middlePos = Mission.Current.Scene.FindEntityWithTag("middle_pos");
            if( middlePos != null)
            {
                this.middlePosition = middlePos.GlobalPosition;
            }
        }

        protected override float NavmeshlessTargetPositionPenalty
        {
            get => base.NavmeshlessTargetPositionPenalty;
            set => base.NavmeshlessTargetPositionPenalty = value;
        }

        public override TextObject GetBehaviorString()
        {
            return new TextObject("巷战Behavior");
        }

        protected override float GetAiWeight()
        {
            return 30f;
        }

        protected override void OnBehaviorActivatedAux()
        {
            System.Diagnostics.Debug.WriteLine("BehaviorLaneFight OnBehaviorActivatedAux");
            this.formation.MovementOrder = base.CurrentOrder;
            this.formation.FacingOrder = this.CurrentFacingOrder;
            this.formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
            this.formation.FormOrder = FormOrder.FormOrderDeep;
            this.formation.WeaponUsageOrder = WeaponUsageOrder.WeaponUsageOrderUseAny;
            this.formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
        }

        protected override void TickOccasionally()
        {
            if (this.formation.QuerySystem.ClosestEnemyFormation != null)
            {
                float enemyDistance = formation.QuerySystem.MedianPosition.Position.Distance(formation.QuerySystem.ClosestEnemyFormation.MedianPosition.Position);
                if( !this.OnEnemyClosed(enemyDistance))
                {
                    this.CalculateCurrentOrder();
                }
            }
            else
            {
                this.CalculateCurrentOrder();
            }

            this.formation.MovementOrder = base.CurrentOrder;
            this.formation.FacingOrder = base.CurrentFacingOrder;
        }

        protected virtual bool OnEnemyClosed(float distance)
        {
            return false;
        }

        protected override void CalculateCurrentOrder()
        {
            //System.Diagnostics.Debug.WriteLine("BehaviorLaneFight CalculateCurrentOrder");
            base.CalculateCurrentOrder();
            WorldPosition fleePosition = Mission.Current.GetClosestFleePositionForFormation(formation);
            if (!fleePosition.IsValid)
            {
                if (this.formation.QuerySystem.ClosestEnemyFormation == null)
                {
                    Gather();
                    return;
                }
                base.CurrentOrder = MovementOrder.MovementOrderRetreat;
            }
            else
            {
                base.CurrentOrder = MovementOrder.MovementOrderMove(fleePosition);

                float fleedDistance = fleePosition.Position.Distance(this.formation.QuerySystem.MedianPosition.Position);
                if (fleedDistance <= GetGatherDistance(fleePosition))
                {   //已经到了集合位置
                    isGatherd = true;
                    Gather();
                }
            }

        }

        /**
         * 集合
         *
         **/
        protected virtual void Gather()
        {
            WorldPosition medianPosition = this.formation.QuerySystem.MedianPosition;
            medianPosition.SetVec2(this.formation.QuerySystem.AveragePosition);
            base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);

            OnGather();
        }

        protected virtual void OnGather()
        {

        }


        protected virtual float GetGatherDistance(WorldPosition fleePosition)
        {
            if (middlePosition != null && middlePosition.IsValid)
            {
                return middlePosition.Distance(fleePosition.Position) * 0.3f;
            }
            return 20f;
        }


        public void SetArrangement(ArrangementOrder order)
        {
            this.formation.ArrangementOrder = order;
        }

        public void SetMovement(MovementOrder order)
        {
            base.CurrentOrder = order;
        }
        public float GetFormationToFleeDistance()
        {
            WorldPosition fleePosition = Mission.Current.GetClosestFleePositionForFormation(formation);
            return fleePosition.Position.Distance(this.formation.QuerySystem.MedianPosition.Position);
        }

        public float GeMiddleToFleeDistance()
        {
            WorldPosition fleePosition = Mission.Current.GetClosestFleePositionForFormation(formation);
            return fleePosition.Position.Distance(this.middlePosition);
        }

        public override string ToString()
        {
            return "巷战";
        }
    }

}
