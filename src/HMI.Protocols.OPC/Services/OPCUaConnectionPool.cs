using HMI.Platform.Core.ConnectionPooling;
using Opc.Ua;
using Opc.Ua.Client;

namespace HMI.Protocols.OPC.Services;

/// <summary>
/// Connection pool for OPC UA connections.
/// </summary>
public class OPCUaConnectionPool : BaseConnectionPool<Session>
{
    private readonly string _endpointUrl;
    private readonly ApplicationConfiguration _applicationConfiguration;

    public OPCUaConnectionPool(string endpointUrl, ConnectionPoolOptions? options = null)
        : base(options)
    {
        _endpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
        _applicationConfiguration = CreateApplicationConfiguration();
    }

    protected override async Task<Session> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var endpointDescription = CoreClientUtils.SelectEndpoint(_endpointUrl, useSecurity: false);
        var configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, EndpointConfiguration.Create(_applicationConfiguration));
        var userIdentity = new UserIdentity(new AnonymousIdentityToken());

        var session = await Session.Create(
            _applicationConfiguration,
            configuredEndpoint,
            false,
            "HMI OPC UA Client",
            60000,
            userIdentity,
            null,
            cancellationToken);

        if (session == null || !session.Connected)
        {
            throw new InvalidOperationException("Failed to create OPC UA session");
        }

        return session;
    }

    protected override async Task<bool> IsConnectionValidAsync(Session connection, CancellationToken cancellationToken)
    {
        try
        {
            if (connection == null || !connection.Connected)
                return false;

            // Perform a simple read operation to verify the connection is actually working
            await Task.Run(() => connection.ReadValue(Variables.Server_ServerStatus_State), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override async Task DisposeConnectionAsync(Session connection)
    {
        try
        {
            if (connection != null)
            {
                await connection.CloseAsync();
                connection.Dispose();
            }
        }
        catch
        {
            // Ignore disposal errors
        }
    }

    private ApplicationConfiguration CreateApplicationConfiguration()
    {
        return new ApplicationConfiguration
        {
            ApplicationName = "HMI OPC UA Client",
            ApplicationUri = Utils.Format(@"urn:{0}:HMIOPCClient", System.Net.Dns.GetHostName()),
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = "OPC Foundation/CertificateStores/DefaultApplications",
                    SubjectName = "HMI OPC UA Client"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "OPC Foundation/CertificateStores/UA Certificate Authorities"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "OPC Foundation/CertificateStores/UA Applications"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "OPC Foundation/CertificateStores/RejectedCertificates"
                }
            }
        };
    }
}

