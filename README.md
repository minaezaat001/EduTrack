# EduTrack 🎓

نظام إدارة المساقات والواجبات التعليمية مبني باستخدام **ASP.NET Core MVC** و **Entity Framework Core** و **ASP.NET Core Identity**.

مشروع تدريبي مخصص لطلاب **Core Academy** تحت إشراف **م. مينا عزت**.

---

## 🚀 المميزات الرئيسية (Features)

- **نظام الحسابات والصلاحيات (Authentication & Authorization):**
  - تسجيل الدخول وإنشاء حساب للطلاب (`Student`) والمحاضرين (`Instructor`).
  - دعم الأدوار والصلاحيات عبر `ASP.NET Core Identity`.
- **إدارة الدورات التدريبية (Courses Management):**
  - عرض الدورات وتفاصيلها، إضافة وتعديل الكورسات وربطها بالمحاضرين.
- **إدارة المحاضرين (Instructors):**
  - عرض قائمة المحاضرين وملفاتهم الشخصية مع رفع الصور.
- **إدارة التكليفات والواجبات (Assignments & Submissions):**
  - إضافة الواجبات للكورسات مع رفع ملفات التكليف وتحديد الموعد النهائي (Deadline).
  - تسليم الطلاب للحلول وتصحيحها ووضع الدرجات والملاحظات من قبل المحاضر.
- **واجهة مستخدم عصرية (Modern RTL UI):**
  - تصميم متجاوب يدعم اللغة العربية (RTL) بالكامل باستخدام Bootstrap 5 وأيقونات Bootstrap Icons.

---

## 🛠️ التقنيات المستخدمة (Tech Stack)

- **Backend:** C# / .NET (ASP.NET Core MVC)
- **Database:** Microsoft SQL Server عبر Entity Framework Core (Code First & Migrations)
- **Security:** ASP.NET Core Identity
- **Frontend:** Razor Views (.cshtml), Bootstrap 5 (RTL), CSS3, JavaScript

---

## 📂 هيكل المشروع (Project Structure)

```
EduTrack/
├── Controllers/       # متحكمات النظام (Courses, Instructors, Assignments, Submissions, Account)
├── Data/              # AppDbContext وإعدادات EF Core
├── Models/            # كلاسات الكيانات (User, Course, Instructor, Student, Assignment, Submission)
├── ViewModels/        # نماذج العرض وإدخال البيانات ورفع الملفات
├── Views/             # شاشات العرض وقوالب Razor (RTL Bootstrap 5)
├── Migrations/        # ترحيلات قاعدة البيانات
└── wwwroot/           # الملفات الثابتة (CSS, الصور، مرفوعات الواجبات والحلول)
```

---

## ⚙️ طريقة التشغيل (Getting Started)

1. **استنساخ المستودع (Clone):**
   ```bash
   git clone https://github.com/minaezaat001/EduTrack.git
   ```

2. **ضبط اتصال قاعدة البيانات (Connection String):**
   - افتح ملف `EduTrack/appsettings.json` وتأكد من سلسلة الاتصال:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=EduTrackDbFinal;Trusted_Connection=True;TrustServerCertificate=True;"
     }
     ```

3. **تطبيق ترحيلات قاعدة البيانات (Run Migrations):**
   ```bash
   dotnet ef database update --project EduTrack
   ```

4. **تشغيل المشروع:**
   ```bash
   dotnet run --project EduTrack
   ```
