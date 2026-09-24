# Elite Dangerous | Orion-Nebula Investigation
Investigation of the Orion Nebula in Elite Dangerous, a space simulation game developed by Frontier Developments.

These are the tools I use to investigate the Orion Nebula and keep progress on my research. The primary use of this project
it to keep track of the data collected in the game using the ED log files and the approximate star systems in the nebula extracted via Spansh.co.uk.

The nebula star systems are extracted from the Spansh.co.uk website with this query: https://www.spansh.co.uk/systems/search/F68FE71A-B5DD-11F1-AB6F-C5F268889FC4/1
The query is not a perfect representation of the nebula, but it is a good approximation of the star systems in the Orion Nebula.

The project is built using C# and .NET 10.0, and it uses Entity Framework Core/MS LocalDB for data access.

## How to build and run the project

### Install the prerequisites
- Install Visual Studio 2026 or later with the .NET desktop development and the Data storage workloads.
- Install the .NET 10.0 SDK from the official Microsoft website: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- Install Entity Framework Core .NET Command-line Tools using the following command in the terminal:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

### Build the project
- Make a new root directory for the project and navigate to it in the terminal.
- Make git clone the repository:
  ```powershell
  git clone https://github.com/Orion-Nebula/Orion-Nebula.git
  ```
- Navigate to the project directory:
  ```powershell
  cd Orion-Nebula
  ```
- Restore the project dependencies:
  ```powershell
  dotnet restore
  ```
- Build the project:
  ```powershell
  dotnet build
  ```
- run the tests:
  ```powershell
  dotnet test
  ```