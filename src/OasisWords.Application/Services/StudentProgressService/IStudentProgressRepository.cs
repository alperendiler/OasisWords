using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Services.StudentProgressService;

public interface IStudentRepository : IAsyncRepository<Student, Guid>, IRepository<Student, Guid> { }

public interface IStudentWordProgressRepository : IAsyncRepository<StudentWordProgress, Guid>, IRepository<StudentWordProgress, Guid> { }

public interface IStudentStreakRepository : IAsyncRepository<StudentStreak, Guid>, IRepository<StudentStreak, Guid> { }

public interface IDailyTargetSessionRepository : IAsyncRepository<DailyTargetSession, Guid>, IRepository<DailyTargetSession, Guid> { }
