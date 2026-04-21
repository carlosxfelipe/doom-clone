# Doom Clone (C# MonoGame)

A retro 2.5D raycasting engine written entirely in C# using MonoGame, inspired by classic corridor shooters like Doom and Wolfenstein 3D. 

This project features a fully functional custom raycaster with DDA (Digital Differential Analyzer) collision, correct texture mapping with fake ambient occlusion, floor casting, animated weapon hitscan mechanics (with dynamic recoil and muzzle flashes), and an infinite seamless parallax skybox.

## Features
- **DDA Raycasting:** Precise and fast mathematical wall rendering with perspective correction to prevent fisheye distortions.
- **Texturing & Lighting:** UV texture mappings on walls and Mode-7 style floor casting, complete with distance-based dynamic voxel shading.
- **Weapon System:** Integrated player viewmodel with procedural idle walking (head bob) and algorithmic parabolic recoil on shooting.
- **Skybox:** Infinite wrapping panoramic background tied to rotational values.
- **AI-Generated Assets:** All graphical textures and sprite assets were generated on-the-fly via AI pixel art prompting.

## How to Run

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or higher) installed on your system.

### Cloning & Running
1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/carlosxfelipe/doom-clone.git
   ```

2. Enter the project directory:
   ```bash
   cd doom-clone
   ```

3. Run the project using the .NET CLI:
   ```bash
   dotnet run
   ```

## Controls
- **W / S:** Move forward and backward.
- **A / D:** Rotate camera left and right.
- **Space:** Fire weapon.
- **Esc:** Exit game.

## Origins & Development
This entire project (from the core engine architectural codebase down to all exact visual pixel art assets) was developed 100% autonomously with **Antigravity**. 

The initial core development was powered by **Gemini 3.1 Pro (High)**. Once the Pro tokens were exhausted, the development and final polish (including enemy spawning systems and performance optimizations) continued seamlessly using **Gemini 3 Flash**.
