using Asp.Versioning;
using FluentAssertions;
using LLMProxy.Admin.API.Controllers.V20251222;
using LLMProxy.Admin.API.Controllers.V20260115;
using LLMProxy.Application.Common;
using LLMProxy.Application.Tenants.Commands;
using LLMProxy.Application.Tenants.Queries;
using LLMProxy.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;

namespace LLMProxy.Admin.API.Tests.Controllers;

/// <summary>
/// Tests de validation du support multi-versions de l'API (ADR-037)
/// </summary>
/// <remarks>
/// Valide que :
/// - Les contrôleurs v1.0 et v2.0 coexistent correctement
/// - Les réponses diffèrent selon la version (v1 simple, v2 enrichie)
/// - Les routes versionnées fonctionnent (/api/v1/ vs /api/v2/)
/// - La pagination v2 est fonctionnelle
/// </remarks>
public class ApiVersioningTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _loggerV1;
    private readonly ILogger<TenantsV2Controller> _loggerV2;

    public ApiVersioningTests()
    {
        _mediator = Substitute.For<IMediator>();
        _loggerV1 = Substitute.For<ILogger<TenantsController>>();
        _loggerV2 = Substitute.For<ILogger<TenantsV2Controller>>();
    }

    /// <summary>
    /// Vérifie que le contrôleur v1 utilise le namespace V20251222
    /// </summary>
    [Fact]
    public void TenantsController_ShouldHaveVersion1Attribute()
    {
        // Arrange & Act
        var controllerType = typeof(TenantsController);
        var namespaceName = controllerType.Namespace;

        // Assert - La version est détectée depuis le namespace
        Check.That(namespaceName).IsEqualTo("LLMProxy.Admin.API.Controllers.V20251222");
        
        // Vérifier qu'il n'y a PAS d'attribut ApiVersion (namespace convention)
        var versionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false)
            .Cast<ApiVersionAttribute>()
            .FirstOrDefault();
        Check.That(versionAttribute).IsNull();
    }

    /// <summary>
    /// Vérifie que le contrôleur v2 utilise le namespace V20260115
    /// </summary>
    [Fact]
    public void TenantsV2Controller_ShouldHaveVersion2Attribute()
    {
        // Arrange & Act
        var controllerType = typeof(TenantsV2Controller);
        var namespaceName = controllerType.Namespace;

        // Assert - La version est détectée depuis le namespace
        Check.That(namespaceName).IsEqualTo("LLMProxy.Admin.API.Controllers.V20260115");
        
        // Vérifier qu'il n'y a PAS d'attribut ApiVersion (namespace convention)
        var versionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false)
            .Cast<ApiVersionAttribute>()
            .FirstOrDefault();
        Check.That(versionAttribute).IsNull();
    }

    /// <summary>
    /// Vérifie que la route utilise le format api/v{version:apiVersion}/[controller]
    /// </summary>
    [Fact]
    public void TenantsController_ShouldHaveVersionedRoute()
    {
        // Arrange & Act
        var controllerType = typeof(TenantsController);
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .FirstOrDefault();

        // Assert
        Check.That(routeAttribute).IsNotNull();
        Check.That(routeAttribute!.Template).IsEqualTo("api/v{version:apiVersion}/[controller]");
    }

    /// <summary>
    /// Vérifie que la route v2 utilise le format api/v{version:apiVersion}/tenants
    /// <summary>
    /// Vérifie que la route v2 utilise le format api/v{version:apiVersion}/tenants
    /// </summary>
    [Fact]
    public void TenantsV2Controller_ShouldHaveVersionedRoute()
    {
        // Arrange & Act
        var controllerType = typeof(TenantsV2Controller);
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>()
            .FirstOrDefault();

        // Assert
        Check.That(routeAttribute).IsNotNull();
        Check.That(routeAttribute!.Template).IsEqualTo("api/v{version:apiVersion}/tenants");
    }

    /// <summary>
    /// Valide que la réponse v1.0 est simple (juste les données)
    /// </summary>
    [Fact]
    public async Task TenantsV1_GetById_ShouldReturnSimpleResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantDto = new TenantDto 
        { 
            Id = tenantId, 
            Name = "Test Tenant",
            Slug = "test-tenant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mediator.Send(Arg.Any<GetTenantByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TenantDto>.Success(tenantDto)));

        var controller = new TenantsController(_mediator, _loggerV1);

        // Act
        var result = await controller.GetById(tenantId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(tenantDto); // V1: Réponse simple, juste les données
    }

    /// <summary>
    /// Valide que la réponse v2.0 est enrichie (données + métadonnées)
    /// </summary>
    [Fact]
    public async Task TenantsV2_GetById_ShouldReturnEnrichedResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantDto = new TenantDto 
        { 
            Id = tenantId, 
            Name = "Test Tenant",
            Slug = "test-tenant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mediator.Send(Arg.Any<GetTenantByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TenantDto>.Success(tenantDto)));

        var controller = new TenantsV2Controller(_mediator, _loggerV2)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // Act
        var result = await controller.GetById(tenantId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as dynamic;
        
        // V2: Réponse enrichie avec métadonnées
        response.Should().NotBeNull();
        ((object)response!.data).Should().Be(tenantDto);
        ((string)response.version).Should().Be("2026-01-15");
        ((object)response.requestId).Should().NotBeNull();
        ((object)response.timestamp).Should().NotBeNull();
    }

    /// <summary>
    /// Valide que v1.0 ne supporte PAS la pagination (GET renvoie toutes les données)
    /// </summary>
    [Fact]
    public async Task TenantsV1_GetAll_ShouldReturnAllItemsWithoutPagination()
    {
        // Arrange
        var tenants = new List<TenantDto>
        { 
            new TenantDto { Id = Guid.NewGuid(), Name = "Tenant 1", Slug = "tenant-1", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TenantDto { Id = Guid.NewGuid(), Name = "Tenant 2", Slug = "tenant-2", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TenantDto { Id = Guid.NewGuid(), Name = "Tenant 3", Slug = "tenant-3", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mediator.Send(Arg.Any<GetAllTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<TenantDto>>.Success(tenants.AsEnumerable())));

        var controller = new TenantsController(_mediator, _loggerV1);

        // Act
        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as IEnumerable<TenantDto>;
        
        response.Should().HaveCount(3); // V1: Pas de pagination, tous les items
    }

    /// <summary>
    /// Valide que v2.0 supporte la pagination avec métadonnées complètes
    /// </summary>
    [Fact]
    public async Task TenantsV2_GetAll_ShouldReturnPaginatedResponse()
    {
        // Arrange
        var tenants = Enumerable.Range(1, 50)
            .Select(i => new TenantDto 
            { 
                Id = Guid.NewGuid(), 
                Name = $"Tenant {i}",
                Slug = $"tenant-{i}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        _mediator.Send(Arg.Any<GetAllTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<TenantDto>>.Success(tenants.AsEnumerable())));

        var controller = new TenantsV2Controller(_mediator, _loggerV2)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // Act
        var result = await controller.GetAll(page: 2, pageSize: 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as dynamic;
        
        response.Should().NotBeNull();
        
        // V2: Page 2 avec 10 items
        var data = (IEnumerable<TenantDto>)response!.data;
        data.Should().HaveCount(10);
        
        // V2: Métadonnées de pagination
        ((int)response.pagination.page).Should().Be(2);
        ((int)response.pagination.pageSize).Should().Be(10);
        ((int)response.pagination.totalCount).Should().Be(50);
        ((int)response.pagination.totalPages).Should().Be(5);
        ((bool)response.pagination.hasNext).Should().BeTrue();
        ((bool)response.pagination.hasPrevious).Should().BeTrue();
    }

    /// <summary>
    /// Valide la validation des paramètres de pagination (page invalide)
    /// </summary>
    [Fact]
    public async Task TenantsV2_GetAll_WithInvalidPage_ShouldReturnBadRequest()
    {
        // Arrange
        var controller = new TenantsV2Controller(_mediator, _loggerV2);

        // Act
        var result = await controller.GetAll(page: 0, pageSize: 10);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value as dynamic;
        
        ((string)response!.error).Should().Contain("numéro de page doit être >= 1");
    }

    /// <summary>
    /// Valide la validation des paramètres de pagination (pageSize invalide)
    /// </summary>
    [Theory]
    [InlineData(0)]     // Trop petit
    [InlineData(-1)]    // Négatif
    [InlineData(101)]   // Trop grand (max 100)
    public async Task TenantsV2_GetAll_WithInvalidPageSize_ShouldReturnBadRequest(int pageSize)
    {
        // Arrange
        var controller = new TenantsV2Controller(_mediator, _loggerV2);

        // Act
        var result = await controller.GetAll(page: 1, pageSize: pageSize);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value as dynamic;
        
        ((string)response!.error).Should().Contain("taille de page doit être entre 1 et 100");
    }

    /// <summary>
    /// Valide que v2.0 crée avec Location header correct (api/v2/tenants/{id})
    /// </summary>
    [Fact]
    public async Task TenantsV2_Create_ShouldReturnCreatedAtActionWithLocationHeader()
    {
        // Arrange
        var newTenantId = Guid.NewGuid();
        var createdTenant = new TenantDto
        {
            Id = newTenantId,
            Name = "New Tenant",
            Slug = "new-tenant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mediator.Send(Arg.Any<CreateTenantCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TenantDto>.Success(createdTenant)));

        var controller = new TenantsV2Controller(_mediator, _loggerV2)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var command = new CreateTenantCommand { Name = "New Tenant", Slug = "new-tenant" };

        // Act
        var result = await controller.Create(command, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        
        createdResult.ActionName.Should().Be(nameof(TenantsV2Controller.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        // Pas de version dans RouteValues avec header versioning
        
        var response = createdResult.Value as dynamic;
        ((Guid)response!.data.id).Should().Be(newTenantId);
        ((string)response.version).Should().Be("2026-01-15");
    }
}
