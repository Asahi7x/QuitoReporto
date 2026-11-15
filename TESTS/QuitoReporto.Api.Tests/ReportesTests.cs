using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Tests
{
    public class ReportesTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory factory;

        public ReportesTests(ApiFactory factory) => this.factory = factory;

        internal static MultipartFormDataContent Formulario(
            string titulo = "Bache en la Av. Amazonas",
            int categoriaId = 1,
            double lat = -0.1807,
            double lon = -78.4678,
            string? zona = "Iñaquito",
            bool conFoto = true)
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(titulo), "Titulo" },
                { new StringContent("Hueco grande frente a la parada"), "Descripcion" },
                { new StringContent(categoriaId.ToString()), "CategoriaId" },
                { new StringContent(lat.ToString(CultureInfo.InvariantCulture)), "Latitud" },
                { new StringContent(lon.ToString(CultureInfo.InvariantCulture)), "Longitud" },
                { new StringContent("Av. Amazonas y Naciones Unidas"), "Direccion" }
            };
            if (zona is not null) form.Add(new StringContent(zona), "Zona");
            if (conFoto)
            {
                var foto = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3]);
                foto.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                form.Add(foto, "Foto", "foto.jpg");
            }
            return form;
        }

        internal static async Task<ReporteDetalleDto> CrearReporteAsync(HttpClient cliente, MultipartFormDataContent? form = null)
        {
            var respuesta = await cliente.PostAsync("/api/reportes", form ?? Formulario());
            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
            return (await respuesta.Content.ReadFromJsonAsync<ReporteDetalleDto>())!;
        }

        [Fact]
        public async Task Categorias_DevuelveLasCuatroCategorias()
        {
            var categorias = await factory.CreateClient().GetFromJsonAsync<List<CategoriaDto>>("/api/categorias");

            Assert.Equal(["Bache", "Luz", "Basura", "Otros"], categorias!.Select(c => c.Nombre));
        }

        [Fact]
        public async Task CrearReporte_SinToken_DevuelveUnauthorized()
        {
            var respuesta = await factory.CreateClient().PostAsync("/api/reportes", Formulario());

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [Fact]
        public async Task CrearReporte_QuedaRecibidoConFotoEHistorial()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("crea@correo.ec", nombre: "Carla");

            var reporte = await CrearReporteAsync(cliente);

            Assert.Equal(EstadoReporte.Recibido, reporte.Estado);
            Assert.Equal("Bache", reporte.Categoria);
            Assert.Equal("Carla", reporte.Autor);
            Assert.True(reporte.EsMio);
            Assert.StartsWith("/uploads/", reporte.FotoUrl);
            Assert.Single(reporte.Historial);
            Assert.Equal(EstadoReporte.Recibido, reporte.Historial[0].Estado);
        }

        [Fact]
        public async Task CrearReporte_CategoriaInexistente_DevuelveBadRequest()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("categoria@correo.ec");

            var respuesta = await cliente.PostAsync("/api/reportes", Formulario(categoriaId: 99));

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task CrearReporte_TituloVacio_DevuelveBadRequest()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("titulo@correo.ec");

            var respuesta = await cliente.PostAsync("/api/reportes", Formulario(titulo: ""));

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task MisReportes_SoloDevuelveLosDelUsuario()
        {
            var ana = await factory.CrearClienteAutenticadoAsync("mis-ana@correo.ec");
            var beto = await factory.CrearClienteAutenticadoAsync("mis-beto@correo.ec");
            var deAna = await CrearReporteAsync(ana, Formulario(titulo: "Reporte de Ana"));
            await CrearReporteAsync(beto, Formulario(titulo: "Reporte de Beto"));

            var mios = await ana.GetFromJsonAsync<List<ReporteResumenDto>>("/api/reportes?mios=true");

            Assert.Single(mios!);
            Assert.Equal(deAna.Id, mios![0].Id);
        }

        [Fact]
        public async Task Listar_FiltraPorCategoriaEstadoYTexto()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("filtros@correo.ec");
            await CrearReporteAsync(cliente, Formulario(titulo: "Luminaria apagada en La Carolina", categoriaId: 2));
            await CrearReporteAsync(cliente, Formulario(titulo: "Basura en la esquina", categoriaId: 3));

            var luces = await cliente.GetFromJsonAsync<List<ReporteResumenDto>>("/api/reportes?mios=true&categoriaId=2");
            var resueltos = await cliente.GetFromJsonAsync<List<ReporteResumenDto>>($"/api/reportes?mios=true&estado={(int)EstadoReporte.Resuelto}");
            var busqueda = await cliente.GetFromJsonAsync<List<ReporteResumenDto>>("/api/reportes?mios=true&buscar=carolina");

            Assert.All(luces!, r => Assert.Equal(2, r.CategoriaId));
            Assert.Single(luces!);
            Assert.Empty(resueltos!);
            Assert.Single(busqueda!);
        }

        [Fact]
        public async Task Listar_PorRadio_ExcluyeReportesLejanos()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("radio@correo.ec");
            var cerca = await CrearReporteAsync(cliente, Formulario(titulo: "Cerca del centro", lat: -0.2202, lon: -78.5126));
            var lejos = await CrearReporteAsync(cliente, Formulario(titulo: "En Guayaquil", lat: -2.1894, lon: -79.8891));

            var cercanos = await cliente.GetFromJsonAsync<List<ReporteResumenDto>>(
                "/api/reportes?mios=true&lat=-0.2210&lon=-78.5120&radioKm=5");

            Assert.Contains(cercanos!, r => r.Id == cerca.Id);
            Assert.DoesNotContain(cercanos!, r => r.Id == lejos.Id);
        }

        [Fact]
        public async Task Apoyo_AlternaYCuentaUnaVezPorUsuario()
        {
            var autor = await factory.CrearClienteAutenticadoAsync("apoyo-autor@correo.ec");
            var vecino = await factory.CrearClienteAutenticadoAsync("apoyo-vecino@correo.ec");
            var reporte = await CrearReporteAsync(autor);

            var primero = await (await vecino.PostAsync($"/api/reportes/{reporte.Id}/apoyo", null)).Content.ReadFromJsonAsync<ApoyoRespuestaDto>();
            var detalle = await vecino.GetFromJsonAsync<ReporteDetalleDto>($"/api/reportes/{reporte.Id}");
            var segundo = await (await vecino.PostAsync($"/api/reportes/{reporte.Id}/apoyo", null)).Content.ReadFromJsonAsync<ApoyoRespuestaDto>();

            Assert.Equal(new ApoyoRespuestaDto(1, true), primero);
            Assert.True(detalle!.Apoyado);
            Assert.False(detalle.EsMio);
            Assert.Equal(new ApoyoRespuestaDto(0, false), segundo);
        }

        [Fact]
        public async Task Comentar_ApareceEnElDetalle()
        {
            var autor = await factory.CrearClienteAutenticadoAsync("coment-autor@correo.ec");
            var vecino = await factory.CrearClienteAutenticadoAsync("coment-vecino@correo.ec", nombre: "Vecino");
            var reporte = await CrearReporteAsync(autor);

            var respuesta = await vecino.PostAsJsonAsync($"/api/reportes/{reporte.Id}/comentarios", new CrearComentarioDto("A mí también me afecta"));
            respuesta.EnsureSuccessStatusCode();
            var detalle = await autor.GetFromJsonAsync<ReporteDetalleDto>($"/api/reportes/{reporte.Id}");

            var comentario = Assert.Single(detalle!.Comentarios);
            Assert.Equal("A mí también me afecta", comentario.Texto);
            Assert.Equal("Vecino", comentario.Autor);
        }

        [Fact]
        public async Task Comentar_ReporteInexistente_DevuelveNotFound()
        {
            var cliente = await factory.CrearClienteAutenticadoAsync("coment-404@correo.ec");

            var respuesta = await cliente.PostAsJsonAsync("/api/reportes/9999/comentarios", new CrearComentarioDto("Hola"));

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        }
    }
}
