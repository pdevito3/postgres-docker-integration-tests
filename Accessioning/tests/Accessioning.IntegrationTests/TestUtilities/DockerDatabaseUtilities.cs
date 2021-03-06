// based on https://blog.dangl.me/archive/running-sql-server-integration-tests-in-net-core-projects-via-docker/

namespace Accessioning.IntegrationTests.TestUtilities
{
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Npgsql;

    public static class DockerSqlDatabaseUtilities
    {
        public const string DB_SA_PASSWORD = "postgres";
        public const string DB_USER = "postgres";
        public const string DB_NAME = "stripe";
        public const string DB_IMAGE = "postgres";
        public const string DB_IMAGE_TAG = "latest";
        public const string DB_CONTAINER_NAME = "IntegrationTestingContainer_Accessioning";
        public const string DB_VOLUME_NAME = "IntegrationTestingVolume_Accessioning";

        public static async Task<(string containerId, string port)> EnsureDockerStartedAndGetContainerIdAndPortAsync()
        {
            await CleanupRunningContainers();
            await CleanupRunningVolumes();
            var dockerClient = GetDockerClient();
            var freePort = GetFreePort(); //"5432"; // GetFreePort();

            // This call ensures that the latest SQL Server Docker image is pulled
            await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = $"{DB_IMAGE}:{DB_IMAGE_TAG}"
            }, null, new Progress<JSONMessage>());

            // create a volume, if one doesn't already exist
            var volumeList = await dockerClient.Volumes.ListAsync();
            var volumeCount = volumeList.Volumes.Where(v => v.Name == DB_VOLUME_NAME).Count();
            if(volumeCount <= 0)
            {
                await dockerClient.Volumes.CreateAsync(new VolumesCreateParameters
                {
                    Name = DB_VOLUME_NAME,
                });
            }

            // create container, if one doesn't already exist
            var contList = await dockerClient
                .Containers.ListContainersAsync(new ContainersListParameters() { All = true });
            var existingCont = contList
                .Where(c => c.Names.Any(n => n.Contains(DB_CONTAINER_NAME))).FirstOrDefault();

            if (existingCont == null)
            {
                var sqlContainer = await dockerClient
                    .Containers
                    .CreateContainerAsync(new CreateContainerParameters
                    {
                        Name = DB_CONTAINER_NAME,
                        Image = $"{DB_IMAGE}:{DB_IMAGE_TAG}",
                        Env = new List<string>
                        {
                            $"POSTGRES_USER={DB_USER}",
                            $"POSTGRES_DB={DB_NAME}",
                            $"POSTGRES_PASSWORD={DB_SA_PASSWORD}"
                        },
                        HostConfig = new HostConfig
                        {
                            PortBindings = new Dictionary<string, IList<PortBinding>>
                            {
                                {
                                    "5432/tcp",
                                    new PortBinding[]
                                    {
                                        new PortBinding
                                        {
                                            HostPort = freePort
                                        }
                                    }
                                }
                            },
                            Binds = new List<string>
                            {
                                $"{DB_VOLUME_NAME}:/var/lib/postgresql/data"
                            }
                        },
                    });

                await dockerClient
                    .Containers
                    .StartContainerAsync(sqlContainer.ID, new ContainerStartParameters());

                await WaitUntilDatabaseAvailableAsync(freePort);
                return (sqlContainer.ID, freePort);
            }

            return (existingCont.ID, existingCont.Ports.FirstOrDefault().PublicPort.ToString());
        }

        public static async Task EnsureDockerContainersStoppedAndRemovedAsync(string dockerContainerId)
        {
            var dockerClient = GetDockerClient();
            await dockerClient.Containers
                .StopContainerAsync(dockerContainerId, new ContainerStopParameters());
            await dockerClient.Containers
                .RemoveContainerAsync(dockerContainerId, new ContainerRemoveParameters());
        }

        public static async Task EnsureDockerVolumesRemovedAsync(string volumeName)
        {
            var dockerClient = GetDockerClient();
            await dockerClient.Volumes.RemoveAsync(volumeName);
        }

        private static DockerClient GetDockerClient()
        {
            var dockerUri = IsRunningOnWindows()
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            return new DockerClientConfiguration(new Uri(dockerUri))
                .CreateClient();
        }

        private static async Task CleanupRunningContainers(int hoursTillExpiration = -24)
        {
            var dockerClient = GetDockerClient();

            var runningContainers = await dockerClient.Containers
                .ListContainersAsync(new ContainersListParameters());

            foreach (var runningContainer in runningContainers.Where(cont => cont.Names.Any(n => n.Contains(DB_CONTAINER_NAME))))
            {
                // Stopping all test containers that are expired -- defaulted to a day
                var expiration = hoursTillExpiration > 0 
                    ? hoursTillExpiration * -1 
                    : hoursTillExpiration;
                if (runningContainer.Created < DateTime.UtcNow.AddHours(expiration))
                {
                    try
                    {
                        await EnsureDockerContainersStoppedAndRemovedAsync(runningContainer.ID);
                    }
                    catch
                    {
                        // Ignoring failures to stop running containers
                    }
                }
            }
        }

        private static async Task CleanupRunningVolumes(int hoursTillExpiration = -24)
        {
            var dockerClient = GetDockerClient();

            var runningVolumes = await dockerClient.Volumes.ListAsync();

            foreach (var runningVolume in runningVolumes.Volumes.Where(v => v.Name == DB_VOLUME_NAME))
            {
                // Stopping all test containers that are older than one hour, they likely failed to cleanup
                var expiration = hoursTillExpiration > 0
                    ? hoursTillExpiration * -1
                    : hoursTillExpiration;
                if (DateTime.Parse(runningVolume.CreatedAt) < DateTime.UtcNow.AddHours(expiration))
                {
                    try
                    {
                        await EnsureDockerVolumesRemovedAsync(runningVolume.Name);
                    }
                    catch
                    {
                        // Ignoring failures to stop running containers
                    }
                }
            }
        }

        private static async Task WaitUntilDatabaseAvailableAsync(string databasePort)
        {
            var start = DateTime.UtcNow;
            const int maxWaitTimeSeconds = 60;
            var connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
            {
                try
                {
                    var sqlConnectionString = GetSqlConnectionString(databasePort);

                    //var sqlConnectionString = $"User ID={DB_USER};Password={DB_SA_PASSWORD};Host=localhost;Port={databasePort};Database={DB_NAME};";
                    //var sqlConnectionString = $"Data Source=localhost,{databasePort};Integrated Security=False;User ID=SA;Password={DB_SA_PASSWORD}";

                    using var sqlConnection = new NpgsqlConnection(sqlConnectionString);
                    await sqlConnection.OpenAsync();
                    connectionEstablised = true;
                }
                catch
                {
                    // If opening the SQL connection fails, SQL Server is not ready yet
                    await Task.Delay(500);
                }
            }

            if (!connectionEstablised)
            {
                throw new Exception("Connection to the SQL docker database could not be established within 60 seconds.");
            }

            return;
        }

        public static string GetSqlConnectionString(string port)
        {
            var sqlConnectionString = new NpgsqlConnectionStringBuilder()
            {
                Host = "localhost",
                Password = DB_SA_PASSWORD,
                Username = DB_USER,
                Database = DB_NAME,
                Port = Int32.Parse(port)
            };

            var con = sqlConnectionString.ToString();
            return con;
        }

        private static string GetFreePort()
        {
            // From https://stackoverflow.com/a/150974/4190785
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port.ToString();
        }

        private static bool IsRunningOnWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
    }
}