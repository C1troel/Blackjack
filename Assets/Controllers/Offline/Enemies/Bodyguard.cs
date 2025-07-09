using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class Bodyguard : BaseEnemy
    {
        public override void PerformAction()
        {
            Debug.Log("PerformingAction...");
            MapManager.Instance.MakeADraw(this);
        }
    }
}