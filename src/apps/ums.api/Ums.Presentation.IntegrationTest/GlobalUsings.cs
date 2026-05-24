global using System.Net;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using Ums.Application.Common.Interfaces;
global using Ums.Domain.Authorization;
global using Ums.Domain.Configuration;
global using Ums.Domain.Configuration.AppConfiguration;
global using Ums.Domain.Configuration.FeatureFlag;
global using Ums.Domain.Configuration.IdpConfiguration;
global using Ums.Domain.Enums;
global using Ums.Domain.Identity;
global using Ums.Domain.Kernel.ValueObjects;
global using Ums.Infrastructure.Persistence;
global using Ums.Infrastructure.Persistence.Authorization;
global using Ums.Infrastructure.Persistence.Configuration;
global using Ums.Infrastructure.Persistence.Identity;
global using Ums.Shell.Aop.Aspects;
global using Ums.Shell.Aop.Aspects.Logger.Serilog;
global using Ums.Shell.Ddd;
global using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
