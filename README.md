This repository contains a prototype matchmaking system built with ASP.NET Core, Redis, and RabbitMQ, designed to explore event-driven architecture. It serves as a hands-on environment for learning how to build scalable, reactive, and real-time services.

## Demo
https://www.youtube.com/watch?v=8J-hV8XQAlE

## Matchmaking flow :
<img width="1701" height="1101" alt="Untitled Diagram drawio(1)" src="https://github.com/user-attachments/assets/98545b9a-e3e3-4514-8447-1f65a8dee4a1" />

## Requirements

Ensure you have the following installed:

* **.NET 8.0 SDK**
* **Docker Desktop**
* **NodeJS**



## Getting Started with the backend

Follow these steps to get the project up and running:

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/abdullahoday710/FancyMatchMaker
    cd FancyMatchMaker
    ```

2.  **Start Docker Services:**
   Start the necessary Kafka and Zookeeper and postgres containers:
    ```bash
    docker-compose up -d
    ```
    The `-d` flag runs the containers in detached mode, so they run in the background.

    TIP : to see all your running detached docker instances you can use the command ```docker container ls```

3. **populate the databases**
   
   Now we need to create the tables for our database, Simply run these commands
   
   ```update-database -Project AuthService -StartupProject AuthService```
   
   ```update-database -Project MatchMakingService -StartupProject MatchMakingService```

5.  **Run a Microservice:**

    * **Using Visual Studio:**
        Open the `RankedRockPaperScissors.sln` file in Visual Studio. You can then select and run any of the individual microservices within the solution.

    * **Using the .NET CLI:**
        From the root directory, you can run the default `AuthService` microservice directly:
        ```bash
        dotnet run --project AuthService/AuthService.csproj
        ```
## Getting started with the frontend
1. **Go into the frontend directory**
   ```bash
   cd frontend
   ```
2. **install dependencies from npm**
   ```bash
   npm install
   ```
3. **run the local dev server**
   ```bash
   npm run dev
   ```

## Useful dev commands cheat sheet
- Add a migration to a specific microservice : ```add-migration {Migration name} -Project {Microservice name} -StartupProject {Microservice name}```
- Update the database for a specific microservice : ```update-database -Project {Microservice name} -StartupProject {Microservice name}```
