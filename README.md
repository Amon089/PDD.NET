# Habitto

## Descripción General

Habitto es una API de backend para una plataforma de alquiler de estadías de corto plazo. Implementa búsquedas de propiedades, reservas con control de disponibilidad, wishlist, autenticación JWT, verificación de identidad y reportes exportables a Excel.

## Problema de Negocio

Se busca permitir a huéspedes explorar inmuebles, reservar fechas disponibles sin solapamientos y habilitar a dueños a consultar su historial de reservas para generar reportes. El sistema también debe exigir verificación de identidad en la primera reserva.

## Objetivos de la Solución

- Proveer una API modular y mantenible.
- Garantizar que no existan reservas solapadas sobre un mismo inmueble.
- Registrar usuarios con credenciales seguras.
- Permitir verificación de identidad antes de la primera reserva.
- Mantener la lógica de notificaciones y KYC intercambiable.
- Exportar reportes de reservas en Excel.

## Arquitectura Implementada

Se utiliza una variante de Clean Architecture con 4 proyectos:

- `Habitto.Domain`: entidades, objetos de valor, excepciones y contratos de repositorio.
- `Habitto.Application`: casos de uso, commands/queries y puertos de servicios.
- `Habitto.Infrastructure`: EF Core, repositorios concretos, servicios mocks y exportación Excel.
- `Habitto.Api`: controllers, configuración de autenticación, Swagger y exposición HTTP.

### Capas y responsabilidades

- Domain: define las reglas de negocio y la invariante de dominio para reservas.
- Application: orquesta casos de uso, aplica reglas de negocio (por ejemplo, primer reserva con KYC) y envía notificaciones.
- Infrastructure: implementa acceso a datos y servicios externos/mock.
- Api: expone endpoints REST y maneja autorización.

### Flujo de dependencias

- `Habitto.Api` depende de `Habitto.Application` y `Habitto.Infrastructure`.
- `Habitto.Application` depende de `Habitto.Domain`.
- `Habitto.Infrastructure` depende de `Habitto.Domain` y `Habitto.Application`.
- Las abstracciones residen en `Habitto.Domain`/`Habitto.Application`, las implementaciones en `Habitto.Infrastructure`.

### Principios SOLID

- Single Responsibility: cada proyecto y clase tiene una responsabilidad clara.
- Open/Closed: servicios de KYC y notificaciones pueden reemplazarse sin editar la lógica de aplicación.
- Liskov/Substitution: las interfaces de repositorios permiten cambiar implementaciones.
- Interface Segregation: repositorios pequeños y específicos.
- Dependency Inversion: Application depende de interfaces, no de implementaciones.

### Patrones utilizados

- Clean Architecture/Onion Architecture.
- CQRS con MediatR.
- Repository Pattern.
- Dependency Injection.
- Domain-Driven Design ligero (aggregate root `Property`, value object `DateRange`).
- Adapter/Strategy para KYC y notificaciones.

## Estructura del Proyecto

