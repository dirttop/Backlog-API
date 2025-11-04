# Backlog-API
### Version 1.1

C# REST API with CRUD operations, deployed using Azure App Service. 
Developed for CS-432, Cloud Computing

## Version Specific Features
- Integrated Azure Key Vault and managed identity for improved security.

## Introduction

This API, inspired by [SteamDB](https://steamdb.info/), is meant to catalogue and store Steam Games and relevant information about them. 
For stable builds look to version branches. The main branch is unstable and may be subject to changes.

## Getting Started
This section is if you want to deploy the Games-API on your own machine or Azure subscription.

>If you want to use the Games-API, go [here](#using-the-games-api).

### Prerequisites
- Development Environment
- Azure Account

### Dependencies
- C# Compiler
- Azure Functions
- Azure Resources
- Azurite

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

### Running Locally

&emsp;In a new terminal:
``` func start ```

&emsp;Test with local route.
> **EX: http://localhost:7071/api/games**

### Deployment

&emsp;Create a Function App within Azure prior to deploying with desired settings.

&emsp;Navigate to the Azure Functions button in your local workspace and select 'Deploy to Azure'

> Screenshots show deployment in Visual Studio Code
<img width="541" height="342" alt="Screenshot 2025-10-09 at 11 52 52 AM" src="https://github.com/user-attachments/assets/710701e0-1f6b-4d85-8065-6603d3e3275d" />

<br>&emsp;Select desired Function App and wait for deployment to conclude.
<br><img width="671" height="128" alt="Screenshot 2025-10-09 at 11 57 09 AM" src="https://github.com/user-attachments/assets/32c2dd51-d16f-423a-a562-565e5ba47afe" />

> Your deployment domain will now be available on Azure. For instruction on testing go [here](#using-the-games-api).

### Setting Up Managed Identity and Key Vault

&emsp; In order to use your deployed API, a Key Vault containing your API key must be created, and the Function App must be granted access.

&emsp; The following steps outline the process, for further clarification look to [this video](https://www.youtube.com/watch?v=b21EQvfjvHc&t=789s).
- Enable managed idenity in your Function App
- Create a Key Vault
- Give yourself Adiministatrative Access to your new Key Vault
- Create a Secret for your API Key
- Give the Key Vault Secrets User role assignment to your Function App
- Add a reference to the Key Vault in your Function App configuration
- Save your configuration and await confirmation from Azure
- Test your function app using your new API Key

## Using the Games-API

### URL

>**games-api-a0gveveefgdyfcap.canadacentral-01.azurewebsites.net**

### Authentication

To access the Games-API endpoints a valid authentication key must be presented.
The key must be included in the HTTP request header as a key/value pair.
Access to the API is currently unavailable publically.
Any materials presented in the documentation containing a key present an outdated Secret.

## Game Parameters
> Every game object contains the following JSON parameters, with only Title and SteamAppID being required fields.
> SteamAppID is a unique value. Official SteamAppID values can be found through [SteamDB](https://steamdb.info/).

```
{
    "Title": "Game Title",
    "Genre": "Genre Name",
    "Developer": "Developer Name",
    "ReleaseYear": 0000,
    "SteamAppID": 1
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
