# API Conteo SIDER ALYS

Azure Functions HTTP API para registrar conteos SIDER enviados por cámara.

## Base URL

Producción:

```text
https://fn-sider.azurewebsites.net/api
```

Local:

```text
http://localhost:7071/api
```

Los endpoints usan `AuthorizationLevel.Function`, por lo que en Azure se debe enviar una Function Key. Se puede enviar como query string:

```text
?code=TU_FUNCTION_KEY
```

O como header:

```text
x-functions-key: TU_FUNCTION_KEY
```

## Endpoints

### POST `/bundle/individual`

Registra un conteo individual. La Function publicada en Azure se llama `NewBundleIndividual`.

URL de producción:

```text
POST https://fn-sider.azurewebsites.net/api/bundle/individual?code=TU_FUNCTION_KEY
```

Ejemplo con `curl`:

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/individual?code=TU_FUNCTION_KEY" \
  -F "headquarter=LIMA" \
  -F "countedAt=2026-06-12T22:45:00Z" \
  -F "camera=CAM01" \
  -F "steelDiameter=1/2" \
  -F "itemCount=10" \
  -F "video=@./conteo.mp4;type=video/mp4"
```

### POST `/bundle/grouped`

Registra un conteo agrupado. La Function publicada en Azure se llama `NewBundleGrouped`.

URL de producción:

```text
POST https://fn-sider.azurewebsites.net/api/bundle/grouped?code=TU_FUNCTION_KEY
```

Ejemplo con `curl`:

```shell
curl -X POST "https://fn-sider.azurewebsites.net/api/bundle/grouped?code=TU_FUNCTION_KEY" \
  -F "headquarter=LIMA" \
  -F "countedAt=2026-06-12T22:45:00Z" \
  -F "camera=CAM01" \
  -F "steelDiameter=1/2" \
  -F "itemCount=100" \
  -F "video=@./conteo-grupal.mp4;type=video/mp4"
```

## Body

El request debe enviarse como `multipart/form-data`.

Campos requeridos:

- `headquarter`: sede o planta del conteo. Ejemplo: `LIMA`.
- `countedAt`: fecha y hora del conteo en formato ISO 8601. Ejemplo: `2026-06-12T22:45:00Z`.
- `camera`: código de cámara. Ejemplo: `CAM01`.
- `steelDiameter`: diámetro del fierro. Ejemplo: `1/2`.
- `itemCount`: cantidad contada. Debe ser mayor a cero.
- `video`: archivo de video del conteo.

En Postman:

1. Seleccionar método `POST`.
2. Usar la URL del endpoint con `?code=TU_FUNCTION_KEY` o agregar el header `x-functions-key`.
3. Ir a `Body`.
4. Seleccionar `form-data`.
5. Crear los campos requeridos.
6. En el campo `video`, cambiar el tipo de `Text` a `File` y adjuntar el video.

## Respuesta Exitosa

Status:

```text
201 Created
```

Ejemplo:

```json
{
  "bundleId": 123,
  "bundleCode": "SIDER-LIMA-CAM01-INDIVIDUAL-20260612224500000-A1B2C3D4",
  "bundleType": "INDIVIDUAL",
  "rebarDiameter": "1/2",
  "itemCount": 10,
  "countedAt": "2026-06-12T22:45:00+00:00",
  "videoPath": "https://storage.blob.core.windows.net/bundle-videos/sider/2026/06/12/SIDER-LIMA-CAM01-INDIVIDUAL-20260612224500000-A1B2C3D4.mp4",
  "sentToSider": true
}
```

## Errores

Si falta un campo requerido o tiene formato inválido:

```text
400 Bad Request
```

Ejemplo:

```json
{
  "error": "itemCount debe ser mayor a cero."
}
```

Si ocurre un error no controlado al guardar en Blob Storage, registrar en base de datos o enviar a SIDER:

```text
500 Internal Server Error
```

## Código de Lote

El código de lote se genera con este formato:

```text
EMPRESA-SEDE-CAMARA-INDIVIDUAL|AGRUPADO-yyyyMMddHHmmss
```

El último segmento corresponde a la fecha y hora actual de Lima, Peru, al momento de crear el registro.

## Blob Storage

Los videos se guardan en el contenedor configurado por `BlobStorageContainerName`. Si no se configura, usa `bundle-videos`.

Ruta del video:

```text
{enterprise}/{yyyy}/{MM}/{dd}/{bundleCode}.mp4
```

## Configuración

Variables requeridas en `local.settings.json` o en Application Settings de Azure:

- `SqlConnectionString`: conexión a SQL Server.
- `BlobStorageConnectionString`: conexión a Azure Blob Storage.
- `BlobStorageContainerName`: contenedor donde se guardan los videos. Opcional, por defecto `bundle-videos`.
- `Enterprise`: código de empresa usado para generar el código de lote. Opcional, por defecto `sider`.
- `SiderApiEndpoint`: endpoint externo de SIDER. Si no se configura, la API registra el conteo pero marca `sentToSider` como `false`.
- `SiderApiKey`: API key para enviar datos a SIDER. Opcional.

## Base de Datos

Antes de usar los endpoints, ejecutar:

```sql
Sql/20260612_add_bundle_type_to_bundle.sql
Sql/20260615_drop_bundle_barcode_unique.sql
Sql/20260615_create_bundle_create_procedure.sql
```

El campo `materialCode` se llena con `steelDiameter` porque en la tabla actual no hay una columna dedicada para el diámetro de fierro.
