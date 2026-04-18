using Microsoft.EntityFrameworkCore;

namespace TailspinToys.Api.Routes;

public static class PublishersRoutes
{
    public static void MapPublishersRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/publishers");

        group.MapGet("/", async (TailspinToysContext db) =>
        {
            var publishers = await db.Publishers
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Name
                })
                .ToListAsync();

            return Results.Ok(publishers);
        });
    }
}
