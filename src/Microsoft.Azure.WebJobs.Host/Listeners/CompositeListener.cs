// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Host.Listeners
{
    internal sealed class CompositeListener : IListener
    {
        private bool _disposed;

        public CompositeListener(params IListener[] listeners)
            : this((IEnumerable<IListener>)listeners)
        {
        }

        public CompositeListener(IEnumerable<IListener> listeners)
        {
            Listeners = listeners;
        }

        internal IEnumerable<IListener> Listeners { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            // start all listeners in parallel
            List<Task> tasks = new List<Task>();
            foreach (IListener listener in Listeners)
            {
                tasks.Add(listener.StartAsync(cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            // stop all listeners in parallel
            List<Task> tasks = new List<Task>();
            foreach (IListener listener in Listeners)
            {
                tasks.Add(listener.StopAsync(cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        public void Cancel()
        {
            ThrowIfDisposed();

            foreach (IListener listener in Listeners)
            {
                listener.Cancel();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (IListener listener in Listeners)
                {
                    listener.Dispose();
                }

                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
