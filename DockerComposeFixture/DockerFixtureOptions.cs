using System;
using System.Collections.Generic;
using System.Text;

namespace DockerComposeFixture
{
    /// <summary>
    /// Options that control how docker-compose is executed
    /// </summary>
    public class DockerFixtureOptions : IDockerFixtureOptions
    {
        /// <summary>
        /// Checks whether the docker-compose services have come up correctly based upon the output of docker-compose
        /// </summary>
        public Func<List<string>, bool> CustomUpTest { get; set; }
        /// <summary>
        /// Array of docker compose files
        /// Files are converted into the arguments '-f file1 -f file2 etc'
        /// </summary>
        public string[] DockerComposeFiles { get; set; }
        /// <summary>
        /// When true this logs docker-compose output to %temp%\docker-compose-*.log
        /// </summary>
        public bool DebugLog { get; set; }
        /// <summary>
        /// Arguments to append after 'docker-compose -f file.yml up'
        /// Default is 'docker-compose -f file.yml up --build'
        /// </summary>
        public string DockerComposeUpArgs { get; set; } = "--build";
        /// <summary>
        /// Arguments to append after 'docker-compose -f file.yml down'
        /// Default is 'docker-compose -f file.yml down --remove-orphans --rmi all'
        /// </summary>
        public string DockerComposeDownArgs { get; set; } = "--remove-orphans --rmi all";

        public void Validate()
        {
            if (this.DockerComposeFiles == null
                || this.DockerComposeFiles.Length == 0)
            {
                throw new ArgumentException(nameof(this.DockerComposeFiles));
            }
        }


    }
}
