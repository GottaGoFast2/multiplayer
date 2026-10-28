Multiplayer Basics Prototype - Unity Netcode for GameObjects

This project is a 3D cooperative multiplayer prototype built using Unity and Netcode for GameObjects (NGO). The game demonstrates core networking principles, including player spawning, transform synchronization, networked puzzle mechanics, and state-based victory conditions across multiple clients.

📑 Project Overview & Mechanics

The game requires two players (Host and Client) to cooperate in real-time to solve a simple networked puzzle:

Multiplayer Connection & Spawning: Supports seamless Host/Client connection with dynamic player object instantiation.

Player Movement & Synchronization: Real-time movement logic bounded by IsOwner checks to ensure independent client controls.

Networked Puzzle Mechanic: A pressure plate trigger that opens a shared barricade/door across all connected instances when activated.

Victory Logic: Collecting the crown triggers a synchronized "Victory!" UI display on both player screens.

🛠️ Tech Stack & Prerequisites

Engine: Unity 2022.3 (LTS)

Networking Framework: Netcode for GameObjects (NGO)

Version Control: Git / GitHub

🚀 How to Run locally

Clone the Repository:

git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY_NAME.git


Open in Unity: Open the project folder in Unity Editor (2022.3+).

Local Testing Setup:

Build a standalone executable of the project (File > Build Settings > Build).

Launch the Unity Editor and click Play (Set as Host).

Launch the Standalone Build (Set as Client).

Observe synchronized movement, interactions, and win conditions side-by-side.

📹 Deliverables Included

Source Code: Fully configured Unity project and scene setups.

Demonstration Video: Demo Video.mp4 (Included in the main directory showcasing Host/Client interaction, pressure plate activation, and victory synchronization).

🎓 Academic Context

Developed as part of Assignment 3: Multiplayer Basics under the First Year Scholar Program.
