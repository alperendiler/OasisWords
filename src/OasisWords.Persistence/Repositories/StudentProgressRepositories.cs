using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;
using OasisWords.Persistence.Contexts;

namespace OasisWords.Persistence.Repositories;

public class StudentRepository : EfRepositoryBase<Student, Guid, OasisWordsDbContext>, IStudentRepository
{
    public StudentRepository(OasisWordsDbContext context) : base(context) { }
}

public class StudentWordProgressRepository : EfRepositoryBase<StudentWordProgress, Guid, OasisWordsDbContext>, IStudentWordProgressRepository
{
    public StudentWordProgressRepository(OasisWordsDbContext context) : base(context) { }
}

public class StudentStreakRepository : EfRepositoryBase<StudentStreak, Guid, OasisWordsDbContext>, IStudentStreakRepository
{
    public StudentStreakRepository(OasisWordsDbContext context) : base(context) { }
}

public class DailyTargetSessionRepository : EfRepositoryBase<DailyTargetSession, Guid, OasisWordsDbContext>, IDailyTargetSessionRepository
{
    public DailyTargetSessionRepository(OasisWordsDbContext context) : base(context) { }
}
