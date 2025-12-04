using BacklogAPI.Data;
using BacklogAPI.Models;
using BacklogAPI.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace BacklogAPI.Controllers;

public class GamesController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GamesController> _logger;
    private readonly IKVHelper _kvHelper;
    private readonly ApiKeySettings _apiKeySettings;
    private readonly TelemetryClient _telemetryClient;

    public GamesController(ApplicationDbContext context, ILogger<GamesController> logger, IKVHelper kvHelper, ApiKeySettings apiKeySettings, TelemetryClient telemetryClient)
    {
        _context = context;
        _logger = logger;
        _kvHelper = kvHelper;
        _apiKeySettings = apiKeySettings;
        _telemetryClient = telemetryClient;
    }

    private HttpResponseData? ValidateApiKey(HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("X-Api-Key", out var values) || values.FirstOrDefault() != _apiKeySettings.ApiKey)
        {
            _logger.LogWarning("Invalid or missing API key.");
            _telemetryClient.TrackEvent("UnauthorizedAccessAttempt");
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }
        
        return null;
    }

    [Function("GetGames")]
    public async Task<HttpResponseData> GetGames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games")] 
        HttpRequestData req)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation("Processing GetGames.");

        var games = await _context.Games.ToListAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(games);
        return response;
    }

    [Function("GetGameById")]
    public async Task<HttpResponseData> GetGameById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games/{id:int}")] 
        HttpRequestData req, 
        int id)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation($"Processing GetGameById: {id}");

        var game = await _context.Games.FindAsync(id);

        if (game == null)
        {
            _logger.LogWarning($"Game with id {id} not found.");
            _telemetryClient.TrackEvent("GameNotFound", new Dictionary<string, string> { { "GameId", id.ToString() } });
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(game);
        return response;
    }

    [Function("GetGameByTitle")]
    public async Task<HttpResponseData> GetGameByTitle(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games/{title}")] 
        HttpRequestData req, 
        string title)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation($"Processing GetGameByTitle: {title}");

        var game = await _context.Games.FirstOrDefaultAsync(g => g.Title == title);

        if (game == null)
        {
            _logger.LogWarning($"Game with title {title} not found.");
            _telemetryClient.TrackEvent("GameNotFound", new Dictionary<string, string> { { "Title", title } });
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(game);
        return response;
    }

    [Function("CreateGame")]
    public async Task<HttpResponseData> CreateGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "games")] 
        HttpRequestData req)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation("Processing CreateGame.");

        Game? newGame;
        try
        {
            newGame = await req.ReadFromJsonAsync<Game>();
            if (newGame == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            var existingGameCheck = await _context.Games.FindAsync(newGame.SteamAppId);
            if (existingGameCheck != null)
            {
                _logger.LogWarning($"Game with SteamAppId {newGame.SteamAppId} already exists.");
                _telemetryClient.TrackEvent("GameCreationConflict", new Dictionary<string, string> { { "SteamAppId", newGame.SteamAppId.ToString() } });
                return req.CreateResponse(HttpStatusCode.Conflict);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not deserialize request body: {ex.Message}");
            _telemetryClient.TrackException(ex);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            await _context.Games.AddAsync(newGame);
            await _context.SaveChangesAsync();

            _telemetryClient.TrackEvent("GameCreated", new Dictionary<string, string> { { "Title", newGame.Title }, { "SteamAppId", newGame.SteamAppId.ToString() } });

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(newGame);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating game: {ex.Message}");
            _telemetryClient.TrackException(ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("UpdateGame")]
    public async Task<HttpResponseData> UpdateGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "games/{id:int}")] 
        HttpRequestData req,
        int id)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation($"Processing UpdateGame: {id}");

        var existingGame = await _context.Games.FindAsync(id);
        if (existingGame == null)
        {
            _logger.LogWarning($"Game with id {id} not found.");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        Game? updatedGame;
        try
        {
            updatedGame = await req.ReadFromJsonAsync<Game>();
            if (updatedGame == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not deserialize request body: {ex.Message}");
            _telemetryClient.TrackException(ex);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        existingGame.Title = updatedGame.Title;
        existingGame.Genre = updatedGame.Genre;
        existingGame.Developer = updatedGame.Developer;
        existingGame.ReleaseYear = updatedGame.ReleaseYear;
    
        if (!existingGame.Completed && updatedGame.Completed)
        {
             _telemetryClient.TrackEvent("GameCompleted", new Dictionary<string, string> { { "Title", existingGame.Title }, { "GameId", id.ToString() } });
        }

        existingGame.Completed = updatedGame.Completed;
        existingGame.CompletedOn = updatedGame.CompletedOn;
        existingGame.Dropped = updatedGame.Dropped;
        existingGame.PlaytimeHours = updatedGame.PlaytimeHours;
        existingGame.Rating = updatedGame.Rating;
        existingGame.Review = updatedGame.Review;

        await _context.SaveChangesAsync();

        _telemetryClient.TrackEvent("GameUpdated", new Dictionary<string, string> { { "GameId", id.ToString() } });

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(existingGame);
        return response;
    }

    [Function("DeleteGame")]
    public async Task<HttpResponseData> DeleteGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "games/{id:int}")] 
        HttpRequestData req,
        int id)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation($"Processing DeleteGame: {id}");

        var gameToDelete = await _context.Games.FindAsync(id);
        if (gameToDelete == null)
        {
            _logger.LogWarning($"Game with id {id} not found.");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _context.Games.Remove(gameToDelete);
        await _context.SaveChangesAsync();

        _telemetryClient.TrackEvent("GameDeleted", new Dictionary<string, string> { { "Title", gameToDelete.Title }, { "GameId", id.ToString() } });

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("ValidateGames")]
    public async Task<HttpResponseData> ValidateGames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "games/validate")] 
        HttpRequestData req)
    {
        var authResponse = ValidateApiKey(req);
        if (authResponse != null) return authResponse;

        _logger.LogInformation("Processing ValidateGames (Bulk).");

        var allGames = await _context.Games.ToListAsync();
        int updatedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var game in allGames)
        {
            bool isModified = false;

            if (game.ReleaseYear.HasValue && game.ReleaseYear.Value > now.Year && game.Completed)
            {
                game.Completed = false;
                game.CompletedOn = null;
                isModified = true;
            }

            bool hasRating = game.Rating.HasValue && game.Rating > 0;
            bool hasReview = !string.IsNullOrEmpty(game.Review);

            if ((hasRating || hasReview) && !game.Completed)
            {
                game.Completed = true;
                isModified = true;
            }

            if (game.Dropped && game.Completed)
            {
                game.Dropped = false;
                isModified = true;
            }

            game.ValidatedOn = now;
            
            if (isModified)
            {
                updatedCount++;
            }
        }

        if (updatedCount > 0 || allGames.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        _telemetryClient.TrackEvent("ValidationTriggered", null, new Dictionary<string, double> { { "UpdatedCount", updatedCount }, { "TotalGames", allGames.Count } });

        var response = req.CreateResponse(HttpStatusCode.OK);
        var result = new
        {
            updatedCount = updatedCount,
            timestamp = now,
            message = $"Validation complete. {updatedCount} games updated."
        };
        await response.WriteAsJsonAsync(result);
        return response;
    }
}