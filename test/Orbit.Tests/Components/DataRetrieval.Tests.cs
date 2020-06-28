using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Orbit.Components;
using Orbit.Data;
using Orbit.Models;
using Orbit.Util;

using Xunit;

namespace Orbit.Tests.Components
{
    public class DbContext_tests
    {
        [Fact]
        public void Can_return_default_value_from_a_set()
        {
            using var scope = OrbitServiceProvider.Instance.CreateScope();

            scope.ServiceProvider.GetRequiredService<OrbitDbContext>().InsertSeedData();

            var service = scope.ServiceProvider.GetService<IMonitoredComponent<WasteWaterStorageTankData>>();
            service.Invoking(async s => await s.GetLatestReportAsync().ConfigureAwait(false)).Should().NotThrow();
        }

        [Fact]
        public void Can_instantiate_OrbitDbContext()
        {
            using var scope = OrbitServiceProvider.Instance.CreateScope();

            scope.ServiceProvider.Invoking(p => p.GetRequiredService<OrbitDbContext>()).Should().NotThrow();
        }

        public static IEnumerable<object[]> GetAllDefinedModels()
        {
            var types = typeof(OrbitDbContext).Assembly.ExportedTypes.Where(t => !t.IsInterface && t.GetInterfaces().Contains(typeof(IModel)));
            foreach (var modelType in types)
            {
                yield return new object[] { modelType };
            }
        }

        [Theory]
        [MemberData(nameof(GetAllDefinedModels))]
        public void DbSet_registered_for_type(Type type)
        {
            var dbType = typeof(DbSet<>).MakeGenericType(type);
            var properties = typeof(OrbitDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.PropertyType);
            properties.Should().ContainEquivalentOf(dbType, because:"any model we define needs to be added explicitly to the DbContext");
        }
    }
}