# 📋 Análisis Detallado - Puntos 2.0 y 4.0

---

## 🧠 2.0 - OBJETIVO DEL SISTEMA (Desarrollo Expandido)

### 2.1 Propósito General
El sistema busca ser la **columna vertebral operativa** de la empresa constructora, garantizando:

#### 🎯 Objetivos Estratégicos
- **Automatización de procesos**: Eliminar manualmente operaciones repetitivas (pesaje, cálculo de neto, facturación)
- **Visibilidad en tiempo real**: Conocer estado de inventario, ventas del día, clientes activos
- **Trazabilidad completa**: Desde entrada de material hasta venta, con registro de quién, cuándo y por qué
- **Reducción de errores**: Cálculos automáticos evitan errores de peso, precio, descuentos
- **Cumplimiento normativo**: Base para facturación electrónica futura (SRI)

### 2.2 Métricas de Éxito
- ✅ Reducción de tiempo de venta: De 15 min a 3 min (pesaje + factura)
- ✅ Precisión en inventario: 99.5% de exactitud
- ✅ Disponibilidad: 99% uptime (offline-first)
- ✅ Auditoría: 100% de transacciones registradas
- ✅ Escalabilidad: Manejo de 500+ transacciones diarias

### 2.3 Procesos Automatizados
1. **Venta completa**:
   - Lectura automática de báscula → Cálculo de peso neto → Cálculo de valor → Generación de factura

2. **Control de inventario**:
   - Entrada de producto → Actualización de stock → Alertas automáticas

3. **Auditoría**:
   - Registro automático de cada acción con timestamp y usuario

### 2.4 Contexto Técnico
- **Limitaciones del cliente**: Conectividad limitada → Necesidad de funcionamiento **offline prioritario**
- **Hardware especializado**: Báscula industrial con conexión RS232 → Integración técnica requerida
- **Base de datos local**: SQL Server Express o SQLite para sincronizar después

---

## 🚛 4.0 - GESTIÓN DE CLIENTES Y VEHÍCULOS (Desarrollo Expandido)

### 4.1 Entidad: CLIENTE

#### 4.1.1 Datos Básicos
```
ID_Cliente (PK)
Nombre completo
Tipo de identificación (RUC, Cédula, Pasaporte)
Número de identificación (único)
Contacto primario (teléfono)
Email
Dirección
Observaciones
```

#### 4.1.2 Datos Comerciales
```
Categoría (Transportista, Mayorista, Minorista, Etc.)
Descuento por defecto (%)
Plazo de crédito (días)
Límite de crédito
Estado (Activo, Inactivo, Bloqueado)
Fecha de registro
```

#### 4.1.3 Casos de Uso
- **UC01**: Registrar nuevo cliente
  - Validar identificación única
  - Asignar código automático
  - Crear registro de movimientos de crédito

- **UC02**: Vincular vehículos a cliente
  - Un cliente puede tener hasta 2 vehículos
  - Validar placa única en el sistema
  - Permitir cambio de vehículo en venta

- **UC03**: Consultar historial de cliente
  - Últimas compras
  - Saldo de crédito
  - Total comprado en período

---

### 4.2 Entidad: VEHÍCULO

#### 4.2.1 Datos Identificación
```
ID_Vehículo (PK)
Placa única (puede ser alfanumérica)
Tipo (Volqueta, Gandola, Furgón, Etc.)
Marca
Modelo
Color
```

#### 4.2.2 Datos Técnicos
```
Capacidad (toneladas)
Año fabricación
Peso tara conocido (para báscula)
VIN (opcional, para seguridad)
Placa de identificación INEN (Ecuador)
```

#### 4.2.3 Datos Operacionales
```
ID_Cliente (FK)
Estado (Activo, Inactivo, Mantenimiento)
Última pesada (fecha)
Combustible (para cálculo de costos futuros)
Observaciones
```

#### 4.2.4 Relación Cliente-Vehículo
```
Un cliente → Hasta 2 vehículos activos
Un vehículo → Pertenece a 1 cliente
Estado: Permite cambiar vehículo durante venta
```

---

### 4.3 Flujo de Negocio: Cliente + Vehículo en Venta

```
┌─────────────────────────────────────────────────┐
│ 1. Seleccionar Cliente                          │
│    └─ Buscar por nombre/RUC/Cédula             │
│    └─ Validar estado (no bloqueado)            │
│    └─ Cargar historial de crédito              │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 2. Seleccionar Vehículo del Cliente             │
│    └─ Mostrar hasta 2 vehículos activos        │
│    └─ Permitir cambio si es necesario          │
│    └─ Cargar tara, capacidad máxima            │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 3. Pesaje Inicial (Tara)                        │
│    └─ Leer desde báscula vía RS232             │
│    └─ Comparar con tara registrada (alerta)    │
│    └─ Registrar timestamp y peso               │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 4. Carga Product en Vehículo (Física)           │
│    └─ Tiempo variable                          │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 5. Pesaje Final (Bruto)                         │
│    └─ Leer desde báscula vía RS232             │
│    └─ Cálculo automático: Neto = Bruto - Tara │
│    └─ Registrar timestamp y peso               │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 6. Seleccionar Productos y Aplicar Precios     │
│    └─ Descuento cliente + descuento volumen    │
│    └─ Calcular valor total                     │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ 7. Generar Documento (Ticket/Factura)           │
│    └─ Incluir datos cliente y vehículo         │
│    └─ Incluir pesajes (tara, bruto, neto)     │
│    └─ Registrar en auditoría                   │
└─────────────────────────────────────────────────┘
```

---

### 4.4 Restricciones y Validaciones

| Restricción | Descripción | Validación |
|------------|-------------|-----------|
| **Identificación única** | RUC/Cédula no puede repetirse | INDEX UNIQUE + trigger |
| **Placa única** | Una placa = un vehículo | INDEX UNIQUE + trigger |
| **Máximo 2 vehículos** | Por cliente | Constraint en BD |
| **Cliente activo** | No vender a clientes bloqueados | Check constraint |
| **Vehículo activo** | No usar vehículos inactivos | Check constraint |
| **Capacidad respetada** | No superar capacidad máxima | Validación en aplicación + trigger |
| **Tara válida** | Comparar con valores históricos | Alerta si >10% diferencia |

---

### 4.5 Datos de Ejemplo

#### Cliente
```
ID: 1
Nombre: Transportes García & Cía.
RUC: 1700123456789
Teléfono: 0987654321
Categoría: Transportista
Descuento: 5%
```

#### Vehículos (2 máximo)
```
Vehículo 1:
  - Placa: EBC-123
  - Tipo: Volqueta
  - Capacidad: 15 toneladas
  - Tara: 6500 kg

Vehículo 2:
  - Placa: EBC-124
  - Tipo: Gandola
  - Capacidad: 20 toneladas
  - Tara: 8000 kg
```

---

### 4.6 Reportes Asociados

- 📊 **Clientes activos por período**
- 🚛 **Vehículos de un cliente**
- 📈 **Top clientes por volumen**
- ⚠️ **Clientes en mora**
- 🔍 **Historial de pesajes por vehículo**
