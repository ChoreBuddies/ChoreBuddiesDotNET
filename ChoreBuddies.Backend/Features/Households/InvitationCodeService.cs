namespace Shared.Households;

public interface IInvitationCodeService
{
    Task<string> GenerateUniqueInvitationCodeAsync();
}

public class InvitationCodeService : IInvitationCodeService
{
    private readonly int _invitationCodeLength = 6;
    private readonly IHouseholdRepository _householdRepository;
    private const string _chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public InvitationCodeService(IHouseholdRepository householdRepository)
    {
        _householdRepository = householdRepository;
    }

    private string GenerateRandomInvitationCode()
    {
        var guidBytes = Guid.NewGuid().ToByteArray();
        long value = BitConverter.ToInt64(guidBytes, 0);
        var code = new char[_invitationCodeLength];

        for (int i = 0; i < _invitationCodeLength; i++)
        {
            code[i] = _chars[(int)(Math.Abs(value) % _chars.Length)];
            value /= _chars.Length;
        }

        return new string(code);
    }

    public async Task<string> GenerateUniqueInvitationCodeAsync()
    {
        const int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            var code = GenerateRandomInvitationCode();
            var exists = await _householdRepository.GetHouseholdByInvitationCodeAsync(code);
            if (exists == null)
                return code;
        }

        throw new InvalidOperationException("Generating unique invitation code failed.");
    }
}
