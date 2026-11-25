using System.Net;
using System.Net.Http.Json;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Tests
{
    public class AdminTests
    {
        [Fact]
        public async Task CambiarEstado_Ciudadano_DevuelveForbidden()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("ciudadano@correo.ec");
            var reporte = await ReportesTests.CrearReporteAsync(ciudadano);

            var respuesta = await ciudadano.PutAsJsonAsync($"/api/reportes/{reporte.Id}/estado",
                new CambiarEstadoDto(EstadoReporte.Resuelto, null));

            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task CambiarEstado_Admin_ActualizaEstadoYLineaDeTiempo()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("vecina@correo.ec");
            var admin = await factory.CrearClienteAdminAsync();
            var reporte = await ReportesTests.CrearReporteAsync(ciudadano);

            var enProceso = await admin.PutAsJsonAsync($"/api/reportes/{reporte.Id}/estado",
                new CambiarEstadoDto(EstadoReporte.EnProceso, "Cuadrilla asignada"));
            var resuelto = await admin.PutAsJsonAsync($"/api/reportes/{reporte.Id}/estado",
                new CambiarEstadoDto(EstadoReporte.Resuelto, null));
            enProceso.EnsureSuccessStatusCode();
            resuelto.EnsureSuccessStatusCode();

            var detalle = await ciudadano.GetFromJsonAsync<ReporteDetalleDto>($"/api/reportes/{reporte.Id}");
            Assert.Equal(EstadoReporte.Resuelto, detalle!.Estado);
            Assert.Equal(
                [EstadoReporte.Recibido, EstadoReporte.EnProceso, EstadoReporte.Resuelto],
                detalle.Historial.Select(h => h.Estado));
            Assert.Equal("Cuadrilla asignada", detalle.Historial[1].Nota);
            Assert.Equal("Administrador", detalle.Historial[1].Responsable);
        }

        [Fact]
        public async Task CambiarEstado_MismoEstado_DevuelveBadRequest()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("mismo@correo.ec");
            var admin = await factory.CrearClienteAdminAsync();
            var reporte = await ReportesTests.CrearReporteAsync(ciudadano);

            var respuesta = await admin.PutAsJsonAsync($"/api/reportes/{reporte.Id}/estado",
                new CambiarEstadoDto(EstadoReporte.Recibido, null));

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task Estadisticas_SoloAdmin()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("stats@correo.ec");

            var respuesta = await ciudadano.GetAsync("/api/admin/estadisticas");

            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task Estadisticas_CuentaPorEstadoCategoriaYZona()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("conteo@correo.ec");
            var admin = await factory.CrearClienteAdminAsync();
            var bache1 = await ReportesTests.CrearReporteAsync(ciudadano, ReportesTests.Formulario(categoriaId: 1, zona: "La Mariscal"));
            await ReportesTests.CrearReporteAsync(ciudadano, ReportesTests.Formulario(categoriaId: 1, zona: "La Mariscal"));
            await ReportesTests.CrearReporteAsync(ciudadano, ReportesTests.Formulario(categoriaId: 3, zona: "Chillogallo"));
            await admin.PutAsJsonAsync($"/api/reportes/{bache1.Id}/estado", new CambiarEstadoDto(EstadoReporte.Resuelto, null));

            var stats = await admin.GetFromJsonAsync<EstadisticasDto>("/api/admin/estadisticas");

            Assert.Equal(3, stats!.Total);
            Assert.Equal(2, stats.Recibidos);
            Assert.Equal(0, stats.EnProceso);
            Assert.Equal(1, stats.Resueltos);
            Assert.Equal(2, stats.PorCategoria.Single(c => c.Nombre == "Bache").Cantidad);
            Assert.Equal(1, stats.PorCategoria.Single(c => c.Nombre == "Basura").Cantidad);
            Assert.Equal(0, stats.PorCategoria.Single(c => c.Nombre == "Luz").Cantidad);
            Assert.Equal(new ConteoDto("La Mariscal", 2), stats.PorZona[0]);
        }

        [Fact]
        public async Task Listar_Admin_FiltraPorZona()
        {
            await using var factory = new ApiFactory();
            var ciudadano = await factory.CrearClienteAutenticadoAsync("zonas@correo.ec");
            var admin = await factory.CrearClienteAdminAsync();
            await ReportesTests.CrearReporteAsync(ciudadano, ReportesTests.Formulario(zona: "Cumbayá"));
            await ReportesTests.CrearReporteAsync(ciudadano, ReportesTests.Formulario(zona: "Quitumbe"));

            var enCumbaya = await admin.GetFromJsonAsync<List<ReporteResumenDto>>("/api/reportes?zona=cumbay");

            var reporte = Assert.Single(enCumbaya!);
            Assert.Equal("Cumbayá", reporte.Zona);
        }
    }
}
