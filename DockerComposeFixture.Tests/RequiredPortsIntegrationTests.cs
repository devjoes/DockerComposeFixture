using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DockerComposeFixture.Exceptions;
using Xunit;

namespace DockerComposeFixture.Tests;

public class RequiredPortsIntegrationTests : IClassFixture<DockerFixture>, IDisposable
{
    private readonly DockerFixture dockerFixture;
    private readonly string dockerComposeFile;

    private const string DockerCompose = @"
version: '3.4'
services:
    echo_server:
        image: hashicorp/http-echo
        ports:
        - 12871:8080
        command: -listen=:8080 -text=""hello world""
        ";
    
    public RequiredPortsIntegrationTests(DockerFixture dockerFixture)
    {
        this.dockerFixture = dockerFixture;
        this.dockerComposeFile = Path.GetTempFileName();
        File.WriteAllText(this.dockerComposeFile, DockerCompose);
    }

    [Fact]
    public void Ports_DetermineUtilizedPorts_ReturnsThoseInUse()
    {
        // Arrange
        const ushort portToOccupy = 12871;
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, portToOccupy);
        using Socket listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        listener.Bind(ipEndPoint);
        listener.Listen();
        
        // Act
        var usedPorts = Ports.DetermineUtilizedPorts([12870, 12871, 12872]);
        
        // Assert
        Assert.Single(usedPorts, p => p == 12871);
    }

    [Fact]
    public void GivenRequiredPortInUse_ThenExceptionIsThrownWithPortNumber()
    {
        // Arrange
        const ushort portToOccupy = 12871;
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, portToOccupy);
        using Socket listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        listener.Bind(ipEndPoint);
        listener.Listen();
        
        // Act & Assert
        var thrownException = Assert.Throws<PortsUnavailableException>(() =>
        {
            dockerFixture.InitOnce(() => new DockerFixtureOptions
            {
                DockerComposeFiles = new[] { this.dockerComposeFile },
                CustomUpTest = output => output.Any(l => l.Contains("server is listening")),
                RequiredPorts = new ushort[] { portToOccupy }
            });
        });
        
        Assert.Equivalent(new ushort[] { portToOccupy }, thrownException.Ports);
    }
    
    public void Dispose()
    {
        File.Delete(this.dockerComposeFile);
    }
}