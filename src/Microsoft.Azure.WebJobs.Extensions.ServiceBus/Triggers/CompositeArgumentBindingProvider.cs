// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.ServiceBus;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class CompositeArgumentBindingProvider<T> : IQueueTriggerArgumentBindingProvider<T>
    {
        private readonly IEnumerable<IQueueTriggerArgumentBindingProvider<T>> _providers;

        public CompositeArgumentBindingProvider(params IQueueTriggerArgumentBindingProvider<T>[] providers)
        {
            _providers = providers;
        }

        public ITriggerDataArgumentBinding<T> TryCreate(ParameterInfo parameter)
        {
            foreach (IQueueTriggerArgumentBindingProvider<T> provider in _providers)
            {
                ITriggerDataArgumentBinding<T> binding = provider.TryCreate(parameter);

                if (binding != null)
                {
                    return binding;
                }
            }

            return null;
        }
    }
}