```
./
├─ docker-compose.yml
├─ Dockerfile
├─ frontend/
├─ src/
│  ├─ Habitto.Api/
│  │  ├─ Controllers/
│  │  │  ├─ AuthController.cs
│  │  │  ├─ PropertiesAndBookingsControllers.cs
│  │  │  └─ WishlistIdentityReportsControllers.cs
│  │  ├─ appsettings.json
│  │  ├─ Habitto.Api.csproj
│  │  └─ Program.cs
│  ├─ Habitto.Application/
│  │  ├─ Bookings/Commands/CreateBookingCommand.cs
│  │  ├─ Common/Ports.cs
│  │  ├─ Identity/VerifyIdentityCommand.cs
│  │  ├─ Reports/GetOwnerBookingReportQuery.cs
│  │  ├─ Reports/GetOwnerBookingReportQueryHandler.cs
│  │  ├─ Wishlist/WishlistCommands.cs
│  │  └─ Habitto.Application.csproj
│  ├─ Habitto.Domain/
│  │  ├─ Entities/AppUser.cs
│  │  ├─ Entities/Booking.cs
│  │  ├─ Entities/Property.cs
│  │  ├─ Entities/WishlistItem.cs
│  │  ├─ Exceptions/DomainExceptions.cs
│  │  ├─ Interfaces/Repositories.cs
│  │  ├─ ValueObjects/DateRange.cs
│  │  ├─ ValueObjects/Enums.cs
│  │  └─ ValueObjects/StayPolicy.cs
│  └─ Habitto.Infrastructure/
│     ├─ DependencyInjection.cs
│     ├─ Habitto.Infrastructure.csproj
│     ├─ Migrations/
│     │  ├─ 20260623195743_InitialCreate.cs
│     │  ├─ 20260623195743_InitialCreate.Designer.cs
│     │  ├─ 20260623202601_FixCoordinatesPrecision.cs
│     │  ├─ 20260623202601_FixCoordinatesPrecision.Designer.cs
│     │  └─ HabittoDbContextModelSnapshot.cs
│     ├─ Persistence/
│     │  ├─ EntityConfigurations.cs
│     │  ├─ HabittoDbContext.cs
│     │  └─ Repositories/Repositories.cs
│     └─ Services/
│        ├─ ClosedXmlReportExporter.cs
│        ├─ MockIdentityVerificationService.cs
│        └─ MockNotificationService.cs
```

## Tecnologías Utilizadas

| Categoría | Tecnología | Uso |
|---|---|---|
| Plataforma | .NET 10 | Runtime y compilación |
| Lenguaje | C# | Implementación del backend |
| Arquitectura | Clean Architecture | Separación de capas |
| Persistencia | EF Core 9.0 | ORM para SQL Server |
| Base de datos | SQL Server 2022 | Contenedor de datos |
| Contenedores | Docker / Docker Compose | Orquestación de API y DB |
| Autenticación | JWT Bearer | Seguridad de endpoints |
| Hashing | BCrypt.Net-Next | Hash de contraseñas |
| Documentación | Swagger / Swashbuckle | API docs |
| Mediación | MediatR | CQRS patterns |
| Exportación | ClosedXML | Generación de Excel |
| Validación | FluentValidation | Paquete instalado (no utilizado actualmente) |

## Requisitos Previos

- Docker
- Docker Compose
- .NET 10 SDK (recomendado para compilación local)

## Configuración del Entorno

La configuración principal está en `src/Habitto.Api/appsettings.json`.
Si usa Docker, la cadena de conexión se sobrescribe desde `docker-compose.yml`.

## Variables de Entorno

- `ConnectionStrings__Default`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`

## Instalación Local

```bash
cd src/Habitto.Api
dotnet restore
dotnet build
```

Para ejecutar local:

```bash
dotnet run --project src/Habitto.Api/Habitto.Api.csproj
```

> Si ejecutas local, debes tener una instancia SQL Server disponible y ajustar la conexión en `appsettings.json`.

## Ejecución con Docker

```bash
docker compose up --build
```

Servicios:
- `sqlserver`: SQL Server 2022 en `localhost:1433`
- `api`: API en `localhost:8080`

## Migraciones

Migraciones existentes:
- `20260623195743_InitialCreate`
- `20260623202601_FixCoordinatesPrecision`

Para aplicar migraciones manualmente:

```bash
cd src/Habitto.Infrastructure
dotnet ef database update --startup-project ../Habitto.Api
```

> `Program.cs` tiene la llamada a `db.Database.Migrate()` comentada, por lo que no se aplica automáticamente al iniciar.

## Base de Datos

- Motor: SQL Server
- Base: `HabittoDb`
- Usuario: `sa`
- Contraseña: `Habitto#2026Strong`

> No hay soporte PostgreSQL en el código actual.

## Credenciales de Prueba

No hay datos de prueba precargados. Usa `/api/auth/register` para crear un usuario.

## Endpoints Disponibles

