using UnityEngine;

namespace Game.GOAP
{
    public class DataPatrolBehaviour : MonoBehaviour
    {
        public MapAction MapAction;
        public ActionPOI currentPOI;

        public void DeathRepost()
        {
            MapAction?.RegisterEnemyDeath();
        }
    }
}