/*
Datos de prueba para el flujo de creacion de pedidos.

Ejecutar sobre la base Pedidos360 despues de crear/aplicar su esquema.
El script solo agrega una categoria, productos y clientes faltantes.
No crea usuarios ni pedidos: confirme el pedido desde la aplicacion con
un usuario de Identity existente para que UsuarioId cumpla su FK.

Pruebas sugeridas:
- PED-TEST-001, cantidad 2, descuento 10 %: subtotal 18000.00,
  impuestos 2340.00 y total 20340.00.
- PED-TEST-002, cantidad 2, descuento fijo 500.00: subtotal y total
  4500.00 (impuesto 0 %).
- PED-TEST-003, cantidad 3: debe rechazarse por stock insuficiente.
*/

USE Pedidos360;
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Pruebas de pedidos')
BEGIN
    INSERT INTO dbo.Categorias (Nombre)
    VALUES (N'Pruebas de pedidos');
END;

DECLARE @CategoriaId INT =
(
    SELECT Id
    FROM dbo.Categorias
    WHERE Nombre = N'Pruebas de pedidos'
);

IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PED-TEST-001')
BEGIN
    INSERT INTO dbo.Productos
        (Codigo, Nombre, CategoriaId, Precio, ImpuestoPorc, Stock, Imagen, ImagenTipo, Activo)
    VALUES
        (N'PED-TEST-001', N'Producto prueba impuesto 13 %', @CategoriaId, 10000.00, 13.00, 25,
         0x89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000B49444154789C636000020000050001A5F645400000000049454E44AE426082,
         N'image/png', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PED-TEST-002')
BEGIN
    INSERT INTO dbo.Productos
        (Codigo, Nombre, CategoriaId, Precio, ImpuestoPorc, Stock, Imagen, ImagenTipo, Activo)
    VALUES
        (N'PED-TEST-002', N'Producto prueba impuesto 0 %', @CategoriaId, 2500.00, 0.00, 10,
         0x89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000B49444154789C636000020000050001A5F645400000000049454E44AE426082,
         N'image/png', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PED-TEST-003')
BEGIN
    INSERT INTO dbo.Productos
        (Codigo, Nombre, CategoriaId, Precio, ImpuestoPorc, Stock, Imagen, ImagenTipo, Activo)
    VALUES
        (N'PED-TEST-003', N'Producto prueba stock limitado', @CategoriaId, 1000.00, 13.00, 2,
         0x89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000B49444154789C636000020000050001A5F645400000000049454E44AE426082,
         N'image/png', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Cedula = N'900000001')
BEGIN
    INSERT INTO dbo.Clientes
        (TipoIdentificacion, Cedula, Nombre, Correo, Telefono, Direccion)
    VALUES
        (1, N'900000001', N'Cliente de prueba pedidos', N'cliente.pedidos@example.com',
         N'80000001', N'Direccion de prueba');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Cedula = N'3109999999')
BEGIN
    INSERT INTO dbo.Clientes
        (TipoIdentificacion, Cedula, Nombre, Correo, Telefono, Direccion)
    VALUES
        (2, N'3109999999', N'Empresa de prueba pedidos S.A.', N'empresa.pedidos@example.com',
         N'80000002', N'Direccion de prueba');
END;

COMMIT TRANSACTION;
GO

SELECT Codigo, Nombre, Precio, ImpuestoPorc, Stock, Activo
FROM dbo.Productos
WHERE Codigo LIKE N'PED-TEST-%'
ORDER BY Codigo;

SELECT Cedula, Nombre, Correo
FROM dbo.Clientes
WHERE Cedula IN (N'900000001', N'3109999999')
ORDER BY Nombre;
GO
