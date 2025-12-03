# Backlog API
### Version 1.4

C# REST API with CRUD operations, deployed using Azure Function App. 
Developed for CS-432, Cloud Computing at Sacred Heart University

## Version Specific Features
- ValidateGames PATCH endpoint

## Introduction

This API is for cataloguing and tracking Steam Games in your backlog. For stable builds look to version branches. The main branch is unstable and may be subject to changes.

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

### Project Structure

**Models** - Defines the Game structure.

**Controllers** - Handles API endpoints, 

**Data** - 

**Helpers**

### Database Setup

> **This requires access to the Azure portal.** <br>

&emsp;Under the resource group which contains your Function App, create a new resource.

&emsp;Search for AzureSQL and create a new AzureSQL Server and Database with preffered settings.

- Set your account as the Entra Administrator for the AzureSQL for local testing.

> **If you don't set it during creation, you can set it in the AzureSQL Server Settings under 'Microsoft Entra ID'** <br>

&emsp;Upon creation of your new resources, use the SQL Server extension within your environment to connect to the SQL server. This will depend on your IDE.

&emsp;Once connected, it's time to set up the databases.

&emsp;First, in local settings, you need to add your connection string which can be found under the AzureSQL Database resource.

``` 
  "Values": {
  ...
  "DefaultConnection": "STRING HERE";
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

### Key Vault Setup

### Running Locally

&emsp;In a new terminal run:
``` func start ```

&emsp;Test with local route.
> **EX: http://localhost:7071/api/games**

### Deployment

&emsp;Create a Function App within Azure prior to deploying with desired settings.

&emsp;Then, within the Function App, under Identity enable System Assigned Identity. 

&emsp;Now we need to set up a Key Vault and Managed Identities. Create a new Key Vault resource under your API Resource Group.

&emsp;In the Key Vault add two new secrets
- ApiKey (With any Key string)
- DefaultConnection (Connection string can be found under the SQL Database)

&emsp;Within the Key Vault under Access Control (IAM) add the Key Vault Secret Officer role to your newly System Assigned Function App member.

&emsp;Then, add a Secret to the Function App named 'KEY_VAULT_NAME' with the name of your Key Vault resource.

&emsp;Under the Database Server, go to the Security section and open the Networking tab. Under exceptions, check 'Allow Azure services and resources to access this server'.

&emsp;Finally, within your Database resource under the Query Editor, run the following query to allow the Function App access to the database.

``` 
CREATE USER [FUNCTION APP NAME] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [FUNCTION APP NAME];
ALTER ROLE db_datawriter ADD MEMBER [FUNCTION APP NAME];
```

&emsp;Then, navigate to the Azure Functions button in your local workspace and select 'Deploy to Azure'

> Screenshots show deployment in Visual Studio Code
<img width="541" height="342" alt="Screenshot 2025-10-09 at 11 52 52 AM" src="https://github.com/user-attachments/assets/710701e0-1f6b-4d85-8065-6603d3e3275d" />

<br>&emsp;Select desired Function App and wait for deployment to conclude.
<br><img width="671" height="128" alt="Screenshot 2025-10-09 at 11 57 09 AM" src="https://github.com/user-attachments/assets/32c2dd51-d16f-423a-a562-565e5ba47afe" />

> Your deployment domain will now be available on Azure. For instruction on testing go [here](#using-the-backlog-api).

### Azure Enhancements

#### 



## Using the Backlog API

### URL

>**games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net**

### Authentication

Use of the API requires the API Key. This version is currently private. In order to use the API, follow the setup instructions on a personal Azure account.

## Game Parameters
> Every game entity contains the following JSON parameters, with only Title and SteamAppID being required fields.
> SteamAppID is the primary key. Official SteamAppID values can be found through [SteamDB](https://steamdb.info/).

```
{
    "SteamAppID": 1 //Primary key
    "Title": "Game Title",
    "Genre": "Genre Name",
    "Developer": "Developer Name",
    "ReleaseYear": 0000,
    "Completed": True,
    "CompletedOn": 00-00-00T00:00, //Set automatically but can be modified
    "Dropped": False,
    "PlaytimeHours": 0.0,
    "Rating": 0.0,
    "Review": "Review",
    "ValidatedOn": 00-00-00T00:00 //System set, not modifiable
}
```

## Endpoints

### CreateGame

> **POST /api/games**

&emsp;Creates a new game entry. Only Title and SteamAppID are required parameters.

#### Request

#### bash / zsh
```
curl -X POST "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games" \
-H "Content-Type: application/json" \
-H "Key: INSERT_KEY_HERE" \
-d '{
    "Title": "Game Title",
    "Genre": "Genre Name",
    "Developer": "Developer Name",
    "ReleaseYear": 0000,
    "SteamAppID": 1
}'
```

#### Request Body (JSON)

```
{
    "Title": "Counter-Strike 2",
    "Genre": "FPS",
    "Developer": "Valve",
    "ReleaseYear": "2012",
    "SteamAppID": 730
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

&emsp;Retrieves all games stored in the API. Games are sorted by their SteamAppID in ascending order. Every SteamAppID is unique.

#### Request
#### bash / zsh
```
curl -L -X GET "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/"
-H "Key: INSERT_KEY_HERE"
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

>**GET /api/games/{SteamAppId:int}**

&emsp;Retrieves a games information by their SteamAppID.

#### Request

#### bash / zsh
```
curl -L -X GET "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{SteamAppId:int}"
-H "Key: INSERT_KEY_HERE"
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

>**PUT /api/games/{SteamAppId:int}**

&emsp;Update a games information by their SteamAppID. SteamAppID cannot be updated.

#### Request
#### bash / zsh
```
curl -X PUT "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{SteamAppId:int}" \
-H "Content-Type: application/json" \
-H "Key: INSERT_KEY_HERE" \
-d '{
    "Title": "Game Title",
    "Genre": "Genre Name",
    "Developer": "Developer Name",
    "ReleaseYear": 0000
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

>**DELETE /api/games/{SteamAppId:int}**

&emsp;Delete a game by their SteamAppID.

#### Request
#### bash / zsh
```
curl -L -X DELETE "https://games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net/api/games/{SteamAppId:int}"
-H "Key: INSERT_KEY_HERE"
```
#### Response
#### Code: 200
```
Game with SteamAppId: 730 deleted successfully.
```

### ValidateGames

>**PATCH /api/games/validate**

&emsp;Ensure games all follow predefined business rules.

## Possible Errors

### Error Codes

| Endpoint | Code    | Message    |
| :---:   | :---: | :---: |
| All | 401   | Unauthorized   |
| CreateGame | 400   | Title and unique non-zero ID are required.   |
| CreateGame | 400   | A game with this ID already exists.   |
| GetGameByID | 404   | Game not found. Input valid ID.   |
| UpdateGame | 400   | Invalid game data.   |
| UpdateGame | 404   | Game not found. Input valid ID.   |
| DeleteGame | 404   | Game not found. Input valid ID.   |

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

### UpdateGame
<img width="948" height="288" alt="Screenshot 2025-10-09 at 10 29 03 PM" src="https://github.com/user-attachments/assets/ea50888c-1340-44ae-930a-ef448a660e9e" />
<img width="989" height="285" alt="Screenshot 2025-10-09 at 10 29 25 PM" src="https://github.com/user-attachments/assets/9b2d2849-071f-46e9-8a7d-14968a83a2f1" />

### DeleteGame
<img width="982" height="181" alt="Screenshot 2025-10-09 at 10 29 45 PM" src="https://github.com/user-attachments/assets/303e27e9-1d10-4f6d-a54a-3a65271d9886" />
<img width="978" height="193" alt="Screenshot 2025-10-09 at 10 30 07 PM" src="https://github.com/user-attachments/assets/a02e0dfe-0572-446a-8d03-af1235a7f175" />

## Sources

[Boilerplate](https://medium.com/dynamics-online/how-to-build-rest-apis-with-azure-functions-b4d26c88aa1d) by Fahad Ahmed
