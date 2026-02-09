# ProductSalesManager

Descripción del proyecto

ProductSalesManager es un panel administrativo web para gestionar clientes, productos, ventas y detalle de ventas (SaleItems).
Incluye autenticación con JWT mediante un AuthController en la API (.NET), y un frontend en Vue que consume esos endpoints.

Tecnologías utilizadas

Backend

ASP.NET Core Web API

Entity Framework Core

SQL Server

JWT (JSON Web Token) para autenticación

BCrypt para encriptar contraseñas

Swagger para probar endpoints

Frontend

Vue 3 + Vite

Vue Router

Axios

Bootstrap 5

Instrucciones para correr la aplicación
1) Backend (.NET API)

Abre el proyecto en Visual Studio.

Verifica el appsettings.json tenga tu DefaultConnection.

Corre la API desde Visual Studio (F5) o desde terminal:

dotnet run

Frontend (Vue)

Entra a la carpeta del frontend (ej: ProductFront)

cd ProductFront

Instala dependencias:

npm install


Levanta Vue:

npm run dev
