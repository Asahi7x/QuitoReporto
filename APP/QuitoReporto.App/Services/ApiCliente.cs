using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using QuitoReporto.App.Helpers;

namespace QuitoReporto.App.Services
{
    /// <summary>Error devuelto por la API con un mensaje listo para mostrar al usuario.</summary>
    public class ApiException(string mensaje, HttpStatusCode? codigo = null) : Exception(mensaje)
    {
        public HttpStatusCode? Codigo { get; } = codigo;
    }

    /// <summary>Envoltorio de HttpClient compartido por los servicios: JSON, token y manejo de errores.</summary>
    public class ApiCliente
    {
        public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly HttpClient http;

        public ApiCliente(HttpClient http)
        {
            this.http = http;
            this.http.BaseAddress ??= new Uri(Constantes.ApiUrl);
            this.http.Timeout = TimeSpan.FromSeconds(30);
        }

        public void EstablecerToken(string? token) =>
            http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);

        /// <summary>Convierte una ruta relativa de la API (por ejemplo /uploads/x.jpg) en URL absoluta.</summary>
        public string? UrlAbsoluta(string? ruta) =>
            string.IsNullOrEmpty(ruta) || Uri.IsWellFormedUriString(ruta, UriKind.Absolute)
                ? ruta
                : new Uri(http.BaseAddress!, ruta).ToString();

        public Task<T> GetAsync<T>(string ruta) =>
            EnviarAsync<T>(() => http.GetAsync(ruta));

        public Task<T> PostAsync<T>(string ruta, object? cuerpo) =>
            EnviarAsync<T>(() => http.PostAsJsonAsync(ruta, cuerpo, Json));

        public Task<T> PutAsync<T>(string ruta, object? cuerpo) =>
            EnviarAsync<T>(() => http.PutAsJsonAsync(ruta, cuerpo, Json));

        public Task<T> PostAsync<T>(string ruta, HttpContent contenido) =>
            EnviarAsync<T>(() => http.PostAsync(ruta, contenido));

        private async Task<T> EnviarAsync<T>(Func<Task<HttpResponseMessage>> peticion)
        {
            HttpResponseMessage respuesta;
            try
            {
                respuesta = await peticion();
            }
            catch (TaskCanceledException)
            {
                throw new ApiException("El servidor tardó demasiado en responder. Inténtalo de nuevo.");
            }
            catch (HttpRequestException)
            {
                throw new ApiException("No se pudo conectar con el servidor. Revisa tu conexión a internet.");
            }

            using (respuesta)
            {
                if (respuesta.IsSuccessStatusCode)
                    return (await respuesta.Content.ReadFromJsonAsync<T>(Json))!;

                throw new ApiException(await LeerMensajeErrorAsync(respuesta), respuesta.StatusCode);
            }
        }

        private async Task<string> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
        {
            try
            {
                using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
                var raiz = doc.RootElement;

                if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                    return mensaje.GetString()!;

                // Errores de validación de ASP.NET Core (ValidationProblemDetails)
                if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                    foreach (var campo in errores.EnumerateObject())
                        foreach (var error in campo.Value.EnumerateArray())
                            return error.GetString() ?? "Datos no válidos.";
            }
            catch (JsonException)
            {
                // El cuerpo no es JSON: se usa el mensaje genérico
            }

            return respuesta.StatusCode switch
            {
                HttpStatusCode.Unauthorized when http.DefaultRequestHeaders.Authorization is not null
                    => "Tu sesión expiró. Vuelve a iniciar sesión.",
                HttpStatusCode.Unauthorized => "Correo o clave incorrectos.",
                HttpStatusCode.Forbidden => "No tienes permiso para realizar esta acción.",
                HttpStatusCode.NotFound => "No se encontró lo que buscabas.",
                HttpStatusCode.RequestEntityTooLarge => "La foto es demasiado grande.",
                _ => "Ocurrió un error en el servidor. Inténtalo más tarde."
            };
        }
    }
}
