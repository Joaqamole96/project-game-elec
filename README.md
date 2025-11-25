--------------------------------------------------
Scripts
--------------------------------------------------
The CUDLR
--------------------------------------------------
The scripts in Dungeon Frontier fall under what I
like to call the CUDLR schematic. A script must be
able to beclassified under one of the five
categories:

1. Core
Core scripts are defined by their MonoBehaviour
inheritance. They are attached to GameObjects as
components, and are the conduit for inputs,
processes, and outputs of a particular feature.

2. Utilities
Utility scripts only serve to help another
script achieve its function, and don't fully fit
into the other script types.

3. Data
Data scripts hold data, duh. This data can be from
the user or the system, and can be created naturally
or during runtime.

4. Logic
Logic scripts handle processing of data into
information for other scripts to use.

5. Renders
Render scripts serve as the output for other scripts
to display information.
--------------------------------------------------
Script Classifications
--------------------------------------------------
Scripts are classified according to function.

1. Commons
Common scripts are universal. They're typically
just regular C# scripts.

2. Configs
Config scripts hold user or system edit-time data.

3. Controllers
Controllers are Core scripts for low-level
entities.

4. Directors
Directors are Core scripts for high-level entities.

5. Generators
Generators process input to produce output. They
usually don't hold data.

6. Managers
Managers are Core scripts for medium-level
entities.

7. Models
Model scripts hold system edit-time or run-time
data.

8. Renderers
Renderers display output onto the actual game.

9. Services
Services assist other scripts in performing tasks.