| Método | Ruta | Autorización | Descripción |
|---|---|---|---|
| POST | `/api/auth/register` | No | Registrar usuario |
| POST | `/api/auth/login` | No | Login y token JWT |
| GET | `/api/properties` | No | Buscar propiedades |
| GET | `/api/properties/{id}` | No | Detalle de propiedad |
| POST | `/api/properties` | Sí | Crear propiedad |
| DELETE | `/api/properties/{id}` | Sí | Desactivar propiedad |
| POST | `/api/bookings` | Sí | Crear reserva |
| GET | `/api/bookings/user/{userId}` | Sí | Reservas de usuario |
| POST | `/api/wishlist` | Sí | Agregar a wishlist |
| DELETE | `/api/wishlist` | Sí | Remover wishlist |
| GET | `/api/wishlist/{userId}` | Sí | Consultar wishlist |
| POST | `/api/identity/verify` | Sí | Verificar identidad |
| GET | `/api/reports/owner/{ownerId}` | Sí | Reporte de reservas |
| GET | `/api/reports/owner/{ownerId}/excel` | Sí | Exportar reporte Excel |

## Casos de Uso Implementados

- Registro y login con JWT.
- Búsqueda de propiedades con filtros por ciudad y fechas.
- Creación y desactivación de propiedades.
- Reservas con cálculo de precio y validación de disponibilidad.
- Prevención de double-booking dentro del agregado `Property`.
- Verificación de identidad en la primera reserva.
- Wishlist CRUD.
- Reportes de dueños y exportación a Excel.
- Notificaciones mock (email logeado e in-app persistido).

## Flujo de Negocio

1. El usuario se registra y obtiene un token JWT.
2. Busca inmuebles por ciudad y disponibilidad.
3. Si es la primera reserva, necesita identidad aprobada.
4. Se crea la reserva solo si no hay solapamientos.
5. Se envía notificación mock al usuario.
6. El dueño puede consultar y exportar reportes.

## Decisiones Técnicas

- Manejo de reservas: la creación vive en `Property.CreateBooking`, preservando la invariante de dominio.
- Prevención de double-booking: `DateRange.Overlaps` define solapamiento entre reservas.
- Seguridad: JWT en `Program.cs`, hashing con BCrypt y autorización de endpoints.
- KYC: implementado como puerto, actualmente con mock en `MockIdentityVerificationService`.
- Dashboard: no existe UI de dashboard; hay endpoints de reporte.
- Exportaciones: `ClosedXmlReportExporter` genera archivos Excel.
- Notificaciones: `MockNotificationService` simula email y notificaciones in-app.

## Seguridad

- Contraseñas almacenadas con BCrypt.
- Autenticación JWT.
- CORS está configurado como `AllowAnyOrigin`, útil para demo pero no recomendado en producción.
- No existe verificación de propiedad explícita en todos los endpoints; se confía en el `UserId` enviado por el cliente.

## Observabilidad

- Swagger habilitado en `/swagger`.
- Logging básico para notificaciones mock.
- No hay métricas ni seguimiento distribuido implementados.

## Mejoras Implementadas

- Clean Architecture con separación de capas.
- Invariante de dominio para reservas.
- Servicios intercambiables de KYC y notificaciones.
- Reportes exportables.
- Autenticación y manejo de usuarios.

## Mejoras Futuras

- Añadir pruebas unitarias e integración.
- Habilitar migraciones automáticas o documentar la aplicación manualmente.
- Optimizar reportes para evitar N+1.
- Reemplazar mocks de KYC y notificaciones con integraciones reales.
- Restringir recursos por claims de usuario.
- Añadir validación de entrada con FluentValidation.
- Implementar seeders de datos de ejemplo.

## Conclusiones

El proyecto está bien diseñado a nivel de arquitectura y cubre la mayoría de los requisitos principales. Sin embargo, hay oportunidades claras de mejora en pruebas, seguridad de autorización, automatización de migraciones y reemplazo de mocks por integraciones reales.
