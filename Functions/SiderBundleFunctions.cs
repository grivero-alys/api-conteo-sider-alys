using System.Net;
using System.Text;
using System.Text.Json;
using api_conteo_sider_alys.Models;
using api_conteo_sider_alys.Services.SiderBundle;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace api_conteo_sider_alys.Functions;

public sealed class SiderBundleFunctions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ISiderBundleService _bundleService;
    private readonly ILogger<SiderBundleFunctions> _logger;

    public SiderBundleFunctions(ISiderBundleService bundleService, ILogger<SiderBundleFunctions> logger)
    {
        _bundleService = bundleService;
        _logger = logger;
    }

    [Function("NewBundleIndividual")]
    public async Task<HttpResponseData> CreateIndividualCount(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bundle/individual")] HttpRequestData req)
    {
        try
        {
            var form = await ReadMultipartFormDataAsync(req);

            var input = new BundleCreationInput
            {
                BundleType = BundleTypes.Individual,
                Headquarter = GetField(form.Fields, "headquarter"),
                CountStartedAt = GetDateField(form.Fields, "countStartedAt"),
                CountFinishedAt = GetDateField(form.Fields, "countFinishedAt"),
                Camera = GetField(form.Fields, "camera"),
                SteelDiameter = GetField(form.Fields, "steelDiameter"),
                ItemCount = GetIntField(form.Fields, "itemCount"),
                Video = form.Video
            };

            var validationError = Validate(input);
            if (validationError is not null)
            {
                return await BadRequestAsync(req, validationError);
            }

            var response = await _bundleService.CreateAsync(input, req.FunctionContext.CancellationToken);
            return await JsonAsync(req, HttpStatusCode.Created, response);
        }
        catch (InvalidDataException exception)
        {
            return await BadRequestAsync(req, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating individual SIDER count.");
            return await InternalServerErrorAsync(req, "No se pudo crear el conteo individual.", exception);
        }
    }

    [Function("NewBundleGrouped")]
    public async Task<HttpResponseData> CreateGroupedCount(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bundle/grouped")] HttpRequestData req)
    {
        try
        {
            var form = await ReadMultipartFormDataAsync(req);
            var input = new BundleCreationInput
            {
                BundleType = BundleTypes.Grouped,
                Headquarter = GetField(form.Fields, "headquarter"),
                CountStartedAt = GetDateField(form.Fields, "countStartedAt"),
                CountFinishedAt = GetDateField(form.Fields, "countFinishedAt"),
                Camera = GetField(form.Fields, "camera"),
                SteelDiameter = GetField(form.Fields, "steelDiameter"),
                ItemCount = GetIntField(form.Fields, "itemCount"),
                Video = form.Video
            };

            var validationError = Validate(input);
            if (validationError is not null)
            {
                return await BadRequestAsync(req, validationError);
            }

            var response = await _bundleService.CreateAsync(input, req.FunctionContext.CancellationToken);
            return await JsonAsync(req, HttpStatusCode.Created, response);
        }
        catch (InvalidDataException exception)
        {
            return await BadRequestAsync(req, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating grouped SIDER count.");
            return await InternalServerErrorAsync(req, "No se pudo crear el conteo agrupado.", exception);
        }
    }

    private static string? Validate(BundleCreationInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Headquarter))
        {
            return "headquarter es requerido.";
        }

        if (string.IsNullOrWhiteSpace(input.Camera))
        {
            return "camera es requerido para generar el código de lote.";
        }

        if (input.CountStartedAt == default)
        {
            return "countStartedAt es requerido.";
        }

        if (input.CountFinishedAt == default)
        {
            return "countFinishedAt es requerido.";
        }

        if (input.CountFinishedAt < input.CountStartedAt)
        {
            return "countFinishedAt debe ser mayor o igual a countStartedAt.";
        }

        if (string.IsNullOrWhiteSpace(input.SteelDiameter))
        {
            return "steelDiameter es requerido.";
        }

        if (input.ItemCount <= 0)
        {
            return "itemCount debe ser mayor a cero.";
        }

        if (input.Video is null)
        {
            return "video es requerido.";
        }

        return null;
    }

    private static async Task<MultipartBundleForm> ReadMultipartFormDataAsync(HttpRequestData req)
    {
        var contentType = GetHeader(req, "Content-Type");
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidDataException("Content-Type multipart/form-data es requerido.");
        }

        var mediaTypeHeader = MediaTypeHeaderValue.Parse(contentType);
        if (!mediaTypeHeader.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("El endpoint recibe multipart/form-data.");
        }

        var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeader.Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Boundary multipart/form-data es requerido.");
        }

        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        UploadedVideo? video = null;
        var reader = new MultipartReader(boundary, req.Body);
        var section = await reader.ReadNextSectionAsync(req.FunctionContext.CancellationToken);

        while (section is not null)
        {
            if (!string.IsNullOrWhiteSpace(section.ContentDisposition))
            {
                var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);
                var name = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value ?? string.Empty;
                var fileName =
                    HeaderUtilities.RemoveQuotes(contentDisposition.FileNameStar).Value ??
                    HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value;

                if (name.Equals("video", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(fileName))
                {
                    await using var memoryStream = new MemoryStream();
                    await section.Body.CopyToAsync(memoryStream, req.FunctionContext.CancellationToken);
                    video = new UploadedVideo
                    {
                        FileName = Path.GetFileName(fileName),
                        ContentType = section.ContentType ?? "application/octet-stream",
                        Content = new MemoryStream(memoryStream.ToArray())
                    };
                }
                else if (!string.IsNullOrWhiteSpace(name))
                {
                    using var streamReader = new StreamReader(section.Body, Encoding.UTF8, leaveOpen: true);
                    fields[name] = (await streamReader.ReadToEndAsync(req.FunctionContext.CancellationToken)).Trim();
                }
            }

            section = await reader.ReadNextSectionAsync(req.FunctionContext.CancellationToken);
        }

        return new MultipartBundleForm(fields, video);
    }

    private static string? GetHeader(HttpRequestData req, string name)
    {
        return req.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }

    private static string GetField(IReadOnlyDictionary<string, string> fields, string name)
    {
        return fields.TryGetValue(name, out var value) ? value : string.Empty;
    }

    private static DateTimeOffset GetDateField(IReadOnlyDictionary<string, string> fields, string name)
    {
        if (!fields.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        if (!DateTimeOffset.TryParse(value, out var parsed))
        {
            throw new InvalidDataException($"{name} debe ser una fecha válida.");
        }

        return parsed;
    }

    private static int GetIntField(IReadOnlyDictionary<string, string> fields, string name)
    {
        if (!fields.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (!int.TryParse(value, out var parsed))
        {
            throw new InvalidDataException($"{name} debe ser un número entero.");
        }

        return parsed;
    }

    private static Task<HttpResponseData> BadRequestAsync(HttpRequestData req, string message)
    {
        return JsonAsync(req, HttpStatusCode.BadRequest, new { error = message });
    }

    private static Task<HttpResponseData> InternalServerErrorAsync(HttpRequestData req, string message, Exception exception)
    {
        return JsonAsync(
            req,
            HttpStatusCode.InternalServerError,
            new
            {
                error = message,
                detail = exception.Message,
                exceptionType = exception.GetType().Name,
                traceId = req.FunctionContext.InvocationId
            });
    }

    private static async Task<HttpResponseData> JsonAsync(HttpRequestData req, HttpStatusCode statusCode, object body)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await JsonSerializer.SerializeAsync(response.Body, body, body.GetType(), JsonOptions);
        return response;
    }

    private sealed record MultipartBundleForm(
        IReadOnlyDictionary<string, string> Fields,
        UploadedVideo? Video);
}
