using System;
using UnityEngine;

namespace Game.GOAP
{
    public class DataPatrolBehaviour : MonoBehaviour
    {
        private float _replanningTimeout = 0f;
        [SerializeField] private GameObject rawRoute;
        private PatrolRoute _route;
        private int _currTargetIndex = -1;
        public Waypoint CurrWaypoint { get; private set; }

        void Awake()
        {
            _route = rawRoute.GetComponent<PatrolRoute>();
        }

        void Update()
        {
            _replanningTimeout = Mathf.Max(0f, _replanningTimeout - Time.deltaTime);
        }

        public void CalculateNextWaypoint()
        {
            if (!_route || _route.Waypoints is null || _route.Waypoints.Length == 0)
            {
                Debug.Log("failed to get nextWaypoint");
                return;
            }

            if (_replanningTimeout > 0f) return;
            _currTargetIndex = (_currTargetIndex + 1) % _route.Waypoints.Length;
            CurrWaypoint = _route.Waypoints[_currTargetIndex];
            _replanningTimeout = 1f;
        }

        public MapAction MapAction;
        public ActionPOI currentPOI;
        
        public void DeathRepost()
        {
            MapAction?.RegisterEnemyDeath();
        }
    }
}