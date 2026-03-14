using MediatR;
using OasisWords.Application.Services.AuthService;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.UserService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;
using OasisWords.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OasisWords.Application.Features.Auth.Commands.Register
{
    internal class RegisterStudentCommand: IRequest<RegisterResponse>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterStudentResponse
    {
        public AccessToken AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, RegisterResponse>
    {
        private readonly IAuthService _authService;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserOperationClaimRepository _userOperationClaimRepository;
        private readonly IOperationClaimRepository _operationClaimRepository;

        public RegisterStudentCommandHandler(
            IAuthService authService,
            IStudentRepository studentRepository,
            IUserOperationClaimRepository userOperationClaimRepository,
            IOperationClaimRepository operationClaimRepository)
        {
            _authService = authService;
            _studentRepository = studentRepository;
            _userOperationClaimRepository = userOperationClaimRepository;
            _operationClaimRepository = operationClaimRepository;
        }

        public async Task<RegisterResponse> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
        {
            // 1. Şifreyi Hashle ve Base User'ı oluştur
            HashingHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);
            User user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            // 2. AuthManager üzerinden User'ı veritabanına kaydet (Email kontrolü vs. burada yapılır)
             await _authService.RegisterAsync(user, cancellationToken);
            Guid createdUser = user.Id;

            // 3. Öğrenci (Student) profilini 1:1 olarak bağla
            Student student = new Student
            {
                UserId = createdUser
            };
            await _studentRepository.AddAsync(student, cancellationToken);

            // 4. "Student" yetkisini (Claim) ata
            OperationClaim? studentClaim = await _operationClaimRepository.GetAsync(c => c.Name == "Student");
            if (studentClaim != null)
            {
                await _userOperationClaimRepository.AddAsync(new UserOperationClaim
                {
                    UserId = createdUser,
                    OperationClaimId = studentClaim.Id
                }, cancellationToken);
            }
            AccessToken accessToken = _authService.CreateAccessTokenAsync(user, cancellationToken);
            RefreshToken refreshToken = _tokenHelper.CreateRefreshToken(createdUser, string.Empty);
            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            // 5. Token Üret ve Dön
            return await _authService.CreateTokensForUserAsync(createdUser, cancellationToken);
        }
    }
}
