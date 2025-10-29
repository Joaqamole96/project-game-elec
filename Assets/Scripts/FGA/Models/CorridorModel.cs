// ==================================================
// Corridor Model
// -----------
// A data model of a Corridor.
// A Corridor is a list of vector points, 2 tiles
// wide or long, connecting one Opening of a Room to
// another.
// ==================================================

using System.Collections.Generic;
using UnityEngine;
using static Helpers.LogHelper;

namespace FGA.Models
{
    public class CorridorModel
    {
        // Initialized Variables
        public (Vector2Int Start, Vector2Int End) Openings { get; private set; }

        // Configured Variables
        public List<Vector2Int> Path { get; private set; }

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        // Accessor Variables
        public int StartX => Openings.Start.x;
        public int StartY => Openings.Start.y;
        public int EndX => Openings.End.x;
        public int EndY => Openings.End.y;

        //----------------------------------------------------------------------------------

        public CorridorModel((Vector2Int, Vector2Int) openings)
        {
            ValidateInitialize(openings);
            
            Openings = openings;
            Declare(this, Openings);

            IsInitialized = true;
            Success(this, "This Corridor has been initialized.");
        }

        public CorridorModel(Vector2Int openingA, Vector2Int openingB)
            : this((openingA, openingB)) { }

        private void ValidateInitialize((Vector2Int openingA, Vector2Int openingB) openings)
        {
            if (IsInitialized)
            {
                throw Failure(this, "This Corridor is already initialized.");
            }
            if (openings.openingA == null || openings.openingB == null)
            {
                throw Failure(this, "This Corridor must not have null ends.");
            }
            if (openings.openingA == openings.openingB)
            {
                throw Failure(this, "This Corridor must have two separate ends.");
            }
        }

        //----------------------------------------------------------------------------------

        public void Configure(List<Vector2Int> path)
        {
            ValidateConfigure(path);

            Path = path;
            Declare(this, Path);
            
            Success(this, "This Corridor has been configured.");
        }

        private void ValidateConfigure(List<Vector2Int> path)
        {
            if (path == null)
            {
                throw Failure(this, "This Corridor cannot have a null path.");
            }
            if (path.Count < 2)
            {
                throw Failure(this, "This Corridor must have a path that is 2 or more tiles long.");
            }
        }

        //----------------------------------------------------------------------------------

        public void Describe()
        {
            // Describe the class object's attributes in the console.
        }

        public void Illustrate()
        {
            // Illustrate the class object as a visual display in the console.
        }
    }
}