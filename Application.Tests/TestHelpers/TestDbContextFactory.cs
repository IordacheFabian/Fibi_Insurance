using System;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Tests.TestHelpers;

public class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
