using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Queries.Products;
using Hypesoft.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new product.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/products
    ///     {
    ///       "name": "Notebook Dell XPS 15",
    ///       "description": "Laptop de alto desempenho com tela OLED 4K",
    ///       "price": 12999.90,
    ///       "stockQuantity": 50,
    ///       "categoryId": "64f1a2b3c4d5e6f7a8b9c0d1"
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [EnableRateLimiting(RateLimitingExtensions.WritesPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Returns a paginated list of products with optional filters.</summary>
    /// <remarks>
    /// Sample response:
    ///
    ///     GET /api/products?pageNumber=1&amp;pageSize=10
    ///     {
    ///       "data": [
    ///         {
    ///           "id": "64f1a2b3c4d5e6f7a8b9c0d1",
    ///           "name": "Notebook Dell XPS 15",
    ///           "description": "Laptop de alto desempenho",
    ///           "price": 12999.90,
    ///           "stockQuantity": 50,
    ///           "categoryId": "64f1a2b3c4d5e6f7a8b9c0d2",
    ///           "categoryName": "Eletrônicos"
    ///         }
    ///       ],
    ///       "pageNumber": 1,
    ///       "pageSize": 10,
    ///       "totalRecords": 1,
    ///       "totalPages": 1
    ///     }
    ///
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool lowStockOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(pageNumber, pageSize, searchTerm, categoryId, lowStockOnly);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single product by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates an existing product.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [EnableRateLimiting(RateLimitingExtensions.WritesPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProductBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, body.Name, body.Description, body.Price, body.StockQuantity, body.CategoryId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates only the stock quantity of a product.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/products/{id}/stock
    ///     {
    ///       "quantity": 75
    ///     }
    ///
    /// </remarks>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "admin")]
    [EnableRateLimiting(RateLimitingExtensions.WritesPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateStockBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateStockCommand(id, body.Quantity), cancellationToken);
        return Ok(result);
    }

    public sealed record UpdateProductBody(string Name, string? Description, decimal Price, int StockQuantity, string? CategoryId);
    public sealed record UpdateStockBody(int Quantity);

    /// <summary>Deletes a product by ID.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [EnableRateLimiting(RateLimitingExtensions.WritesPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
