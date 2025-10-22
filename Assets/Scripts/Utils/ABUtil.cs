using UnityEngine;

namespace JYW.ArrowBattle.Utils
{

    public class ABUtil
    {

        public static bool isOpponentOnLeft(GameObject startGO, GameObject[] GoalGOs)
        {



            Transform nearest = null;
            float bestAbsDx = float.MaxValue;

            for (int i = 0; i < GoalGOs.Length; i++)
            {
                float dx = GoalGOs[i].transform.position.x - startGO.transform.position.x;
                float abs = Mathf.Abs(dx);
                if (abs < bestAbsDx)
                {
                    bestAbsDx = abs;
                    nearest = GoalGOs[i].transform;
                }
            }
            if (nearest == null) return false;

            float dxNearest = nearest.position.x - startGO.transform.position.x;
            return dxNearest < 0f;
        }

    }
}