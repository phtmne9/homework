using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PvzApi.Controllers;
using PvzApi.Features.Parcels;
using PvzApi.Models;
using PvzApi.Services;
using Xunit;

namespace PvzApi.Tests;

public class ParcelsControllerTests
{
    private readonly Mock<IParcelService> _parcels = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<ParcelCreateDto>> _validator = new();

    private ParcelsController Controller() =>
        new(_parcels.Object, _mediator.Object, _validator.Object);

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _parcels
            .Setup(p => p.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParcelDto?)null);

        var result = await Controller().GetById(5, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        _parcels
            .Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ParcelDto { Id = 1, TrackNumber = "PVZ-1" });

        var result = await Controller().GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("PVZ-1", Assert.IsType<ParcelDto>(ok.Value).TrackNumber);
    }

    [Fact]
    public async Task Accept_ReturnsBadRequest_WhenValidationFails()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ParcelCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Weight", "Вес") }));

        var result = await Controller().Accept(new ParcelCreateDto(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _parcels.Verify(
            p => p.AcceptAsync(It.IsAny<ParcelCreateDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Accept_CallsService_WhenValid()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ParcelCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _parcels
            .Setup(p => p.AcceptAsync(It.IsAny<ParcelCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Посылка принята");

        var result = await Controller().Accept(new ParcelCreateDto(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _parcels.Verify(
            p => p.AcceptAsync(It.IsAny<ParcelCreateDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Issue_SendsCommandThroughMediator()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<IssueParcelCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Посылка выдана");

        var result = await Controller().Issue(1, new IssueRequest { Phone = "+77011112233" });

        Assert.IsType<OkObjectResult>(result);
        _mediator.Verify(
            m => m.Send(
                It.Is<IssueParcelCommand>(c => c.Id == 1 && c.Phone == "+77011112233"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
