using MediatR;
using OasisWords.Application.Features.Users.Constants;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.UserService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Security.Entities;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository    _userRepo;
    private readonly IStudentRepository _studentRepo;

    public UpdateUserCommandHandler(IUserRepository userRepo, IStudentRepository studentRepo)
    {
        _userRepo    = userRepo;
        _studentRepo = studentRepo;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        User user = await _userRepo.GetAsync(u => u.Id == request.UserId, cancellationToken: ct)
            ?? throw new NotFoundException(string.Format(UserMessages.UserNotFound, request.UserId));

        user.FirstName = request.FirstName;
        user.LastName  = request.LastName;
        User updated   = await _userRepo.UpdateAsync(user, ct);

        // Öğrencinin günlük hedefini de güncelle (varsa)
        if (request.DailyWordGoal.HasValue)
        {
            Student? student = await _studentRepo.GetAsync(
                s => s.UserId == request.UserId, cancellationToken: ct);

            if (student is not null)
            {
                student.DailyWordGoal = request.DailyWordGoal.Value;
                await _studentRepo.UpdateAsync(student, ct);
            }
        }

        return new UpdateUserResponse
        {
            UserId    = updated.Id,
            FirstName = updated.FirstName,
            LastName  = updated.LastName,
            Email     = updated.Email
        };
    }
}
