-- ============================================================================
-- SCRIPT DE BASE DE DATOS
-- Sistema de Gestión Comercial e Inventario con Integración de Báscula
-- DBMS: SQL Server Express / SQL Server 2019+
-- Autor: Sistema de Gestión
-- Fecha: 2026-04-17
-- ============================================================================

USE master;
GO

-- Crear base de datos
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'GestionComercial_BD')
BEGIN
    DROP DATABASE GestionComercial_BD;
END
GO

CREATE DATABASE GestionComercial_BD;
GO

USE GestionComercial_BD;
GO

-- ============================================================================
-- 1. TABLA: ROLES
-- Describe los roles de usuario en el sistema
-- ============================================================================
CREATE TABLE Roles (
    ID_Rol INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(MAX),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================================
-- 2. TABLA: USUARIOS
-- Almacena usuarios del sistema con autenticación
-- ============================================================================
CREATE TABLE Usuarios (
    ID_Usuario INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario NVARCHAR(50) NOT NULL UNIQUE,
    NombreCompleto NVARCHAR(100) NOT NULL,
    Contrasena NVARCHAR(MAX) NOT NULL,  -- Debe estar hasheada (bcrypt/SHA256)
    Email NVARCHAR(100),
    Telefono NVARCHAR(20),
    ID_Rol INT NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'ACTIVO' CHECK (Estado IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO')),
    UltimoLogin DATETIME,
    IntentosFallidos INT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ID_Rol) REFERENCES Roles(ID_Rol)
);
GO

-- ============================================================================
-- 3. TABLA: CLIENTES
-- Información de clientes (transportistas, mayoristas, etc.)
-- ============================================================================
CREATE TABLE Clientes (
    ID_Cliente INT PRIMARY KEY IDENTITY(1,1),
    CodigoCliente NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(150) NOT NULL,
    TipoIdentificacion NVARCHAR(20) NOT NULL,  -- RUC, Cédula, Pasaporte
    NumeroIdentificacion NVARCHAR(50) NOT NULL UNIQUE,
    Categoria NVARCHAR(50) NOT NULL,  -- Transportista, Mayorista, Minorista
    Contacto NVARCHAR(20),
    Email NVARCHAR(100),
    Direccion NVARCHAR(250),
    DescuentoPorDefecto DECIMAL(5,2) DEFAULT 0,  -- Porcentaje
    PlazoCredito INT DEFAULT 0,  -- Días
    LimiteCredito DECIMAL(15,2) DEFAULT 0,
    SaldoCredito DECIMAL(15,2) DEFAULT 0,
    Estado NVARCHAR(20) DEFAULT 'ACTIVO' CHECK (Estado IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO')),
    Observaciones NVARCHAR(MAX),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    UsuarioCreacion INT,
    FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Clientes_NumeroIdentificacion ON Clientes(NumeroIdentificacion);
GO

-- ============================================================================
-- 4. TABLA: VEHICULOS
-- Vehículos asociados a clientes (máximo 2 por cliente)
-- ============================================================================
CREATE TABLE Vehiculos (
    ID_Vehiculo INT PRIMARY KEY IDENTITY(1,1),
    ID_Cliente INT NOT NULL,
    Placa NVARCHAR(20) NOT NULL UNIQUE,
    Tipo NVARCHAR(50) NOT NULL,  -- Volqueta, Gandola, Furgón, etc.
    Marca NVARCHAR(50),
    Modelo NVARCHAR(50),
    Color NVARCHAR(30),
    Capacidad DECIMAL(10,2),  -- Toneladas
    AnoFabricacion INT,
    PesoTara DECIMAL(10,2),  -- kg - Peso conocido del vehículo vacío
    VIN NVARCHAR(50),
    PlacaINEN NVARCHAR(50),
    Estado NVARCHAR(20) DEFAULT 'ACTIVO' CHECK (Estado IN ('ACTIVO', 'INACTIVO', 'MANTENIMIENTO')),
    UltimaPesada DATETIME,
    Observaciones NVARCHAR(MAX),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    UsuarioCreacion INT,
    FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID_Cliente),
    FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Vehiculos_Placa ON Vehiculos(Placa);
CREATE INDEX IX_Vehiculos_Cliente ON Vehiculos(ID_Cliente);
GO

-- Restricción: Máximo 2 vehículos por cliente
CREATE TRIGGER trg_ValidarMaximoVehiculos ON Vehiculos
INSTEAD OF INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM Vehiculos v
        INNER JOIN INSERTED i ON v.ID_Cliente = i.ID_Cliente
        WHERE v.Estado = 'ACTIVO'
        GROUP BY v.ID_Cliente
        HAVING COUNT(*) >= 2
    )
    BEGIN
        RAISERROR('Un cliente no puede tener más de 2 vehículos activos', 16, 1);
        ROLLBACK TRANSACTION;
    END
    ELSE
    BEGIN
        INSERT INTO Vehiculos
        SELECT * FROM INSERTED;
    END
END
GO

-- ============================================================================
-- 5. TABLA: PRODUCTOS
-- Catálogo de productos comercializados
-- ============================================================================
CREATE TABLE Productos (
    ID_Producto INT PRIMARY KEY IDENTITY(1,1),
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(150) NOT NULL,
    TipoMaterial NVARCHAR(100) NOT NULL,  -- Arena, Piedra, Cemento, etc.
    Unidad NVARCHAR(20) NOT NULL CHECK (Unidad IN ('Kg', 'Tonelada', 'Unidad', 'Metro', 'Metro2', 'Metro3')),
    PrecioBase DECIMAL(15,4) NOT NULL,  -- Por unidad
    Stock DECIMAL(15,2) DEFAULT 0,
    StockMinimo DECIMAL(15,2) DEFAULT 0,
    StockMaximo DECIMAL(15,2) DEFAULT 0,
    Descripcion NVARCHAR(MAX),
    Estado NVARCHAR(20) DEFAULT 'ACTIVO' CHECK (Estado IN ('ACTIVO', 'INACTIVO')),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    UsuarioCreacion INT,
    FOREIGN KEY (UsuarioCreacion) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Productos_Codigo ON Productos(Codigo);
GO

-- ============================================================================
-- 6. TABLA: MOVIMIENTOS_INVENTARIO
-- Registro de entradas/salidas de productos
-- ============================================================================
CREATE TABLE Movimientos_Inventario (
    ID_Movimiento INT PRIMARY KEY IDENTITY(1,1),
    ID_Producto INT NOT NULL,
    TipoMovimiento NVARCHAR(20) NOT NULL CHECK (TipoMovimiento IN ('ENTRADA', 'SALIDA', 'AJUSTE')),
    Cantidad DECIMAL(15,2) NOT NULL,
    StockAnterior DECIMAL(15,2),
    StockPosterior DECIMAL(15,2),
    Referencia NVARCHAR(100),  -- Número de compra, venta, etc.
    Observaciones NVARCHAR(MAX),
    FechaMovimiento DATETIME DEFAULT GETDATE(),
    UsuarioMovimiento INT NOT NULL,
    FOREIGN KEY (ID_Producto) REFERENCES Productos(ID_Producto),
    FOREIGN KEY (UsuarioMovimiento) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Movimientos_Producto ON Movimientos_Inventario(ID_Producto);
CREATE INDEX IX_Movimientos_Fecha ON Movimientos_Inventario(FechaMovimiento);
GO

-- ============================================================================
-- 7. TABLA: PESAJES
-- Registro de pesajes desde la báscula (RS232)
-- ============================================================================
CREATE TABLE Pesajes (
    ID_Pesaje INT PRIMARY KEY IDENTITY(1,1),
    ID_Vehiculo INT NOT NULL,
    TipoPesaje NVARCHAR(20) NOT NULL CHECK (TipoPesaje IN ('TARA', 'BRUTO')),
    PesoKg DECIMAL(10,2) NOT NULL,
    Temperatura DECIMAL(5,2),  -- Opcional, si la báscula lo registra
    Humedad DECIMAL(5,2),      -- Opcional
    EstadoBascula NVARCHAR(50),  -- Normal, Error, Etc.
    NumeroSerie NVARCHAR(50),  -- De la báscula si es identificable
    FechaPesaje DATETIME DEFAULT GETDATE(),
    UsuarioPesaje INT NOT NULL,
    Observaciones NVARCHAR(MAX),
    FOREIGN KEY (ID_Vehiculo) REFERENCES Vehiculos(ID_Vehiculo),
    FOREIGN KEY (UsuarioPesaje) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Pesajes_Vehiculo ON Pesajes(ID_Vehiculo);
CREATE INDEX IX_Pesajes_Fecha ON Pesajes(FechaPesaje);
GO

-- ============================================================================
-- 8. TABLA: VENTAS
-- Encabezado de las ventas realizadas
-- ============================================================================
CREATE TABLE Ventas (
    ID_Venta INT PRIMARY KEY IDENTITY(1,1),
    NumeroVenta NVARCHAR(20) NOT NULL UNIQUE,
    ID_Cliente INT NOT NULL,
    ID_Vehiculo INT,
    ID_PesajeTara INT,
    ID_PesajeBruto INT,
    PesoTaraKg DECIMAL(10,2),
    PesoBrutoKg DECIMAL(10,2),
    PesoNetoKg DECIMAL(10,2),  -- Calculado: Bruto - Tara
    Subtotal DECIMAL(15,4),
    DescuentosAplicados DECIMAL(15,4) DEFAULT 0,
    IVA DECIMAL(15,4) DEFAULT 0,
    TotalVenta DECIMAL(15,4),
    TipoDocumento NVARCHAR(20) NOT NULL CHECK (TipoDocumento IN ('TICKET', 'FACTURA')),
    EstadoVenta NVARCHAR(20) DEFAULT 'COMPLETADA' CHECK (EstadoVenta IN ('BORRADOR', 'COMPLETADA', 'ANULADA')),
    UsuarioVenta INT NOT NULL,
    FechaVenta DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID_Cliente),
    FOREIGN KEY (ID_Vehiculo) REFERENCES Vehiculos(ID_Vehiculo),
    FOREIGN KEY (ID_PesajeTara) REFERENCES Pesajes(ID_Pesaje),
    FOREIGN KEY (ID_PesajeBruto) REFERENCES Pesajes(ID_Pesaje),
    FOREIGN KEY (UsuarioVenta) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Ventas_Numero ON Ventas(NumeroVenta);
CREATE INDEX IX_Ventas_Cliente ON Ventas(ID_Cliente);
CREATE INDEX IX_Ventas_Fecha ON Ventas(FechaVenta);
GO

-- ============================================================================
-- 9. TABLA: DETALLE_VENTAS
-- Líneas de detalle de cada venta (puede haber múltiples productos)
-- ============================================================================
CREATE TABLE Detalle_Ventas (
    ID_DetalleVenta INT PRIMARY KEY IDENTITY(1,1),
    ID_Venta INT NOT NULL,
    ID_Producto INT NOT NULL,
    Cantidad DECIMAL(15,2) NOT NULL,
    PrecioUnitario DECIMAL(15,4) NOT NULL,
    DescuentoLinea DECIMAL(5,2) DEFAULT 0,  -- Porcentaje
    ValorDescuento DECIMAL(15,4),
    SubtotalLinea DECIMAL(15,4),
    FOREIGN KEY (ID_Venta) REFERENCES Ventas(ID_Venta) ON DELETE CASCADE,
    FOREIGN KEY (ID_Producto) REFERENCES Productos(ID_Producto)
);
GO

CREATE INDEX IX_DetalleVentas_Venta ON Detalle_Ventas(ID_Venta);
GO

-- ============================================================================
-- 10. TABLA: DESCUENTOS
-- Configuración de descuentos por cliente y por volumen
-- ============================================================================
CREATE TABLE Descuentos (
    ID_Descuento INT PRIMARY KEY IDENTITY(1,1),
    ID_Cliente INT,  -- NULL si es descuento general por volumen
    ID_Producto INT,  -- NULL si aplica a todos los productos
    TipoDescuento NVARCHAR(20) NOT NULL CHECK (TipoDescuento IN ('CLIENTE', 'VOLUMEN', 'TEMPORAL')),
    PorcentajeDescuento DECIMAL(5,2) NOT NULL,
    VolumenMinimo DECIMAL(15,2),  -- Para descuentos por volumen
    FechaInicio DATETIME,
    FechaFin DATETIME,
    Observaciones NVARCHAR(MAX),
    Estado NVARCHAR(20) DEFAULT 'ACTIVO',
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID_Cliente),
    FOREIGN KEY (ID_Producto) REFERENCES Productos(ID_Producto)
);
GO

-- ============================================================================
-- 11. TABLA: FACTURAS
-- Almacenamiento de facturas formales (diferentes de tickets)
-- ============================================================================
CREATE TABLE Facturas (
    ID_Factura INT PRIMARY KEY IDENTITY(1,1),
    NumeroFactura NVARCHAR(50) NOT NULL UNIQUE,
    ID_Venta INT NOT NULL UNIQUE,
    ID_Cliente INT NOT NULL,
    FechaEmision DATETIME DEFAULT GETDATE(),
    FechaVencimiento DATETIME,
    SubtotalFactura DECIMAL(15,4),
    IVAFactura DECIMAL(15,4),
    TotalFactura DECIMAL(15,4),
    EstadoFactura NVARCHAR(20) DEFAULT 'EMITIDA' CHECK (EstadoFactura IN ('EMITIDA', 'PAGADA', 'ANULADA', 'CRÉDITO')),
    ObservacionesFactura NVARCHAR(MAX),
    UsuarioEmision INT NOT NULL,
    FOREIGN KEY (ID_Venta) REFERENCES Ventas(ID_Venta),
    FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID_Cliente),
    FOREIGN KEY (UsuarioEmision) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Facturas_Numero ON Facturas(NumeroFactura);
CREATE INDEX IX_Facturas_Cliente ON Facturas(ID_Cliente);
GO

-- ============================================================================
-- 12. TABLA: AUDITORIA
-- Registro de todas las acciones del sistema para trazabilidad
-- ============================================================================
CREATE TABLE Auditoria (
    ID_Auditoria INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario INT,
    Tabla NVARCHAR(50) NOT NULL,
    TipoOperacion NVARCHAR(20) NOT NULL CHECK (TipoOperacion IN ('INSERT', 'UPDATE', 'DELETE')),
    RegistroID NVARCHAR(100),
    DatosAnteriores NVARCHAR(MAX),
    DatosNuevos NVARCHAR(MAX),
    FechaOperacion DATETIME DEFAULT GETDATE(),
    DireccionIP NVARCHAR(50),
    Razon NVARCHAR(250),
    FOREIGN KEY (ID_Usuario) REFERENCES Usuarios(ID_Usuario)
);
GO

CREATE INDEX IX_Auditoria_Usuario ON Auditoria(ID_Usuario);
CREATE INDEX IX_Auditoria_Fecha ON Auditoria(FechaOperacion);
CREATE INDEX IX_Auditoria_Tabla ON Auditoria(Tabla);
GO

-- ============================================================================
-- 13. TABLA: LOTES_INVENTARIO (Opcional pero recomendado)
-- Rastreo de lotes de productos para mejor control
-- ============================================================================
CREATE TABLE Lotes_Inventario (
    ID_Lote INT PRIMARY KEY IDENTITY(1,1),
    ID_Producto INT NOT NULL,
    NumeroLote NVARCHAR(50) NOT NULL,
    FechaFabricacion DATETIME,
    FechaVencimiento DATETIME,
    QuantidadRecibida DECIMAL(15,2),
    QuantidadDisponible DECIMAL(15,2),
    Estado NVARCHAR(20) DEFAULT 'DISPONIBLE' CHECK (Estado IN ('DISPONIBLE', 'AGOTADO', 'EXPIRADO')),
    Proveedor NVARCHAR(100),
    FechaIngreso DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ID_Producto) REFERENCES Productos(ID_Producto)
);
GO

CREATE INDEX IX_Lotes_Producto ON Lotes_Inventario(ID_Producto);
CREATE INDEX IX_Lotes_Numero ON Lotes_Inventario(NumeroLote);
GO

-- ============================================================================
-- INSERCIÓN DE DATOS BÁSICOS
-- ============================================================================

-- Insertar Roles
INSERT INTO Roles (Nombre, Descripcion) VALUES
('Administrador', 'Acceso completo al sistema'),
('Operador', 'Registro de ventas y consultas'),
('Supervisor', 'Reportes y supervisión');
GO

-- Insertar Usuario Administrador por defecto (contraseña: 123456 - DEBE CAMBIARSE)
INSERT INTO Usuarios (NombreUsuario, NombreCompleto, Contrasena, Email, Telefono, ID_Rol, Estado)
VALUES 
('admin', 'Administrador Sistema', 'e99a18c428cb38d5f260853678922e03', 'admin@gestioncomercial.com', '0999999999', 1, 'ACTIVO');
GO

-- Insertar operador de prueba
INSERT INTO Usuarios (NombreUsuario, NombreCompleto, Contrasena, Email, Telefono, ID_Rol, Estado)
VALUES 
('operador', 'Operador Principal', 'e99a18c428cb38d5f260853678922e03', 'operador@gestioncomercial.com', '0988888888', 2, 'ACTIVO');
GO

-- Insertar Clientes de ejemplo
INSERT INTO Clientes (CodigoCliente, Nombre, TipoIdentificacion, NumeroIdentificacion, Categoria, Contacto, Email, Direccion, DescuentoPorDefecto, PlazoCredito, LimiteCredito, Estado, UsuarioCreacion)
VALUES 
('CLI001', 'Transportes García & Cía.', 'RUC', '1700123456789', 'Transportista', '0987654321', 'garcia@email.com', 'Avenida Principal 123', 5.00, 30, 50000.00, 'ACTIVO', 1),
('CLI002', 'Constructora Edificio Plus', 'RUC', '1700987654321', 'Mayorista', '0987123456', 'info@edificioplus.com', 'Calle Secundaria 456', 3.00, 15, 30000.00, 'ACTIVO', 1);
GO

-- Insertar Vehículos de ejemplo
INSERT INTO Vehiculos (ID_Cliente, Placa, Tipo, Marca, Modelo, Color, Capacidad, AnoFabricacion, PesoTara, Estado, UsuarioCreacion)
VALUES 
(1, 'EBC-123', 'Volqueta', 'Hino', '500', 'Azul', 15.00, 2020, 6500.00, 'ACTIVO', 1),
(1, 'EBC-124', 'Gandola', 'Volvo', 'FH16', 'Rojo', 20.00, 2021, 8000.00, 'ACTIVO', 1),
(2, 'ABC-789', 'Furgón', 'CAT', '320', 'Blanco', 10.00, 2019, 4500.00, 'ACTIVO', 1);
GO

-- Insertar Productos de ejemplo
INSERT INTO Productos (Codigo, Nombre, TipoMaterial, Unidad, PrecioBase, Stock, StockMinimo, StockMaximo, Estado, UsuarioCreacion)
VALUES 
('PROD001', 'Arena Fina', 'Arena', 'Kg', 0.02, 100000.00, 10000.00, 200000.00, 'ACTIVO', 1),
('PROD002', 'Piedra Bola', 'Piedra', 'Kg', 0.035, 80000.00, 5000.00, 150000.00, 'ACTIVO', 1),
('PROD003', 'Cemento Portland', 'Cemento', 'Unidad', 7.50, 5000.00, 500.00, 10000.00, 'ACTIVO', 1),
('PROD004', 'Ladrillo Rojo', 'Ladrillo', 'Unidad', 0.45, 25000.00, 2000.00, 50000.00, 'ACTIVO', 1);
GO

-- Insertar Descuentos de ejemplo
INSERT INTO Descuentos (ID_Cliente, ID_Producto, TipoDescuento, PorcentajeDescuento, VolumenMinimo, Estado)
VALUES 
(1, 1, 'CLIENTE', 5.00, NULL, 'ACTIVO'),
(NULL, 1, 'VOLUMEN', 10.00, 5000.00, 'ACTIVO');  -- 10% si compra más de 5 toneladas
GO

-- ============================================================================
-- VISTAS ÚTILES PARA CONSULTAS
-- ============================================================================

-- Vista: Resumen de último pesaje por vehículo
CREATE VIEW vw_UltimoPesajeVehiculo AS
SELECT 
    v.ID_Vehiculo,
    v.Placa,
    c.Nombre AS ClienteNombre,
    p.PesoKg,
    p.TipoPesaje,
    p.FechaPesaje,
    ROW_NUMBER() OVER (PARTITION BY v.ID_Vehiculo ORDER BY p.FechaPesaje DESC) AS Ranking
FROM Vehiculos v
INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
LEFT JOIN Pesajes p ON v.ID_Vehiculo = p.ID_Vehiculo
GO

-- Vista: Stock bajo (debajo del mínimo)
CREATE VIEW vw_ProductosStockBajo AS
SELECT 
    ID_Producto,
    Codigo,
    Nombre,
    Stock,
    StockMinimo,
    (StockMinimo - Stock) AS Deficit
FROM Productos
WHERE Stock < StockMinimo AND Estado = 'ACTIVO'
GO

-- Vista: Ventas del día
CREATE VIEW vw_VentasDelDia AS
SELECT 
    v.NumeroVenta,
    c.Nombre AS ClienteNombre,
    vh.Placa,
    v.PesoNetoKg,
    v.TotalVenta,
    u.NombreCompleto AS OperadorVenta,
    CAST(v.FechaVenta AS DATE) AS FechaVenta
FROM Ventas v
INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
LEFT JOIN Vehiculos vh ON v.ID_Vehiculo = vh.ID_Vehiculo
INNER JOIN Usuarios u ON v.UsuarioVenta = u.ID_Usuario
WHERE CAST(v.FechaVenta AS DATE) = CAST(GETDATE() AS DATE)
GO

-- Vista: Resumen de cliente (historial de crédito)
CREATE VIEW vw_ResumenCliente AS
SELECT 
    c.ID_Cliente,
    c.CodigoCliente,
    c.Nombre,
    COUNT(v.ID_Venta) AS TotalTransacciones,
    SUM(v.TotalVenta) AS MontoTotalComprado,
    c.SaldoCredito,
    c.LimiteCredito,
    (c.LimiteCredito - c.SaldoCredito) AS DisponibleCredito
FROM Clientes c
LEFT JOIN Ventas v ON c.ID_Cliente = v.ID_Cliente 
    AND YEAR(v.FechaVenta) = YEAR(GETDATE()) 
    AND MONTH(v.FechaVenta) = MONTH(GETDATE())
GROUP BY c.ID_Cliente, c.CodigoCliente, c.Nombre, c.SaldoCredito, c.LimiteCredito
GO

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================
PRINT '✓ Base de datos creada exitosamente';
PRINT '✓ Todas las tablas, índices e vistas han sido creadas';
PRINT '✓ Datos de prueba insertados';
PRINT '';
PRINT 'INFORMACIÓN IMPORTANTE:';
PRINT '- Cambiar contraseña del admin (hash MD5: e99a18c428cb38d5f260853678922e03 = 123456)';
PRINT '- Implementar encriptación de contraseñas en la aplicación (bcrypt recomendado)';
PRINT '- Configurar backups automáticos';
PRINT '- Revisar y ajustar las restricciones según datos locales';
