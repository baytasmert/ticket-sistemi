using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Services
{
    // Identity'nin varsayilan (Ingilizce) dogrulama mesajlarini Turkce'ye cevirir.
    // Sifre politikasi Program.cs'te TEK yerde tanimli; kullaniciya gosterilen
    // mesajlar da merkezi olarak buradan gelir.
    public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length) => new()
        {
            Code = nameof(PasswordTooShort),
            Description = $"Şifre en az {length} karakter olmalıdır."
        };

        public override IdentityError PasswordRequiresUpper() => new()
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "Şifre en az bir büyük harf içermelidir."
        };

        public override IdentityError PasswordRequiresLower() => new()
        {
            Code = nameof(PasswordRequiresLower),
            Description = "Şifre en az bir küçük harf içermelidir."
        };

        public override IdentityError PasswordRequiresDigit() => new()
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "Şifre en az bir rakam içermelidir."
        };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new()
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "Şifre en az bir özel karakter (örn. @$!%*?&) içermelidir."
        };

        public override IdentityError DuplicateEmail(string email) => new()
        {
            Code = nameof(DuplicateEmail),
            Description = $"'{email}' adresi zaten kayıtlı."
        };

        public override IdentityError DuplicateUserName(string userName) => new()
        {
            Code = nameof(DuplicateUserName),
            Description = $"'{userName}' kullanıcı adı zaten alınmış."
        };
    }
}
