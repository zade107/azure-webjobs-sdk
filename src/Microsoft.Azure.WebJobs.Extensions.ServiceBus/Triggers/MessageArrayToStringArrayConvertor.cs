// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class MessageArrayToStringArrayConvertor : IAsyncConverter<Message[], string[]>
    {
        private MessageToStringConverter _innerConvertor = new MessageToStringConverter();

        public async Task<string[]> ConvertAsync(Message[] input, CancellationToken cancellationToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Task<string>> tasks = input.Select(async x => {
                return await _innerConvertor.ConvertAsync(x, cancellationToken);
            });

            return await Task.WhenAll(tasks);
        }
    }
}
