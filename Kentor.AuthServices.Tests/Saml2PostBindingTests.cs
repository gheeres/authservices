﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Web;
using FluentAssertions;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Xml;
using System.Text;

namespace Kentor.AuthServices.Tests
{
    [TestClass]
    public class Saml2PostBindingTests
    {
        private HttpRequestBase CreateRequest(string encodedResponse)
        {
            var r = Substitute.For<HttpRequestBase>();
            r.HttpMethod.Returns("POST");
            r.Form.Returns(new NameValueCollection() { { "SAMLResponse", encodedResponse } });
            return r;
        }

        [TestMethod]
        public void Saml2PostBinding_Unbind_Nullcheck()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.Unbind(null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("request");
        }

        [TestMethod]
        public void Saml2PostBinding_Unbind_ThrowsOnNotBase64Encoded()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.Unbind(CreateRequest("foo")))
                .ShouldThrow<FormatException>();
        }

        [TestMethod]
        public void Saml2PostBinding_Unbind_ReadsSaml2ResponseId()
        {
            string response =
            @"responsestring";

            var r = CreateRequest(Convert.ToBase64String(Encoding.UTF8.GetBytes(response)));

            Saml2Binding.Get(Saml2BindingType.HttpPost).Unbind(r).Should().Be(response);
        }

        [TestMethod]
        public void Saml2PostBinding_Bind_Nullcheck_payload()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.Bind(null, new Uri("http://host"), "-"))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("payload");
        }

        [TestMethod]
        public void Saml2PostBinding_Bind_Nullcheck_destinationUri()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.Bind("-", null, "-"))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("destinationUri");
        }

        [TestMethod]
        public void Saml2PostBinding_Bind_Nullcheck_messageName()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.Bind("-", new Uri("http://host"), null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("messageName");
        }

        [TestMethod]
        public void Saml2PostBinding_Bind()
        {
            var xmlData = "<root><content>data</content></root>";
            var destinationUri = new Uri("http://www.example.com/acs");
            var messageName = "SAMLMessageName";

            var subject = Saml2Binding.Get(Saml2BindingType.HttpPost).Bind(xmlData, destinationUri, messageName);

            var expected = new CommandResult()
            {
                Content = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN""
""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"">
<body onload=""document.forms[0].submit()"">
<noscript>
<p>
<strong>Note:</strong> Since your browser does not support JavaScript, 
you must press the Continue button once to proceed.
</p>
</noscript>
<form action=""http://www.example.com/acs"" 
method=""post"">
<div>
<input type=""hidden"" name=""SAMLMessageName"" 
value=""PHJvb3Q+PGNvbnRlbnQ+ZGF0YTwvY29udGVudD48L3Jvb3Q+""/>
</div>
<noscript>
<div>
<input type=""submit"" value=""Continue""/>
</div>
</noscript>
</form>
</body>
</html>"
            };

            subject.ShouldBeEquivalentTo(expected);
        }

        [TestMethod]
        public void Saml2PostBinding_CanUnbind_Nullcheck()
        {
            Saml2Binding.Get(Saml2BindingType.HttpPost)
                .Invoking(b => b.CanUnbind(null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("request");
        }
    }
}
