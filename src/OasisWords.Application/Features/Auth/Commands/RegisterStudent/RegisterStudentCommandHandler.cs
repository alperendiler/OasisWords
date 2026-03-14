using MediatR;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.UserService;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Auth.Commands.RegisterStudent;

public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, RegisterStudentResponse>
{
    private readonly IUserRepository            _userRepository;
    private readonly IRefreshTokenRepository    _refreshTokenRepository;
    private readonly IStudentRepository         _studentRepository;
    private readonly ITokenHelper               _tokenHelper;
    private readonly AuthBusinessRules          _rules;

    public RegisterStudentCommandHandler(
        IUserRepository         userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IStudentRepository      studentRepository,
        ITokenHelper            tokenHelper,
        AuthBusinessRules       rules)
    {
        _userRepository         = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _studentRepository      = studentRepository;
        _tokenHelper            = tokenHelper;
        _rules                  = rules;
    }

    public async Task<RegisterStudentResponse> Handle(RegisterStudentCommand request, CancellationToken ct)
    {
        await _rules.EmailCannotBeDuplicatedWhenRegistered(request.Email, ct);

        // 1 — Kullanıcı oluştur
        HashingHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        User user = new()
        {
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
        user = await _userRepository.AddAsync(user, ct);

        // 2 — Öğrenci profilini 1:1 oluştur
        Student student = new()
        {
            UserId       = user.Id,
            DailyWordGoal = request.DailyWordGoal,
            CreatedAt    = DateTime.UtcNow
        };
        student = await _studentRepository.AddAsync(student, ct);

        // 3 — Dil profili oluştur
        StudentLanguageProfile langProfile = new()
        {
            StudentId        = student.Id,
            NativeLanguageId = request.NativeLanguageId,
            TargetLanguageId = request.TargetLanguageId,
            TargetCefrLevel  = request.TargetCefrLevel,
            CreatedAt        = DateTime.UtcNow
        };
        // StudentLanguageProfile kendi repository'si yok — DbContext üzerinden ekliyoruz.
        // IStudentRepository'nin ek bir metod barındırmasından kaçınmak için,
        // lang profile'ı student navigation property üzerinden ekleyebiliriz;
        // ancak en temiz yol: IStudentLanguageProfileRepository eklemektir.
        // Bu sprintte StudentRepository'yi extend edeceğiz.
        await _studentRepository.AddLanguageProfileAsync(langProfile, ct);

        // 4 — Token üret
        AccessToken accessToken  = _tokenHelper.CreateToken(user, new List<OasisWords.Core.Security.Entities.OperationClaim>());
        RefreshToken refreshToken = _tokenHelper.CreateRefreshToken(user, string.Empty);
        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return new RegisterStudentResponse
        {
            UserId       = user.Id,
            StudentId    = student.Id,
            AccessToken  = accessToken,
            RefreshToken = refreshToken.Token
        };
    }
}
