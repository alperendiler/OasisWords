using Moq;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Domain.Entities;
using System.Linq.Expressions;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Application.Tests.Common;

/// <summary>
/// Factory helpers for creating pre-configured Moq mocks of repository interfaces.
/// </summary>
public static class MockRepositoryFactory
{
    /// <summary>
    /// Creates a mock IStudentWordProgressRepository that:
    ///  - Returns <paramref name="existing"/> from GetAsync (nullable)
    ///  - Captures what is passed to AddAsync / UpdateAsync and returns it
    /// </summary>
    public static Mock<IStudentWordProgressRepository> CreateProgressRepo(
        StudentWordProgress? existing = null)
    {
        var mock = new Mock<IStudentWordProgressRepository>();

        mock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<StudentWordProgress, bool>>>(),
                It.IsAny<Func<IQueryable<StudentWordProgress>,
                    Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<StudentWordProgress, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        mock.Setup(r => r.AddAsync(
                It.IsAny<StudentWordProgress>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentWordProgress p, CancellationToken _) => p);

        mock.Setup(r => r.UpdateAsync(
                It.IsAny<StudentWordProgress>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentWordProgress p, CancellationToken _) => p);

        return mock;
    }
}
