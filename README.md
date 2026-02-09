# ProductSalesManager

Descripción del proyecto

ProductSalesManager es una aplicación web diseñada para la gestión integral de ventas y productos dentro de una organización. El sistema permite administrar clientes, productos, ventas y detalles de venta mediante un panel administrativo moderno, accesible desde navegador web. Incluye autenticación con JWT mediante un AuthController en la API (.NET), y un frontend en Vue que consume esos endpoints.

Tecnologías utilizadas

Backend
1. ASP.NET Core Web API
2. Entity Framework Core
3. SQL Server
4. JWT (JSON Web Token) para autenticación
5. BCrypt para encriptar contraseñas
6. Swagger para probar endpoints

Frontend
1. Vue 3 + Vite
2. Vue Router
3. Axios
4. Bootstrap 5

Instrucciones para correr la aplicación
Backend (.NET API)
1) Abre el proyecto en Visual Studio.
2) Verifica el appsettings.json tenga tu DefaultConnection.
3) Corre la API desde Visual Studio (F5) o desde terminal: dotnet run

Frontend (Vue)
1. Entra a la carpeta del frontend (ej: ProductFront): cd ProductFront
2. Instala dependencias: npm install
3. Levanta Vue: npm run dev
