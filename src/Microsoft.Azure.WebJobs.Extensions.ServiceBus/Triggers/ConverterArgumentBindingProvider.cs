// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.ServiceBus;
using System.Linq;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class ConverterArgumentBindingProvider<TInput, TOutput> : IQueueTriggerArgumentBindingProvider<TInput>
        where TInput : class
    {
        private readonly IAsyncConverter<TInput, TOutput> _converter;

        public ConverterArgumentBindingProvider(IAsyncConverter<TInput, TOutput> converter)
        {
            _converter = converter;
        }

        public ITriggerDataArgumentBinding<TInput> TryCreate(ParameterInfo parameter)
        {
            if (parameter.ParameterType != typeof(TOutput))
            {
                return null;
            }

            return new ConverterArgumentBinding(_converter);
        }

        internal class ConverterArgumentBinding : ITriggerDataArgumentBinding<TInput>
        {
            private readonly IAsyncConverter<TInput, TOutput> _converter;

            public ConverterArgumentBinding(IAsyncConverter<TInput, TOutput> converter)
            {
                _converter = converter;
            }

            public Type ValueType
            {
                get { return typeof(TOutput); }
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                get { return null; }
            }

            public async Task<ITriggerData> BindAsync(TInput value, ValueBindingContext context)
            {
                IValueProvider provider = null;
                Message message = value as Message;
                if (message != null)
                {
                    Message messageClone = message.Clone();
                    object converted = await _converter.ConvertAsync(message as TInput, context.CancellationToken);
                    provider = await MessageValueProvider.CreateAsync(messageClone, converted, typeof(TInput),
                        context.CancellationToken);
                }
                else
                {
                    Message[] messages = value as Message[];
                    Message[] arrayClone = messages.Select(x => x.Clone()).ToArray();
                    object converted = await _converter.ConvertAsync(arrayClone as TInput, context.CancellationToken);
                    provider = await MessageValueProvider.CreateAsync(arrayClone, converted, typeof(TInput),
                        context.CancellationToken);
                }
                return new TriggerData(provider, null);
            }
        }
    }

    //internal class ConverterArgumentArrayBindingProvider<T> : IQueueTriggerArgumentBindingProvider<Message[]>
    //{
    //    private readonly IAsyncConverter<Message[], T> _converter;

    //    public ConverterArgumentArrayBindingProvider(IAsyncConverter<Message[], T> converter)
    //    {
    //        _converter = converter;
    //    }

    //    public ITriggerDataArgumentBinding<Message[]> TryCreate(ParameterInfo parameter)
    //    {
    //        if (parameter.ParameterType != typeof(T))
    //        {
    //            return null;
    //        }

    //        return new ConverterArrayArgumentBinding(_converter);
    //    }

    //    internal class ConverterArrayArgumentBinding : ITriggerDataArgumentBinding<Message[]>
    //    {
    //        private readonly IAsyncConverter<Message[], T> _converter;

    //        public ConverterArrayArgumentBinding(IAsyncConverter<Message[], T> converter)
    //        {
    //            _converter = converter;
    //        }

    //        public Type ValueType
    //        {
    //            get { return typeof(T); }
    //        }

    //        public IReadOnlyDictionary<string, Type> BindingDataContract
    //        {
    //            get { return null; }
    //        }

    //        public async Task<ITriggerData> BindAsync(Message[] value, ValueBindingContext context)
    //        {
    //            Message[] cloneArray = value.Select(x => x.Clone()).ToArray();
    //            object converted = await _converter.ConvertAsync(value, context.CancellationToken);
    //            IValueProvider provider = await MessageValueProvider.CreateAsync(cloneArray, converted, typeof(T),
    //                context.CancellationToken);
    //            return new TriggerData(provider, null);
    //        }
    //    }
    //}
}
