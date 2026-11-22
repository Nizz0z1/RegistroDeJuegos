using System.Globalization;
using Microsoft.Data.Sqlite;

// ============================================================================
//  Registro de videojuegos — ejemplo de ADO.NET con SQLite
//  1. Crea (o abre) la base de datos videojuegos.db
//  2. Crea la tabla Videojuegos si no existe
//  3. Inserta tres registros iniciales con SqliteCommand
//  4. Consulta y muestra los registros con SqliteDataReader
//  5. Permite agregar un videojuego escrito por el usuario (consulta parametrizada)
// ============================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Cadena de conexión: indica qué archivo usar. Si no existe, SQLite lo crea.
const string CadenaConexion = "Data Source=videojuegos.db";

Console.WriteLine("==============================================");
Console.WriteLine("   REGISTRO DE VIDEOJUEGOS  (ADO.NET + SQLite)");
Console.WriteLine("==============================================\n");

try
{
    CrearTabla();
    InsertarRegistrosIniciales();
    MostrarVideojuegos();
    AgregarVideojuegoDesdeConsola();
}
catch (SqliteException ex)
{
    Console.WriteLine($"\n[ERROR de SQLite] {ex.Message}");
}

Console.WriteLine("\nFin del programa. La información quedó guardada en videojuegos.db.");


// ---------------------------------------------------------------------------
// 1 y 2. Crear la base de datos y la tabla
// ---------------------------------------------------------------------------
void CrearTabla()
{
    // "using" garantiza que la conexión se cierre al terminar el método,
    // incluso si ocurre un error.
    using var conexion = new SqliteConnection(CadenaConexion);
    conexion.Open(); // Aquí se crea el archivo videojuegos.db si no existía

    using var comando = conexion.CreateCommand();
    comando.CommandText = @"
        CREATE TABLE IF NOT EXISTS Videojuegos (
            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
            Titulo       TEXT    NOT NULL,
            Plataforma   TEXT    NOT NULL,
            Genero       TEXT    NOT NULL,
            Anio         INTEGER NOT NULL,
            Calificacion REAL    NOT NULL
        );";
    comando.ExecuteNonQuery(); // No devuelve filas: solo ejecuta la instrucción

    Console.WriteLine("[1] Conexión abierta correctamente.");
    Console.WriteLine($"    Archivo: {Path.GetFullPath(conexion.DataSource)}");
    Console.WriteLine("[2] Tabla 'Videojuegos' lista.\n");
}

// ---------------------------------------------------------------------------
// 3. Insertar registros con SqliteCommand
// ---------------------------------------------------------------------------
void InsertarRegistrosIniciales()
{
    using var conexion = new SqliteConnection(CadenaConexion);
    conexion.Open();

    // Primero se revisa si la tabla ya tiene datos, para no duplicarlos
    // cada vez que se ejecuta el programa.
    using (var conteo = conexion.CreateCommand())
    {
        conteo.CommandText = "SELECT COUNT(*) FROM Videojuegos;";
        var cantidad = Convert.ToInt64(conteo.ExecuteScalar()); // ExecuteScalar: un solo valor

        if (cantidad > 0)
        {
            Console.WriteLine($"[3] La tabla ya tiene {cantidad} registro(s) de ejecuciones anteriores.");
            Console.WriteLine("    No se insertan de nuevo (los datos persisten en el archivo).\n");
            return;
        }
    }

    Console.WriteLine("[3] Insertando registros iniciales...");

    var iniciales = new[]
    {
        (Titulo: "The Legend of Zelda: Breath of the Wild", Plataforma: "Nintendo Switch", Genero: "Aventura", Anio: 2017, Calificacion: 9.7),
        (Titulo: "Minecraft",                              Plataforma: "Multiplataforma", Genero: "Sandbox",  Anio: 2011, Calificacion: 9.0),
        (Titulo: "Hollow Knight",                          Plataforma: "PC",              Genero: "Metroidvania", Anio: 2017, Calificacion: 9.2),
    };

    foreach (var juego in iniciales)
    {
        var filas = Insertar(conexion, juego.Titulo, juego.Plataforma, juego.Genero, juego.Anio, juego.Calificacion);
        Console.WriteLine($"    + \"{juego.Titulo}\" insertado ({filas} fila afectada)");
    }
    Console.WriteLine();
}

