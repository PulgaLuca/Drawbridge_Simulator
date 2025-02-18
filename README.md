# Drawbridge Simulator 🚗⛴️

## Description 🏗️
Drawbridge Simulator is a console application written in C# that simulates the operation of an alternating one-way drawbridge. The software allows the management of vehicle traffic and the passage of a ship, adjusting the dynamics with traffic lights and thread synchronisation.

## Functionality ✨
- Addition of waiting cars on both sides of the bridge.
- Simulation of cars crossing in alternating mode.
- Traffic management with a maximum number of simultaneous vehicles on the bridge.
- Animation of a passing ship temporarily interrupting traffic.
- Textual interface with ASCII animations for dynamic visualisation.

## Technologies Used 🛠️
- Language: C#
- Threading: Use of Thread and SemaphoreSlim for concurrency management.

## Installation and Use 🚀

- Clone repository:
> git clone https://github.com/tuo-repo/drawbridge-simulator.git

> cd drawbridge-simulator

Compile and run with Visual Studio or from the terminal:

> dotnet run

Commands available in the menu after starting the game:

- L → Add a car to the left.
- R → Add a car on the right.
- P → Start car passing.
- S → Simulate ship passing.
- E → Exit simulator.
 