// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Azure.ServiceBus;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class StringArrayToTextMessageArrayConverter : IConverter<string[], Message[]>
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Message[] Convert(string[] input)
        {
            return input.Select(x =>
            {
                Message message = new Message(StrictEncodings.Utf8.GetBytes(x))
                {
                    ContentType = ContentTypes.TextPlain
                };
                return message;
            }).ToArray();
        }
    }

    internal class StringArrayToBinarydMessageArrayConverter : IConverter<string[], Message[]>
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Message[] Convert(string[] input)
        {
            return input.Select(x =>
            {
                byte[] contents = StrictEncodings.Utf8.GetBytes(x);
                Message message = new Message(contents);
                message.ContentType = ContentTypes.ApplicationOctetStream;
                return message;
            }).ToArray();
        }
    }

    internal class StringArrayToJsonMessageArrayConverter : IConverter<string[], Message[]>
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Message[] Convert(string[] input)
        {
            return input.Select(x =>
            {
                Message message = new Message(StrictEncodings.Utf8.GetBytes(x));
                message.ContentType = ContentTypes.ApplicationJson;
                return message;
            }).ToArray();
        }
    }
}
