using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Bot.Builder;

using Autofac;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace StuddyBot
{
    public class BotAzureBlobStorage:IStorage
    {
        private readonly IStorage azureBlob;

        public BotAzureBlobStorage()
        {
            azureBlob= new AzureBlobStorage(
                  "DefaultEndpointsProtocol=https;AccountName=rdbotstorage;AccountKey=8mePuJGlKxPah6H8UZ6B44WLW9HYzmT82KERgGLDCGQDHA7Xtt+isqt1oQeAr/SPjP0DsrZRUt2blEsPDiHUPg==;EndpointSuffix=core.windows.net",
                  "blobcontainerforrdbot");
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            return azureBlob.DeleteAsync(keys, cancellationToken);
        }

        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            return azureBlob.ReadAsync(keys, cancellationToken);
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            return azureBlob.WriteAsync(changes, cancellationToken);
        }
    }
}
