using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;

namespace NovaSortPro.Services
{
    public class LocalizationService
    {
        private string _currentLanguage = "en-US";
        public event EventHandler? LanguageChanged;

        private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            ["en-US"] = new()
            {
                ["AppName"] = "NovaSort Pro",
                ["SmartOrganizer"] = "Smart Organizer",
                ["Analyzer"] = "System Analyzer",
                ["Rules"] = "Smart Rules",
                ["Bookmarks"] = "Bookmarks",
                ["Profiles"] = "Profiles",
                ["History"] = "History & Undo",
                ["Settings"] = "Settings",
                ["SelectFolder"] = "Select Folder",
                ["SelectDrive"] = "Select Drive",
                ["Scan"] = "Scan Folder",
                ["Analyze"] = "Analyze System",
                ["Apply"] = "Apply sorting",
                ["Undo"] = "Undo Operation",
                ["Search"] = "Search files...",
                ["Filter"] = "Filter by",
                ["Name"] = "Name",
                ["Size"] = "Size",
                ["Extension"] = "Extension",
                ["Date"] = "Date",
                ["Ascending"] = "Ascending",
                ["Descending"] = "Descending",
                ["Sorting"] = "Sorting",
                ["Category"] = "Category",
                ["TotalFiles"] = "Total Files",
                ["TotalSize"] = "Total Size",
                ["EstimatedTime"] = "Estimated Time",
                ["DuplicateFiles"] = "Duplicate Files",
                ["EmptyFiles"] = "Empty Files",
                ["EmptyFolders"] = "Empty Folders",
                ["LargeFiles"] = "Large Files",
                ["OldFiles"] = "Old Files",
                ["UnknownExtensions"] = "Unknown Extensions",
                ["ConfirmationNeeded"] = "Confirmation Needed",
                ["ConfirmMessage"] = "Are you sure you want to proceed with sorting? This will rearrange files inside your selected directory.",
                ["ConflictResolution"] = "Conflict Resolution",
                ["ConflictMessage"] = "A file with the same name already exists in the destination. Please select an action:",
                ["Rename"] = "Rename (Keep both)",
                ["Replace"] = "Replace (Overwrite)",
                ["Skip"] = "Skip (Do nothing)",
                ["AskEveryTime"] = "Ask Every Time",
                ["Export"] = "Export Rules",
                ["Import"] = "Import Rules",
                ["Language"] = "Language",
                ["Theme"] = "UI Theme",
                ["Dark"] = "Dark",
                ["Light"] = "Light",
                ["System"] = "System Default",
                ["English"] = "English",
                ["Persian"] = "Farsi (فارسی)",
                ["Ready"] = "Ready",
                ["Completed"] = "Operation completed successfully!",
                ["UndoSuccess"] = "Last operation undone successfully!",
                ["UndoError"] = "Failed to revert files. Some files may have been deleted or moved.",
                ["AddRule"] = "Add Custom Rule",
                ["Favorites"] = "Favorites",
                ["Recents"] = "Recents",
                ["Pinned"] = "Pinned",
                ["ExportLogs"] = "Export Logs",
                ["TotalFolders"] = "Total Folders",
                ["LargestFolder"] = "Largest Folder",
                ["LargestFile"] = "Largest File",
                ["ExtensionStats"] = "Extension Distribution",
                ["OfflineDisclaimer"] = "NovaSort Pro runs 100% offline. Your files are safe and never uploaded."
            },
            ["fa-IR"] = new()
            {
                ["AppName"] = "نووا سورت پرو",
                ["SmartOrganizer"] = "سازماندهی هوشمند",
                ["Analyzer"] = "آنالیزور سیستم",
                ["Rules"] = "قوانین هوشمند",
                ["Bookmarks"] = "نشانک‌ها",
                ["Profiles"] = "پروفایل‌ها",
                ["History"] = "تاریخچه و واگرد",
                ["Settings"] = "تنظیمات",
                ["SelectFolder"] = "انتخاب پوشه",
                ["SelectDrive"] = "انتخاب درایو",
                ["Scan"] = "اسکن پوشه",
                ["Analyze"] = "آنالیز پوشه",
                ["Apply"] = "اعمال مرتب‌سازی",
                ["Undo"] = "واگرد عملیات",
                ["Search"] = "جستجوی فایل‌ها...",
                ["Filter"] = "فیلتر بر اساس",
                ["Name"] = "نام",
                ["Size"] = "اندازه",
                ["Extension"] = "پسوند",
                ["Date"] = "تاریخ",
                ["Ascending"] = "صعودی",
                ["Descending"] = "نزولی",
                ["Sorting"] = "مرتب‌سازی",
                ["Category"] = "دسته‌بندی",
                ["TotalFiles"] = "تعداد کل فایل‌ها",
                ["TotalSize"] = "حجم کل",
                ["EstimatedTime"] = "زمان تخمینی عملیات",
                ["DuplicateFiles"] = "فایل‌های تکراری",
                ["EmptyFiles"] = "فایل‌های خالی",
                ["EmptyFolders"] = "پوشه‌های خالی",
                ["LargeFiles"] = "فایل‌های بزرگ",
                ["OldFiles"] = "فایل‌های قدیمی",
                ["UnknownExtensions"] = "پسوندهای ناشناخته",
                ["ConfirmationNeeded"] = "نیاز به تایید کاربر",
                ["ConfirmMessage"] = "آیا مطمئن هستید که می‌خواهید مرتب‌سازی را شروع کنید؟ این عملیات فایل‌های پوشه انتخابی را بازآرایی می‌کند.",
                ["ConflictResolution"] = "حل تداخل نام فایل",
                ["ConflictMessage"] = "فایلی با همین نام در مقصد وجود دارد. لطفاً یک اقدام را انتخاب کنید:",
                ["Rename"] = "تغییر نام (نگهداری هر دو)",
                ["Replace"] = "جایگزینی (بازنویسی)",
                ["Skip"] = "نادیده گرفتن (بدون تغییر)",
                ["AskEveryTime"] = "هر بار سوال شود",
                ["Export"] = "خروجی گرفتن قوانین",
                ["Import"] = "وارد کردن قوانین",
                ["Language"] = "زبان سیستم",
                ["Theme"] = "پوسته رابط کاربری",
                ["Dark"] = "تاریک",
                ["Light"] = "روشن",
                ["System"] = "پیش‌فرض سیستم",
                ["English"] = "English",
                ["Persian"] = "فارسی",
                ["Ready"] = "آماده به کار",
                ["Completed"] = "عملیات با موفقیت انجام شد!",
                ["UndoSuccess"] = "آخرین عملیات با موفقیت واگرد شد!",
                ["UndoError"] = "خطا در بازگردانی فایل‌ها. ممکن است فایل‌ها جابجا یا حذف شده باشند.",
                ["AddRule"] = "افزودن قانون سفارشی",
                ["Favorites"] = "علاقه‌مندی‌ها",
                ["Recents"] = "اخیر",
                ["Pinned"] = "پین شده",
                ["ExportLogs"] = "خروجی گزارش‌ها",
                ["TotalFolders"] = "تعداد کل پوشه‌ها",
                ["LargestFolder"] = "بزرگترین پوشه",
                ["LargestFile"] = "بزرگترین فایل",
                ["ExtensionStats"] = "توزیع پسوندها",
                ["OfflineDisclaimer"] = "برنامه نووا سورت کاملاً آفلاین کار می‌کند. فایل‌های شما در امنیت کامل هستند."
            }
        };

