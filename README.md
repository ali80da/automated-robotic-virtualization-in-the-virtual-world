








# Docker Web Dashboard

📦 A Cross-Platform ASP.NET Core MVC Application to Monitor Docker Containers, Show Live Logs, and Visualize Container Stats.

🌐 یک برنامه‌ی چندسکویی ASP.NET Core MVC برای مانیتورینگ کانتینرهای Docker، نمایش لاگ زنده، و مشاهده‌ی گرافیکی وضعیت آن‌ها.

---

## 🎯 About This Project / درباره‌ی این پروژه

### 🇬🇧 English

This project is part of the **AutomationSolution** initiative and is the foundation for a broader system:

> A smart, simple, and professional solution that allows developers to run and manage their Dockerized tools and services across **any operating system** (Windows, Linux, macOS) using a unified and intelligent UI.

It enables developers to:
- Run needed applications/services in containers automatically
- Monitor running containers visually
- Access live logs and port links
- Manage environments efficiently across platforms

> ✅ **This is just the beginning.** More advanced features like container lifecycle management, user access control, environment tagging, and smart automation are on the way.

---

### 🇮🇷 فارسی

این پروژه بخشی از سیستم هوشمند **AutomationSolution** است و به عنوان پایه‌ای برای یک پلتفرم گسترده‌تر طراحی شده:

> یک راهکار هوشمند، ساده و حرفه‌ای برای اینکه توسعه‌دهندگان بتوانند ابزارها و سرویس‌های موردنیاز خود را در قالب کانتینر، روی هر سیستم‌عاملی (ویندوز، لینوکس، مک) به‌راحتی اجرا، مانیتور و مدیریت کنند.

ویژگی اصلی این سیستم:
- اجرای خودکار اپلیکیشن‌ها در داکر
- مانیتورینگ گرافیکی کانتینرهای در حال اجرا
- دسترسی به لاگ زنده و پورت‌ها
- مدیریت ساده محیط توسعه در هر سیستم‌عاملی

> ✅ **این تازه شروع کار است.** در آینده قابلیت‌هایی مانند مدیریت چرخه‌ی کانتینر، سطح دسترسی کاربران، گروه‌بندی سرویس‌ها و اتوماسیون هوشمند به سیستم افزوده خواهد شد.

---

## 🚀 Features / امکانات

- 📊 Graphical dashboard showing total, running, and exited containers
- 📡 Real-time log streaming using SSE
- 📈 Image usage chart (Pie)
- ✅ Cross-platform: works on **Windows** (via `npipe`) and **Linux/macOS** (`unix socket`)
- 🔐 Secure-by-default: readonly Docker access, clean separation

---

## 🛠 Under Development / در حال توسعه

This project is actively evolving, with new features being gradually added to improve usability, automation, and control.

📌 **Planned Features:**

✅ **Start / Stop / Restart / Remove containers**  
  Full control over your running Docker environments via UI

✅ **Role-Based Access Control**  
  Manage user permissions and access levels securely

✅ **Search, Filtering & Grouping**  
  Easily find and organize containers based on names, tags, or images

✅ **Tagging & Environment Templates**  
  Define reusable development/staging environments with custom labels

✅ **Notifications & Health Monitoring**  
  Smart alerts, resource usage tracking, and container uptime status

---

این پروژه به‌صورت فعال در حال توسعه است و امکانات جدید به‌مرور اضافه می‌شوند تا مدیریت داکر را هوشمندتر، امن‌تر و حرفه‌ای‌تر کنند.

📌 **قابلیت‌های در دست توسعه:**

✅ **مدیریت کامل کانتینرها (شروع / توقف / ری‌استارت / حذف)**  
  کنترل کامل از طریق رابط کاربری

✅ **سطح‌بندی دسترسی کاربران**  
  تعیین سطح دسترسی برای کاربران مختلف به صورت امن

✅ **جستجو، فیلتر و گروه‌بندی کانتینرها**  
  پیدا کردن و دسته‌بندی سریع کانتینرها بر اساس نام، برچسب یا ایمیج

✅ **قالب‌سازی محیط‌ها و تگ‌گذاری**  
  ساخت محیط‌های توسعه/آزمایش قابل استفاده مجدد با تنظیمات اختصاصی

✅ **اعلان‌ها و مانیتورینگ سلامت کانتینرها**  
  هشدار هوشمند، نمایش وضعیت منابع و زمان فعالیت کانتینرها


## ⚙️ Installation & Run

### 💻 Run via Docker (Linux/macOS)

```bash
git clone https://github.com/ali80da/virtualization-in-the-virtual-world.git
cd virtualization-in-the-virtual-world
docker-compose up --build


```




