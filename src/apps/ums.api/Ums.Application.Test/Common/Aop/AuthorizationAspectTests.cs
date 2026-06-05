using System;
using System.Reflection;
using BeyondNetCode.Shell.Aop;
using FluentAssertions;
using Moq;
using Ums.Application.Common.Aop;
using Ums.Application.Common.Interfaces;
using Xunit;

namespace Ums.Application.Test.Common.Aop;

public class AuthorizationAspectTests
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly AuthorizationAspect _sut;
    private readonly Mock<IJoinPoint> _joinPointMock;

    public AuthorizationAspectTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _sut = new AuthorizationAspect(_userContextMock.Object);
        _joinPointMock = new Mock<IJoinPoint>();

        var targetType = typeof(CreateUserCommandHandler);
        _joinPointMock.Setup(j => j.TargetType).Returns(targetType);
        _joinPointMock.Setup(j => j.MethodInfo).Returns(targetType.GetMethod(nameof(CreateUserCommandHandler.Handle))!);
    }

    [Fact]
    public void Apply_WhenNoAttribute_Proceeds()
    {
        // Arrange
        var targetType = typeof(UnprotectedCommandHandler);
        _joinPointMock.Setup(j => j.TargetType).Returns(targetType);
        _joinPointMock.Setup(j => j.MethodInfo).Returns(targetType.GetMethod(nameof(UnprotectedCommandHandler.Handle))!);

        // Act
        _sut.Apply(_joinPointMock.Object);

        // Assert
        _joinPointMock.Verify(j => j.Proceed(), Times.Once);
    }

    [Fact]
    public void Apply_WithAttributeAndPermission_Proceeds()
    {
        // Arrange
        // Simular que Shell.Aop provee el atributo
        _userContextMock.Setup(u => u.HasPermission("user:create")).Returns(true);

        // Act
        _sut.Apply(_joinPointMock.Object);
        
        // Assert
        _joinPointMock.Verify(j => j.Proceed(), Times.Once);
    }
}

// Clases simuladas para el test
[AuthorizationAspect]
public class CreateUserCommandHandler 
{
    public void Handle() { }
}

public class UnprotectedCommandHandler 
{
    public void Handle() { }
}
