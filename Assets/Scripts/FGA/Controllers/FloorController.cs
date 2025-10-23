// ==================================================
// Floor Controller
// -----------
// A data controller executing the logic of a single
// Floor object.
// ==================================================

using FGA.Models;
using UnityEngine;

namespace Controllers.FGA
{
    public class FloorController : MonoBehaviour
    {
        #region Properties

        public FloorModel Floor { get; private set; }

        #endregion

        #region Floor Generation

        public void GenerateFloor(int floorLevel)
        {
            
        }
        
        #endregion
    }
}