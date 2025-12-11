# Backlog API
### Version 2.1

C# REST API with CRUD operations, deployed using Azure Function App. 
Developed for CS-432, Cloud Computing at Sacred Heart University

## Version Specific Features
- Custom Telemetery

## Introduction

This API is for cataloguing and tracking Steam Games in your backlog. For stable builds look to version branches. The main branch is unstable and may be subject to changes. To find the companion front-end interface go [here](https://github.com/dirttop/Backlog-Interface).

## Getting Started
This section will get you started with deploying the API to Microsoft Azure for use.

>If you want to learn how to use the Backlog-API, go [here](#using-the-backlog-api).

### Prerequisites
- Development Environment
- Azure Account

### Dependencies
- C# Compiler
- Azure Tools Extension Pack
- Azurite
- Microsoft EntityFrameworkCore

### Recommended Packages For Testing
- Thunder Client
- Postman
- Azure CLI

### Setup

> **Ensure that all dependencies are installed and working.** <br>

&emsp;This project is built in C# with .NET 9.0

&emsp;Navigate to the Azure Functions button in your local workspace and press 'Create Function'

> Screenshots show setup in Visual Studio Code

<img width="533" height="335" alt="Screenshot 2025-10-09 at 12 04 11 PM" src="https://github.com/user-attachments/assets/8e94d260-ca76-4700-ab4e-dc25c66f4f53" />

- Select HTTP Trigger template.
- Input a Function Name and Namespace
- Access Rights: Anonymous
- Wait for setup to conclude.

<br><img width="735" height="127" alt="Screenshot 2025-10-09 at 12 04 32 PM" src="https://github.com/user-attachments/assets/5cb6574b-8235-42b3-b3c4-1b3694d73bd9" />

<br>

&emsp;Then, in a fresh Powershell terminal, install the Azure CLI

For Windows: ``` winget install --exact --id Microsoft.AzureCLI ```

For Mac: ``` brew update && brew install azure-cli ```

For Linux: ``` curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash ```

This is for a fresh start. Clone the repo to get exact source code.

### Project Structure and Architecture

<img width="1626" height="809" alt="image" src="https://github.com/user-attachments/assets/cc7d599d-1291-441e-8634-c5f5e18932da" />

**Models** - Defines the data structure and business entities (e.g., `Game.cs`).

**Controllers** - Handles API endpoints, HTTP requests, and responses (e.g., `GamesController.cs`).

**Data** - Manages database context and configuration (e.g., `ApplicationDbContext.cs`, `ApplicationDbContextFactory.cs`).

**Helpers** - Contains utility classes for shared functionality (e.g., `KVHelper.cs` for Key Vault operations, `ApiKeyHelper.cs`).

### Database Setup

> **This requires access to the Azure portal.** <br>

&emsp;Under the resource group which contains your Function App, create a new resource.

&emsp;Search for AzureSQL and create a new AzureSQL Server and Database with preferred settings.

- Set your account as the Entra Administrator for the AzureSQL for local testing.

> **If you don't set it during creation, you can set it in the AzureSQL Server Settings under 'Microsoft Entra ID'** <br>

&emsp;Upon creation of your new resources, use the SQL Server extension within your environment to connect to the SQL server. This will depend on your IDE.

&emsp;Once connected, it's time to set up the databases.

&emsp;First, in local settings, you need to add your connection string which can be found under the AzureSQL Database resource.

``` 
  "Values": {
    ...
    "DefaultConnection": "STRING HERE"
  }
```

&emsp;Once that's set up it's time to create our database.

&emsp; Login to the Azure CLI with:

``` az login ```

&emsp;In a new terminal run:

``` dotnet ef migrations add InitialCreate```

> **If this fails, ensure that your database server is connected.** <br>

&emsp;After completion run:

``` dotnet ef database update ```

&emsp;This will create a new migration, which should be reflected with a new table in your database. You can now move on to testing locally.

&emsp;Finally, within your Database resource under the Query Editor, run the following query to allow the Function App access to the database.

``` 
CREATE USER [FUNCTION APP NAME] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [FUNCTION APP NAME];
ALTER ROLE db_datawriter ADD MEMBER [FUNCTION APP NAME];
```

### Key Vault Setup

1.  **Create Key Vault**: Create a new Key Vault resource in your Azure Resource Group.
2.  **Add Secrets**:
    *   `ApiKey`: A strong, random string to be used as your API Key.
    *   `DefaultConnection`: Your Azure SQL Database connection string.
3.  **Managed Identity**:
    *   **User Assigned Identity**: Create a User Assigned Managed Identity in Azure.
    *   **Configuration**: Set the `MANAGED_IDENTITY_CLIENT_ID` environment variable in your Function App to the Client ID of your User Assigned Identity.
    *   **Key Vault Access**: Go to your Key Vault -> **Access Control (IAM)** -> **Add Role Assignment**.
    *   Assign the **Key Vault Secrets User** role to your User Assigned Identity.
    *   *Note: For local development, ensure your personal Azure account also has this role.*

### Running Locally

> **Prerequisite:** You must have the `KEY_VAULT_NAME` environment variable set and be logged in via Azure CLI (`az login`) to access the Key Vault.

&emsp;In a new terminal run:
``` func start ```

&emsp;Test with local route.
> **EX: http://localhost:7071/api/games**

### Deployment

&emsp;Create a Function App within Azure prior to deploying with desired settings.

&emsp;Then, within the Function App, under Identity assign your previously created User Assigned Managed Identity to the Function App. 

&emsp;Then, navigate to the Azure Functions button in your local workspace and select 'Deploy to Azure'

> Screenshots show deployment in Visual Studio Code
<img width="541" height="342" alt="Screenshot 2025-10-09 at 11 52 52 AM" src="https://github.com/user-attachments/assets/710701e0-1f6b-4d85-8065-6603d3e3275d" />

<br>&emsp;Select desired Function App and wait for deployment to conclude.
<br><img width="671" height="128" alt="Screenshot 2025-10-09 at 11 57 09 AM" src="https://github.com/user-attachments/assets/32c2dd51-d16f-423a-a562-565e5ba47afe" />

> Your deployment domain will now be available on Azure. For instruction on testing go [here](#using-the-backlog-api).

### Governance Features

This API implements governance features to ensure that the API is following best practice for cloud-native applications.

#### 1. Secret Management w/ Azure Key Vault
* The API Key is never stored in config files. It is securely stored in Azure Key Vault as a secret named `ApiKey`.
* The Function App retrieves this key at runtime using User Assigned Managed Identity.
* This setup allows for easy key changes without redeploying.

#### 2. Logging w/ Application Insights
* Application Insights is integrated to track request rates, failures, and performance metrics.
* `ILogger` is used throughout the application to log critical events.
* Failed requests and exceptions are captured with traces to support debugging and security.

#### 3. Validation Timestamping
* The `ValidateGames` endpoint updates the `validatedOn` timestamp for every game entity it processes.
* This field provides audit logging through MS Teams messages. This shows when the data was last checked against business rules, ensuring the backlog remains clean. 

### Application Insights Integration

This API is fully integrated with Azure Application Insights for telemetry and monitoring.

<img width="1965" height="908" alt="image" src="https://github.com/user-attachments/assets/fd26ccef-b1a6-498f-a526-6ea5ac6933e0" />

#### Custom Telemetry
It tracks specific events to gain insights into system performance:

*   `GameCreated`: Triggered when a new game is added.
*   `GameUpdated`: Triggered when a game's details are modified.
*   `GameCompleted`: Triggered specifically when a game is marked as completed.
*   `GameDeleted`: Triggered when a game is removed.
*   `ValidationTriggered`: Logs the execution of the bulk validation process.
*   `GameNotFound`: Logs attempts to access non-existent games.
*   `UnauthorizedAccessAttempt`: Logs failed API key validations.

#### Sample KQL Queries
These KQL Queries can be used to test Custom Telemetry:

**View Recent Custom Events:**
```kusto
customEvents
| project timestamp, name, customDimensions
| order by timestamp desc
```

**Analyze Game Completions:**
```kusto
customEvents
| where name == "GameCompleted"
| project timestamp, GameTitle = customDimensions.Title, GameId = customDimensions.GameId
```

**Monitor Validation Results:**
```kusto
customEvents
| where name == "ValidationTriggered"
| extend UpdatedCount = toint(customDimensions.UpdatedCount), TotalGames = toint(customDimensions.TotalGames)
| project timestamp, UpdatedCount, TotalGames
```

### Logic App Integration

An Azure Logic App is used to automate the validation of the game backlog.

*   **Trigger**: Recurrence trigger.
*   **Action**: Calls the `PATCH /api/games/validate` endpoint.
*   **Authentication**: Uses Managed Identity to securely authenticate with the API.
*   **Purpose**: Ensures that all game records adhere to business rules without manual intervention.

The following is a sample Logic App layout to integrate automatic validation.
<img width="623" height="780" alt="image" src="https://github.com/user-attachments/assets/006f3715-507c-4833-8295-4df8a35773db" />

Run history can be tracked under Overview.
<img width="974" height="295" alt="image" src="https://github.com/user-attachments/assets/dc92399b-f872-4f32-b1ec-8138d49dad0b" />

### Sample Dashboard
> Use for further Governence and quickly checking application health.
<img width="2003" height="839" alt="image" src="https://github.com/user-attachments/assets/6cf3e76f-0f1a-4c9a-9627-5dcc12084dcf" />

## Using the Backlog API

### Authentication

Use of the API requires the API Key passed in the `X-Api-Key` header. This version is currently private. In order to use the API, follow the setup instructions on a personal Azure account.

## Game Parameters
> Every game entity contains the following JSON parameters, with only title and steamAppId being required fields.
> steamAppId is the primary key. Official steamAppId values can be found through [SteamDB](https://steamdb.info/).

```
{
    "steamAppId": 1, //Primary key
    "title": "Game Title",
    "genre": "Genre Name",
    "developer": "Developer Name",
    "releaseYear": 0000,
    "completed": true,
    "completedOn": "0000-00-00T00:00:00", //Set automatically but can be modified
    "dropped": false,
    "playtimeHours": 0.0,
    "rating": 0.0,
    "review": "Review",
    "validatedOn": "0000-00-00T00:00:00" //System set, not modifiable
}
```

## Endpoints

### CreateGame

> **POST /api/games**

&emsp;Creates a new game entry. Only title and steamAppId are required parameters.

#### Request

#### bash / zsh
```
curl -X POST "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games" \
-H "Content-Type: application/json" \
-H "X-Api-Key: INSERT_KEY_HERE" \
-d '{
    "title": "Game Title",
    "genre": "Genre Name",
    "developer": "Developer Name",
    "releaseYear": 0000,
    "steamAppId": 1
}'
```

#### Request Body (JSON)

```
{
    "title": "Counter-Strike 2",
    "genre": "FPS",
    "developer": "Valve",
    "releaseYear": 2012,
    "steamAppId": 730
}
```

#### Response (JSON)
#### Code: 201

```
{
  "title": "Counter-Strike 2",
  "genre": "FPS",
  "developer": "Valve",
  "releaseYear": "2012",
  "steamAppId": 730
}
```


### GetGames

>**GET /api/games**

&emsp;Retrieves all games stored in the API. Games are sorted by their steamAppId in ascending order. Every steamAppId is unique.

#### Request
#### bash / zsh
```
curl -L -X GET "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/" \
-H "X-Api-Key: INSERT_KEY_HERE"
```
#### Response (JSON)
#### Code: 200

```
{
  "730": {
    "title": "Counter-Strike 2",
    "genre": "FPS",
    "developer": "Valve",
    "releaseYear": 2012,
    "steamAppId": 730
  },
  "367520": {
    "title": "Hollow Knight",
    "genre": null,
    "developer": null,
    "releaseYear": 0,
    "steamAppId": 367520
  }
}
```

### GetGameByID

>**GET /api/games/{steamAppId:int}**

&emsp;Retrieves a game's information by its steamAppId.

#### Request

#### bash / zsh
```
curl -L -X GET "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{steamAppId:int}" \
-H "X-Api-Key: INSERT_KEY_HERE"
```
#### Response (JSON)
#### Code: 200
```
{
  "title": "Counter-Strike 2",
  "genre": "FPS",
  "developer": "Valve",
  "releaseYear": 2012,
  "steamAppId": 730
}
```
### GetGameByTitle

>**GET /api/games/{title}**

&emsp;Retrieves a game's information by its Title.

#### Request

#### bash / zsh
```
curl -L -X GET "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{title}" \
-H "X-Api-Key: INSERT_KEY_HERE"
```
#### Response (JSON)
#### Code: 200
```
{
  "title": "Counter-Strike 2",
  "genre": "FPS",
  "developer": "Valve",
  "releaseYear": 2012,
  "steamAppId": 730
}
```

### UpdateGame

>**PUT /api/games/{steamAppId:int}**

&emsp;Update a game's information by its steamAppId. steamAppId cannot be updated.

#### Request
#### bash / zsh
```
curl -X PUT "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{steamAppId:int}" \
-H "Content-Type: application/json" \
-H "X-Api-Key: INSERT_KEY_HERE" \
-d '{
    "title": "Game Title",
    "genre": "Genre Name",
    "developer": "Developer Name",
    "releaseYear": 0000
}'
```
#### Response (JSON)
#### Code: 200
```
{
  "title": "Hollow Knight",
  "genre": "Metroidvania",
  "developer": "Team Cherry",
  "releaseYear": 2017,
  "steamAppId": 367520
}
```

### DeleteGame

>**DELETE /api/games/{steamAppId:int}**

&emsp;Delete a game by its steamAppId.

#### Request
#### bash / zsh
```
curl -L -X DELETE "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{steamAppId:int}" \
-H "X-Api-Key: INSERT_KEY_HERE"
```
#### Response
#### Code: 200
```
Game with steamAppId: 730 deleted successfully.
```

### ValidateGames

>**PATCH /api/games/validate**

&emsp;Ensure games all follow predefined business rules. The rules are as follows:

- If a game is marked as completed, but it's release year is in the future, it is set to incomplete.
- If a game has a rating and/or review, but it is set incomplete, set to complete.
- If a game is set to complete AND set to drop, prioritize completion and set to complete.

#### Request
#### bash / zsh
```
curl -X PATCH "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/validate" \
-H "X-Api-Key: INSERT_KEY_HERE"
```
#### Response (JSON)
#### Code: 200
```
{
    "updatedCount": 5,
    "timestamp": "2025-10-31T13:00:00Z",
    "message": "Validation complete. 5 games updated."
}
```

## Possible Errors

### Error Codes

| Endpoint | Code    | Message    |
| :---:   | :---: | :---: |
| All | 401   | Unauthorized |
| All | 404 | None (Incorrect Endpoint URL) |
| CreateGame | 400   | Could not deserialize request body:    |
| CreateGame | 409   | Game with SteamAppId {newGame.SteamAppId} already exists. |
| GetGameByID | 404   | Game with id {id} not found.   |
| GetGameByTitle | 404 | Game 
| UpdateGame | 400   | Invalid game data. |
| UpdateGame | 404   | Game with id {id} not found. |
| DeleteGame | 404   | Game with id {id} not found.   |

## Screenshots

### Authentication Error
<img width="980" height="304" alt="Screenshot 2025-10-09 at 10 36 44 PM" src="https://github.com/user-attachments/assets/4c710da2-6e67-49fe-a5b9-794c8b490888" />

### CreateGame
<img width="945" height="306" alt="Screenshot 2025-10-09 at 10 25 19 PM" src="https://github.com/user-attachments/assets/bb2dd25d-ac1b-4225-8cb2-29740b1e581a" />
<img width="987" height="308" alt="Screenshot 2025-10-09 at 10 26 02 PM" src="https://github.com/user-attachments/assets/a069b90b-aa5a-4cd8-b2e2-de8ae72ae3a8" />
<img width="980" height="299" alt="Screenshot 2025-10-09 at 10 26 38 PM" src="https://github.com/user-attachments/assets/8d875cc2-473e-4507-bb58-6b797cc0bf0b" />

### GetGames
<img width="963" height="262" alt="Screenshot 2025-10-09 at 10 27 24 PM" src="https://github.com/user-attachments/assets/8c5e4a62-f4da-41cd-b4c1-dc5524e7aa80" />

### GetGameByID
<img width="981" height="238" alt="Screenshot 2025-10-09 at 10 27 53 PM" src="https://github.com/user-attachments/assets/c06edc68-f4ee-4628-9ed5-1e97fd4ab145" />
<img width="970" height="247" alt="Screenshot 2025-10-09 at 10 28 10 PM" src="https://github.com/user-attachments/assets/235ed308-52ca-4d6f-916c-8846b2093bd7" />

### GetGameByTitle

### UpdateGame
<img width="948" height="288" alt="Screenshot 2025-10-09 at 10 29 03 PM" src="https://github.com/user-attachments/assets/ea50888c-1340-44ae-930a-ef448a660e9e" />
<img width="989" height="285" alt="Screenshot 2025-10-09 at 10 29 25 PM" src="https://github.com/user-attachments/assets/9b2d2849-071f-46e9-8a7d-14968a83a2f1" />

### DeleteGame
<img width="982" height="181" alt="Screenshot 2025-10-09 at 10 29 45 PM" src="https://github.com/user-attachments/assets/303e27e9-1d10-4f6d-a54a-3a65271d9886" />
<img width="978" height="193" alt="Screenshot 2025-10-09 at 10 30 07 PM" src="https://github.com/user-attachments/assets/a02e0dfe-0572-446a-8d03-af1235a7f175" />

### ValidateGames
<img width="2184" height="532" alt="image" src="https://github.com/user-attachments/assets/ea2744b9-3692-4962-afd7-8c97cedac915" />
<img width="2182" height="506" alt="image" src="https://github.com/user-attachments/assets/e55773b4-cc40-4b4f-861a-9ce5a5c4266a" />

## Sources

[Boilerplate](https://medium.com/dynamics-online/how-to-build-rest-apis-with-azure-functions-b4d26c88aa1d) by Fahad Ahmed

[Video 1 on ASP.NET API](https://www.youtube.com/watch?v=0J_T5qRynSI&t=3457s) by ABi Helpline

[Video 2 on ASP.NET API](https://www.youtube.com/watch?v=6YIRKBsRWVI&t=1318s) by Sameer Saini
