# F1.Web — Formula One starter Razor Pages site

CLI commands used to scaffold (run locally in your workspace):

1. dotnet new webapp -o src/F1.Web
2. dotnet new sln -n F1Site
3. dotnet sln add src/F1.Web
4. dotnet add src/F1.Web package Markdig
5. dotnet add src/F1.Web package Serilog.AspNetCore

How to run:

```powershell
dotnet restore
dotnet build
dotnet run --project src/F1.Web
```

Visit http://localhost:5000 (or the URL printed by dotnet run).

Tailwind: this starter uses the CDN for quick styling. For a full Tailwind build (recommended for production):

- npm init -y
- npm install -D tailwindcss postcss autoprefixer
- npx tailwindcss init
- add tailwind directives to a CSS file and build via PostCSS.

TODOs (production):
- Replace file-based newsletter/contact with a real service (SendGrid/Mailgun) — see // TODO comments in services.
- Replace placeholder images with optimized assets and use a CDN.
- Add server-side DB if you need persistence beyond filesystem.
