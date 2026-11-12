using System;
using System.Collections.Generic;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Models
{
    // Una salida asignada al guía, con su ocupación calculada en el servidor.
    public class SalidaGuiaModel
    {
        public int SalidaId { get; set; }
        public string CodigoTour { get; set; }
        public string NombreTour { get; set; }
        public string PuntoEncuentro { get; set; }
        public DateTime FechaHora { get; set; }
        public int CupoMaximo { get; set; }
        public int CuposDisponibles { get; set; }
        public int PasajerosConfirmados { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public bool Activa { get; set; }
        public bool ListaCerrada { get; set; }
        public bool EsHoy { get; set; }
    }

    // Salidas del guía separadas en próximas (incluye las de hoy) y pasadas.
    public class SalidasGuiaModel
    {
        public List<SalidaGuiaModel> Proximas { get; set; }
        public List<SalidaGuiaModel> Pasadas { get; set; }

        public SalidasGuiaModel()
        {
            Proximas = new List<SalidaGuiaModel>();
            Pasadas = new List<SalidaGuiaModel>();
        }
    }

    public class PasajeroGuiaModel
    {
        public int PaseId { get; set; }
        public string CodigoPase { get; set; }
        public string NombrePasajero { get; set; }
        public string Identificacion { get; set; }
        public bool Asistio { get; set; }
        public int DetalleReservaId { get; set; }
        public string TitularReserva { get; set; }
        public string EstadoDetalle { get; set; }
    }

    public class ListaPasajerosModel
    {
        public SalidaGuiaModel Salida { get; set; }
        public List<PasajeroGuiaModel> Pasajeros { get; set; }

        public ListaPasajerosModel()
        {
            Pasajeros = new List<PasajeroGuiaModel>();
        }
    }
}