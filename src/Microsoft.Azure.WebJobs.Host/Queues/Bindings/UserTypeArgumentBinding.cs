﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.WebJobs.Host.Queues.Bindings
{
    internal class UserTypeArgumentBinding : IArgumentBinding<CloudQueue>
    {
        private readonly Type _valueType;

        public UserTypeArgumentBinding(Type valueType)
        {
            _valueType = valueType;
        }

        public Type ValueType
        {
            get { return _valueType; }
        }

        public Task<IValueProvider> BindAsync(CloudQueue value, ValueBindingContext context)
        {
            IValueProvider provider = new UserTypeValueBinder(value, _valueType, context.FunctionInstanceId,
                context.MessageEnqueuedWatcher);
            return Task.FromResult(provider);
        }

        private class UserTypeValueBinder : IOrderedValueBinder
        {
            private readonly CloudQueue _queue;
            private readonly Type _valueType;
            private readonly Guid _functionInstanceId;
            private readonly IMessageEnqueuedWatcher _messageEnqueuedWatcher;

            public UserTypeValueBinder(CloudQueue queue, Type valueType, Guid functionInstanceId,
                IMessageEnqueuedWatcher messageEnqueuedWatcher)
            {
                _queue = queue;
                _valueType = valueType;
                _functionInstanceId = functionInstanceId;
                _messageEnqueuedWatcher = messageEnqueuedWatcher;
            }

            public int StepOrder
            {
                get { return BindStepOrders.Enqueue; }
            }

            public Type Type
            {
                get { return _valueType; }
            }

            public object GetValue()
            {
                return null;
            }

            public string ToInvokeString()
            {
                return _queue.Name;
            }

            public async Task SetValueAsync(object value, CancellationToken cancellationToken)
            {
                CloudQueueMessage message = QueueCausalityManager.EncodePayload(_functionInstanceId, value);

                await _queue.AddMessageAndCreateIfNotExistsAsync(message, cancellationToken);

                if (_messageEnqueuedWatcher != null)
                {
                    _messageEnqueuedWatcher.Notify(_queue.Name);
                }
            }
        }
    }
}