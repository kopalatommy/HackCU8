using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IActionable
    {
        bool RequiresAnimation
        {
            get;
        }

        void Interact();

        string Message();
    }
}
