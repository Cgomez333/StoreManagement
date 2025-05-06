
# 🛒 Sistema de Inventario de una Tienda

Proyecto desarrollado en ASP.NET Core MVC para la gestión de productos, categorías, proveedores y movimientos de inventario en una tienda.

## 👥 Autores

- **Carlos Alberto Gómez Posada**
- **Juan Camilo Cruz Parra**

Curso: *Desarrollo Avanzado de Aplicaciones Web con .NET*  
Docente: *Cristian Giovanni Castrillón Arias*  
Semestre: 2025 - 1

---

## 🧩 Descripción General

Este sistema permite registrar, editar, eliminar y visualizar productos, así como gestionar las entradas y salidas del inventario. Está dirigido a un único administrador que controla todo el sistema, ya que se trata de un sistema interno.

Las entidades principales son:

- **Producto**: contiene nombre, descripción, precio, stock, categoría y proveedor.
- **Categoría**: clasifica los productos.
- **Proveedor**: proveedor del producto.
- **Movimiento de Inventario**: registra entradas y salidas del inventario.
- **Cliente**: información de los clientes (previsto para futuras versiones).
- **Venta**: registro de las ventas realizadas (previsto para futuras versiones).
- **Detalle de Venta**: detalles de cada venta, como productos vendidos y cantidades (previsto para futuras versiones).

---

## ⚙️ Tecnologías Usadas

- **ASP.NET Core MVC**
- **Entity Framework Core**
- **Bootstrap 5** (personalizado)
- **Postgre ~ Supabase**
- **Jira** para la gestión de tareas (SCRUM)

---

## 🔁 Metodología SCRUM

El proyecto se desarrolló en 3 sprints:

| Sprint | Objetivo |
|--------|----------|
| 1 | Registro y búsqueda de productos |
| 2 | Entradas/salidas y reportes |
| 3 | Pruebas, ajustes visuales y mejora de experiencia de usuario |

---

## ✅ Funcionalidades

- [x] CRUD completo de productos, categorías, proveedores, etc.
- [x] Clasificación por categoría y proveedor
- [x] Registro de movimientos de inventario (entrada y salida)
- [x] Validación y prevención de ventas sin stock
- [x] Reporte de productos con stock bajo
- [x] Diseño responsivo y moderno

---

## 🚀 Cómo ejecutar el proyecto

1. Clona el repositorio:
   ```bash
   git clone https://github.com/Cgomez333/StoreManagement.git
   cd StoreManagement
   ```

2. Restaura los paquetes NuGet:
   ```bash
   dotnet restore
   ```

3. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

---

## 📂 Estructura del Proyecto

```
/Controllers
/Models
/Views
    /Product
    /Shared
    /~ demás vistas
/Migrations
/wwwroot
Program.cs
```

---

## 🔒 Privacidad y Seguridad

Este sistema está pensado para un único administrador. No se almacenan datos personales de usuarios. Solo se registran datos de productos, proveedores, categorías y movimientos de inventario en la base de datos local.

---

## 🎨 Diseño

- Navbar y Footer con esquema de color violeta modificable.
- Fuente personalizada: **Inter**, aplicada globalmente.
- Bootstrap 5 adaptado con `gradient-bg` para darle identidad visual moderna.


---

## 📃 Licencia

Este proyecto es parte del curso universitario y no está licenciado para uso comercial sin autorización expresa de los autores o la institución.

---
