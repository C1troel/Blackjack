using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class TimeStopper : BasePlayerController
    {
        public override void ActivateAbility()
        {
            Debug.Log($"Player {this.characterName} activates TimeStop");
        }
    }
}