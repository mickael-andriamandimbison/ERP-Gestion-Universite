using erp_univ_back.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        
        int statusCode;
        string titre;
        string detail = exception.Message;

        if (exception is AppException ex)
        {
            statusCode = ex.StatusCode;
            titre = "Erreur Métier";
            _logger.LogWarning("Erreur métier contrôlée : {Msg}", exception.Message);
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            titre = "Erreur Interne";
            detail = "Une erreur technique imprévue est survenue.";
            _logger.LogError(exception, "CRASH TECHNIQUE APPLICATIF !");
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titre,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}
