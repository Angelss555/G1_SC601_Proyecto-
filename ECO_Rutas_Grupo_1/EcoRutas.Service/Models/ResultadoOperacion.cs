namespace EcoRutas.Service.Models
{
    // Persona 4 — Valeria Calvo Chaves
    // Resultado estándar de una operación de negocio. El código de estado sigue el
    // contrato HTTP del proyecto (200, 400, 403, 404, 409) para que, en la entrega final,
    // la Web API pueda devolverlo tal cual.
    public class ResultadoOperacion
    {
        public const int CodigoOk = 200;
        public const int CodigoDatosInvalidos = 400;
        public const int CodigoProhibido = 403;
        public const int CodigoNoEncontrado = 404;
        public const int CodigoConflicto = 409;

        public int CodigoEstado { get; protected set; }
        public string Mensaje { get; protected set; }

        public bool Exito
        {
            get { return CodigoEstado >= 200 && CodigoEstado < 300; }
        }

        public static ResultadoOperacion Ok(string mensaje = null)
        {
            return new ResultadoOperacion { CodigoEstado = CodigoOk, Mensaje = mensaje };
        }

        public static ResultadoOperacion Error(int codigoEstado, string mensaje)
        {
            return new ResultadoOperacion { CodigoEstado = codigoEstado, Mensaje = mensaje };
        }

        public static ResultadoOperacion DatosInvalidos(string mensaje) { return Error(CodigoDatosInvalidos, mensaje); }
        public static ResultadoOperacion Prohibido(string mensaje) { return Error(CodigoProhibido, mensaje); }
        public static ResultadoOperacion NoEncontrado(string mensaje) { return Error(CodigoNoEncontrado, mensaje); }
        public static ResultadoOperacion Conflicto(string mensaje) { return Error(CodigoConflicto, mensaje); }
    }

    // Variante que además devuelve datos (por ejemplo, la lista de pasajeros de una salida).
    public class ResultadoOperacion<T> : ResultadoOperacion
    {
        public T Datos { get; private set; }

        public static ResultadoOperacion<T> Ok(T datos)
        {
            return new ResultadoOperacion<T> { CodigoEstado = CodigoOk, Datos = datos };
        }

        public static new ResultadoOperacion<T> Error(int codigoEstado, string mensaje)
        {
            return new ResultadoOperacion<T> { CodigoEstado = codigoEstado, Mensaje = mensaje };
        }
    }
}