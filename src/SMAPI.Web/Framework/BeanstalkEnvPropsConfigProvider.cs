using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Reads configuration values from the AWS Beanstalk environment properties file (if present).</summary>
    /// <remarks>This is a workaround for AWS Beanstalk injection not working with .NET Core apps.</remarks>
    internal class BeanstalkEnvPropsConfigProvider : ConfigurationProvider, IConfigurationSource
    {
        /*********
        ** Properties
        *********/
        /// <summary>The absolute path to the container configuration file on an Amazon EC2 instance.</summary>
        private const string ContainerConfigPath = @"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration";


        /*********
        ** Public methods
        *********/
        /// <summary>Build the configuration provider for this source.</summary>
        /// <param name="builder">The configuration builder.</param>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new BeanstalkEnvPropsConfigProvider();
        }

        /// <summary>Load the environment properties.</summary>
        public override void Load()
        {
            this.Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // get Beanstalk config file
            FileInfo file = new FileInfo(BeanstalkEnvPropsConfigProvider.ContainerConfigPath);
            if (!file.Exists)
                return;

            // parse JSON
            JObject jsonRoot = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file.FullName));
            if (jsonRoot["iis"]?["env"] is JArray jsonProps)
            {
                foreach (string prop in jsonProps.Values<string>())
                {
                    string[] parts = prop.Split('=', 2); // key=value
                    if (parts.Length == 2)
                        this.Data[parts[0]] = parts[1];
                }
            }
        }
    }
}