        public LocalizationService()
        {
            // Detect system language or default to Persian or English
            var currentCulture = CultureInfo.CurrentUICulture.Name;
            if (currentCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase))
            {
                _currentLanguage = "fa-IR";
            }
            else
            {
                _currentLanguage = "en-US";
            }
        }

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value && _translations.ContainsKey(value))
                {
                    _currentLanguage = value;
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string Get(string key)
        {
            if (_translations[_currentLanguage].TryGetValue(key, out var val))
            {
                return val;
            }
            if (_translations["en-US"].TryGetValue(key, out var fallback))
            {
                return fallback;
            }
            return key;
        }

        public FlowDirection LayoutDirection => _currentLanguage == "fa-IR" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        public string FontFamilyName => _currentLanguage == "fa-IR" ? "ms-appx:///Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn" : "Segoe UI";

        /// <summary>
        /// Converts Latin numbers to Persian digits if Farsi is selected.
        /// </summary>
        public string LocalizeNumbers(string input)
        {
            if (_currentLanguage != "fa-IR") return input;
            
            var sb = new StringBuilder();
            foreach (var ch in input)
            {
                if (ch >= '0' && ch <= '9')
                {
                    sb.Append((char)(ch + 1728)); // Offset to Persian digits in Unicode
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        public string FormatDate(DateTime date)
        {
            if (_currentLanguage == "fa-IR")
            {
                var pc = new PersianCalendar();
                return $"{pc.GetYear(date)}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00} {date.Hour:00}:{date.Minute:00}";
            }
            return date.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
