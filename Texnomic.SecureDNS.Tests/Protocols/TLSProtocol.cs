﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Texnomic.SecureDNS.Abstractions;
using Texnomic.SecureDNS.Abstractions.Enums;
using Texnomic.SecureDNS.Core;
using Texnomic.SecureDNS.Core.DataTypes;
using Texnomic.SecureDNS.Protocols;
using Texnomic.SecureDNS.Protocols.Options;

namespace Texnomic.SecureDNS.Tests.Protocols
{
    [TestClass]
    public class TLSProtocol
    {
        private IProtocol Resolver;
        private IMessage RequestMessage;
        private IMessage ResponseMessage;


        [TestInitialize]
        public void Initialize()
        {
            var TLSOptions = new TLSOptions()
            {
                IPv4Address = "1.1.1.1",
                Port = 853,
                Timeout = new TimeSpan(0, 0, 0, 10),
                CommonName = "cloudflare-dns.com",
                Thumbprint = "6656840172B4FBBCD6D0A4A103491E93004D195F"
            };

            var TLSOptionsMonitor = Mock.Of<IOptionsMonitor<TLSOptions>>(Options => Options.CurrentValue == TLSOptions);

            Resolver = new TLS(TLSOptionsMonitor);

            RequestMessage = new Message()
            {
                ID = (ushort) new Random().Next(),
                RecursionDesired = true
            };
        }

        [TestMethod]
        [TestProperty("rdweb.wvd.microsoft.com", "A")]
        [TestProperty(" mountaineerpublishing.com", "MX")]
        public async Task ResolveAsync()
        {
            var Type = GetType();

            var Method = Type.GetMethod(nameof(ResolveAsync));

            var Attributes = Method?.GetCustomAttributes(typeof(TestPropertyAttribute), false);

            if (Attributes == null) throw new TestCanceledException();

            foreach (var Attribute in Attributes)
            {
                var TestDomain = ((TestPropertyAttribute) Attribute).Name;

                var TestRecord = Enum.Parse<RecordType>(((TestPropertyAttribute) Attribute).Value);

                RequestMessage.Questions = new List<IQuestion>()
                {
                    new Question()
                    {
                        Domain = Domain.FromString(TestDomain),
                        Class = RecordClass.Internet,
                        Type = TestRecord
                    }
                };

                ResponseMessage = await Resolver.ResolveAsync(RequestMessage);

                Assert.AreEqual(RequestMessage.ID, ResponseMessage.ID);
                //Assert.IsNotNull(ResponseMessage.Questions);
                //Assert.IsNotNull(ResponseMessage.Answers);
                //Assert.IsInstanceOfType(ResponseMessage.Answers.Last().Record, typeof(SecureDNS.Core.Records.A));
            }
        }
    }
}
