# 🛒 StoreManagement - Sistema de Gestión de Tienda

Aplicación web construida con ASP.NET Core MVC para la gestión de productos, categorías, proveedores y movimientos de inventario de una tienda. Este proyecto está contenerizado con Docker y desplegado en Microsoft Azure.

**Enlace a la aplicación desplegada:** [Visitar la aplicación en Azure](https://store-management-gkcqh3asexdkbcgy.eastus2-01.azurewebsites.net) ---

---

## 👥 Autores

- Carlos Alberto Gómez Posada  
- Juan Camilo Cruz Parra

**Curso:** Desarrollo Avanzado de Aplicaciones Web con .NET  
**Docente:** Cristian Giovanni Castrillón Arias  
**Semestre:** 2025 - 1

---

## 🧩 Descripción General

Este sistema permite registrar, editar, eliminar y visualizar productos, así como gestionar las entradas y salidas del inventario. Está dirigido a un único administrador que controla todo el sistema.

**Entidades principales:**

- **Producto:** contiene nombre, descripción, precio, stock, categoría y proveedor.
- **Categoría:** clasifica los productos.
- **Proveedor:** proveedor del producto.
- **Movimiento de Inventario:** registra entradas y salidas del inventario.
- **Usuario:** gestiona el inicio de sesión del administrador.

---

## ⚙️ Tecnologías Usadas

- **Backend:** ASP.NET Core MVC  
- **Base de Datos:** SQL Server  
- **ORM:** Entity Framework Core  
- **Contenerización:** Docker  
- **Plataforma Cloud:** Microsoft Azure (App Service, SQL Database, Container Registry)  
- **Frontend:** HTML, CSS, JavaScript, Bootstrap 5 (personalizado)  
- **Gestión de Proyecto:** Jira (Metodología SCRUM)

---

## 🚀 Cómo Ejecutar Localmente (usando Docker)

Sigue estos pasos para ejecutar la aplicación en tu máquina local de forma aislada y consistente.

### Prerrequisitos

- .NET SDK  
- Docker Desktop

### Pasos

1. Clona este repositorio.

```bash
git clone https://github.com/Cgomez333/StoreManagement.git
cd StoreManagement
```

2. Configura la cadena de conexión local:

La aplicación buscará una base de datos SQL Server. Asegúrate de tener una instancia local (por ejemplo, con SQL Server Express). Luego, crea un archivo `appsettings.Development.json` en la raíz del proyecto y añade tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StoreManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

3. Construye la imagen de Docker:

```bash
docker build -t store-management-app .
```

4. Ejecuta el contenedor:

```bash
docker run --rm -d -p 8080:8080 --name store-app -e "ConnectionStrings__DefaultConnection=TU_CADENA_DE_CONEXION_LOCAL_AQUI" store-management-app
```

**Nota:** Reemplaza `TU_CADENA_DE_CONEXION_LOCAL_AQUI` con la cadena de conexión del paso 2.

5. Abre la aplicación en tu navegador:

[http://localhost:8080](http://localhost:8080)

---

## ☁️ Despliegue en Azure

La aplicación está configurada para ser desplegada en Azure App Service como un contenedor.

### Crear Recursos en Azure:

- Azure SQL Database  
- Azure Container Registry (ACR)  
- Azure App Service (configurado para Linux y Docker)

### Subir la Imagen a ACR:

1. Construir la imagen de Docker localmente.
2. Etiquetar (tag) la imagen para el registro de ACR.
3. Subir (push) la imagen a ACR.

### Configurar App Service:

- Configurar el App Service para que use la imagen desde ACR.
- En la sección de Configuración -> Cadenas de conexión, añadir la cadena de conexión de la Azure SQL Database con el nombre `DefaultConnection`.

**Nota:** El `Program.cs` está configurado para aplicar las migraciones de la base de datos automáticamente al iniciar la aplicación en Azure.

---

## ✅ Funcionalidades

- [x] CRUD completo de productos, categorías, y proveedores.  
- [x] Autenticación de usuario administrador.  
- [x] Registro de movimientos de inventario (entrada y salida).  
- [x] Validación y prevención de ventas sin stock.  
- [x] Reporte de productos con stock bajo.  
- [x] Diseño responsivo y moderno.

---

## 🎨 Diseño

- Navbar, Footer y la mayoría de elementos tienen un esquema de color azul modificable.  
- Fuente personalizada: Inter, aplicada globalmente.  
- Bootstrap 5 adaptado con `gradient-bg` para darle identidad visual moderna.

---

## 📃 Licencia

Este proyecto es parte del curso universitario y no está licenciado para uso comercial sin autorización expresa de los autores o la institución.
