# EVIA — A Smart Online Fashion Platform

ASP.NET Core MVC implementation of the FYDP proposal "Evia: A Smart Online Fashion Platform"
(Ayesha Javaid, Isha Zubair, Kalsoom Ghaffar — FCIT, University of the Punjab).

Built using the exact stack from the proposal's Tools & Technologies table:
- **Frontend:** HTML, CSS, jQuery (Razor views)
- **Backend:** ASP.NET Core (.NET 8, MVC)
- **Database:** SQL Server via EF Core
- **Dev tool:** Visual Studio / VS Code

## What's included

| Area | Description |
|---|---|
| `Home` | Landing page with hero banner and featured products |
| `Shop` | Product catalog with category/style/search filters |
| `Design` | The "Design Your Own" feature — your original `design.html` rebuilt as a Razor view, with live jQuery preview wired to a real `Design/Save` API endpoint that persists to SQL Server |
| `Account` (Identity) | Login / Register / Logout, via ASP.NET Core Identity, with Role support |
| `Cart` / `Checkout` | Session-based cart, order placement, confirmation page |
| `About` / `Contact` | Static info page + a working contact form saved to the database |
| `Admin` (area) | Admin-only dashboard: add/edit/delete products with real image upload, view orders and contact messages |
| AI outfit preview | "Generate AI Preview" button on Design Your Own turns the selected fabric/color/pattern/style into a real generated product image |

Database tables: `Products`, `CustomDesigns`, `Orders`, `OrderItems`, `ContactMessages`, plus
the standard ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles`, etc.).

## Admin panel

On first run, the app seeds an `Admin` role and a default admin account:

- **Email:** `admin@evia.com`
- **Password:** `Admin@123`

Log in with these credentials, then click **Admin** in the navbar (only visible to admins) to
reach `/Admin/Dashboard`. From there:

- **Products** — add a new product with name, category, style, price, stock, and an actual
  image file (uploaded to `wwwroot/images/products/<guid>.ext` and referenced dynamically —
  nothing is hardcoded). Edit or delete existing products the same way.
- **Orders** — see every order placed by customers.
- **Messages** — see everything submitted through the Contact form.

**Change the default admin password** (or remove the seeding block in `Program.cs`) before
deploying anywhere public.

## AI-generated outfit previews

The "Design Your Own" page now has a **Generate AI Preview** button next to Save. Here's how
it's wired up, and how to make it fully live:

1. `Services/IAiDesignService.cs` defines a small interface: given the customer's style,
   fabric, color, pattern, and size, return an image URL.
2. `Services/OpenAiDesignService.cs` is the default implementation. It builds a natural-language
   prompt from the selections (e.g. *"a casual outfit, made of cotton fabric, in #ff9800 color,
   with a subtle dots pattern..."*) and calls OpenAI's image generation endpoint (DALL·E 3).
3. `DesignController.GenerateAiPreview` exposes this as `POST /Design/GenerateAiPreview`, called
   via jQuery `$.ajax` from `Views/Design/Index.cshtml`. The returned image replaces the CSS-box
   preview inside the outfit mockup.
4. When you click **Save Outfit Design**, the generated image URL (if any) is stored alongside
   the design in `CustomDesigns.GeneratedImageUrl`, and shown later on the **My Designs** page.

**To make it actually generate images:**
- Get an API key from your chosen provider (OpenAI is wired up by default; Stability AI,
  Replicate, or Azure OpenAI work the same way — just write a new class implementing
  `IAiDesignService` and swap the registration in `Program.cs`).
- Set it locally with `dotnet user-secrets set AiDesign:ApiKey "sk-..."` (don't put real keys
  in `appsettings.json`), or via the `AiDesign__ApiKey` environment variable in production.
- That's the only change needed — no controller or view code has to be touched.

If no API key is configured, the button will return a friendly "AI image generation isn't
configured yet" message instead of failing silently.

### Other ways to integrate an AI model here (for your report/defense)

- **Image generation** (what's implemented): DALL·E 3, Stable Diffusion (via Stability AI or
  Replicate), or Azure OpenAI's image models — turns selections into a rendered garment photo.
- **Image-to-image / virtual try-on**: services like Replicate's IDM-VTON or similar models can
  overlay the generated/selected garment onto a photo the customer uploads, for a closer-to-real
  preview than a flat product shot.
- **Text/description generation**: call a text model (e.g. the Anthropic or OpenAI chat API) to
  auto-write a product description or styling tip based on the customer's selections — cheap to
  add using the same `IAiDesignService`-style pattern.
- **Recommendation**: use embeddings or a simple rules engine over `Products`/`CustomDesigns`
  data to suggest existing catalog items similar to what a customer just designed.

## Running it

You'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download) and SQL Server
(LocalDB, which ships with Visual Studio, works fine for development).

```bash
cd Evia.Web

# Restore & build
dotnet restore
dotnet build

# Create the database (first time only)
dotnet tool install --global dotnet-ef   # if you don't already have it
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run
dotnet run
```

Then open the URL shown in the console (e.g. `https://localhost:5001`).

If you don't have LocalDB, update the `DefaultConnection` string in
`appsettings.json` to point at your own SQL Server instance.

## Notes for your defense / report

- The **Design Your Own** page is your original HTML/CSS/jQuery, almost unchanged visually —
  only the "Save" button now calls a real backend endpoint (`POST /Design/Save`) instead of
  just showing a message, and the saved design (color, style, fabric, pattern, size,
  measurements, estimated price) is written to the `CustomDesigns` table in SQL Server.
- A simple price estimator (`DesignController.EstimatePrice`) combines a base price per style
  with a fabric surcharge — feel free to tune the numbers or replace it with a real pricing model.
- Authentication uses ASP.NET Core Identity's default scaffolded UI, so Login/Register pages
  work out of the box without any extra Razor Pages files.
- The cart is session-based (no login required to browse/add items), but checkout requires
  login, matching a typical e-commerce flow.
- This is a working scaffold meant to demonstrate the architecture end-to-end; you'll likely
  want to add real product photography, refine validation messages, and expand the admin side
  (e.g. an admin area for managing products) before a final submission/demo.
