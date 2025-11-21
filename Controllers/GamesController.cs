using BacklogAPI.Data;
using BacklogAPI.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BacklogAPI.Controllers;

public class GamesController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GamesController> _logger;

    public GamesController(ApplicationDbContext context, ILogger<GamesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Function("GetGames")]
    public async Task<HttpResponseData> GetGames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games")] 
        HttpRequestData req)
    {
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
        _logger.LogInformation($"Processing GetGameById: {id}");

        var game = await _context.Games.FindAsync(id);

        if (game == null)
        {
            _logger.LogWarning($"Game with id {id} not found.");
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
        _logger.LogInformation("Processing CreateGame.");

        Game? newGame;
        try
        {
            newGame = await req.ReadFromJsonAsync<Game>();
            if (newGame == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not deserialize request body: {ex.Message}");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            await _context.Games.AddAsync(newGame);
            await _context.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(newGame);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating game: {ex.Message}");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("UpdateGame")]
    public async Task<HttpResponseData> UpdateGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "games/{id:int}")] 
        HttpRequestData req,
        int id)
    {
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
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        existingGame.Title = updatedGame.Title;
        existingGame.Genre = updatedGame.Genre;
        existingGame.Developer = updatedGame.Developer;
        existingGame.ReleaseYear = updatedGame.ReleaseYear;
        existingGame.Completed = updatedGame.Completed;
        existingGame.PlaytimeHours = updatedGame.PlaytimeHours;
        existingGame.Rating = updatedGame.Rating;
        existingGame.Review = updatedGame.Review;

        await _context.SaveChangesAsync();

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
        _logger.LogInformation($"Processing DeleteGame: {id}");

        var gameToDelete = await _context.Games.FindAsync(id);
        if (gameToDelete == null)
        {
            _logger.LogWarning($"Game with id {id} not found.");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _context.Games.Remove(gameToDelete);
        await _context.SaveChangesAsync();

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}