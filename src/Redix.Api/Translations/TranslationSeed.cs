using System.Security.Cryptography;
using System.Text;

namespace Redix.Api.Translations;

/// <summary>The words every database starts with; a migration carries each new word to the database.</summary>
public static class TranslationSeed
{
    private static readonly (string Key, string English, string Arabic)[] Words =
    [
        ("app.title", "REDIX", "REDIX"),
        ("app.subtitle", "Project expenses", "مصروفات المشاريع"),
        ("signIn.username", "Username", "اسم المستخدم"),
        ("signIn.password", "Password", "كلمة المرور"),
        ("signIn.hint", "Sign in to follow your projects' expenses", "سجّل الدخول لمتابعة مصروفات مشاريعك"),
        ("signIn.submit", "Sign in", "دخول"),
        ("signIn.forgotPassword", "Forgot your password? Contact the financial supervisor.", "نسيت كلمة المرور؟ تواصل مع المشرف المالي."),
        ("signIn.invalidCredentials", "The username or password is wrong.", "اسم المستخدم أو كلمة المرور غير صحيحة."),
        ("signIn.accountLocked", "Too many wrong tries. Try again later.", "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً."),
        ("signIn.sessionExpired", "Your session ended. Sign in again.", "انتهت الجلسة. سجّل الدخول مرة أخرى."),
        ("signIn.noConnection", "No connection to the server. Try again.", "لا يوجد اتصال بالخادم. حاول مرة أخرى."),
        ("changePassword.title", "Change your password", "غيّر كلمة المرور"),
        ("changePassword.hint", "Choose a new password of at least 8 characters before you start.", "اختر كلمة مرور جديدة من 8 أحرف على الأقل قبل أن تبدأ."),
        ("changePassword.current", "Current password", "كلمة المرور الحالية"),
        ("changePassword.new", "New password", "كلمة المرور الجديدة"),
        ("changePassword.confirm", "New password again", "أعد كتابة كلمة المرور الجديدة"),
        ("changePassword.submit", "Save", "حفظ"),
        ("changePassword.mismatch", "The new passwords do not match.", "كلمتا المرور الجديدتان غير متطابقتين."),
        ("changePassword.invalidCredentials", "The current password is wrong.", "كلمة المرور الحالية غير صحيحة."),
        ("changePassword.accountLocked", "Too many wrong tries. Try again later.", "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً."),
        ("changePassword.weakPassword", "Use at least 8 characters, different from the current password.", "استخدم 8 أحرف على الأقل، مختلفة عن كلمة المرور الحالية."),
        ("changePassword.noConnection", "No connection to the server. Try again.", "لا يوجد اتصال بالخادم. حاول مرة أخرى."),
        ("myAccount.title", "My account", "حسابي"),
        ("myAccount.language", "Language", "اللغة"),
        ("myAccount.arabic", "العربية", "العربية"),
        ("myAccount.english", "English", "English"),
        ("myAccount.languageNeedsNetwork", "Switching the language needs a network.", "تغيير اللغة يحتاج إلى شبكة."),
        ("myAccount.changePassword", "Change password", "تغيير كلمة المرور"),
        ("settings.title", "Settings", "الإعدادات"),
        ("projects.title", "Projects", "المشاريع"),
        ("projects.new", "New project", "مشروع جديد"),
        ("projects.empty", "No projects yet. Create the first one.", "لا توجد مشاريع بعد. أنشئ أول مشروع."),
        ("projects.loadFailed", "Could not load the projects. Check the network and try again.", "تعذّر تحميل المشاريع. تحقق من الشبكة وحاول مرة أخرى."),
        ("projects.retry", "Try again", "حاول مرة أخرى"),
        ("projects.running", "Running", "جارٍ"),
        ("projects.finished", "Finished", "منتهٍ"),
        ("projects.readOnly", "Read-only", "للقراءة فقط"),
        ("projects.startDate", "Start date", "تاريخ البدء"),
        ("projects.name", "Project name", "اسم المشروع"),
        ("projects.units", "Units", "الوحدات"),
        ("projects.noUnits", "No units yet.", "لا توجد وحدات بعد."),
        ("projects.unitName", "Unit name", "اسم الوحدة"),
        ("projects.addUnit", "Add unit", "إضافة وحدة"),
        ("projects.removeUnit", "Remove unit", "حذف الوحدة"),
        ("projects.create", "Create project", "إنشاء المشروع"),
        ("projects.cancel", "Cancel", "إلغاء"),
        ("projects.close", "Close project", "إغلاق المشروع"),
        ("projects.closeQuestion", "Close this project?", "إغلاق هذا المشروع؟"),
        ("projects.closeExplain", "It becomes read-only: no new entries or approvals, while its reports stay available. This cannot be undone.", "يصبح للقراءة فقط: لا إدخال ولا اعتماد، وتبقى تقاريره متاحة. لا يمكن التراجع عن ذلك."),
        ("projects.invalidName", "Enter a name of 1 to 100 characters.", "اكتب اسماً من 1 إلى 100 حرف."),
        ("projects.projectFinished", "This project is finished, so it cannot change.", "هذا المشروع منتهٍ، فلا يمكن تغييره."),
        ("projects.noConnection", "No connection to the server. Try again.", "لا يوجد اتصال بالخادم. حاول مرة أخرى."),
        ("projects.refused", "The server refused this change.", "رفض الخادم هذا التغيير."),
    ];

    /// <summary>One key per seeded word, with an id derived from the key so every migration and database agree on it.</summary>
    public static IEnumerable<TranslationKey> Keys => Words.Select(word => new TranslationKey { Id = StableId(word.Key), Key = word.Key });

    /// <summary>Each seeded word in English and Arabic, with ids derived from the key and the language.</summary>
    public static IEnumerable<Translation> Values => Words.SelectMany(word => new[]
    {
        new Translation { Id = StableId($"{word.Key}:en"), TranslationKeyId = StableId(word.Key), Language = "en", Value = word.English },
        new Translation { Id = StableId($"{word.Key}:ar"), TranslationKeyId = StableId(word.Key), Language = "ar", Value = word.Arabic },
    });

    private static Guid StableId(string name) => new(SHA256.HashData(Encoding.UTF8.GetBytes(name)).AsSpan(0, 16));
}
