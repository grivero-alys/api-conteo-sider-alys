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
| `camera` | Text | Si | `camera01` | Codigo de camara. |
| `countedAt` | Text | Si | `2026-06-15 12:30:50` | Fecha/hora del conteo. |
| `steelDiameter` | Text | Si | `2` | Diametro del acero/fierro. |
| `itemCount` | Text | Si | `10` | Cantidad contada. Debe ser mayor a cero. |
| `video` | File | Si | `conteo.mp4` | Archivo de video del conteo. |

### Ejemplo cURL

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/individual" \
  -H "x-functions-key: <FUNCTION_KEY>" \
  -F "headquarter=chimbote" \
  -F "camera=camera01" \
  -F "countedAt=2026-06-15 12:30:50" \
  -F "steelDiameter=2" \
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
| `camera` | Text | Si | `camera01` | Codigo de camara. |
| `countedAt` | Text | Si | `2026-06-15 12:30:50` | Fecha/hora del conteo. |
| `steelDiameter` | Text | Si | `2` | Diametro del acero/fierro. |
| `itemCount` | Text | Si | `10` | Cantidad contada. Debe ser mayor a cero. |
| `video` | File | Si | `conteo.mp4` | Archivo de video del conteo. |

### Ejemplo cURL

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/grouped" \
  -H "x-functions-key: <FUNCTION_KEY>" \
  -F "headquarter=chimbote" \
  -F "camera=camera01" \
  -F "countedAt=2026-06-15 12:30:50" \
  -F "steelDiameter=2" \
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
  "steelDiameter": "2",
  "itemCount": 10,
  "countedAt": "2026-06-15T12:30:50+00:00",
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
5. Agregar los campos `headquarter`, `camera`, `countedAt`, `steelDiameter`, `itemCount`.
6. Agregar `video` como tipo `File`.
7. Enviar el request.
