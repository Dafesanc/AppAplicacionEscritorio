-- ============================================================================
-- FIX: Agregar columna IntentosFallidos y corregir CHECK constraint de Estado
-- Ejecutar este script UNA vez en la base de datos existente
-- ============================================================================

USE GestionComercialDB;  -- cambia si tu BD tiene otro nombre
GO

-- 1. Agregar columna IntentosFallidos (si no existe)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Usuarios')
      AND name = 'IntentosFallidos'
)
BEGIN
    ALTER TABLE Usuarios
        ADD IntentosFallidos INT NOT NULL DEFAULT 0;
    PRINT 'Columna IntentosFallidos agregada correctamente.';
END
ELSE
BEGIN
    PRINT 'Columna IntentosFallidos ya existe — sin cambios.';
END
GO

-- 2. Eliminar CHECK constraint antiguo de Estado (solo permite ACTIVO/INACTIVO)
--    y recrearlo incluyendo BLOQUEADO
DECLARE @constraintName NVARCHAR(256);

SELECT @constraintName = cc.name
FROM sys.check_constraints cc
INNER JOIN sys.columns col
    ON cc.parent_object_id = col.object_id
   AND cc.parent_column_id = col.column_id
WHERE cc.parent_object_id = OBJECT_ID('Usuarios')
  AND col.name = 'Estado';

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Usuarios DROP CONSTRAINT ' + @constraintName);
    PRINT 'CHECK constraint anterior eliminado: ' + @constraintName;
END

ALTER TABLE Usuarios
    ADD CONSTRAINT CK_Usuarios_Estado
        CHECK (Estado IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO'));

PRINT 'CHECK constraint CK_Usuarios_Estado creado correctamente.';
GO

-- 3. Verificación final
SELECT
    COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Usuarios'
ORDER BY ORDINAL_POSITION;
GO
