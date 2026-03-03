using Hypesoft.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns a summary with total products, total stock value, low-stock products and products grouped by category.</summary>
    /// <remarks>
    /// Sample response:
    ///
    ///     GET /api/dashboard
    ///     {
    ///       "totalProducts": 120,
    ///       "totalStockValue": 485320.50,
    ///       "lowStockCount": 8,
    ///       "productsByCategory": [
    ///         { "categoryName": "Eletrônicos", "productCount": 45 },
    ///         { "categoryName": "Periféricos", "productCount": 30 }
    ///       ],
    ///       "lowStockProducts": [
    ///         {
    ///           "id": "64f1a2b3c4d5e6f7a8b9c0d1",
    ///           "name": "Teclado Mecânico",
    ///           "stockQuantity": 3,
    ///           "categoryName": "Periféricos"
    ///         }
    ///       ]
    ///     }
    ///
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}
