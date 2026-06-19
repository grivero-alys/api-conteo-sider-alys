# API Conteo SIDER - Guia de Consumo TI

Este documento describe como consumir los endpoints HTTP de la Function App `fn-sider` para registrar conteos individuales y agrupados.

## Base URL

Produccion:

```text
https://fn-sider.azurewebsites.net/api
```

## Autenticacion

Todos los endpoints requieren Function Key.

Enviar la key en el header:

```text
x-functions-key: <FUNCTION_KEY>
```

Tambien se puede enviar como query string:

```text
?code=<FUNCTION_KEY>
```

Recomendado: usar el header `x-functions-key` y guardar la key como secreto o variable de entorno en el sistema consumidor.

## Content Type

Los endpoints reciben `multipart/form-data`.

En Postman se debe seleccionar:

```text
Body -> form-data
```

No configurar manualmente el header `Content-Type`; Postman lo genera automaticamente con el `boundary`.

## Valores permitidos

### camera

Valores aceptados:

```text
camera01
camera02
camera03
camera04
```

### steelDiameter

Enviar el codigo numerico (`diameterMm`) que corresponde al diametro nominal:

| Diametro nominal | steelDiameter |
| --- | --- |
| 6mm | `6000` |
| 8mm | `8000` |
| 3/8" | `9525` |
| 12mm | `12000` |
| 1/2" | `12700` |
| 5/8" | `15875` |
| 3/4" | `19050` |
| 1" | `25400` |
| 1 3/8" | `34925` |

Valores permitidos:

```text
6000, 8000, 9525, 12000, 12700, 15875, 19050, 25400, 34925
```

## Endpoint 1: Conteo Individual

Registra un conteo individual.

```text
POST https://fn-sider.azurewebsites.net/api/bundle/individual
```

Function en Azure:

```text
NewBundleIndividual
```

### Campos form-data

| Campo | Tipo | Requerido | Ejemplo | Descripcion |
| --- | --- | --- | --- | --- |
| `headquarter` | Text | Si | `chimbote` | Sede o planta donde se realiza el conteo. |
| `camera` | Text | Si | `camera01` | Codigo de camara. Valores: `camera01`, `camera02`, `camera03`, `camera04`. |
| `countStartedAt` | Text | Si | `2026-06-15 12:30:00` | Fecha/hora de inicio del conteo. |
| `countFinishedAt` | Text | Si | `2026-06-15 12:30:50` | Fecha/hora de fin del conteo. |
| `steelDiameter` | Text | Si | `12700` | Codigo numerico del diametro (`diameterMm`). Ver tabla de valores permitidos. |
| `itemCount` | Text | Si | `10` | Cantidad contada. Debe ser mayor a cero. |
| `video` | File | Si | `conteo.mp4` | Archivo de video del conteo. |

### Ejemplo cURL

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/individual" \
  -H "x-functions-key: <FUNCTION_KEY>" \
  -F "headquarter=chimbote" \
  -F "camera=camera01" \
  -F "countStartedAt=2026-06-15 12:30:00" \
  -F "countFinishedAt=2026-06-15 12:30:50" \
  -F "steelDiameter=12700" \
  -F "itemCount=10" \
  -F "video=@./conteo.mp4;type=video/mp4"
```

## Endpoint 2: Conteo Agrupado

Registra un conteo agrupado.

```text
POST https://fn-sider.azurewebsites.net/api/bundle/grouped
```

Function en Azure:

```text
NewBundleGrouped
```

### Campos form-data

| Campo | Tipo | Requerido | Ejemplo | Descripcion |
| --- | --- | --- | --- | --- |
| `headquarter` | Text | Si | `chimbote` | Sede o planta donde se realiza el conteo. |
| `camera` | Text | Si | `camera01` | Codigo de camara. Valores: `camera01`, `camera02`, `camera03`, `camera04`. |
| `countStartedAt` | Text | Si | `2026-06-15 12:30:00` | Fecha/hora de inicio del conteo. |
| `countFinishedAt` | Text | Si | `2026-06-15 12:30:50` | Fecha/hora de fin del conteo. |
| `steelDiameter` | Text | Si | `12700` | Codigo numerico del diametro (`diameterMm`). Ver tabla de valores permitidos. |
| `itemCount` | Text | Si | `10` | Cantidad contada. Debe ser mayor a cero. |
| `video` | File | Si | `conteo.mp4` | Archivo de video del conteo. |

### Ejemplo cURL

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/grouped" \
  -H "x-functions-key: <FUNCTION_KEY>" \
  -F "headquarter=chimbote" \
  -F "camera=camera01" \
  -F "countStartedAt=2026-06-15 12:30:00" \
  -F "countFinishedAt=2026-06-15 12:30:50" \
  -F "steelDiameter=12700" \
  -F "itemCount=10" \
  -F "video=@./conteo.mp4;type=video/mp4"
```

## Respuesta Exitosa

Status:

```text
201 Created
```

Ejemplo:

```json
{
  "bundleId": 80,
  "bundleCode": "SIDER-CHIMBOTE-CAMERA01-INDIVIDUAL-20260615123050",
  "bundleType": "INDIVIDUAL",
  "steelDiameter": "12700",
  "itemCount": 10,
  "countStartedAt": "2026-06-15T12:30:00+00:00",
  "countFinishedAt": "2026-06-15T12:30:50+00:00",
  "countTime": "00:00:50",
  "videoPath": "https://stplataformaindustriadev.blob.core.windows.net/bundle-videos/sider/2026/06/15/SIDER-CHIMBOTE-CAMERA01-INDIVIDUAL-20260615123050.mp4",
  "sentToSider": false
}
```

Para el endpoint agrupado, `bundleType` retorna:

```text
AGRUPADO
```

## Formato del bundleCode

El `bundleCode` se genera en la API con este formato:

```text
EMPRESA-SEDE-CAMARA-TIPO-yyyyMMddHHmmss
```

Ejemplo:

```text
SIDER-CHIMBOTE-CAMERA01-INDIVIDUAL-20260615123050
```

El ultimo segmento (`yyyyMMddHHmmss`) corresponde a la fecha y hora actual de Lima, Peru, al momento de crear el registro.

## Errores

### 400 Bad Request

Ocurre cuando falta un campo requerido o el formato es invalido.

Ejemplo:

```json
{
  "error": "itemCount debe ser mayor a cero."
}
```

### 401 Unauthorized

Ocurre cuando no se envia la Function Key o la key es incorrecta.

Solucion:

```text
x-functions-key: <FUNCTION_KEY>
```

### 500 Internal Server Error

Ocurre cuando falla un proceso interno, por ejemplo SQL Server, Blob Storage o integracion con SIDER.

Ejemplo:

```json
{
  "error": "No se pudo crear el conteo individual.",
  "detail": "Mensaje tecnico del error.",
  "exceptionType": "SqlException",
  "traceId": "d328f107-ced9-49ce-98b7-9ca83764bab6"
}
```

El campo `traceId` sirve para buscar el error en logs de Azure.

## Checklist para Postman

1. Metodo: `POST`.
2. URL: usar `/api/bundle/individual` o `/api/bundle/grouped`.
3. Header: agregar `x-functions-key`.
4. Body: seleccionar `form-data`.
5. Agregar los campos `headquarter`, `camera`, `countStartedAt`, `countFinishedAt`, `steelDiameter`, `itemCount`.
6. Agregar `video` como tipo `File`.
7. Enviar el request.
