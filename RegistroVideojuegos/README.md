# Registro de Videojuegos — ADO.NET + SQLite

Programa de consola en **C# / .NET 8** que muestra cómo acceder a una base de datos **SQLite** con **ADO.NET** (`Microsoft.Data.Sqlite`).

> Actividad de investigación — Semana 12: Fundamentos de acceso a datos con ADO.NET
> Estudiante: Nicolás Castillo

## Qué hace el programa

1. Crea (o abre) el archivo de base de datos `videojuegos.db`.
2. Abre la conexión con `SqliteConnection`.
3. Crea la tabla `Videojuegos` (6 columnas) si no existe.
4. Inserta **tres registros** iniciales con `SqliteCommand` y parámetros.
5. Consulta y muestra todos los registros con `SqliteDataReader`.
6. Extra: pregunta si se quiere agregar un videojuego escrito por el usuario y lo guarda con una consulta parametrizada.

Todos los objetos de ADO.NET (conexión, comando y lector) se crean con `using`, de modo que se liberan automáticamente.

## Requisitos

- [SDK de .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) o superior (`dotnet --version` para verificar).
- No hay que instalar SQLite: el paquete `Microsoft.Data.Sqlite` lo incluye.

## Cómo ejecutarlo

```bash
git clone https://github.com/<usuario>/RegistroVideojuegos.git
cd RegistroVideojuegos
dotnet restore
dotnet run
```

En la **primera ejecución** se crean la base y la tabla, y se insertan los tres videojuegos.
En las **siguientes ejecuciones** el programa detecta que ya hay datos y no los vuelve a insertar: así se comprueba que la información persiste.

Para empezar desde cero, borra el archivo `videojuegos.db` que se crea en la carpeta del proyecto.

## Estructura

```
RegistroVideojuegos/
├── RegistroVideojuegos.csproj   # Proyecto .NET 8 + paquete Microsoft.Data.Sqlite
├── Program.cs                   # Todo el ejemplo, dividido en métodos comentados
├── evidencias/                  # Capturas de la ejecución
└── README.md
```

## Tabla `Videojuegos`

| Columna | Tipo | Descripción |
|---|---|---|
| `Id` | INTEGER PRIMARY KEY AUTOINCREMENT | Identificador |
| `Titulo` | TEXT NOT NULL | Nombre del juego |
| `Plataforma` | TEXT NOT NULL | Consola o PC |
| `Genero` | TEXT NOT NULL | Género |
| `Anio` | INTEGER NOT NULL | Año de lanzamiento |
| `Calificacion` | REAL NOT NULL | Nota de 0 a 10 |

## Dependencias

| Paquete | Versión |
|---|---|
| Microsoft.Data.Sqlite | 8.0.11 |
