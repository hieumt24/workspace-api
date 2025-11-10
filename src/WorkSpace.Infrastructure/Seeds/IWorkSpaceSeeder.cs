namespace WorkSpace.Infrastructure.Seeds;

public interface IWorkSpaceSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}