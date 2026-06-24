# Habitto — Plataforma de Rentas Cortas (Prueba ASP.NET C#)

## Requisitos previos
- Docker y Docker Compose instalados.
- (Solo si vas a generar la migración inicial, ver paso 2) SDK de .NET 10.

## Cómo levantar el proyecto

```bash
docker compose up --build
```

Esto levanta:
- `sqlserver`: SQL Server 2022 en contenedor (puerto 1433), con healthcheck.
- `api`: la API .NET 10 (puerto 8080), que espera a que SQL Server esté *healthy* y aplica las migraciones de EF Core automáticamente al iniciar (`db.Database.Migrate()` en `Program.cs`).

Swagger disponible en: `http://localhost:8080/swagger`

## ⚠️ Paso manual pendiente: migración inicial de EF Core

Este entorno de generación de código no tuvo el SDK de .NET instalado, por lo
que **no se generó la carpeta `Migrations` con la migración inicial**. Antes de
tu primer `docker compose up`, corre esto una sola vez en tu máquina:

```bash
cd src/Habitto.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Habitto.Api
```

Si no tienes la herramienta `dotnet-ef`:
```bash
dotnet tool install --global dotnet-ef
```

Sin este paso, `db.Database.Migrate()` no tendrá nada que aplicar y la API
fallará al iniciar por falta de tablas.

## Arquitectura

Clean Architecture en 4 proyectos, con dependencias apuntando siempre hacia el centro:

```
Habitto.Api  -> Habitto.Application -> Habitto.Domain
       \-> Habitto.Infrastructure -------/
```

- **Domain**: entidades (`Property`, `Booking`, `AppUser`, `WishlistItem`), Value Objects (`DateRange`, `StayPolicy`) e invariantes de negocio. Cero dependencias externas.
- **Application**: casos de uso con MediatR (Commands/Queries), puertos (`IIdentityVerificationService`, `INotificationService`) que Infrastructure implementa.
- **Infrastructure**: EF Core + SQL Server, repositorios, mocks de KYC/notificaciones, exportador Excel (ClosedXML).
- **Api**: controllers delgados, JWT, Swagger.

### Decisiones técnicas relevantes

1. **No-double-booking como invariante de dominio, no como validación de capa superior.**
   La regla vive en `PropertyBookingExtensions.CreateBooking` (extensión sobre el aggregate root `Property`), usando `DateRange.Overlaps`. Cualquier punto de entrada que cree una reserva pasa por este único camino — no es posible *saltarse* la regla desde un controller descuidado.

2. **KYC y Notificaciones como puertos reemplazables (Strategy/Adapter).**
   `IIdentityVerificationService` e `INotificationService` viven en Application. Hoy están implementados como mocks en Infrastructure (`MockIdentityVerificationService`, `MockNotificationService`). Pasar a un proveedor real (Azure Face API, SendGrid, un microservicio en Node/Laravel) es agregar una clase nueva y cambiar 2 líneas de registro en `DependencyInjection.cs` — Domain y Application no se tocan.

3. **Auth diferida.** `PropertiesController.Search` es `[AllowAnonymous]`; `BookingsController`, `WishlistController` e `IdentityController` exigen `[Authorize]`. Esto refleja literalmente el requerimiento: explorar sin login, autenticarse solo al reservar/favoritear permanente/pagar.

4. **Privacidad del documento de identidad.** El flujo de KYC procesa la imagen en memoria y nunca la persiste (ver comentario en `VerifyIdentityCommandHandler`). No hay tabla de "documentos subidos".

### Limitaciones conocidas (transparencia, no se escondieron)

- `CreateBookingCommandHandler.HasAnyPreviousBookingAsync` está **stub** (`return false` siempre). La regla de "KYC obligatorio solo en la primera reserva" está modelada pero la detección de "primera reserva" no quedó conectada a una consulta real por falta de tiempo (alcance de 8h). Para completarla: agregar `IBookingRepository.HasAnyBookingByUserAsync(userId)`.
- `GetOwnerBookingReportQueryHandler` resuelve usuario e inmueble por reserva en un loop (N+1). Aceptable para el volumen de datos académico; en producción se reemplazaría por un `Include`/proyección SQL directa.
- No se incluyeron pruebas unitarias por restricción de tiempo. El candidato natural a testear primero sería `DateRange.Overlaps` y `PropertyBookingExtensions.CreateBooking` (es donde vive el riesgo real de negocio).
- Las versiones de paquetes NuGet (EF Core 9.0.0 contra `net10.0`, ClosedXML, MediatR) se escribieron de memoria sin poder ejecutar `dotnet restore` en este entorno — si al restaurar localmente alguna versión no existe en NuGet, sube/baja el patch version, la API pública no debería cambiar.
