using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Playspot.Application.DTOs.Auth;
using Playspot.Application.Features.Auth.Commands;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace Playspot.Tests;

public class AuthHandlerTests
{
    // ── Helpers ──────────────────────────────────────────────

    private static Mock<IAppDbContext> CreateDbMock(List<User> users)
    {
        var mockSet = CreateMockDbSet(users);
        var mockDb  = new Mock<IAppDbContext>();
        mockDb.Setup(d => d.Users).Returns(mockSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(1);
        return mockDb;
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet   = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
               .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
               .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.As<IQueryable<T>>()
               .Setup(m => m.Provider)
               .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return mockSet;
    }

    private static Mock<IJwtTokenGenerator> CreateJwtMock()
    {
        var mock = new Mock<IJwtTokenGenerator>();
        mock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
        return mock;
    }

    // ── RegisterHandler Tests ─────────────────────────────────

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsNull()
    {
        var existing = new User
        {
            Email        = "taken@test.com",
            Username     = "existing",
            PasswordHash = "hash"
        };
        var db  = CreateDbMock(new List<User> { existing });
        var jwt = CreateJwtMock();

        var handler = new RegisterHandler(db.Object, jwt.Object);
        var command = new RegisterCommand(new RegisterDto
        {
            Email     = "taken@test.com",
            Username  = "newuser",
            Password  = "Password@123!",
            FirstName = "Test",
            LastName  = "User",
            City      = "Skopje"
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsTokenAndUsername()
    {
        var db  = CreateDbMock(new List<User>());
        var jwt = CreateJwtMock();

        var handler = new RegisterHandler(db.Object, jwt.Object);
        var command = new RegisterCommand(new RegisterDto
        {
            Email     = "new@test.com",
            Username  = "newuser",
            Password  = "Password@123!",
            FirstName = "Test",
            LastName  = "User",
            City      = "Skopje"
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
        result.Username.Should().Be("newuser");
    }

    // ── LoginHandler Tests ────────────────────────────────────

    [Fact]
    public async Task Login_UserNotFound_ReturnsNull()
    {
        var db  = CreateDbMock(new List<User>());
        var jwt = CreateJwtMock();

        var handler = new LoginHandler(db.Object, jwt.Object);
        var command = new LoginCommand(new LoginDto
        {
            Email    = "ghot@test.com",
            Password = "whatever"
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var user = new User
        {
            Email        = "user@test.com",
            Username     = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@1!")
        };
        var db  = CreateDbMock(new List<User> { user });
        var jwt = CreateJwtMock();

        var handler = new LoginHandler(db.Object, jwt.Object);
        var command = new LoginCommand(new LoginDto
        {
            Email    = "user@test.com",
            Password = "WrongPassword@1!"
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Email        = "user@test.com",
            Username     = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@1!")
        };
        var db  = CreateDbMock(new List<User> { user });
        var jwt = CreateJwtMock();

        var handler = new LoginHandler(db.Object, jwt.Object);
        var command = new LoginCommand(new LoginDto
        {
            Email    = "user@test.com",
            Password = "CorrectPassword@1!"
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
        result.Username.Should().Be("testuser");
    }
}

// ── Async EF Core support helpers ─────────────────────────────

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
}

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;
    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
    public IQueryable CreateQuery(System.Linq.Expressions.Expression e) => _inner.CreateQuery(e);
    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression e) => new TestAsyncEnumerable<TElement>(e);
    public object? Execute(System.Linq.Expressions.Expression e) => _inner.Execute(e);
    public TResult Execute<TResult>(System.Linq.Expressions.Expression e) => _inner.Execute<TResult>(e);
    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression e, CancellationToken ct = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var result     = _inner.Execute(e);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                                    .MakeGenericMethod(resultType)
                                    .Invoke(null, new[] { result })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression e) : base(e) { }
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}