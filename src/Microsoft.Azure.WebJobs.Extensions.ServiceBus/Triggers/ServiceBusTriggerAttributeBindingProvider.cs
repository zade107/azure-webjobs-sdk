// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.ServiceBus.Triggers
{
    internal class ServiceBusTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private static readonly IQueueTriggerArgumentBindingProvider<Message> InnerProvider =
            new CompositeArgumentBindingProvider<Message>(
                new ConverterArgumentBindingProvider<Message, Message>(
                    new AsyncConverter<Message, Message>(new IdentityConverter<Message>())),
                new ConverterArgumentBindingProvider<Message, string>(new MessageToStringConverter()),
                new ConverterArgumentBindingProvider<Message, byte[]>(new MessageToByteArrayConverter()),
                new UserTypeArgumentBindingProvider<Message>()); // Must come last, because it will attempt to bind all types.

        private static readonly IQueueTriggerArgumentBindingProvider<Message[]> InnerArrayProvider =
            new CompositeArgumentBindingProvider<Message[]>(
                new ConverterArgumentBindingProvider<Message[], Message[]>(
                    new AsyncConverter<Message[], Message[]>(new IdentityConverter<Message[]>())),
                new ConverterArgumentBindingProvider<Message[], string[]>(new MessageArrayToStringArrayConvertor()),
                new ConverterArgumentBindingProvider<Message[], byte[][]>(new MessageArrayToByteArrayArrayConvertor()),
                new UserTypeArgumentBindingProvider<Message[]>()
                ); // Must come last, because it will attempt to bind all types.

        private readonly INameResolver _nameResolver;
        private readonly ServiceBusOptions _options;
        private readonly MessagingProvider _messagingProvider;
        private readonly IConfiguration _configuration;

        public ServiceBusTriggerAttributeBindingProvider(INameResolver nameResolver, ServiceBusOptions options, MessagingProvider messagingProvider, IConfiguration configuration)
        {
            _nameResolver = nameResolver ?? throw new ArgumentNullException(nameof(nameResolver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            var attribute = TypeUtility.GetResolvedAttribute<ServiceBusTriggerAttribute>(parameter);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string entityPath = null;
            if (!string.IsNullOrEmpty(attribute.QueueName))
            {
                entityPath = Resolve(attribute.QueueName);
            }
            else if (!string.IsNullOrEmpty(attribute.TopicName) && !string.IsNullOrEmpty(attribute.SubscriptionName))
            {
                entityPath = EntityNameHelper.FormatSubscriptionPath(attribute.TopicName, attribute.SubscriptionName);
            }

            attribute.Connection = Resolve(attribute.Connection);
            ServiceBusAccount account = new ServiceBusAccount(_options, _configuration, entityPath, attribute, attribute.IsSessionsEnabled);

            ITriggerBinding binding;
            if (parameter.ParameterType.IsArray)
            {
                ITriggerDataArgumentBinding<Message[]> argumentArrayBinding = InnerArrayProvider.TryCreate(parameter);
                if (argumentArrayBinding == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Can't bind ServiceBusTrigger to type '{0}'.", parameter.ParameterType));
                }
                binding = new ServiceBusTriggerBinding<Message[]>(parameter.Name, parameter.ParameterType, argumentArrayBinding, account, _options, _messagingProvider);
            }
            else
            {
                ITriggerDataArgumentBinding<Message> argumentBinding = InnerProvider.TryCreate(parameter);
                if (argumentBinding == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Can't bind ServiceBusTrigger to type '{0}'.", parameter.ParameterType));
                }
                binding = new ServiceBusTriggerBinding<Message>(parameter.Name, parameter.ParameterType, argumentBinding, account, _options, _messagingProvider);
            }

            return Task.FromResult<ITriggerBinding>(binding);
        }

        private string Resolve(string queueName)
        {
            if (_nameResolver == null)
            {
                return queueName;
            }

            return _nameResolver.ResolveWholeString(queueName);
        }
    }
}
