using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TailspinToys.Api;

namespace TailspinToys.Api.Routes;

public static class GamesRoutes
{
    public static void MapGamesRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/games");

        group.MapGet("/", async (int? categoryId, int? publisherId, TailspinToysContext db) =>
        {
            IQueryable<Api.Models.Game> query = db.Games
                .AsNoTracking()
                .Include(g => g.Publisher)
                .Include(g => g.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(g => g.CategoryId == categoryId.Value);
            }

            if (publisherId.HasValue)
            {
                query = query.Where(g => g.PublisherId == publisherId.Value);
            }

            var games = await query
                .OrderBy(g => g.Id)
                .ToListAsync();

            return Results.Ok(games.Select(g => g.ToDict()));
        });

        group.MapGet("/{id:int}", async (int id, TailspinToysContext db) =>
        {
            var game = await db.Games
                .Include(g => g.Publisher)
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game is null)
                return Results.NotFound(new { error = "Game not found" });

            return Results.Ok(game.ToDict());
        });
    }
}
