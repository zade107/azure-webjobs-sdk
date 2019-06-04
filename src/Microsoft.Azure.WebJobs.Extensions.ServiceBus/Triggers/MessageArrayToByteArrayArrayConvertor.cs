// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class MessageArrayToByteArrayArrayConvertor : IAsyncConverter<Message[], byte[][]>
    {
        public Task<byte[][]> ConvertAsync(Message[] input, CancellationToken cancellationToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(input.Select(x => x.Body).ToArray());
        }
    }
}
