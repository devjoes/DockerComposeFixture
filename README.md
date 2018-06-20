# DockerComposeFixture
A XUnit fixture that allows you to spin up docker compose files and then run tests against them.

# Example Integration Test

    public class IntegrationTests : IClassFixture<DockerFixture>
    {
        public IntegrationTests(DockerFixture dockerFixture)
        {
            dockerFixture.InitOnce(() => new DockerFixtureOptions
            {
                DockerComposeFiles = new[] { "docker-compose.yml" },
                CustomUpTest = output => output.Any(l => l.Contains("App is ready"))
            });
        }

        // Tests go here
    }
