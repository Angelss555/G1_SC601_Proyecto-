# Avances 1 y 2

## Tipos de datos y relaciones

Las entidades se mapean mediante Entity Framework a SQL Server. Las claves identificadoras son `int`; las claves de ASP.NET Identity son `nvarchar(128)`; los textos tienen el límite `nvarchar` indicado por `StringLength`; los precios son `decimal(10,2)`; las fechas son `datetime2` y los indicadores son `bit`. `CuposDisponibles` y `CupoMaximo` son `int`.

La tabla `ECO_RESENA_TB` contiene `ResenaId int`, `UsuarioId nvarchar(128)`, `TourId int`, `Calificacion int`, `Comentario nvarchar(1000)` y `Fecha datetime2`. La migración `AgregarResenas` crea la restricción `UNIQUE (UsuarioId, TourId)`.

## ASP.NET Identity

`ApplicationUser : IdentityUser` extiende el modelo estándar y se almacena en `AspNetUsers`. Los roles se almacenan en `AspNetRoles` y la relación muchos a muchos en `AspNetUserRoles`, ambas administradas por `IdentityDbContext<ApplicationUser>`. Las relaciones de negocio que apuntan a un usuario, como `ECO_RESERVA_TB.UsuarioId`, `ECO_SALIDA_TB.GuiaId` y `ECO_RESENA_TB.UsuarioId`, son claves foráneas hacia `AspNetUsers.Id`.

## Normalización

1. **Primera forma normal (1FN):** cada columna contiene un solo valor atómico; las listas de pasajeros, imágenes y detalles se separan en tablas relacionadas.
2. **Segunda forma normal (2FN):** cada atributo no clave depende de toda la clave de su fila. En particular, la restricción compuesta de reseñas identifica una reseña por usuario y tour, y los atributos de la reseña dependen de esa relación completa.
3. **Tercera forma normal (3FN):** los atributos dependen de la clave y no de otro atributo no clave. Categorías, estados, dificultades, tours, salidas y reservas se mantienen en tablas separadas con claves foráneas.

Se conservan dos datos deliberadamente redundantes. `Salidas.CuposDisponibles` evita recalcular la ocupación en cada consulta y permite controlar cupos durante una reserva; debe actualizarse dentro de la misma transacción de confirmación. `DetalleReserva.PrecioPorPersona` congela el precio pagado, aunque el precio actual de la salida cambie posteriormente, preservando el histórico financiero.

## Desacoplamiento

Los controladores deben depender de interfaces de servicio (`ICatalogoService`, `ITourService` e `ISalidaService`) y no de `EfUnitOfWork`. La implementación concreta puede recibir `IUnitOfWorkFactory` mediante inyección de dependencias; de ese modo la capa web no conoce Entity Framework y los servicios pueden probarse con dobles de prueba.
