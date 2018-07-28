# Docker Compose Fixture
A XUnit fixture that allows you to spin up docker compose files and then run tests against them.

## Example Integration Test

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

## Logging
To enable XUnit logging you will have to add a xunit.runner.json file to your test project. The file should be copied to the output directory and should look like this:
    
    { "diagnosticMessages":  true }
