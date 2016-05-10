// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace Microsoft.Azure.WebJobs.Host.Blobs.Listeners
{
    [Singleton(Mode = SingletonMode.Listener)]
    internal sealed class SingletonBlobListener : BlobListener
    {
        public SingletonBlobListener(ISharedListener sharedListener) : base(sharedListener)
        {
        }
    }
}
