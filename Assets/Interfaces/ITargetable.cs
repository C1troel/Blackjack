using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public interface IOutlinable
    {
        public void SetOutline();
        public void RemoveOutline();
        event Action<bool> OnOutlineChanged;
    }
}