/// Inserta un videojuego usando parámetros. Devuelve el número de filas afectadas.
int Insertar(SqliteConnection conexion, string titulo, string plataforma, string genero, int anio, double calificacion)
{
    using var comando = conexion.CreateCommand();
    comando.CommandText = @"
        INSERT INTO Videojuegos (Titulo, Plataforma, Genero, Anio, Calificacion)
        VALUES ($titulo, $plataforma, $genero, $anio, $calificacion);";

    // Los valores viajan como parámetros, nunca pegados al texto SQL.
    // Así se evita la inyección SQL y los errores con comillas.
    comando.Parameters.AddWithValue("$titulo", titulo);
    comando.Parameters.AddWithValue("$plataforma", plataforma);
    comando.Parameters.AddWithValue("$genero", genero);
    comando.Parameters.AddWithValue("$anio", anio);
    comando.Parameters.AddWithValue("$calificacion", calificacion);

    return comando.ExecuteNonQuery();
}

// ---------------------------------------------------------------------------
// 4. Consultar y mostrar con SqliteDataReader
// ---------------------------------------------------------------------------
void MostrarVideojuegos()
{
    using var conexion = new SqliteConnection(CadenaConexion);
    conexion.Open();

    using var comando = conexion.CreateCommand();
    comando.CommandText = @"
        SELECT Id, Titulo, Plataforma, Genero, Anio, Calificacion
        FROM Videojuegos
        ORDER BY Id;";

    // ExecuteReader devuelve un lector que recorre los resultados fila por fila.
    using var lector = comando.ExecuteReader();

    Console.WriteLine("[4] Videojuegos almacenados en la base de datos:\n");
    Console.WriteLine($"  {"ID",-4}{"TÍTULO",-42}{"PLATAFORMA",-18}{"GÉNERO",-14}{"AÑO",-6}{"NOTA",5}");
    Console.WriteLine("  " + new string('-', 89));

    var total = 0;
    while (lector.Read()) // Read() avanza a la siguiente fila; devuelve false al terminar
    {
        var id = lector.GetInt32(0);
        var titulo = lector.GetString(1);
        var plataforma = lector.GetString(2);
        var genero = lector.GetString(3);
        var anio = lector.GetInt32(4);
        var calificacion = lector.GetDouble(5);

        Console.WriteLine($"  {id,-4}{Recortar(titulo, 40),-42}{plataforma,-18}{genero,-14}{anio,-6}{calificacion,5:0.0}");
        total++;
    }

    Console.WriteLine("  " + new string('-', 89));
    Console.WriteLine($"  Total: {total} videojuego(s)\n");
}

static string Recortar(string texto, int maximo) =>
    texto.Length <= maximo ? texto : texto[..(maximo - 3)] + "...";

// ---------------------------------------------------------------------------
// 5. (Extra) Agregar un videojuego con datos escritos por el usuario
// ---------------------------------------------------------------------------
void AgregarVideojuegoDesdeConsola()
{
    Console.Write("¿Desea agregar un videojuego nuevo? (s/n): ");
    var respuesta = Console.ReadLine();
    if (!string.Equals(respuesta?.Trim(), "s", StringComparison.OrdinalIgnoreCase)) return;

    var titulo = LeerTexto("  Título: ");
    var plataforma = LeerTexto("  Plataforma: ");
    var genero = LeerTexto("  Género: ");
    var anio = LeerEntero("  Año de lanzamiento: ", 1950, DateTime.Now.Year);
    var calificacion = LeerDecimal("  Calificación (0 a 10): ", 0, 10);

    using var conexion = new SqliteConnection(CadenaConexion);
    conexion.Open();
    var filas = Insertar(conexion, titulo, plataforma, genero, anio, calificacion);

    Console.WriteLine($"\n  + \"{titulo}\" guardado ({filas} fila afectada).\n");
    MostrarVideojuegos();
}

static string LeerTexto(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        var texto = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(texto)) return texto;
        Console.WriteLine("    El valor no puede estar vacío.");
    }
}

static int LeerEntero(string mensaje, int minimo, int maximo)
{
    while (true)
    {
        Console.Write(mensaje);
        if (int.TryParse(Console.ReadLine(), out var valor) && valor >= minimo && valor <= maximo)
            return valor;
        Console.WriteLine($"    Escriba un número entero entre {minimo} y {maximo}.");
    }
}

static double LeerDecimal(string mensaje, double minimo, double maximo)
{
    while (true)
    {
        Console.Write(mensaje);
        var texto = Console.ReadLine()?.Trim().Replace(',', '.');
        if (double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)
            && valor >= minimo && valor <= maximo)
            return valor;
        Console.WriteLine($"    Escriba un número entre {minimo} y {maximo} (ej: 8.5).");
    }
}
