<div align="center">

# 📍 QuitoReporto

**App móvil de reportes ciudadanos para la ciudad de Quito**

Reporta baches, luminarias dañadas o basura acumulada con una foto y tu ubicación,
y sigue el avance hasta que el problema se resuelva.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-App%20m%C3%B3vil-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-5C2D91?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Base%20de%20datos-4169E1?logo=postgresql&logoColor=white)
![Plataformas](https://img.shields.io/badge/plataformas-Android%20%7C%20iOS-3DDC84)
![Estado](https://img.shields.io/badge/estado-en%20desarrollo-orange)

</div>

---

## 📖 Sobre el proyecto

**QuitoReporto** permite que los ciudadanos reporten problemas de su entorno, como baches, luminarias dañadas o acumulación de basura, adjuntando una **fotografía** y la **ubicación exacta** del incidente.

Los reportes se almacenan en un servicio web con base de datos y se muestran en un **mapa**, donde cualquier usuario puede consultarlos y apoyarlos. Un **administrador** (personal de EMAPS, del Municipio o de la entidad responsable) revisa los reportes y actualiza su estado hasta que el problema se resuelve:

```
📥 Recibido  ──►  🔧 En proceso  ──►  ✅ Resuelto
```

Proyecto final de la asignatura **Desarrollo de Aplicaciones Móviles**, desarrollado con la metodología **Mobile-D**.

## 👥 Actores

| Actor | Descripción |
| --- | --- |
| 🙋 **Ciudadano** | Crea reportes, explora el mapa, consulta su historial y apoya reportes de otras personas. |
| 🛠️ **Administrador** | Revisa los reportes, cambia su estado y consulta estadísticas. |
| 🗺️ **API de mapas** | Servicio externo (Google Maps u OpenStreetMap) que dibuja el mapa y convierte coordenadas en direcciones. |

## ✨ Funcionalidades

### Ciudadano
- 🔐 Registro, inicio de sesión y recuperación de contraseña por correo.
- 📝 Crear reportes con título, categoría (**bache, luz, basura u otros**), descripción, foto y ubicación.
- 📷 Captura de evidencia con la **cámara** del dispositivo.
- 📍 Ubicación automática con **GPS** y dirección obtenida desde la API de mapas.
- 🗺️ Mapa de reportes cercanos con filtros por categoría y estado.
- 📋 Historial personal de reportes con su estado actual.
- 👍 Apoyar (votar) y comentar reportes de otras personas.

### Administrador
- 🔎 Revisar y filtrar reportes por zona, categoría o estado.
- 🔄 Cambiar el estado de un reporte (Recibido, En proceso, Resuelto).
- 📊 Panel de estadísticas por categoría, zona y estado.

## 📱 Pantallas

Basadas en los [mockups del proyecto](https://drive.google.com/file/d/1lurh5FRPkNp3SoshMBfLlyHudyLFqXvs/view?usp=sharing):

| Pantalla | Qué muestra |
| --- | --- |
| **Inicio de sesión / Registro** | Acceso con usuario y clave, registro de cuenta y recuperación de contraseña con código de verificación. |
| **Mapa de reportes** | Ubicación del usuario y reportes cercanos como marcadores. |
| **Nuevo reporte** | Selección de categoría, foto, ubicación y descripción. |
| **Mis reportes** | Lista con buscador y filtros, y el estado de cada reporte. |
| **Detalle del reporte** | Línea de tiempo del estado, apoyos y comentarios. |
| **Panel del administrador** | Totales por estado, reportes por categoría y reportes pendientes para cambiar de estado. |

## 🧩 Casos de uso

| Código | Caso de uso | Actor |
| --- | --- | --- |
| CU-01 | Iniciar sesión | Ciudadano, Administrador |
| CU-02 | Crear reporte | Ciudadano |
| CU-03 | Tomar foto *(incluido en CU-02)* | Ciudadano |
| CU-04 | Obtener ubicación *(incluido en CU-02)* | Ciudadano, API de mapas |
| CU-05 | Explorar mapa | Ciudadano, API de mapas |
| CU-06 | Ver mis reportes | Ciudadano |
| CU-07 | Apoyar o comentar | Ciudadano |
| CU-08 | Revisar reportes | Administrador |
| CU-09 | Cambiar estado | Administrador |
| CU-10 | Ver estadísticas | Administrador |

## 🛠️ Tecnologías

| Tecnología | Uso |
| --- | --- |
| **.NET 10 + .NET MAUI** (C# y XAML) | Aplicación móvil multiplataforma |
| **CommunityToolkit.Mvvm** | Patrón MVVM |
| **ASP.NET Core Web API** | Backend REST |
| **PostgreSQL** | Usuarios, reportes, comentarios y estados |
| **Google Maps / OpenStreetMap** | Mapas y geocodificación |
| **xUnit** | Pruebas automatizadas |
| **Git, GitHub y GitHub Actions** | Control de versiones e integración continua |
| **Trello** | Seguimiento de tareas del equipo |

## 📂 Estructura del repositorio

```
QuitoReporto/
├── API/
│   └── QuitoReporto.Api/      # Backend en ASP.NET Core Web API
├── APP/
│   └── QuitoReporto.App/      # Aplicación móvil en .NET MAUI
├── QuitoReporto.slnx          # Solución de Visual Studio
└── README.md
```

## 🏛️ Arquitectura de la app

La app sigue el patrón **MVVM (Model-View-ViewModel)**, que es el que recomienda Microsoft para .NET MAUI, y se implementa con **CommunityToolkit.Mvvm**. Encima se agrega una capa de **servicios** que se registran con inyección de dependencias en `MauiProgram.cs`.

```
┌──────────────┐  binding   ┌──────────────┐   llama   ┌──────────────┐   HTTP    ┌──────────────┐
│     View     │ ─────────► │  ViewModel   │ ────────► │   Service    │ ────────► │   API REST   │
│ (XAML, UI)   │ ◄───────── │ (estado y    │ ◄──────── │ (API, GPS,   │ ◄──────── │ + PostgreSQL │
│              │  comandos  │  comandos)   │  Models   │  cámara)     │   JSON    │              │
└──────────────┘            └──────────────┘           └──────────────┘           └──────────────┘
```

| Capa | Responsabilidad |
| --- | --- |
| **Views** | Pantallas en XAML. Solo muestran datos y no tienen lógica de negocio. |
| **ViewModels** | Estado de cada pantalla, validaciones y comandos (`[ObservableProperty]`, `[RelayCommand]`). |
| **Models** | Clases de datos: usuario, reporte, comentario, categoría y estado. |
| **Services** | Acceso a la API, autenticación, GPS, cámara y mapas. Cada servicio tiene su interfaz para poder probarlo con xUnit. |

### Árbol de carpetas

```
APP/QuitoReporto.App/
├── Models/
│   ├── Usuario.cs
│   ├── Reporte.cs
│   ├── Comentario.cs
│   ├── Categoria.cs              # Bache, Luz, Basura, Otros
│   └── EstadoReporte.cs          # Recibido, EnProceso, Resuelto
├── ViewModels/
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   ├── RegistroViewModel.cs
│   │   └── RecuperarClaveViewModel.cs
│   ├── Ciudadano/
│   │   ├── MapaViewModel.cs
│   │   ├── NuevoReporteViewModel.cs
│   │   ├── MisReportesViewModel.cs
│   │   └── DetalleReporteViewModel.cs
│   └── Admin/
│       └── PanelAdminViewModel.cs
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   ├── RegistroPage.xaml
│   │   └── RecuperarClavePage.xaml
│   ├── Ciudadano/
│   │   ├── MapaPage.xaml
│   │   ├── NuevoReportePage.xaml
│   │   ├── MisReportesPage.xaml
│   │   └── DetalleReportePage.xaml
│   └── Admin/
│       └── PanelAdminPage.xaml
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IReporteService.cs
│   │   ├── IUbicacionService.cs
│   │   └── ICamaraService.cs
│   ├── AuthService.cs            # Login, registro y recuperación de clave
│   ├── ReporteService.cs         # CRUD de reportes, apoyos y comentarios
│   ├── UbicacionService.cs       # GPS y geocodificación
│   └── CamaraService.cs          # Captura de fotos
├── Converters/                   # Ej.: EstadoAColorConverter
├── Helpers/
│   └── Constantes.cs             # URL de la API, claves, etc.
├── Resources/                    # Íconos, fuentes, imágenes y estilos
├── Platforms/                    # Código y permisos por plataforma (cámara, GPS)
├── App.xaml
├── AppShell.xaml                 # Navegación y rutas
└── MauiProgram.cs                # Registro de servicios, ViewModels y Views
```

**Convenciones:**
- Cada pantalla tiene su par `NombrePage.xaml` ↔ `NombreViewModel.cs`.
- Las Views reciben su ViewModel por constructor (inyección de dependencias) y lo asignan a `BindingContext`.
- Los ViewModels nunca llaman directamente a `HttpClient`, al GPS ni a la cámara; siempre lo hacen a través de un servicio.

## 🚀 Cómo ejecutarlo

### Requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (o compatible) con la carga de trabajo **Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET**
- Emulador de Android o un dispositivo físico

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/Asahi7x/QuitoReporto.git
cd QuitoReporto

# 2. Levantar la API
dotnet run --project API/QuitoReporto.Api

# 3. Ejecutar la app en Android
dotnet build APP/QuitoReporto.App -t:Run -f net10.0-android
```

También se puede abrir `QuitoReporto.slnx` en Visual Studio y ejecutar cada proyecto desde ahí.

## 🗺️ Alcance de la primera versión

**Incluido:** registro e inicio de sesión, creación de reportes con foto y ubicación, mapa de reportes cercanos, historial personal, detalle del reporte, apoyos y comentarios, y panel administrativo.

**Fuera de esta versión:**
- Asignación automática de reportes a cuadrillas municipales.
- Detección automática del tipo de daño con IA.
- Integración con sistemas internos del Municipio de Quito.
- Notificaciones SMS o servicios externos de pago.
- Aplicación web pública para ciudadanos.

## 👨‍💻 Equipo

| Integrante |
| --- |
| Anthony Lanchi |
| Christian Gallegos |

**Universidad Tecnológica Israel** · Facultad de Ciencias de la Ingeniería
**Asignatura:** Desarrollo de Aplicaciones Móviles
**Docente:** PhD. Renato Mauricio Toasa Guachi
